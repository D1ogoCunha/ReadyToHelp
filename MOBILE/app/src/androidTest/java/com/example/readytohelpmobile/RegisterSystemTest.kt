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
        mockWebServer = MockWebServer()
        mockWebServer.start()

        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create())
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()

        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

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

        composeTestRule.onNodeWithText("Name").performTextInput("New User")
        composeTestRule.onNodeWithText("Email").performTextInput("new@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        try {
            composeTestRule.waitUntil(timeoutMillis = 5000) { registerSuccessCalled }
        } catch (e: Exception) {
            composeTestRule.onRoot().printToLog("DEBUG_FAIL")
            throw e
        }

        val request1 = mockWebServer.takeRequest()
        assert(request1.path == "/user/register")
        assert(request1.method == "POST")

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
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("wrongpass")

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        assert(mockWebServer.requestCount == 0)
    }

    @Test
    fun system_register_server_error_shows_message() {
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
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        composeTestRule.waitUntil(timeoutMillis = 5000) {
            composeTestRule.onAllNodesWithText("Registration failed: 409", substring = true)
                .fetchSemanticsNodes().isNotEmpty()
        }
    }
}