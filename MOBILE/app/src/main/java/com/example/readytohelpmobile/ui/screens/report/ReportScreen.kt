package com.example.readytohelpmobile.ui.screens.report

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.Place
import androidx.compose.material.icons.filled.Phone
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.TextFieldValue
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.readytohelpmobile.model.report.OccurrenceType
import com.example.readytohelpmobile.model.report.ResponsibleEntityContact
import com.example.readytohelpmobile.viewmodel.ReportUiState
import com.example.readytohelpmobile.viewmodel.ReportViewModel
import com.mapbox.geojson.Point
import com.mapbox.maps.extension.compose.MapboxMap
import com.mapbox.maps.extension.compose.animation.viewport.rememberMapViewportState

// Define the primary brand color for consistent styling
private val BrandColor = Color(0xFF4353AB)

/**
 * A dialog Composable that allows users to report a new occurrence.
 * It manages the form state (title, description, type) and handles the submission process via the ViewModel.
 *
 * @param onDismiss Callback function to close the dialog.
 * @param viewModel The ViewModel responsible for handling the reporting logic.
 */
@Composable
fun ReportOccurrenceDialog(
    onDismiss: () -> Unit,
    viewModel: ReportViewModel = viewModel()
) {
    // Observe the UI state from the ViewModel
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    // Local state for form fields
    var title by remember { mutableStateOf(TextFieldValue()) }
    var description by remember { mutableStateOf(TextFieldValue()) }
    var selectedType by remember { mutableStateOf<OccurrenceType?>(null) }

    // State for location mode: True = GPS, False = Map Picker
    var useCurrentLocation by remember { mutableStateOf(true) }

    // Map Viewport State for the picker
    val mapViewportState = rememberMapViewportState {
        setCameraOptions {
            zoom(14.0)
            // Default center (Lisbon) if location not loaded yet
            center(Point.fromLngLat(-9.1393, 38.7223))
        }
    }

    // Effect to initialize map center to user location when opening "Pick on Map"
    LaunchedEffect(useCurrentLocation) {
        if (!useCurrentLocation) {
            val loc = viewModel.getCurrentLocation()
            if (loc != null) {
                mapViewportState.setCameraOptions {
                    center(Point.fromLngLat(loc.longitude, loc.latitude))
                    zoom(15.0)
                }
            }
        }
    }

    // State for the dropdown menu
    var expanded by remember { mutableStateOf(false) }

    // Handle different UI states
    when (val state = uiState) {
        // If report is successful, show the success dialog with entity details
        is ReportUiState.Success -> {
            SuccessReportDialog(
                responsible = state.response.responsibleEntity,
                onClose = {
                    viewModel.resetState()
                    onDismiss()
                }
            )
        }
        // Otherwise, show the form dialog
        else -> {
            AlertDialog(
                onDismissRequest = onDismiss,
                confirmButton = {
                    Button(
                        onClick = {
                            if (title.text.isNotBlank()
                                && description.text.isNotBlank()
                                && selectedType != null
                            ) {
                                // Logic to decide which location to send
                                if (useCurrentLocation) {
                                    // Send normally (ViewModel fetches GPS)
                                    viewModel.submitReport(
                                        title.text.trim(),
                                        description.text.trim(),
                                        selectedType!!
                                    )
                                } else {
                                    // Send with manual coordinates from map center
                                    val center = mapViewportState.cameraState?.center
                                    viewModel.submitReport(
                                        title.text.trim(),
                                        description.text.trim(),
                                        selectedType!!,
                                        manualLat = center?.latitude(),
                                        manualLng = center?.longitude()
                                    )
                                }
                            }
                        },
                        // Disable button if fields are empty or if loading
                        enabled = selectedType != null
                                && title.text.isNotBlank()
                                && description.text.isNotBlank()
                                && uiState !is ReportUiState.Loading,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = BrandColor,
                            contentColor = Color.White
                        ),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        if (uiState is ReportUiState.Loading) {
                            CircularProgressIndicator(
                                color = Color.White,
                                modifier = Modifier.size(18.dp),
                                strokeWidth = 2.dp
                            )
                        } else {
                            Text("Submit", fontWeight = FontWeight.SemiBold)
                        }
                    }
                },
                dismissButton = {
                    TextButton(
                        onClick = onDismiss,
                        colors = ButtonDefaults.textButtonColors(contentColor = BrandColor)
                    ) {
                        Text("Cancel")
                    }
                },
                title = {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(
                            "Report Occurrence",
                            fontWeight = FontWeight.Bold,
                            color = BrandColor,
                            style = MaterialTheme.typography.headlineSmall
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            "Please complete the information below",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                },
                text = {
                    Column(modifier = Modifier.fillMaxWidth()) {
                        // Show error message if submission failed
                        if (uiState is ReportUiState.Error) {
                            Text(
                                text = (uiState as ReportUiState.Error).message,
                                color = MaterialTheme.colorScheme.error,
                                style = MaterialTheme.typography.bodySmall,
                                modifier = Modifier.padding(bottom = 8.dp)
                            )
                        }

                        // Title Input
                        OutlinedTextField(
                            value = title,
                            onValueChange = { title = it },
                            label = { Text("Title") },
                            singleLine = true,
                            modifier = Modifier.fillMaxWidth(),
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedBorderColor = BrandColor,
                                focusedLabelColor = BrandColor,
                                cursorColor = BrandColor,
                                unfocusedBorderColor = BrandColor.copy(alpha = 0.5f),
                                focusedContainerColor = Color.White,
                                unfocusedContainerColor = Color.White,
                                focusedTextColor = Color.Black,
                                unfocusedTextColor = Color.Black
                            ),
                            shape = RoundedCornerShape(10.dp)
                        )

                        Spacer(modifier = Modifier.height(8.dp))

                        // Description Input
                        OutlinedTextField(
                            value = description,
                            onValueChange = { description = it },
                            label = { Text("Description") },
                            modifier = Modifier
                                .fillMaxWidth()
                                .heightIn(min = 100.dp),
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedBorderColor = BrandColor,
                                focusedLabelColor = BrandColor,
                                cursorColor = BrandColor,
                                unfocusedBorderColor = BrandColor.copy(alpha = 0.5f),
                                focusedContainerColor = Color.White,
                                unfocusedContainerColor = Color.White,
                                focusedTextColor = Color.Black,
                                unfocusedTextColor = Color.Black
                            ),
                            shape = RoundedCornerShape(10.dp)
                        )

                        Spacer(modifier = Modifier.height(16.dp))

                        // Location Selection Toggle
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            // Button: Current Location
                            LocationOptionButton(
                                text = "Current Location",
                                icon = Icons.Filled.LocationOn,
                                isSelected = useCurrentLocation,
                                onClick = { useCurrentLocation = true },
                                modifier = Modifier.weight(1f)
                            )
                            // Button: Pick on Map
                            LocationOptionButton(
                                text = "Pick on Map",
                                icon = Icons.Filled.LocationOn,
                                isSelected = !useCurrentLocation,
                                onClick = { useCurrentLocation = false },
                                modifier = Modifier.weight(1f)
                            )
                        }

                        Spacer(modifier = Modifier.height(8.dp))

                        // Map Preview (Visible only if "Pick on Map" is selected)
                        if (!useCurrentLocation) {
                            Box(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .height(200.dp)
                                    .clip(RoundedCornerShape(10.dp))
                                    .border(1.dp, BrandColor.copy(alpha = 0.5f), RoundedCornerShape(10.dp))
                            ) {
                                MapboxMap(
                                    Modifier.fillMaxSize(),
                                    mapViewportState = mapViewportState,
                                )
                                // Center Pin Icon (Target)
                                Icon(
                                    imageVector = Icons.Filled.Place,
                                    contentDescription = "Selected Location",
                                    tint = BrandColor, // Pin in Brand Color
                                    modifier = Modifier
                                        .size(40.dp)
                                        .align(Alignment.Center)
                                        .padding(bottom = 16.dp)
                                )
                            }
                            Spacer(modifier = Modifier.height(8.dp))
                        }

                        // Occurrence Type Dropdown
                        Box(modifier = Modifier.fillMaxWidth()) {
                            OutlinedTextField(
                                value = selectedType?.name?.replace('_', ' ') ?: "",
                                onValueChange = {},
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable { expanded = true },
                                enabled = false,
                                label = { Text("Occurrence Type") },
                                readOnly = true,
                                colors = OutlinedTextFieldDefaults.colors(
                                    disabledBorderColor = BrandColor.copy(alpha = 0.5f),
                                    disabledLabelColor = BrandColor,
                                    disabledTextColor = Color.Black,
                                    disabledContainerColor = Color.White
                                ),
                                shape = RoundedCornerShape(10.dp)
                            )
                            Box(
                                modifier = Modifier
                                    .matchParentSize()
                                    .clickable { expanded = true }
                            )
                        }

                        DropdownMenu(
                            expanded = expanded,
                            onDismissRequest = { expanded = false },
                            modifier = Modifier.background(Color.White)
                        ) {
                            OccurrenceType.values().forEach { type ->
                                DropdownMenuItem(
                                    text = {
                                        Text(
                                            text = type.name.replace('_', ' '),
                                            color = Color.Black
                                        )
                                    },
                                    onClick = {
                                        selectedType = type
                                        expanded = false
                                    }
                                )
                            }
                        }
                    }
                },
                shape = RoundedCornerShape(24.dp),
                containerColor = Color.White
            )
        }
    }
}

// Helper composable for the location toggle buttons
@Composable
fun LocationOptionButton(
    text: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    isSelected: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    val containerColor = if (isSelected) BrandColor else Color.Transparent
    val contentColor = if (isSelected) Color.White else BrandColor
    val borderColor = if (isSelected) Color.Transparent else BrandColor.copy(alpha = 0.5f)

    OutlinedButton(
        onClick = onClick,
        modifier = modifier,
        colors = ButtonDefaults.outlinedButtonColors(
            containerColor = containerColor,
            contentColor = contentColor
        ),
        border = androidx.compose.foundation.BorderStroke(1.dp, borderColor),
        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp),
        shape = RoundedCornerShape(10.dp)
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Icon(imageVector = icon, contentDescription = null, modifier = Modifier.size(20.dp))
            Text(text = text, style = MaterialTheme.typography.labelSmall, maxLines = 1)
        }
    }
}

/**
 * Dialog shown upon successful report submission.
 * Displays contact information for the responsible entity if available.
 */
@Composable
private fun SuccessReportDialog(
    responsible: ResponsibleEntityContact?,
    onClose: () -> Unit
) {
    val context = LocalContext.current

    AlertDialog(
        onDismissRequest = onClose,
        confirmButton = {
            Button(
                onClick = onClose,
                colors = ButtonDefaults.buttonColors(
                    containerColor = BrandColor,
                    contentColor = Color.White
                ),
                shape = RoundedCornerShape(10.dp)
            ) {
                Text("Close")
            }
        },
        title = {
            Text(
                "Report submitted successfully!",
                fontWeight = FontWeight.Bold,
                color = BrandColor
            )
        },
        text = {
            if (responsible == null) {
                Text("It was not possible to determine the Responsible Entity.")
            } else {
                Surface(
                    shape = RoundedCornerShape(12.dp),
                    tonalElevation = 2.dp,
                    color = Color.White,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Column(modifier = Modifier.padding(12.dp)) {
                        Text(responsible.name, fontWeight = FontWeight.Bold, color = BrandColor)

                        Spacer(modifier = Modifier.height(8.dp))

                        if (responsible.email.isNotBlank()) {
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable {
                                        try {
                                            val gmailIntent = Intent(Intent.ACTION_SENDTO, Uri.parse("mailto:${responsible.email}"))
                                                .apply { `package` = "com.google.android.gm" }
                                            context.startActivity(gmailIntent)
                                        } catch (e: Exception) {
                                            try {
                                                val fallback = Intent(Intent.ACTION_SENDTO, Uri.parse("mailto:${responsible.email}"))
                                                context.startActivity(fallback)
                                            } catch (_: Exception) {  }
                                        }
                                    }
                                    .padding(vertical = 6.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Email,
                                    contentDescription = null,
                                    tint = BrandColor,
                                    modifier = Modifier.size(20.dp)
                                )
                                Spacer(modifier = Modifier.width(10.dp))
                                Text(
                                    responsible.email,
                                    color = Color.Black
                                )
                            }
                        }

                        val phoneStr = responsible.contactPhone.toString()
                        if (phoneStr.isNotBlank()) {
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable {
                                        try {
                                            val dialIntent = Intent(Intent.ACTION_DIAL, Uri.parse("tel:$phoneStr"))
                                            context.startActivity(dialIntent)
                                        } catch (_: Exception) {  }
                                    }
                                    .padding(vertical = 6.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.Phone,
                                    contentDescription = null,
                                    tint = BrandColor,
                                    modifier = Modifier.size(20.dp)
                                )
                                Spacer(modifier = Modifier.width(10.dp))
                                Text(
                                    phoneStr,
                                    color = Color.Black
                                )
                            }
                        }

                        if (responsible.address.isNotBlank()) {
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable {
                                        try {
                                            val gMapIntent = Intent(
                                                Intent.ACTION_VIEW,
                                                Uri.parse("geo:0,0?q=${Uri.encode(responsible.address)}")
                                            ).apply { `package` = "com.google.android.apps.maps" }
                                            context.startActivity(gMapIntent)
                                        } catch (e: Exception) {
                                            try {
                                                val webIntent = Intent(
                                                    Intent.ACTION_VIEW,
                                                    Uri.parse("https://www.google.com/maps/search/?api=1&query=${Uri.encode(responsible.address)}")
                                                )
                                                context.startActivity(webIntent)
                                            } catch (_: Exception) {  }
                                        }
                                    }
                                    .padding(vertical = 6.dp)
                            ) {
                                Icon(
                                    imageVector = Icons.Filled.LocationOn,
                                    contentDescription = null,
                                    tint = BrandColor,
                                    modifier = Modifier.size(20.dp)
                                )
                                Spacer(modifier = Modifier.width(10.dp))
                                Text(
                                    responsible.address,
                                    color = Color.Black
                                )
                            }
                        }
                    }
                }
            }
        },
        shape = RoundedCornerShape(12.dp),
        containerColor = Color.White
    )
}