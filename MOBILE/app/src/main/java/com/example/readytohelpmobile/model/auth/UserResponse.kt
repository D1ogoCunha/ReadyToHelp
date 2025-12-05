package com.example.readytohelpmobile.model.auth

/**
 * API response model after a successful registration or login (depending on backend implementation).
 * Represents the public user data.
 *
 * @property id The unique user ID in the database.
 * @property name The user's name (can be null).
 * @property email The user's email (can be null).
 * @property profile The user's profile/role (e.g., "CITIZEN", "ADMIN").
 */
data class UserResponse(
    val id: Int,
    val name: String?,
    val email: String?,
    val profile: String
)