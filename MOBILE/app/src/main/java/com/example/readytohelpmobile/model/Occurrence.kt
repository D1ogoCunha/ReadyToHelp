package com.example.readytohelpmobile.model

import com.squareup.moshi.Json

data class Occurrence(
    val id: Int,
    val title: String,
    val type: String,
    val latitude: Double,
    val longitude: Double,
    val status: String,
    val priority: String
)