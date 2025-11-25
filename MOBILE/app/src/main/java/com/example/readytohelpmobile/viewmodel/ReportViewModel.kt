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

sealed class ReportUiState {
    data object Idle : ReportUiState()
    data object Loading : ReportUiState()
    data class Success(val response: ReportResponse) : ReportUiState()
    data class Error(val message: String) : ReportUiState()
}

class ReportViewModel(application: Application) : AndroidViewModel(application) {

    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    private val _uiState = MutableStateFlow<ReportUiState>(ReportUiState.Idle)
    val uiState: StateFlow<ReportUiState> = _uiState

    fun resetState() {
        _uiState.value = ReportUiState.Idle
    }

    fun submitReport(
        title: String,
        description: String,
        type: OccurrenceType
    ) {
        viewModelScope.launch {
            _uiState.value = ReportUiState.Loading

            val location = getCurrentLocation()
            if (location == null) {
                _uiState.value = ReportUiState.Error("It was not possible to get the user location.")
                return@launch
            }

            val userId = TokenManager(getApplication()).getUserIdFromToken()
            if (userId == 0) {
                _uiState.value = ReportUiState.Error("User not authenticated.")
                return@launch
            }

            val dto = Report(
                title = title,
                description = description,
                type = type,
                userId = userId,
                latitude = location.latitude,
                longitude = location.longitude
            )

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

    @SuppressLint("MissingPermission")
    private suspend fun getCurrentLocation(): Location? {
        return kotlinx.coroutines.suspendCancellableCoroutine { cont ->
            try {
                fusedLocationClient.getCurrentLocation(
                    Priority.PRIORITY_HIGH_ACCURACY,
                    null
                ).addOnSuccessListener { loc ->
                    cont.resume(loc, onCancellation = null)
                }.addOnFailureListener { e ->
                    cont.resume(null, onCancellation = null)
                }
            } catch (e: Exception) {
                cont.resume(null, onCancellation = null)
            }
        }
    }
}
