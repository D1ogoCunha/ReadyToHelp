package com.example.readytohelpmobile.model.auth

data class UserResponse(
    val id: Int,
    val name: String?,
    val email: String?,
    val profile: String
)