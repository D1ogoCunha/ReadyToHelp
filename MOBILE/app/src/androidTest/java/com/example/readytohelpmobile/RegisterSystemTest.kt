package com.example.readytohelpmobile

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.ui.screens.auth.RegisterScreen
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import io.mockk.every
import io.mockk.mockkObject
import io.mockk.unmockkAll
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import retrofit2.converter.scalars.ScalarsConverterFactory

@RunWith(AndroidJUnit4::class)
class RegisterSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    private lateinit var mockWebServer: MockWebServer
    private lateinit var viewModel: AuthViewModel

    @Before
    fun setup() {
        // 1. Iniciar servidor falso
        mockWebServer = MockWebServer()
        mockWebServer.start()

        // 2. Configurar Retrofit com Moshi (JSON) e Scalars (String)
        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create()) // Para o token (String)
            .addConverterFactory(MoshiConverterFactory.create(moshi)) // Para o User (JSON)
            .build()

        // 3. Intercetar o Singleton NetworkClient
        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        // 4. Inicializar o ViewModel REAL
        val context = ApplicationProvider.getApplicationContext<android.app.Application>()
        viewModel = AuthViewModel(context)
    }

    @After
    fun tearDown() {
        mockWebServer.shutdown()
        unmockkAll()
    }

    @Test
    fun system_register_success_flow() {
        // O teu ViewModel faz "Auto-Login" após o registo.
        // Precisamos de ENFILEIRAR 2 RESPOSTAS:

        // Resposta 1: Registo (Devolve objeto UserResponse em JSON)
        // register -> UserResponse
        mockWebServer.enqueue(MockResponse()
            .setResponseCode(200)
            .addHeader("Content-Type", "application/json")
            .setBody("""
                {
                    "id": 101, 
                    "name": "New User", 
                    "email": "new@test.com", 
                    "profile": "CITIZEN"
                }
            """.trimIndent())
        )

        // Resposta 2: Login Automático (Devolve Token em String)
        // login -> String
        mockWebServer.enqueue(MockResponse()
            .setResponseCode(200)
            .addHeader("Content-Type", "text/plain; charset=utf-8")
            .setBody("fake-jwt-token-from-auto-login")
        )

        var registerSuccessCalled = false

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = { registerSuccessCalled = true },
                onNavigateToLogin = {},
                viewModel = viewModel // Usamos o ViewModel REAL
            )
        }

        // 1. Preencher Formulário
        composeTestRule.onNodeWithText("Name").performTextInput("New User")
        composeTestRule.onNodeWithText("Email").performTextInput("new@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        // 2. Clicar em Sign Up (usar scroll para garantir visibilidade)
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // 3. Verificar se o callback de sucesso foi chamado
        // Se falhar aqui, o debug printa a árvore para vermos se houve erro na UI
        try {
            composeTestRule.waitUntil(timeoutMillis = 5000) { registerSuccessCalled }
        } catch (e: Exception) {
            composeTestRule.onRoot().printToLog("DEBUG_FAIL")
            throw e
        }

        // 4. Validar os pedidos HTTP feitos ao servidor falso

        // Pedido 1: Register
        val request1 = mockWebServer.takeRequest()
        assert(request1.path == "/user/register")
        assert(request1.method == "POST")

        // Pedido 2: Login (Auto-login)
        val request2 = mockWebServer.takeRequest()
        assert(request2.path == "/auth/login/mobile")
        assert(request2.method == "POST")
    }

    @Test
    fun system_register_client_validation_fails() {
        // Teste de validação local (Passwords diferentes)
        // Não precisa de respostas do servidor porque o pedido nunca deve sair.

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = viewModel
            )
        }

        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("wrongpass")

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // Verificar erro na UI
        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        // Garantir que NENHUM pedido foi enviado à API
        assert(mockWebServer.requestCount == 0)
    }

    @Test
    fun system_register_server_error_shows_message() {
        // Simular erro do servidor (ex: Email já existe - 409 Conflict ou 400 Bad Request)
        mockWebServer.enqueue(MockResponse()
            .setResponseCode(409)
            .setBody("Email already exists")
        )

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = viewModel
            )
        }

        // Preencher dados válidos
        composeTestRule.onNodeWithText("Name").performTextInput("User")
        composeTestRule.onNodeWithText("Email").performTextInput("exists@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // Verificar se a mensagem de erro vinda do servidor (ou genérica do VM) aparece
        // O AuthViewModel faz: _uiState.value = AuthUiState.Error(e.message)
        // O Retrofit lança exceção no erro, o ViewModel apanha.
        composeTestRule.waitUntil(timeoutMillis = 5000) {
            composeTestRule.onAllNodesWithText("Registration failed: 409", substring = true)
                .fetchSemanticsNodes().isNotEmpty()
        }
    }
}