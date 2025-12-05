package com.example.readytohelpmobile.utils

import android.content.Context
import android.content.SharedPreferences
import android.util.Base64
import org.json.JSONObject

/**
 * Utility class for managing JWT authentication tokens.
 * Handles saving, retrieving, clearing, and decoding tokens from SharedPreferences.
 *
 * @param context The application context used to access SharedPreferences.
 */
class TokenManager(context: Context) {

    // Initialize SharedPreferences with a private mode
    private val prefs: SharedPreferences =
        context.getSharedPreferences("auth_prefs", Context.MODE_PRIVATE)

    /**
     * Saves the JWT token string to persistent storage.
     * @param token The JWT string.
     */
    fun saveToken(token: String) {
        prefs.edit().putString("jwt_token", token).apply()
    }

    /**
     * Retrieves the stored JWT token.
     * @return The token string or null if not found.
     */
    fun getToken(): String? {
        return prefs.getString("jwt_token", null)
    }

    /**
     * Removes the stored token, effectively logging the user out locally.
     */
    fun clearToken() {
        prefs.edit().remove("jwt_token").apply()
    }

    /**
     * Decodes the payload part of the JWT to extract the User ID.
     * Attempts to find the ID in common claim fields like "id", "nameid", or "sub".
     *
     * @return The extracted User ID as an Int, or 0 if extraction fails.
     */
    fun getUserIdFromToken(): Int {
        val token = getToken() ?: return 0
        try {
            // JWT structure: Header.Payload.Signature
            val parts = token.split(".")
            if (parts.size < 2) return 0

            // Decode the payload (second part) from Base64
            val decodedBytes = Base64.decode(parts[1], Base64.URL_SAFE)
            val decodedString = String(decodedBytes, Charsets.UTF_8)

            // Parse the JSON payload
            val json = JSONObject(decodedString)

            // Try to find the ID field
            return if (json.has("id")) {
                json.getString("id").toIntOrNull() ?: 0
            } else if (json.has("nameid")) {
                json.getString("nameid").toIntOrNull() ?: 0
            } else if (json.has("sub")) {
                json.getString("sub").toIntOrNull() ?: 0
            } else {
                0
            }
        } catch (e: Exception) {
            e.printStackTrace()
            return 0
        }
    }
}