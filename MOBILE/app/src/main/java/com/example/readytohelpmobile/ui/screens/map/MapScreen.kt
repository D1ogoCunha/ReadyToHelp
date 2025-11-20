package com.example.readytohelpmobile.ui.screens.map

import android.Manifest
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.content.res.AppCompatResources
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
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
import com.mapbox.maps.plugin.annotation.generated.OnPointAnnotationClickListener
import com.mapbox.maps.plugin.annotation.generated.PointAnnotationOptions
import com.mapbox.maps.plugin.annotation.generated.createPointAnnotationManager
import com.mapbox.maps.plugin.locationcomponent.createDefault2DPuck
import com.mapbox.maps.plugin.locationcomponent.location

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

    val markerBitmap = bitmapFromDrawableRes(com.mapbox.maps.extension.compose.R.drawable.default_marker)

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

                        MapEffect(state.occurrences, markerBitmap) { mapView ->
                            val pointAnnotationManager = mapView.annotations.createPointAnnotationManager()
                            pointAnnotationManager.deleteAll()

                            pointAnnotationManager.addClickListener(
                                OnPointAnnotationClickListener { annotation ->
                                    val clickedOccurrence = state.occurrences.find {
                                        it.longitude == annotation.point.longitude() &&
                                                it.latitude == annotation.point.latitude()
                                    }

                                    clickedOccurrence?.let {
                                        Toast.makeText(
                                            context,
                                            "${it.title}: ${it.title}",
                                            Toast.LENGTH_LONG
                                        ).show()
                                    }
                                    true
                                }
                            )

                            markerBitmap?.let { bmp ->
                                val annotationOptionsList = state.occurrences.map { occurrence ->
                                    PointAnnotationOptions()
                                        .withPoint(Point.fromLngLat(occurrence.longitude, occurrence.latitude))
                                        .withIconImage(bmp)
                                    // .withData(Gson().toJsonTree(occurrence))
                                }
                                pointAnnotationManager.create(annotationOptionsList)
                            }
                        }
                    }
                } else {
                    Text(text = "A permissão de localização é necessária para mostrar o mapa.")
                }
            }
        }
    }
}