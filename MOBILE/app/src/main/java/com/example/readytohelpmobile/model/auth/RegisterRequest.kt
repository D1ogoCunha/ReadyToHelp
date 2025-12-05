package com.example.readytohelpmobile.model.auth

/**
 * Model for the Register new user request body sent to the API.
 *
 * @property name The user's full name.
 * @property email The email address.
 * @property password The chosen password.
 */
data class RegisterRequest(
    val name: String,
    val email: String,
    val password: String
)