package com.example.readytohelpmobile.viewModel

import android.annotation.SuppressLint
import android.app.Application
import android.location.Location
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.services.OccurrenceService
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.mapbox.geojson.Point
import kotlinx.coroutines.channels.Channel // Importante
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.receiveAsFlow // Importante
import kotlinx.coroutines.launch
import java.util.Timer
import java.util.TimerTask

sealed class MapUiState {
    data object Loading : MapUiState()
    data class Success(val occurrences: List<Occurrence>) : MapUiState()
    data class Error(val message: String) : MapUiState()
}

class MapViewModel(application: Application) : AndroidViewModel(application) {
    private val fusedLocationClient: FusedLocationProviderClient =
        LocationServices.getFusedLocationProviderClient(application)

    private val _toastEvent = Channel<String>(Channel.BUFFERED)
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
            println("DEBUG: A buscar ocorrências...") // Log

            val result = OccurrenceService.getAllOccurrences()

            if (result != null) {
                // Filtra apenas as ativas e com localização válida
                val activeOccurrences = result.filter {
                    it.location != null && it.status == "ACTIVE"
                }

                println("DEBUG: ${activeOccurrences.size} ocorrências ativas carregadas.") // Log
                _uiState.value = MapUiState.Success(activeOccurrences)

                // Verifica logo se já estamos numa ocorrência ao carregar
                _currentLocation.value?.let { userLoc ->
                    checkProximity(userLoc, activeOccurrences)
                }
            } else {
                println("DEBUG: Erro ao carregar ocorrências.") // Log
                _uiState.value = MapUiState.Error("Error loading occurrences.")
            }
        }
    }

    @SuppressLint("MissingPermission")
    fun getUserLocation() {
        try {
            // Força alta precisão para garantir que o emulador responde
            fusedLocationClient.getCurrentLocation(Priority.PRIORITY_HIGH_ACCURACY, null)
                .addOnSuccessListener { location ->
                    if (location != null) {
                        val point = Point.fromLngLat(location.longitude, location.latitude)
                        _currentLocation.value = point

                        val currentState = _uiState.value
                        if (currentState is MapUiState.Success) {
                            checkProximity(point, currentState.occurrences)
                        }
                    } else {
                    }
                }
                .addOnFailureListener { e ->
                }
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    private fun startLocationUpdates() {
        // Atualiza a cada 3 segundos para ser mais reativo no teste
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
                val radius = occurrence.proximityRadius

                // Log detalhado para uma ocorrência específica (ex: a primeira da lista) para não spammar
                if (occurrences.indexOf(occurrence) == 0) {
                    // println("DEBUG: Distância para ${occurrence.title}: ${distanceInMeters}m (Raio: ${radius}m)")
                }

                if (distanceInMeters <= radius) {
                    // Se entrou no raio
                    if (!notifiedOccurrences.contains(occurrence.id)) {
                        println("DEBUG: !!! DENTRO DO RAIO !!! A enviar Toast para: ${occurrence.title}")

                        viewModelScope.launch {
                            _toastEvent.send("⚠️ Entrou na zona de perigo: ${occurrence.title}")
                        }
                        notifiedOccurrences.add(occurrence.id)
                    }
                } else {
                    // Se saiu do raio
                    if (notifiedOccurrences.contains(occurrence.id)) {
                        println("DEBUG: Saiu do raio de: ${occurrence.title}. Resetting notificação.")
                        notifiedOccurrences.remove(occurrence.id)
                    }
                }
            }
        }
    }
}