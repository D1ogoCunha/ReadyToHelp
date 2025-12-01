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
import java.util.concurrent.TimeUnit

@RunWith(AndroidJUnit4::class)
class RegisterSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    private lateinit var mockWebServer: MockWebServer
    private lateinit var viewModel: AuthViewModel

    @Before
    fun setup() {
        mockWebServer = MockWebServer()
        mockWebServer.start()

        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create())
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()

        NetworkClient.retrofit = null
        val context = ApplicationProvider.getApplicationContext<android.app.Application>()
        context.getSharedPreferences("auth_prefs", android.content.Context.MODE_PRIVATE).edit().clear().commit()

        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        viewModel = AuthViewModel(context)
    }

    @After
    fun tearDown() {
        NetworkClient.retrofit = null
        mockWebServer.shutdown()
        unmockkAll()
    }

    @Test
    fun system_register_success_flow() {
        mockWebServer.enqueue(MockResponse().setResponseCode(200).addHeader("Content-Type", "application/json").setBody("""{"id": 1, "name": "User", "email": "a@b.com", "profile": "CITIZEN"}"""))
        mockWebServer.enqueue(MockResponse().setResponseCode(200).addHeader("Content-Type", "text/plain").setBody("token-123"))

        var registerSuccessCalled = false
        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = { registerSuccessCalled = true },
                onNavigateToLogin = {},
                viewModel = viewModel
            )
        }

        composeTestRule.onNodeWithText("Name").performTextInput("User")
        composeTestRule.onNodeWithText("Email").performTextInput("a@b.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")

        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}
        composeTestRule.waitForIdle()

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        try {
            composeTestRule.waitUntil(5000) { registerSuccessCalled }
        } catch (e: Exception) {
            composeTestRule.onRoot().printToLog("DEBUG_SUCCESS_FAIL")
            throw e
        }

        assert(mockWebServer.takeRequest().path!!.contains("/register"))
        assert(mockWebServer.takeRequest().path!!.contains("/login"))
    }

    @Test
    fun system_register_client_validation_fails() {
        composeTestRule.setContent {
            RegisterScreen(onRegisterSuccess = {}, onNavigateToLogin = {}, viewModel = viewModel)
        }

        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("wrongpass")
        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}

        composeTestRule.waitForIdle()
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        // CORREÇÃO: Verificar se houve pedidos IMPORTANTES (POST)
        // Se houver pedidos GET /occurrence, ignoramos (são lixo de outros testes)
        if (mockWebServer.requestCount > 0) {
            // Drenar a fila para ver se há algum POST
            var request = mockWebServer.takeRequest(100, TimeUnit.MILLISECONDS)
            while (request != null) {
                if (request.method == "POST") {
                    throw AssertionError("ERRO CRÍTICO: Foi enviado um POST ${request.path} com dados inválidos!")
                }
                // Se for GET (ex: /occurrence), ignoramos e continuamos a procurar
                request = mockWebServer.takeRequest(100, TimeUnit.MILLISECONDS)
            }
        }
    }

    @Test
    fun system_register_server_error_shows_message() {
        mockWebServer.enqueue(MockResponse().setResponseCode(409).setBody("Email exists"))

        composeTestRule.setContent {
            RegisterScreen(onRegisterSuccess = {}, onNavigateToLogin = {}, viewModel = viewModel)
        }

        composeTestRule.onNodeWithText("Name").performTextInput("U")
        composeTestRule.onNodeWithText("Email").performTextInput("e@e.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123")
        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123")
        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}

        composeTestRule.waitForIdle()
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        try {
            composeTestRule.waitUntil(timeoutMillis = 5000) {
                composeTestRule.onAllNodesWithText("409", substring = true)
                    .fetchSemanticsNodes().isNotEmpty()
            }
        } catch (e: androidx.compose.ui.test.ComposeTimeoutException) {
            composeTestRule.onRoot().printToLog("DEBUG_UI_ERROR")
            throw AssertionError("Mensagem de erro não apareceu. Ver Logcat DEBUG_UI_ERROR", e)
        }
    }
}