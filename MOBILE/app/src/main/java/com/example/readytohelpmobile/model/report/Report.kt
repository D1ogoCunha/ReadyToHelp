package com.example.readytohelpmobile.model.report

data class Report(
    val title: String,
    val description: String,
    val type: OccurrenceType,
    val userId: Int,
    val latitude: Double,
    val longitude: Double
)