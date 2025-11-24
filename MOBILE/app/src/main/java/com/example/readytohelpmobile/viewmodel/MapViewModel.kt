package com.example.readytohelpmobile.viewmodel

import android.annotation.SuppressLint
import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.services.OccurrenceService
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.mapbox.geojson.Point
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class MapUiState {
    data object Loading : MapUiState()
    data class Success(val occurrences: List<Occurrence>) : MapUiState()
    data class Error(val message: String) : MapUiState()
}

class MapViewModel(application: Application) : AndroidViewModel(application) {

    private val occurrenceService = OccurrenceService(application)

    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    private val _currentLocation = MutableStateFlow<Point?>(null)
    val currentLocation: StateFlow<Point?> = _currentLocation.asStateFlow()

    private val _uiState = MutableStateFlow<MapUiState>(MapUiState.Loading)
    val uiState: StateFlow<MapUiState> = _uiState.asStateFlow()

    init {
        fetchOccurrences()
    }

    fun fetchOccurrences() {
        viewModelScope.launch {
            _uiState.value = MapUiState.Loading

            val result = occurrenceService.getActiveOccurrences()

            if (result != null) {
                _uiState.value = MapUiState.Success(result)
            } else {
                _uiState.value = MapUiState.Error("Não foi possível carregar as ocorrências.")
            }
        }
    }

    @SuppressLint("MissingPermission")
    fun getUserLocation() {
        viewModelScope.launch {
            try {
                fusedLocationClient.lastLocation.addOnSuccessListener { location ->
                    location?.let {
                        _currentLocation.value = Point.fromLngLat(it.longitude, it.latitude)
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }
}