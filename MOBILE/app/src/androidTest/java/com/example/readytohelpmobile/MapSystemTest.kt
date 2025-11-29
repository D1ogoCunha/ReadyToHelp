package com.example.readytohelpmobile

import android.Manifest
import android.util.Base64
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.rule.GrantPermissionRule
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.ui.screens.map.MapScreen
import com.example.readytohelpmobile.utils.TokenManager
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import com.example.readytohelpmobile.viewmodel.MapViewModel
import com.example.readytohelpmobile.viewmodel.ReportViewModel
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import io.mockk.every
import io.mockk.mockkObject
import io.mockk.unmockkAll
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.json.JSONObject
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
class MapSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    @get:Rule
    val permissionRule: GrantPermissionRule = GrantPermissionRule.grant(
        Manifest.permission.ACCESS_FINE_LOCATION,
        Manifest.permission.ACCESS_COARSE_LOCATION
    )

    private lateinit var mockWebServer: MockWebServer

    private lateinit var mapViewModel: MapViewModel
    private lateinit var authViewModel: AuthViewModel
    private lateinit var reportViewModel: ReportViewModel

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

        val jsonPayload = JSONObject().put("id", 100).toString()
        val fakePayload = Base64.encodeToString(jsonPayload.toByteArray(), Base64.URL_SAFE or Base64.NO_PADDING)

        val fakeToken = "fakeHeader.$fakePayload.fakeSignature"

        TokenManager(context).saveToken(fakeToken)

        mapViewModel = MapViewModel(context)
        authViewModel = AuthViewModel(context)
        reportViewModel = ReportViewModel(context)
    }

    @After
    fun tearDown() {
        mockWebServer.shutdown()
        unmockkAll()
    }

    @Test
    fun system_map_loads_and_shows_ui_elements() {
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("[]"))

        composeTestRule.setContent {
            MapScreen(
                viewModel = mapViewModel,
                authViewModel = authViewModel,
                reportViewModel = reportViewModel
            )
        }

        val request = mockWebServer.takeRequest(5, TimeUnit.SECONDS)
        assert(request?.path?.contains("/occurrence") == true)

        composeTestRule.waitForIdle()
        composeTestRule.onNodeWithContentDescription("Report Occurrence").assertIsDisplayed()
        composeTestRule.onNodeWithContentDescription("Logout").assertIsDisplayed()
    }

    @Test
    fun system_report_dialog_submission_flow() {
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("[]"))

        mockWebServer.enqueue(MockResponse()
            .setResponseCode(200)
            .setBody("""
            {
                "reportId": 10,
                "occurrenceId": 100,
                "occurrenceStatus": "WAITING",
                "responsibleEntity": null
            }
        """.trimIndent())
        )

        composeTestRule.setContent {
            MapScreen(
                viewModel = mapViewModel,
                authViewModel = authViewModel,
                reportViewModel = reportViewModel
            )
        }

        mockWebServer.takeRequest()

        composeTestRule.onNodeWithContentDescription("Report Occurrence").performClick()

        composeTestRule.onNodeWithText("Title").performTextInput("Buraco na estrada")
        composeTestRule.onNodeWithText("Description").performTextInput("Perigo para carros")

        composeTestRule.onNodeWithText("Occurrence Type").performClick()
        composeTestRule.onNodeWithText("ROAD ACCIDENT").performClick()

        composeTestRule.onNodeWithText("Submit")
            .assertIsEnabled()
            .performClick()

        val submitButtonNode = composeTestRule.onAllNodesWithText("Submit")
        if (submitButtonNode.fetchSemanticsNodes().isNotEmpty()) {
            submitButtonNode.onFirst().performClick()
        }

        val postRequest = mockWebServer.takeRequest(5, TimeUnit.SECONDS)
        assert(postRequest != null)
        assert(postRequest?.method == "POST")
        assert(postRequest?.path?.contains("/reports") == true)

        val requestBody = postRequest?.body?.readUtf8() ?: ""
        assert(requestBody.contains("Buraco na estrada"))
        assert(requestBody.contains("ROAD_ACCIDENT"))
    }
}