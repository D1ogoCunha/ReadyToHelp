package com.example.readytohelpmobile.model

/**
 * Main data model representing an Occurrence (incident) in the system.
 * Contains all details required to display markers on the map and calculate proximity.
 *
 * @property id Unique identifier of the occurrence.
 * @property title A short title describing the occurrence.
 * @property description A more detailed description (nullable).
 * @property type The type/category of the occurrence (e.g., "FOREST_FIRE").
 * @property status The current status of the occurrence (e.g., "ACTIVE").
 * @property priority The priority level (e.g., "HIGH").
 * @property proximityRadius The radius in meters around the location for proximity alerts.
 * @property location The [GeoPoint] containing latitude and longitude. Can be null if location data is missing.
 */
data class Occurrence(
    val id: Int,
    val title: String,
    val description: String?,
    val type: String,
    val status: String,
    val priority: String,
    val proximityRadius: Double,
    val location: GeoPoint?
)