package com.example.readytohelpmobile.model

import com.squareup.moshi.Json

data class Occurrence(
    val id: Int, // O Id no C# é um int
    val title: String,
    val type: String, // Recebe o Type do backend
    val latitude: Double,
    val longitude: Double,
    val status: String, // Recebe o Status do backend
    val priority: String // Recebe a Priority do backend
)