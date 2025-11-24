package com.example.readytohelpmobile.utils

import android.content.Context
import android.content.SharedPreferences
import android.util.Base64
import org.json.JSONObject

class TokenManager(context: Context) {

    private val prefs: SharedPreferences =
        context.getSharedPreferences("auth_prefs", Context.MODE_PRIVATE)

    fun saveToken(token: String) {
        prefs.edit().putString("jwt_token", token).apply()
    }

    fun getToken(): String? {
        return prefs.getString("jwt_token", null)
    }

    fun clearToken() {
        prefs.edit().remove("jwt_token").apply()
    }

    /**
     * Descodifica o Token JWT para obter o ID do utilizador.
     * Tenta encontrar campos comuns como "id", "nameid" (Identity) ou "sub".
     */
    fun getUserIdFromToken(): Int {
        val token = getToken() ?: return 0
        try {
            val parts = token.split(".")
            if (parts.size < 2) return 0

            val decodedBytes = Base64.decode(parts[1], Base64.URL_SAFE)
            val decodedString = String(decodedBytes, Charsets.UTF_8)

            val json = JSONObject(decodedString)

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