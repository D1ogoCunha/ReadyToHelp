package com.example.readytohelpmobile.viewmodel

import android.annotation.SuppressLint
import android.app.Application
import android.location.Location
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.model.report.Report
import com.example.readytohelpmobile.model.report.OccurrenceType
import com.example.readytohelpmobile.model.report.ReportResponse
import com.example.readytohelpmobile.services.ReportService
import com.example.readytohelpmobile.utils.TokenManager
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

/**
 * Represents the UI state for the Report screen/dialog.
 */
sealed class ReportUiState {
    /** Initial state, nothing happening. */
    data object Idle : ReportUiState()

    /** Submitting report to server. */
    data object Loading : ReportUiState()

    /** Report submitted successfully. */
    data class Success(val response: ReportResponse) : ReportUiState()

    /** Error during submission or location retrieval. */
    data class Error(val message: String) : ReportUiState()
}

/**
 * ViewModel responsible for handling the creation of new occurrence reports.
 * It manages location retrieval and API submission.
 */
class ReportViewModel(application: Application) : AndroidViewModel(application) {

    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    private val _uiState = MutableStateFlow<ReportUiState>(ReportUiState.Idle)

    /** Public observable state. */
    val uiState: StateFlow<ReportUiState> = _uiState

    /**
     * Resets the UI state back to Idle.
     * Useful after a success dialog is closed.
     */
    fun resetState() {
        _uiState.value = ReportUiState.Idle
    }

    /**
     * Submits a new report to the backend.
     * 1. Gets current user location.
     * 2. Gets user ID from token.
     * 3. Sends data to API.
     *
     * @param title Title of the report.
     * @param description Description of the report.
     * @param type Type of occurrence.
     */
    fun submitReport(
        title: String,
        description: String,
        type: OccurrenceType
    ) {
        viewModelScope.launch {
            _uiState.value = ReportUiState.Loading

            // Get location first
            val location = getCurrentLocation()
            if (location == null) {
                _uiState.value = ReportUiState.Error("It was not possible to get the user location.")
                return@launch
            }

            // Get User ID
            val userId = TokenManager(getApplication()).getUserIdFromToken()
            if (userId == 0) {
                _uiState.value = ReportUiState.Error("User not authenticated.")
                return@launch
            }

            // Create DTO
            val dto = Report(
                title = title,
                description = description,
                type = type,
                userId = userId,
                latitude = location.latitude,
                longitude = location.longitude
            )

            // Call Service
            val result = ReportService.createReport(getApplication(), dto)
            result
                .onSuccess { resp ->
                    _uiState.value = ReportUiState.Success(resp)
                }
                .onFailure { e ->
                    _uiState.value =
                        ReportUiState.Error(e.message ?: "Error submitting report.")
                }
        }
    }

    /**
     * Suspend function to get the current location once.
     * Uses a cancellable coroutine to wrap the Play Services callback API.
     *
     * @return The [Location] object or null if failed.
     */
    @SuppressLint("MissingPermission")
    private suspend fun getCurrentLocation(): Location? {
        return kotlinx.coroutines.suspendCancellableCoroutine { cont ->
            try {
                fusedLocationClient.getCurrentLocation(
                    Priority.PRIORITY_HIGH_ACCURACY,
                    null
                ).addOnSuccessListener { loc ->
                    // Resume coroutine with location
                    cont.resume(loc, onCancellation = null)
                }.addOnFailureListener { e ->
                    // Resume with null on failure
                    cont.resume(null, onCancellation = null)
                }
            } catch (e: Exception) {
                cont.resume(null, onCancellation = null)
            }
        }
    }
}