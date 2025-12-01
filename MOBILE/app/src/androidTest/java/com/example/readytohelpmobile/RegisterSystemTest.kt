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
        // Start a local server to mock API responses
        mockWebServer = MockWebServer()
        mockWebServer.start()

        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()

        // Configure Retrofit to point to localhost instead of the real API
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create()) // For plain string tokens
            .addConverterFactory(MoshiConverterFactory.create(moshi)) // For JSON objects
            .build()

        // Intercept the Singleton NetworkClient to return our fake Retrofit instance
        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        // Initialize the real ViewModel with the test context
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
        // The app performs auto-login after registration, so we need two responses in the queue.

        // 1. Response for Registration (JSON User Object)
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

        // 2. Response for Auto-Login (String Token)
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
                viewModel = viewModel
            )
        }

        // Fill form fields
        composeTestRule.onNodeWithText("Name").performTextInput("New User")
        composeTestRule.onNodeWithText("Email").performTextInput("new@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        // Scroll ensures the button is visible on smaller screens
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // Wait for the success callback (which triggers on successful auto-login)
        try {
            composeTestRule.waitUntil(timeoutMillis = 5000) { registerSuccessCalled }
        } catch (e: Exception) {
            // Debug helper: print UI hierarchy if timeout occurs
            composeTestRule.onRoot().printToLog("DEBUG_FAIL")
            throw e
        }

        // Verify the Registration request
        val request1 = mockWebServer.takeRequest()
        assert(request1.path == "/user/register")
        assert(request1.method == "POST")

        // Verify the Auto-Login request
        val request2 = mockWebServer.takeRequest()
        assert(request2.path == "/auth/login/mobile")
        assert(request2.method == "POST")
    }

    @Test
    fun system_register_client_validation_fails() {
        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = viewModel
            )
        }

        composeTestRule.onNodeWithText("Password").performTextInput("123456")

        // Explicitly close keyboard to prevent UI instability
        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}

        // Enter mismatching password
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("wrongpass")

        try { androidx.test.espresso.Espresso.closeSoftKeyboard() } catch (e: Exception) {}
        composeTestRule.waitForIdle()

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // Verify the validation error is displayed
        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        // Critical: Ensure NO network request was sent
        if (mockWebServer.requestCount > 0) {
            val request = mockWebServer.takeRequest()
            throw AssertionError(
                "O teste falhou porque um pedido foi enviado ao servidor!\n" +
                        "Endpoint chamado: ${request.method} ${request.path}\n" +
                        "Corpo do pedido: ${request.body.readUtf8()}\n" +
                        "Isto não devia acontecer com passwords diferentes."
            )
        }

        assert(mockWebServer.requestCount == 0)
    }

    @Test
    fun system_register_server_error_shows_message() {
        // Simulate a 409 Conflict error (e.g., Email already exists)
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

        composeTestRule.onNodeWithText("Name").performTextInput("User")
        composeTestRule.onNodeWithText("Email").performTextInput("exists@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")

        try {
            androidx.test.espresso.Espresso.closeSoftKeyboard()
        } catch (e: Exception) {  }

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        composeTestRule.waitForIdle()
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // Diagnostic: Verify the button click actually triggered a request
        try {
            val request = mockWebServer.takeRequest(5, java.util.concurrent.TimeUnit.SECONDS)
            assert(request != null) { "O pedido não chegou ao servidor. O botão 'Sign Up' foi clicado?" }
        } catch (e: Exception) {
            composeTestRule.onRoot().printToLog("DEBUG_FAIL_CLICK")
            throw e
        }

        // Wait for the error message to appear in the UI
        try {
            composeTestRule.waitUntil(timeoutMillis = 5000) {
                // Match substring because the full error might include HTTP codes
                composeTestRule.onAllNodesWithText("Registration failed: 409", substring = true)
                    .fetchSemanticsNodes().isNotEmpty()
            }
        } catch (e: androidx.compose.ui.test.ComposeTimeoutException) {
            // Print UI tree to Logcat to debug what was actually displayed
            composeTestRule.onRoot().printToLog("DEBUG_UI_ERROR")

            throw AssertionError("O teste falhou. Verifica no Logcat pela tag 'DEBUG_UI_ERROR' para ver o que estava escrito no ecrã em vez da mensagem esperada.", e)
        }
    }
}