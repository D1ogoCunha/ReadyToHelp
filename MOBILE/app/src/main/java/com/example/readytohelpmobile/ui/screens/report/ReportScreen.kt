package com.example.readytohelpmobile.ui.screens.report

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.Phone
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
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
                                viewModel.submitReport(
                                    title.text.trim(),
                                    description.text.trim(),
                                    selectedType!!
                                )
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
                    TextButton(onClick = onDismiss) {
                        Text("Cancel")
                    }
                },
                title = {
                    Column {
                        Text("Report Occurrence", fontWeight = FontWeight.Bold)
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
                                cursorColor = BrandColor
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
                                cursorColor = BrandColor
                            ),
                            shape = RoundedCornerShape(10.dp)
                        )

                        Spacer(modifier = Modifier.height(8.dp))

                        // Occurrence Type Dropdown
                        OutlinedTextField(
                            value = selectedType?.name ?: "",
                            onValueChange = {},
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable { expanded = true },
                            enabled = false,
                            label = { Text("Occurrence Type") },
                            readOnly = true,
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedBorderColor = BrandColor,
                                focusedLabelColor = BrandColor
                            ),
                            shape = RoundedCornerShape(10.dp)
                        )

                        DropdownMenu(
                            expanded = expanded,
                            onDismissRequest = { expanded = false }
                        ) {
                            OccurrenceType.values().forEach { type ->
                                DropdownMenuItem(
                                    text = { Text(type.name.replace('_', ' ')) },
                                    onClick = {
                                        selectedType = type
                                        expanded = false
                                    }
                                )
                            }
                        }
                    }
                },
                shape = RoundedCornerShape(16.dp),
                containerColor = MaterialTheme.colorScheme.surfaceVariant
            )
        }
    }
}

/**
 * Dialog shown upon successful report submission.
 * Displays contact information for the responsible entity if available.
 *
 * @param responsible The entity details returned by the backend.
 * @param onClose Callback to close the dialog.
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
            Text("Report submitted successfully!", fontWeight = FontWeight.Bold)
        },
        text = {
            if (responsible == null) {
                Text("It was not possible to determine the Responsible Entity.")
            } else {
                Surface(
                    shape = RoundedCornerShape(12.dp),
                    tonalElevation = 2.dp,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Column(modifier = Modifier.padding(12.dp)) {
                        Text(responsible.name, fontWeight = FontWeight.Bold, color = BrandColor)

                        Spacer(modifier = Modifier.height(8.dp))

                        // Email Row (Clickable to open Email App)
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
                                Text(responsible.email, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            }
                        }

                        // Phone Row (Clickable to open Dialer)
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
                                Text(phoneStr, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            }
                        }

                        // Address Row (Clickable to open Maps)
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
                                Text(responsible.address, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            }
                        }
                    }
                }
            }
        },
        shape = RoundedCornerShape(12.dp)
    )
}