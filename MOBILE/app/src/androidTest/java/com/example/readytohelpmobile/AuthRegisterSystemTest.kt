package com.example.readytohelpmobile

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.core.app.ApplicationProvider
import androidx.test.espresso.Espresso
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

/**
 * System tests for the User Register screen functionality.
 *
 * This class validates the integration between the [RegisterScreen], the [AuthViewModel],
 * and the Data Layer. It uses [MockWebServer] to simulate API responses.
 */
@RunWith(AndroidJUnit4::class)
class AuthRegisterSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    private lateinit var mockWebServer: MockWebServer
    private lateinit var viewModel: AuthViewModel

    /**
     * Prepares the test environment.
     * 1. Resets global Singletons ([NetworkClient]) and Mocks
     * 2. Starts the [MockWebServer].
     * 3. Configures a fake Retrofit instance pointing to `localhost`.
     * 4. Mocks the [NetworkClient] to return the fake Retrofit.
     * 5. Clears SharedPreferences to ensure a clean "logged out" state.
     */
    @Before
    fun setup() {
        // CLEANUP: Ensure clean state for Singleton objects
        unmockkAll()
        NetworkClient.retrofit = null

        // Start Fake Server
        mockWebServer = MockWebServer()
        mockWebServer.start()

        // Configure Retrofit
        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create())
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()

        val context = ApplicationProvider.getApplicationContext<android.app.Application>()

        // Clear Local Storage (Simulate fresh install)
        context.getSharedPreferences("auth_prefs", android.content.Context.MODE_PRIVATE)
            .edit().clear().commit()

        // Inject Mock
        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        viewModel = AuthViewModel(context)
    }

    /**
     * Cleans up resources after each test.
     * Resets the Retrofit singleton and shuts down the mock server.
     */
    @After
    fun tearDown() {
        NetworkClient.retrofit = null
        mockWebServer.shutdown()
        unmockkAll()
    }

    /**
     * Tests the "Happy Path" for registering a new user.
     *
     * Scenario:
     * 1. User fills in valid details.
     * 2. Server accepts registration (200 OK).
     * 3. App automatically attempts login (200 OK with token).
     *
     * Expected Outcome:
     * - The `onRegisterSuccess` callback is invoked.
     * - Two API requests are sent: POST /user/register and POST /auth/login/mobile.
     */
    @Test
    fun system_register_success_flow() {
        // Enqueue responses: 1. Register Success, 2. Auto-Login Success
        mockWebServer.enqueue(MockResponse().setResponseCode(200).addHeader("Content-Type", "application/json").setBody("""{"id": 1, "name": "User", "email": "a@b.com", "profile": "CITIZEN"}"""))
        mockWebServer.enqueue(MockResponse().setResponseCode(200).addHeader("Content-Type", "text/plain").setBody("\"token-123\""))

        var registerSuccessCalled = false
        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = { registerSuccessCalled = true },
                onNavigateToLogin = {},
                viewModel = viewModel
            )
        }

        // Fill Form
        composeTestRule.onNodeWithText("Name").performTextInput("User")
        composeTestRule.onNodeWithText("Email").performTextInput("a@b.com")

        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        // Use Espresso to safely close keyboard (avoids animation issues in Jacoco)
        Espresso.closeSoftKeyboard()

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")
        Espresso.closeSoftKeyboard()

        composeTestRule.waitForIdle()

        // Click Sign Up
        composeTestRule.onNodeWithText("Sign Up")
            .performScrollTo()
            .performClick()

        // Wait for success callback (Extended timeout for Coverage/CI environments)
        try {
            composeTestRule.waitUntil(timeoutMillis = 20000) { registerSuccessCalled }
        } catch (e: Exception) {
            composeTestRule.onRoot().printToLog("DEBUG_SUCCESS_FAIL")
            throw e
        }

        // Verify API Calls
        assert(mockWebServer.takeRequest().path!!.contains("/register"))
        assert(mockWebServer.takeRequest().path!!.contains("/login"))
    }

    /**
     * Tests client-side validation logic.
     *
     * Scenario:
     * 1. User enters passwords that do not match.
     *
     * Expected Outcome:
     * - UI displays "Passwords do not match!".
     * - NO network request is sent to the server.
     */
    @Test
    fun system_register_client_validation_fails() {
        composeTestRule.setContent {
            RegisterScreen(onRegisterSuccess = {}, onNavigateToLogin = {}, viewModel = viewModel)
        }

        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        Espresso.closeSoftKeyboard()

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("wrongpass")
        Espresso.closeSoftKeyboard()

        composeTestRule.waitForIdle()

        composeTestRule.onNodeWithText("Sign Up")
            .performScrollTo()
            .performClick()

        // Verify Error Message
        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        // Verify NO Network Traffic
        if (mockWebServer.requestCount > 0) {
            val request = mockWebServer.takeRequest(100, TimeUnit.MILLISECONDS)
            if (request != null) {
                throw AssertionError("ERROR: Data was sent despite validation failure!")
            }
        }
    }

    /**
     * Tests handling of server-side errors.
     *
     * Scenario:
     * 1. User enters valid data.
     * 2. Server returns 409 Conflict (e.g., Email already exists).
     *
     * Expected Outcome:
     * - UI displays an error message containing the status code (409).
     */
    @Test
    fun system_register_server_error_shows_message() {
        // Enqueue Error Response
        mockWebServer.enqueue(MockResponse().setResponseCode(409).setBody("Email exists"))

        composeTestRule.setContent {
            RegisterScreen(onRegisterSuccess = {}, onNavigateToLogin = {}, viewModel = viewModel)
        }

        // Fill Form
        composeTestRule.onNodeWithText("Name").performTextInput("U")
        composeTestRule.onNodeWithText("Email").performTextInput("e@e.com")

        composeTestRule.onNodeWithText("Password").performTextInput("123")
        Espresso.closeSoftKeyboard()

        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123")
        Espresso.closeSoftKeyboard()

        composeTestRule.waitForIdle()

        // Click Sign Up
        composeTestRule.onNodeWithText("Sign Up")
            .performScrollTo()
            .performClick()

        // Wait for Error Message
        try {
            composeTestRule.waitUntil(timeoutMillis = 10000) {
                composeTestRule.onAllNodesWithText("409", substring = true)
                    .fetchSemanticsNodes().isNotEmpty()
            }
        } catch (e: Exception) {
            throw AssertionError("Error message did not appear in time.", e)
        }
    }
}