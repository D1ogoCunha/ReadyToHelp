package com.example.readytohelpmobile

import android.Manifest
import android.util.Base64
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.rule.GrantPermissionRule
import com.example.readytohelpmobile.model.MapEvent
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.ui.screens.map.MapScreen
import com.example.readytohelpmobile.utils.TokenManager
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import com.example.readytohelpmobile.viewmodel.MapUiState
import com.example.readytohelpmobile.viewmodel.MapViewModel
import com.example.readytohelpmobile.viewmodel.ReportViewModel
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkObject
import io.mockk.unmockkAll
import io.mockk.verify
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
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

/**
 * System tests for the Map Screen functionality and Report screen.
 *
 * This class validates the integration of the [MapScreen], the Location Services,
 * the [ReportViewModel], and the [MapViewModel]. It mocks the backend API using
 * [MockWebServer] but runs the real UI and ViewModel logic.
 */
@RunWith(AndroidJUnit4::class)
class MapSystemTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    /**
     * Global rule to automatically grant Location permissions.
     */
    @get:Rule
    val permissionRule: GrantPermissionRule = GrantPermissionRule.grant(
        Manifest.permission.ACCESS_FINE_LOCATION,
        Manifest.permission.ACCESS_COARSE_LOCATION
    )

    private lateinit var mockWebServer: MockWebServer

    private lateinit var mapViewModel: MapViewModel
    private lateinit var authViewModel: AuthViewModel
    private lateinit var reportViewModel: ReportViewModel

    /**
     * Sets up the test environment.
     * 1. Starts the MockWebServer.
     * 2. Configures a fake Retrofit instance.
     * 3. Mocks the [NetworkClient] singleton to redirect API calls to localhost.
     * 4. Injects a fake JWT token into [TokenManager]. The [ReportViewModel]
     * requires a logged-in user (with a valid ID) to submit reports.
     */
    @Before
    fun setup() {
        // Start Mock Server
        mockWebServer = MockWebServer()
        mockWebServer.start()

        // Configure Retrofit
        val moshi = Moshi.Builder().addLast(KotlinJsonAdapterFactory()).build()
        val fakeRetrofit = Retrofit.Builder()
            .baseUrl(mockWebServer.url("/"))
            .addConverterFactory(ScalarsConverterFactory.create())
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()

        // Intercept Singleton
        mockkObject(NetworkClient)
        every { NetworkClient.getRetrofitInstance(any()) } returns fakeRetrofit

        val context = ApplicationProvider.getApplicationContext<android.app.Application>()

        // Create a fake JWT payload containing a User ID (id: 100).
        // This simulates a logged-in state so ReportViewModel doesn't throw an "Unauthenticated" error.
        val jsonPayload = JSONObject().put("id", 100).toString()
        val fakePayload = Base64.encodeToString(
            jsonPayload.toByteArray(),
            Base64.URL_SAFE or Base64.NO_PADDING or Base64.NO_WRAP
        )
        val fakeToken = "fakeHeader.$fakePayload.fakeSignature"

        TokenManager(context).saveToken(fakeToken)

        // Initialize ViewModels with test context
        mapViewModel = MapViewModel(context)
        authViewModel = AuthViewModel(context)
        reportViewModel = ReportViewModel(context)
    }

    @After
    fun tearDown() {
        mockWebServer.shutdown()
        unmockkAll()
    }

    /**
     * Tests that the Map Screen loads correctly.
     *
     * Scenario:
     * 1. The screen initializes and requests occurrences from the API.
     * 2. API returns an empty list (200 OK).
     *
     * Expected Outcome:
     * - A GET request is made to the /occurrence endpoint.
     * - Essential UI elements (Report, Logout Button) become visible.
     */
    @Test
    fun system_map_loads_and_shows_ui_elements() {
        // Enqueue empty list of occurrences
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("[]"))

        composeTestRule.setContent {
            MapScreen(
                viewModel = mapViewModel,
                authViewModel = authViewModel,
                reportViewModel = reportViewModel
            )
        }

        // Verify API call
        val request = mockWebServer.takeRequest(5, TimeUnit.SECONDS)
        assert(request?.path?.contains("/occurrence") == true)

        // Wait for UI to settle
        composeTestRule.waitForIdle()

        // Assert UI visibility
        composeTestRule.onNodeWithContentDescription("Report Occurrence").assertIsDisplayed()
        composeTestRule.onNodeWithContentDescription("Logout").assertIsDisplayed()
    }

    /**
     * Tests the full flow of reporting a new occurrence.
     *
     * Scenario:
     * 1. User opens the Report Dialog.
     * 2. Fills in Title and Description.
     * 3. Selects "ROAD ACCIDENT" from the type dropdown.
     * 4. Submits the report.
     * 5. Server responds with success and Entity details (PSP de Lisboa).
     *
     * Expected Outcome:
     * - A POST request is sent with the correct JSON body.
     * - The Success Dialog appears displaying the responsible entity's contact info.
     * - The dialog can be closed, returning to the map.
     */
    @Test
    fun system_report_dialog_submission_flow() {
        // 1. Initial Map Load response
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("[]"))

        // 2. Report Submission Response (Success with Entity Data)
        mockWebServer.enqueue(MockResponse()
            .setResponseCode(200)
            .setBody("""
            {
                "reportId": 10,
                "occurrenceId": 100,
                "occurrenceStatus": "WAITING",
                "responsibleEntity": {
                    "name": "PSP de Lisboa",
                    "email": "contacto@psp.pt",
                    "address": "Esquadra Principal",
                    "contactPhone": 210000000
                }
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

        // Consume the initial map load request
        mockWebServer.takeRequest()
        composeTestRule.waitForIdle()

        // --- Interaction Step ---

        // Open Dialog
        composeTestRule.onNodeWithContentDescription("Report Occurrence").performClick()

        // Fill Text Fields
        composeTestRule.onNodeWithText("Title").performTextInput("Buraco na estrada")
        composeTestRule.onNodeWithText("Description").performTextInput("Perigo para carros")

        // Select Dropdown Item
        composeTestRule.onNodeWithText("Occurrence Type").performClick()
        composeTestRule.onNodeWithText("ROAD ACCIDENT").performClick()

        // Click Submit (Button should be enabled now)
        composeTestRule.onNodeWithText("Submit")
            .assertIsEnabled()
            .performClick()

        // Retry logic for robustness (Double click strategy if first fails due to focus/overlay)
        val submitButtonNode = composeTestRule.onAllNodesWithText("Submit")
        if (submitButtonNode.fetchSemanticsNodes().isNotEmpty()) {
            submitButtonNode.onFirst().performClick()
        }

        // --- Verification Step ---

        // Verify HTTP Request
        val postRequest = mockWebServer.takeRequest(5, TimeUnit.SECONDS)
        assert(postRequest != null)
        assert(postRequest?.method == "POST")
        assert(postRequest?.path?.contains("/reports") == true)

        // Verify Request Body Content
        val requestBody = postRequest?.body?.readUtf8() ?: ""
        assert(requestBody.contains("Buraco na estrada"))
        assert(requestBody.contains("ROAD_ACCIDENT"))

        // Verify Success Dialog Content
        // Wait for the success message to appear
        composeTestRule.waitUntil(timeoutMillis = 10000) {
            composeTestRule.onAllNodesWithText("Report submitted successfully!")
                .fetchSemanticsNodes().isNotEmpty()
        }

        // Check if Entity details from JSON are displayed
        composeTestRule.onNodeWithText("PSP de Lisboa").assertIsDisplayed()
        composeTestRule.onNodeWithText("contacto@psp.pt").assertIsDisplayed()
        composeTestRule.onNodeWithText("210000000").assertIsDisplayed()

        // Close Dialog
        composeTestRule.onNodeWithText("Close").performClick()

        // Ensure we are back on the main screen
        composeTestRule.onNodeWithContentDescription("Report Occurrence").assertIsDisplayed()
    }



    /**
     * Tests the proximity alert Snackbar logic (feedbacks) and user interaction.
     *
     * This test validates that:
     * 1. When the ViewModel emits a [MapEvent] (e.g., "In zone"), the Snackbar appears.
     * 2. The "Confirm" button is displayed correctly.
     * 3. Clicking "Confirm" triggers the [MapViewModel.confirmPresence] function with `true`.
     *
     * **Technical Note on Clock Management:**
     * The Snackbar in [MapScreen] includes a linear progress indicator that animates for 30 seconds.
     * By default, Compose tests wait for all animations to finish (idle state) before performing assertions.
     * To avoid a timeout waiting for the 30s animation, we disable `mainClock.autoAdvance`.
     * We then manually advance the clock by small increments (e.g., 1 second) to allow the
     * Snackbar to transition onto the screen without waiting for the full animation to complete.
     */
    @Test
    fun system_snackbar_appears_and_handles_confirmation() {
        // 1. Queue responses
        // Empty list for map initialization
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("[]"))
        // Response for the feedback submission
        mockWebServer.enqueue(MockResponse().setResponseCode(200).setBody("{}"))

        // 2. Create Mock ViewModel
        // We use a relaxed mock to avoid configuring every single method
        val mockVM = mockk<MapViewModel>(relaxed = true)

        // Mock the UI State to be Success so the map renders
        every { mockVM.uiState } returns MutableStateFlow(
            MapUiState.Success(emptyList())
        )

        // 3. Configure the Event Flow
        // Even if the MapScreen starts collecting slightly after we emit, it will still receive the event.
        val eventFlow = MutableSharedFlow<MapEvent>(replay = 1)
        eventFlow.tryEmit(MapEvent("⚠️ In zone: Teste", 999))
        every { mockVM.mapEvent } returns eventFlow

        // 4. Disable Auto Advance
        // This prevents the test from hanging while waiting for the 30s progress bar animation.
        composeTestRule.mainClock.autoAdvance = false

        composeTestRule.setContent {
            MapScreen(
                viewModel = mockVM,
                authViewModel = authViewModel,
                reportViewModel = reportViewModel
            )
        }

        // 5. Manually advance time
        // Advance 1 second to allow the UI to recompose and the Snackbar to appear.
        composeTestRule.mainClock.advanceTimeBy(1000)

        // 6. Verify and Click
        composeTestRule.onNodeWithText("Confirm").assertIsDisplayed()
        composeTestRule.onNodeWithText("Confirm").performClick()

        // Advance slightly to process the click event
        composeTestRule.mainClock.advanceTimeBy(500)

        // 7. Verify ViewModel interaction
        // Ensure the confirmPresence method was called with the correct ID and boolean.
        verify(timeout = 3000) {
            mockVM.confirmPresence(999, true)
        }

        // Reset clock behavior for subsequent tests
        composeTestRule.mainClock.autoAdvance = true
    }
}