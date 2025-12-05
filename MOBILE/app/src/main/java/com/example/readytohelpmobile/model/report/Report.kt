package com.example.readytohelpmobile.model.report

/**
 * Data model used to send a new occurrence report to the API.
 *
 * @property title Title of the occurrence.
 * @property description Detailed description of the situation.
 * @property type The occurrence type selected from the [OccurrenceType] enum.
 * @property userId The ID of the user making the report.
 * @property latitude The latitude where the occurrence was sighted.
 * @property longitude The longitude where the occurrence was sighted.
 */
data class Report(
    val title: String,
    val description: String,
    val type: OccurrenceType,
    val userId: Int,
    val latitude: Double,
    val longitude: Double
)