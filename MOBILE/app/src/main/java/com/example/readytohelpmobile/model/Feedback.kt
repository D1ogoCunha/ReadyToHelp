package com.example.readytohelpmobile.model

data class Feedback(
    val id: Int = 0,
    val occurrenceId: Int,
    val userId: Int,
    val isConfirmed: Boolean
)