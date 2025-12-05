package com.example.readytohelpmobile.model.auth

/**
 * Model for the Login request body sent to the API.
 *
 * @property email The user's email address.
 * @property password The user's password.
 */
data class LoginRequest(
    val email: String,
    val password: String
)