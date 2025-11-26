package com.example.readytohelpmobile.model

/**
 * Represents a one-time event sent from the MapViewModel to the UI.
 * Typically used for transient notifications like Toasts or Snackbars when entering a geofence.
 *
 * @property message The text message to display to the user.
 * @property occurrenceId The ID of the occurrence that triggered this event (used for actions like confirmation).
 */
data class MapEvent(
    val message: String,
    val occurrenceId: Int
)