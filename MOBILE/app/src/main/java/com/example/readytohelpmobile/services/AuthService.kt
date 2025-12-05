package com.example.readytohelpmobile.services

import android.content.Context
import com.example.readytohelpmobile.model.auth.LoginRequest
import com.example.readytohelpmobile.model.auth.RegisterRequest
import com.example.readytohelpmobile.model.auth.UserResponse
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.utils.TokenManager

/**
 * Service class that acts as a repository for Authentication logic.
 * It handles API calls and local token management.
 *
 * @param context Context required for NetworkClient and TokenManager.
 */
class AuthService(context: Context) {

    // Initialize API using the NetworkClient
    private val api = NetworkClient.getRetrofitInstance(context).create(AuthApi::class.java)
    private val tokenManager = TokenManager(context)

    /**
     * Performs login and saves the token if successful.
     */
    suspend fun login(email: String, pass: String): Result<String> {
        return try {
            val response = api.login(LoginRequest(email, pass))
            if (response.isSuccessful && response.body() != null) {
                // Save the received token locally
                val token = response.body()!!
                tokenManager.saveToken(token)
                Result.success(token)
            } else {
                Result.failure(Exception("Login failed: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    /**
     * Performs user registration.
     */
    suspend fun register(name: String, email: String, pass: String): Result<UserResponse?> {
        return try {
            val response = api.register(RegisterRequest(name, email, pass))
            if (response.isSuccessful) {
                Result.success(response.body())
            } else {
                Result.failure(Exception("Registration failed: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    /**
     * Logs out the user by calling the API and clearing local data.
     */
    suspend fun logout() {
        try {
            api.logout()
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            // Ensure local token is cleared even if API call fails
            tokenManager.clearToken()
        }
    }
}