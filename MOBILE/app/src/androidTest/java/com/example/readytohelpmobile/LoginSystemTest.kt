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

@RunWith(AndroidJUnit4::class)
class LoginSystemTest {

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
            .baseUrl(mockWebServer.url("/")) // Aponta para o teste
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
    fun system_login_success_navigates_correctly() {

        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("fake-jwt-token-123"))

        var loginSuccessCalled = false

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = { loginSuccessCalled = true },
                onNavigateToRegister = {},
                viewModel = viewModel // ViewModel REAL
            )
        }

        composeTestRule.onNodeWithText("Email").performTextInput("admin@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")

        composeTestRule.onNodeWithText("Sign In").performClick()

        composeTestRule.waitUntil(timeoutMillis = 5000) { loginSuccessCalled }

        val request = mockWebServer.takeRequest()
        assert(request.path == "/auth/login/mobile")
    }

    @Test
    fun system_login_failure_shows_error_message() {
        mockWebServer.enqueue(MockResponse().setResponseCode(401).setBody("Unauthorized"))

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = viewModel
            )
        }

        composeTestRule.onNodeWithText("Email").performTextInput("wrong@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("wrongpass")
        composeTestRule.onNodeWithText("Sign In").performClick()

        composeTestRule.waitUntil(timeoutMillis = 5000) {
            composeTestRule.onAllNodesWithText("Login failed: 401").fetchSemanticsNodes().isNotEmpty()
        }
    }
}