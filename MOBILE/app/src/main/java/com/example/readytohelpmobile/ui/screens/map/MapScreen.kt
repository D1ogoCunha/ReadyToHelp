package com.example.readytohelpmobile.ui.screens.map

import android.Manifest
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.content.res.AppCompatResources
import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.tween
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.Create
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarData
import androidx.compose.material3.SnackbarDuration
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.SnackbarResult
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import androidx.core.graphics.drawable.toBitmap
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import com.example.readytohelpmobile.viewmodel.MapViewModel
import com.example.readytohelpmobile.ui.screens.report.ReportOccurrenceDialog
import com.example.readytohelpmobile.viewmodel.ReportViewModel
import com.mapbox.geojson.Point
import com.mapbox.maps.extension.compose.MapEffect
import com.mapbox.maps.extension.compose.MapboxMap
import com.mapbox.maps.extension.compose.animation.viewport.rememberMapViewportState
import com.mapbox.maps.plugin.PuckBearing
import com.mapbox.maps.plugin.annotation.annotations
import com.mapbox.maps.plugin.annotation.generated.PointAnnotationOptions
import com.mapbox.maps.plugin.annotation.generated.PointAnnotationManager
import com.mapbox.maps.plugin.locationcomponent.createDefault2DPuck
import com.example.readytohelpmobile.R
import com.example.readytohelpmobile.viewmodel.MapUiState
import com.mapbox.maps.plugin.annotation.generated.createPointAnnotationManager
import com.mapbox.maps.plugin.locationcomponent.location
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch

/**
 * Helper function to convert a drawable resource ID into a Bitmap.
 * This is used to create marker icons for the Mapbox map.
 *
 * @param drawableResId The resource ID of the drawable.
 * @return A Bitmap representation of the drawable, or null if decoding fails.
 */
@Composable
fun bitmapFromDrawableRes(drawableResId: Int): Bitmap? {
    val context = LocalContext.current
    return remember(drawableResId) {
        AppCompatResources.getDrawable(context, drawableResId)?.toBitmap()
    }
}

/**
 * Main Composable for the Map Screen.
 * Displays a Mapbox map with occurrence markers, handles user location,
 * reports new occurrences, and manages presence confirmation via a custom Snackbar.
 *
 * @param viewModel The [MapViewModel] responsible for fetching occurrences and handling map logic.
 * @param authViewModel The [AuthViewModel] used for logout functionality.
 * @param onLogout Callback function triggered when the user logs out.
 */
@Composable
fun MapScreen(
    viewModel: MapViewModel = viewModel(),
    authViewModel: AuthViewModel = viewModel(),
    onLogout: () -> Unit = {}
) {
    val context = LocalContext.current
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    // Keep a reference to the latest UI state to be accessed within the Mapbox click listener callback.
    // This ensures the click listener always has access to the latest data without needing recreation.
    val currentUiState by rememberUpdatedState(uiState)

    // State to hold the Mapbox PointAnnotationManager, initialized once the map is ready.
    var pointAnnotationManager by remember { mutableStateOf<PointAnnotationManager?>(null) }

    // UI State for the custom Snackbar and progress animation
    val snackbarHostState = remember { SnackbarHostState() }
    val scope = rememberCoroutineScope()
    val progressAnimatable = remember { Animatable(1f) }

    // --- Load Bitmap Resources for Map Markers ---
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

    // --- Location Permissions ---
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
            Toast.makeText(context, "Location permission denied.", Toast.LENGTH_SHORT).show()
        }
    }

    // State for the Report Occurrence Dialog
    var showReportDialog by remember { mutableStateOf(false) }
    val reportViewModel: ReportViewModel = viewModel()

    // --- Effect: Handle Custom Snackbar with Timer ---
    // Listens for map events (e.g., entering an occurrence radius) and displays a snackbar
    // with a countdown timer (progress bar).
    LaunchedEffect(Unit) {
        viewModel.mapEvent.collectLatest { event ->
            // Start the countdown timer logic
            val timerJob = scope.launch {
                // Reset progress to full (1.0)
                progressAnimatable.snapTo(1f)
                // Animate to empty (0.0) over 30 seconds
                progressAnimatable.animateTo(
                    targetValue = 0f,
                    animationSpec = tween(durationMillis = 30000, easing = LinearEasing)
                )
                // Dismiss snackbar if timer completes
                snackbarHostState.currentSnackbarData?.dismiss()
            }

            // Display the snackbar indefinitely (controlled manually by the timer)
            val result = snackbarHostState.showSnackbar(
                message = event.message,
                actionLabel = "Confirm",
                duration = SnackbarDuration.Indefinite
            )

            // Cancel timer if user interacted
            timerJob.cancel()
            scope.launch { progressAnimatable.stop() }

            // Handle user action
            if (result == SnackbarResult.ActionPerformed) {
                viewModel.confirmPresence(event.occurrenceId, true)
            } else {
                // If dismissed (timeout or swipe), send false
                viewModel.confirmPresence(event.occurrenceId, false)
            }
        }
    }

    // --- Effect: Handle Generic Toasts ---
    LaunchedEffect(Unit) {
        viewModel.toastEvent.collectLatest { message ->
            Toast.makeText(context, message, Toast.LENGTH_LONG).show()
        }
    }

    // --- Effect: Check Location Permission on Start ---
    LaunchedEffect(hasLocationPermission) {
        if (hasLocationPermission) {
            viewModel.getUserLocation()
        } else {
            permissionLauncher.launch(Manifest.permission.ACCESS_FINE_LOCATION)
        }
    }

    // --- Effect: Update Map Markers ---
    // Triggers whenever the UI state (data) changes or the annotation manager is initialized.
    LaunchedEffect(uiState, pointAnnotationManager) {
        val manager = pointAnnotationManager ?: return@LaunchedEffect
        val state = uiState

        if (state is MapUiState.Success) {
            // 1. Clear existing markers to avoid duplicates
            manager.deleteAll()

            // 2. Map occurrences to PointAnnotationOptions
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

            // 3. Add the new markers to the map
            manager.create(annotationOptionsList)
        }
    }

    // Main UI Structure
    Scaffold(
        floatingActionButton = {
            FloatingActionButton(onClick = { showReportDialog = true }) {
                Icon(
                    imageVector = Icons.Filled.Create,
                    contentDescription = "Report Occurrence"
                )
            }
        }
    ) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            contentAlignment = Alignment.Center
        ) {
            if (hasLocationPermission) {
                // Initial Map Viewport settings
                val mapViewportState = rememberMapViewportState {
                    setCameraOptions {
                        zoom(3.0)
                        center(Point.fromLngLat(-9.1393, 38.7223))
                    }
                }

                // Mapbox Component
                MapboxMap(
                    Modifier.fillMaxSize(),
                    mapViewportState = mapViewportState,
                ) {
                    // --- Initial Map Setup (Runs Once) ---
                    MapEffect(Unit) { mapView ->
                        // 1. Configure Location Component (Blue Puck)
                        mapView.location.updateSettings {
                            enabled = true
                            locationPuck = createDefault2DPuck(withBearing = true)
                            puckBearing = PuckBearing.HEADING
                        }
                        mapViewportState.transitionToFollowPuckState()

                        // 2. Initialize Annotation Manager and store it in state
                        val manager = mapView.annotations.createPointAnnotationManager()
                        pointAnnotationManager = manager

                        // 3. Configure Marker Click Listener
                        manager.addClickListener { annotation ->
                            // Access the latest data using currentUiState
                            val currentState = currentUiState
                            if (currentState is MapUiState.Success) {
                                val clickedOccurrence = currentState.occurrences.find {
                                    val lat = it.location?.latitude
                                    val lng = it.location?.longitude
                                    lng == annotation.point.longitude() && lat == annotation.point.latitude()
                                }

                                clickedOccurrence?.let {
                                    Toast.makeText(
                                        context,
                                        "${it.title}: Radius ${it.proximityRadius}m",
                                        Toast.LENGTH_LONG
                                    ).show()
                                }
                            }
                            true
                        }
                    }
                }
            } else {
                Text(text = "Location permission is required to display the map.")
            }

            // Loading State Indicator (Only if no map manager is ready yet to avoid flickering)
            if (uiState is MapUiState.Loading && pointAnnotationManager == null) {
                CircularProgressIndicator()
            } else if (uiState is MapUiState.Error && pointAnnotationManager == null) {
                Text(text = (uiState as MapUiState.Error).message)
            }

            // Report Occurrence Dialog
            if (showReportDialog) {
                ReportOccurrenceDialog(
                    onDismiss = {
                        showReportDialog = false
                    },
                    viewModel = reportViewModel
                )
            }

            // Logout Button (Top Right)
            Surface(
                modifier = Modifier
                    .align(Alignment.TopEnd)
                    .padding(16.dp)
                    .shadow(4.dp, CircleShape),
                shape = CircleShape,
                color = Color.White.copy(alpha = 0.9f)
            ) {
                IconButton(
                    onClick = { authViewModel.logout(onLogoutComplete = onLogout) }
                ) {
                    Icon(
                        imageVector = Icons.AutoMirrored.Filled.ExitToApp,
                        contentDescription = "Logout",
                        tint = Color(0xFF4353AB)
                    )
                }
            }

            // Custom Snackbar Host (Top Center)
            SnackbarHost(
                hostState = snackbarHostState,
                modifier = Modifier
                    .align(Alignment.TopCenter)
                    .padding(top = 16.dp, start = 16.dp, end = 16.dp)
            ) { data: SnackbarData ->
                // Custom Snackbar Design
                Surface(
                    color = Color(0xFF4353AB),
                    contentColor = Color.White,
                    shape = RoundedCornerShape(12.dp),
                    shadowElevation = 6.dp,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Column {
                        // Message and Button
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
                                    text = data.visuals.actionLabel ?: "Confirm",
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }

                        // Progress Bar (Timer)
                        LinearProgressIndicator(
                            progress = { progressAnimatable.value },
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(4.dp),
                            color = Color(0xFFFFFFFF),
                            trackColor = Color(0x40FFFFFF),
                        )
                    }
                }
            }
        }
    }
}