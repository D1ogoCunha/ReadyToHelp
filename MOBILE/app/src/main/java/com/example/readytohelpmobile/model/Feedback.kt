package com.example.readytohelpmobile.model

/**
 * Represents the feedback provided by a user regarding a specific occurrence.
 * Used to confirm or deny the existence of an occurrence.
 *
 * @property id Unique identifier for the feedback. Default is 0 (indicating a new record for DBs with Identity).
 * @property occurrenceId The ID of the occurrence this feedback relates to.
 * @property userId The ID of the user submitting the feedback.
 * @property isConfirmed Boolean indicating if the user confirms (true) or denies (false) the occurrence.
 */
data class Feedback(
    val id: Int = 0,
    val occurrenceId: Int,
    val userId: Int,
    val isConfirmed: Boolean
)