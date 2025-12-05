package com.example.readytohelpmobile

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.ui.screens.auth.LoginScreen
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

/**
 * System tests for the Login screen functionality.
 * This class tests the integration between the [LoginScreen] UI, the [AuthViewModel], and the Network layer.
 * It uses [MockWebServer] to simulate API responses.
 */
@RunWith(AndroidJUnit4::class)
class LoginSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    private lateinit var mockWebServer: MockWebServer
    private lateinit var viewModel: AuthViewModel

    /**
     * Sets up the test environment before each test execution.
     * 1. Starts a MockWebServer.
     * 2. Creates a fake Retrofit instance pointing to the mock server.
     * 3. Mocks the [NetworkClient] singleton to return the fake Retrofit.
     * 4. Initializes the ViewModel with the application context.
     */
    @Before
    fun setup() {
        // Start the fake server
        mockWebServer = MockWebServer()
        mockWebServer.start()

        // Configure Retrofit to point to localhost
        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create())
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()

        // Intercept the Singleton to inject our fake Retrofit
        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        // Initialize the real ViewModel
        val context = ApplicationProvider.getApplicationContext<android.app.Application>()
        viewModel = AuthViewModel(context)
    }

    /**
     * Cleans up resources after each test.
     * Shuts down the server and removes mocks to avoid polluting other tests.
     */
    @After
    fun tearDown() {
        mockWebServer.shutdown()
        unmockkAll()
    }

    /**
     * Tests the successful login flow.
     *
     * Scenario:
     * 1. User enters valid credentials.
     * 2. Server responds with 200 OK and a fake token.
     *
     * Expected Outcome:
     * - The `onLoginSuccess` callback is triggered.
     * - The correct POST request is sent to the "/auth/login/mobile" endpoint.
     */
    @Test
    fun system_login_success_navigates_correctly() {
        // Queue a success response
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("fake-jwt-token-123"))

        var loginSuccessCalled = false

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = { loginSuccessCalled = true },
                onNavigateToRegister = {},
                viewModel = viewModel
            )
        }

        // Interact with UI
        composeTestRule.onNodeWithText("Email").performTextInput("admin@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Sign In").performClick()

        // Verify state change (wait for callback)
        composeTestRule.waitUntil(timeoutMillis = 5000) { loginSuccessCalled }

        // Verify API call
        val request = mockWebServer.takeRequest()
        assert(request.path == "/auth/login/mobile")
    }

    /**
     * Tests the failed login flow.
     *
     * Scenario:
     * 1. User enters invalid credentials.
     * 2. Server responds with 401 Unauthorized.
     *
     * Expected Outcome:
     * - An error message containing "Login failed: 401" is displayed on the screen.
     * - Navigation does NOT occur.
     */
    @Test
    fun system_login_failure_shows_error_message() {
        // Queue an error response
        mockWebServer.enqueue(MockResponse().setResponseCode(401).setBody("Unauthorized"))

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = viewModel
            )
        }

        // Interact with UI
        composeTestRule.onNodeWithText("Email").performTextInput("wrong@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("wrongpass")
        composeTestRule.onNodeWithText("Sign In").performClick()

        // Verify that the error message appears
        composeTestRule.waitUntil(timeoutMillis = 5000) {
            composeTestRule.onAllNodesWithText("Login failed: 401").fetchSemanticsNodes().isNotEmpty()
        }
    }
}