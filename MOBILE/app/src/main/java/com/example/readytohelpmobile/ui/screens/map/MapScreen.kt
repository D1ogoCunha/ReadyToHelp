package com.example.readytohelpmobile.ui.screens.map

import android.Manifest
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.content.res.AppCompatResources
import androidx.compose.animation.core.Animatable // Importante
import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.tween
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Snackbar
import androidx.compose.material3.SnackbarData
import androidx.compose.material3.SnackbarDuration
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.SnackbarResult
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import androidx.core.graphics.drawable.toBitmap
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.readytohelpmobile.viewModel.MapUiState
import com.example.readytohelpmobile.viewModel.MapViewModel
import com.mapbox.geojson.Point
import com.mapbox.maps.extension.compose.MapEffect
import com.mapbox.maps.extension.compose.MapboxMap
import com.mapbox.maps.extension.compose.animation.viewport.rememberMapViewportState
import com.mapbox.maps.plugin.PuckBearing
import com.mapbox.maps.plugin.annotation.annotations
import com.mapbox.maps.plugin.annotation.generated.PointAnnotationOptions
import com.mapbox.maps.plugin.annotation.generated.createPointAnnotationManager
import com.mapbox.maps.plugin.locationcomponent.createDefault2DPuck
import com.example.readytohelpmobile.R
import com.mapbox.maps.plugin.locationcomponent.location
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch

@Composable
fun bitmapFromDrawableRes(drawableResId: Int): Bitmap? {
    val context = LocalContext.current
    return remember(drawableResId) {
        AppCompatResources.getDrawable(context, drawableResId)?.toBitmap()
    }
}

@Composable
fun MapScreen(
    viewModel: MapViewModel = viewModel()
) {
    val context = LocalContext.current
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    val snackbarHostState = remember { SnackbarHostState() }
    val scope = rememberCoroutineScope()

    // ALTERAÇÃO 1: Usar Animatable para controlo manual da animação
    val progressAnimatable = remember { Animatable(1f) }

    val animal_on_road_pin = bitmapFromDrawableRes(R.drawable.animal_on_road)
    val crime_pin = bitmapFromDrawableRes(R.drawable.crime)
    val domestic_violence_pin = bitmapFromDrawableRes(R.drawable.domestic_violence)
    val electrical_network_pin = bitmapFromDrawableRes(R.drawable.electrical_network)
    val flood_pin = bitmapFromDrawableRes(R.drawable.flood)
    val forest_fire_pin = bitmapFromDrawableRes(R.drawable.forest_fire)
    val injured_animal_pin = bitmapFromDrawableRes(R.drawable.injured_animal)
    val landslide_pin = bitmapFromDrawableRes(R.drawable.landslide)
    val lost_animal = bitmapFromDrawableRes(R.drawable.lost_animal)
    val medical_emergency_pin = bitmapFromDrawableRes(R.drawable.medical_emergency)
    val pollution_pin = bitmapFromDrawableRes(R.drawable.pollution)
    val public_disturbance_pin = bitmapFromDrawableRes(R.drawable.public_disturbance)
    val public_lighting_pin = bitmapFromDrawableRes(R.drawable.public_lighting)
    val road_accident_pin = bitmapFromDrawableRes(R.drawable.road_accident)
    val road_obstruction_pin = bitmapFromDrawableRes(R.drawable.road_obstruction)
    val sanitation_pin = bitmapFromDrawableRes(R.drawable.sanitation)
    val traffic_congestion_pin = bitmapFromDrawableRes(R.drawable.traffic_congestion)
    val traffic_light_failure_pin = bitmapFromDrawableRes(R.drawable.traffic_light_failure)
    val urban_fire_pin = bitmapFromDrawableRes(R.drawable.urban_fire)
    val road_damage = bitmapFromDrawableRes(R.drawable.road_damage)
    val vehicle_breakdown_pin = bitmapFromDrawableRes(R.drawable.vehicle_breakdown)
    val work_accident_pin = bitmapFromDrawableRes(R.drawable.work_accident)
    val defaultPinBitmap = bitmapFromDrawableRes(com.mapbox.maps.extension.compose.R.drawable.default_marker_inner)

    var hasLocationPermission by remember {
        mutableStateOf(
            ContextCompat.checkSelfPermission(
                context,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) == PackageManager.PERMISSION_GRANTED
        )
    }

    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted: Boolean ->
        if (isGranted) {
            hasLocationPermission = true
        } else {
            Toast.makeText(context, "Permissão de localização negada.", Toast.LENGTH_SHORT).show()
        }
    }

    // --- LÓGICA DO SNACKBAR COM TIMER E BARRA DE PROGRESSO ---
    LaunchedEffect(Unit) {
        viewModel.mapEvent.collectLatest { event ->

            // ALTERAÇÃO 2: Lógica de animação corrigida
            val timerJob = scope.launch {
                // 1. Reset instantâneo para cheio
                progressAnimatable.snapTo(1f)

                // 2. Animar para 0 em 30 segundos
                progressAnimatable.animateTo(
                    targetValue = 0f,
                    animationSpec = tween(durationMillis = 30000, easing = LinearEasing)
                )

                // 3. Se a animação acabar, fechar o snackbar
                snackbarHostState.currentSnackbarData?.dismiss()
            }

            val result = snackbarHostState.showSnackbar(
                message = event.message,
                actionLabel = "Confirmar",
                duration = SnackbarDuration.Indefinite
            )

            timerJob.cancel()
            // Garantir que pára a animação se o utilizador clicar
            scope.launch { progressAnimatable.stop() }

            if (result == SnackbarResult.ActionPerformed) {
                viewModel.confirmPresence(event.occurrenceId, true)
            } else {
                viewModel.confirmPresence(event.occurrenceId, false)
            }
        }
    }

    LaunchedEffect(Unit) {
        viewModel.toastEvent.collectLatest { message ->
            Toast.makeText(context, message, Toast.LENGTH_LONG).show()
        }
    }

    LaunchedEffect(hasLocationPermission) {
        if (hasLocationPermission) {
            viewModel.getUserLocation()
        } else {
            permissionLauncher.launch(Manifest.permission.ACCESS_FINE_LOCATION)
        }
    }

    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        when (val state = uiState) {
            is MapUiState.Loading -> {
                CircularProgressIndicator()
            }
            is MapUiState.Error -> {
                Text(text = state.message)
            }
            is MapUiState.Success -> {
                if (hasLocationPermission) {
                    val mapViewportState = rememberMapViewportState {
                        setCameraOptions {
                            zoom(3.0)
                            center(Point.fromLngLat(-9.1393, 38.7223))
                        }
                    }

                    MapboxMap(
                        Modifier.fillMaxSize(),
                        mapViewportState = mapViewportState,
                    ) {
                        MapEffect(Unit) { mapView ->
                            mapView.location.updateSettings {
                                enabled = true
                                locationPuck = createDefault2DPuck(withBearing = true)
                                puckBearing = PuckBearing.HEADING
                            }
                            mapViewportState.transitionToFollowPuckState()
                        }

                        MapEffect(state.occurrences) { mapView ->
                            val pointAnnotationManager = mapView.annotations.createPointAnnotationManager()
                            pointAnnotationManager.deleteAll()

                            pointAnnotationManager.addClickListener { annotation ->
                                val clickedOccurrence = state.occurrences.find {
                                    val lat = it.location?.latitude
                                    val lng = it.location?.longitude
                                    lng == annotation.point.longitude() && lat == annotation.point.latitude()
                                }

                                clickedOccurrence?.let {
                                    Toast.makeText(
                                        context,
                                        "${it.title}: Raio ${it.proximityRadius}m",
                                        Toast.LENGTH_SHORT
                                    ).show()
                                }
                                true
                            }

                            val annotationOptionsList = state.occurrences.mapNotNull { occurrence ->
                                val loc = occurrence.location ?: return@mapNotNull null

                                val bmp = when (occurrence.type) {
                                    "ANIMAL_ON_ROAD" -> animal_on_road_pin
                                    "CRIME" -> crime_pin
                                    "DOMESTIC_VIOLENCE" -> domestic_violence_pin
                                    "ELECTRICAL_NETWORK" -> electrical_network_pin
                                    "FLOOD" -> flood_pin
                                    "FOREST_FIRE" -> forest_fire_pin
                                    "INJURED_ANIMAL" -> injured_animal_pin
                                    "LANDSLIDE" -> landslide_pin
                                    "LOST_ANIMAL" -> lost_animal
                                    "MEDICAL_EMERGENCY" -> medical_emergency_pin
                                    "POLLUTION" -> pollution_pin
                                    "PUBLIC_DISTURBANCE" -> public_disturbance_pin
                                    "PUBLIC_LIGHTING" -> public_lighting_pin
                                    "ROAD_ACCIDENT" -> road_accident_pin
                                    "ROAD_OBSTRUCTION" -> road_obstruction_pin
                                    "SANITATION" -> sanitation_pin
                                    "TRAFFIC_CONGESTION" -> traffic_congestion_pin
                                    "TRAFFIC_LIGHT_FAILURE" -> traffic_light_failure_pin
                                    "URBAN_FIRE" -> urban_fire_pin
                                    "VEHICLE_BREAKDOWN" -> vehicle_breakdown_pin
                                    "WORK_ACCIDENT" -> work_accident_pin
                                    "ROAD_DAMAGE" -> road_damage
                                    else -> defaultPinBitmap
                                }

                                bmp?.let {
                                    PointAnnotationOptions()
                                        .withPoint(Point.fromLngLat(loc.longitude, loc.latitude))
                                        .withIconImage(it)
                                }
                            }

                            pointAnnotationManager.create(annotationOptionsList)
                        }
                    }
                } else {
                    Text(text = "A permissão de localização é necessária para mostrar o mapa.")
                }
            }
        }

        // --- SNACKBAR PERSONALIZADO ---
        SnackbarHost(
            hostState = snackbarHostState,
            modifier = Modifier
                .align(Alignment.TopCenter)
                .padding(top = 16.dp, start = 16.dp, end = 16.dp)
        ) { data: SnackbarData ->
            Surface(
                color = Color(0xFF4353AB),
                contentColor = Color.White,
                shape = RoundedCornerShape(12.dp),
                shadowElevation = 6.dp,
                modifier = Modifier.fillMaxWidth()
            ) {
                Column {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text(
                            text = data.visuals.message,
                            style = MaterialTheme.typography.bodyLarge,
                            fontWeight = FontWeight.Bold,
                            modifier = Modifier.fillMaxWidth()
                        )

                        Spacer(modifier = Modifier.height(16.dp))

                        Button(
                            onClick = { data.performAction() },
                            colors = ButtonDefaults.buttonColors(
                                containerColor = Color(0xFFFFFFFF),
                                contentColor = Color(0xFF4353AB)
                            ),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text(
                                text = data.visuals.actionLabel ?: "Confirmar",
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }

                    // ALTERAÇÃO 3: Usar o valor do Animatable
                    LinearProgressIndicator(
                        progress = { progressAnimatable.value },
                        modifier = Modifier.fillMaxWidth().height(4.dp),
                        color = Color(0xFFFFFFFF),
                        trackColor = Color(0x40FFFFFF),
                    )
                }
            }
        }
    }
}