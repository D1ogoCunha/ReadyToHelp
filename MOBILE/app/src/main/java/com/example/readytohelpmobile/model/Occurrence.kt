package com.example.readytohelpmobile.model

data class GeoPoint(
    val latitude: Double,
    val longitude: Double
)

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