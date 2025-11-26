package com.example.readytohelpmobile.viewmodel

import android.annotation.SuppressLint
import android.app.Application
import android.location.Location
import android.util.Log
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.model.Feedback
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.model.MapEvent
import com.example.readytohelpmobile.services.FeedbackService
import com.example.readytohelpmobile.services.OccurrenceService
import com.example.readytohelpmobile.utils.TokenManager
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.mapbox.geojson.Point
import kotlinx.coroutines.Job
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.receiveAsFlow
import kotlinx.coroutines.launch
import java.util.Timer
import java.util.TimerTask

/**
 * Represents the different states of the User Interface (UI) of the Map screen.
 * Uses a Sealed Class to ensure that only these states are possible.
 */
sealed class MapUiState {
    /**
     * Loading state. Indicates that the application is fetching data or location.
     * The UI should display a progress indicator (spinner).
     */
    data object Loading : MapUiState()

    /**
     * Success state. Indicates that the data was loaded correctly.
     *
     * @property occurrences The list of active occurrences obtained from the API.
     */
    data class Success(val occurrences: List<Occurrence>) : MapUiState()

    /**
     * Error state. Indicates that there was a failure loading data or location.
     *
     * @property message A descriptive error message to show the user.
     */
    data class Error(val message: String) : MapUiState()
}
/**
 * ViewModel for the Map Screen.
 * Handles fetching occurrences, tracking user location, calculating proximity to occurrences,
 * and submitting user feedback (confirmation of presence).
 */
class MapViewModel(application: Application) : AndroidViewModel(application) {

    // Client for accessing device location
    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    // Manager for JWT tokens
    private val tokenManager = TokenManager(application)

    // Channels for one-time events (like Toasts or Snackbars)
    private val _toastEvent = Channel<String>(Channel.BUFFERED)
    private val _mapEvent = Channel<MapEvent>(Channel.BUFFERED)

    // Public flows for UI to observe events
    val mapEvent = _mapEvent.receiveAsFlow()
    val toastEvent = _toastEvent.receiveAsFlow()

    // State for current user location
    private val _currentLocation = MutableStateFlow<Point?>(null)
    val currentLocation: StateFlow<Point?> = _currentLocation.asStateFlow()

    // Main UI state (Loading, Success, Error)
    private val _uiState = MutableStateFlow<MapUiState>(MapUiState.Loading)
    val uiState: StateFlow<MapUiState> = _uiState.asStateFlow()

    // Set to track occurrences user has already been notified about to avoid spamming
    private val notifiedOccurrences = mutableSetOf<Int>()

    // Job for the periodic update loop
    private var updateJob: Job? = null

    init {
        // Start periodic tasks on initialization
        startOccurrenceUpdates()
        startLocationUpdates()
    }

    /**
     * Starts a coroutine that fetches occurrences every 30 seconds.
     */
    private fun startOccurrenceUpdates() {
        updateJob?.cancel()
        updateJob = viewModelScope.launch {
            while (true) {
                fetchOccurrences()
                delay(30000) // Wait 30 seconds
            }
        }
    }

    /**
     * Fetches the list of occurrences from the API.
     * Updates the UI state and checks proximity if location is known.
     */
    fun fetchOccurrences() {
        viewModelScope.launch {
            // Only show loading state if we don't have data yet (avoids flickering on updates)
            if (_uiState.value !is MapUiState.Success && _uiState.value !is MapUiState.Error) {
                _uiState.value = MapUiState.Loading
            }

            try {
                Log.d("MapViewModel", "Starting fetchOccurrences...")

                // Pass application context to ensure Retrofit initializes correctly
                val result = OccurrenceService.getAllOccurrences(getApplication())

                if (result != null) {
                    Log.d("MapViewModel", "Occurrences loaded: ${result.size}")
                    // Filter for active occurrences with valid locations
                    val activeOccurrences = result.filter {
                        it.location != null && it.status == "ACTIVE"
                    }

                    _uiState.value = MapUiState.Success(activeOccurrences)

                    // If user location is known, check if they are near any occurrence
                    _currentLocation.value?.let { userLoc ->
                        checkProximity(userLoc, activeOccurrences)
                    }
                } else {
                    Log.e("MapViewModel", "Occurrences returned NULL")
                    if (_uiState.value !is MapUiState.Success) {
                        _uiState.value = MapUiState.Error("Could not load occurrences.")
                    }
                }
            } catch (e: Exception) {
                Log.e("MapViewModel", "Critical error in fetchOccurrences", e)
                if (_uiState.value !is MapUiState.Success) {
                    _uiState.value = MapUiState.Error("Error: ${e.localizedMessage}")
                }
                e.printStackTrace()
            }
        }
    }

    /**
     * Requests the single current location of the user with high accuracy.
     * Updates [_currentLocation] and triggers proximity check.
     */
    @SuppressLint("MissingPermission")
    fun getUserLocation() {
        try {
            fusedLocationClient.getCurrentLocation(Priority.PRIORITY_HIGH_ACCURACY, null)
                .addOnSuccessListener { location ->
                    if (location != null) {
                        val point = Point.fromLngLat(location.longitude, location.latitude)
                        _currentLocation.value = point

                        val currentState = _uiState.value
                        if (currentState is MapUiState.Success) {
                            checkProximity(point, currentState.occurrences)
                        }
                    }
                }
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    /**
     * Starts a timer to update user location every 3 seconds.
     * Note: In a real production app, consider using LocationCallback for better battery efficiency.
     */
    private fun startLocationUpdates() {
        Timer().scheduleAtFixedRate(object : TimerTask() {
            override fun run() {
                getUserLocation()
            }
        }, 0, 3000)
    }

    /**
     * Checks the distance between the user and all active occurrences.
     * Triggers a [MapEvent] if the user enters the proximity radius of an occurrence.
     */
    private fun checkProximity(userPoint: Point, occurrences: List<Occurrence>) {
        occurrences.forEach { occurrence ->
            occurrence.location?.let { occLocation ->
                val results = FloatArray(1)
                Location.distanceBetween(
                    userPoint.latitude(), userPoint.longitude(),
                    occLocation.latitude, occLocation.longitude,
                    results
                )

                val distanceInMeters = results[0]

                if (distanceInMeters <= occurrence.proximityRadius) {
                    // User entered the radius
                    if (!notifiedOccurrences.contains(occurrence.id)) {
                        viewModelScope.launch {
                            _mapEvent.send(
                                MapEvent(
                                    message = "⚠️ In zone: ${occurrence.title}",
                                    occurrenceId = occurrence.id
                                )
                            )
                        }
                        notifiedOccurrences.add(occurrence.id)
                    }
                } else {
                    // User left the radius, allow notification again
                    if (notifiedOccurrences.contains(occurrence.id)) {
                        notifiedOccurrences.remove(occurrence.id)
                    }
                }
            }
        }
    }

    /**
     * Sends a feedback to the API confirming (or not) the presence of an occurrence.
     *
     * @param occurrenceId The ID of the occurrence.
     * @param confirmed True if user confirms the occurrence exists, False otherwise.
     */
    fun confirmPresence(occurrenceId: Int, confirmed: Boolean) {
        viewModelScope.launch {
            val userId = tokenManager.getUserIdFromToken()

            if (userId == 0) {
                _toastEvent.send("Error: Invalid Session")
                return@launch
            }

            val feedback = Feedback(
                occurrenceId = occurrenceId,
                userId = userId,
                isConfirmed = confirmed
            )

            val success = FeedbackService.createFeedback(feedback)

            if (success) {
                _toastEvent.send("Thanks for your feedback")
            } else {
                _toastEvent.send("Already reported this occurence in the last hour")
            }
        }
    }

    override fun onCleared() {
        super.onCleared()
        // Clean up the periodic update job to prevent memory leaks
        updateJob?.cancel()
    }
}