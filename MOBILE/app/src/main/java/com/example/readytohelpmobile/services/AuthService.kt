package com.example.readytohelpmobile.services

import android.content.Context
import com.example.readytohelpmobile.model.auth.LoginRequest
import com.example.readytohelpmobile.model.auth.RegisterRequest
import com.example.readytohelpmobile.model.auth.UserResponse
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.utils.TokenManager

class AuthService(context: Context) {

    private val api = NetworkClient.getRetrofitInstance(context).create(AuthApi::class.java)
    private val tokenManager = TokenManager(context)

    suspend fun login(email: String, pass: String): Result<String> {
        return try {
            val response = api.login(LoginRequest(email, pass))
            if (response.isSuccessful && response.body() != null) {
                val token = response.body()!!
                tokenManager.saveToken(token)
                Result.success(token)
            } else {
                Result.failure(Exception("Login falhou: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun register(name: String, email: String, pass: String): Result<UserResponse?> {
        return try {
            val response = api.register(RegisterRequest(name, email, pass))
            if (response.isSuccessful) {
                Result.success(response.body())
            } else {
                Result.failure(Exception("Registo falhou: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun logout() {
        try {
            api.logout()
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            tokenManager.clearToken()
        }
    }
}