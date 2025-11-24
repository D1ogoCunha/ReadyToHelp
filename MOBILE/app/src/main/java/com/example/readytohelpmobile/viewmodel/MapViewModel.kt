package com.example.readytohelpmobile.viewmodel

import android.annotation.SuppressLint
import android.app.Application
import android.location.Location
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.model.Feedback
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.services.FeedbackService
import com.example.readytohelpmobile.services.OccurrenceService
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.mapbox.geojson.Point
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.receiveAsFlow
import kotlinx.coroutines.launch
import java.util.Timer
import java.util.TimerTask

data class MapEvent(
    val message: String,
    val occurrenceId: Int
)

sealed class MapUiState {
    data object Loading : MapUiState()
    data class Success(val occurrences: List<Occurrence>) : MapUiState()
    data class Error(val message: String) : MapUiState()
}

class MapViewModel(application: Application) : AndroidViewModel(application) {

    private val occurrenceService = OccurrenceService(application)

    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    private val _toastEvent = Channel<String>(Channel.BUFFERED)
    private val _mapEvent = Channel<MapEvent>(Channel.BUFFERED)

    val mapEvent = _mapEvent.receiveAsFlow()
    val toastEvent = _toastEvent.receiveAsFlow()

    private val _currentLocation = MutableStateFlow<Point?>(null)
    val currentLocation: StateFlow<Point?> = _currentLocation.asStateFlow()

    private val _uiState = MutableStateFlow<MapUiState>(MapUiState.Loading)
    val uiState: StateFlow<MapUiState> = _uiState.asStateFlow()

    private val notifiedOccurrences = mutableSetOf<Int>()

    init {
        fetchOccurrences()
        startLocationUpdates()
    }

    fun fetchOccurrences() {
        viewModelScope.launch {
            _uiState.value = MapUiState.Loading
            val result = OccurrenceService.getAllOccurrences()

            if (result != null) {
                val activeOccurrences = result.filter {
                    it.location != null && it.status == "ACTIVE"
                }
                _uiState.value = MapUiState.Success(activeOccurrences)

                _currentLocation.value?.let { userLoc ->
                    checkProximity(userLoc, activeOccurrences)
                }
            } else {
                _uiState.value = MapUiState.Error("Error loading occurrences.")
            }
        }
    }

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

    private fun startLocationUpdates() {
        Timer().scheduleAtFixedRate(object : TimerTask() {
            override fun run() {
                getUserLocation()
            }
        }, 0, 3000)
    }

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
                    if (!notifiedOccurrences.contains(occurrence.id)) {
                        viewModelScope.launch {
                            // Envia para o canal que o Snackbar ouve
                            _mapEvent.send(
                                MapEvent(
                                    message = "⚠️ Na zona de: ${occurrence.title}",
                                    occurrenceId = occurrence.id
                                )
                            )
                        }
                        notifiedOccurrences.add(occurrence.id)
                    }
                } else {
                    if (notifiedOccurrences.contains(occurrence.id)) {
                        notifiedOccurrences.remove(occurrence.id)
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    fun confirmPresence(occurrenceId: Int, confirmed: Boolean) {
        viewModelScope.launch {
            val feedback = Feedback(
                occurrenceId = occurrenceId,
                userId = 7,
                isConfirmed = confirmed
            )

            FeedbackService.createFeedback(feedback)
        }
    }
}