package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.auth.LoginRequest
import com.example.readytohelpmobile.model.auth.RegisterRequest
import com.example.readytohelpmobile.model.auth.UserResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApi {
    @POST("auth/login/mobile")
    suspend fun login(@Body request: LoginRequest): Response<String>

    @POST("user/register")
    suspend fun register(@Body request: RegisterRequest): Response<UserResponse>

    @POST("auth/logout")
    suspend fun logout(): Response<Void>
}