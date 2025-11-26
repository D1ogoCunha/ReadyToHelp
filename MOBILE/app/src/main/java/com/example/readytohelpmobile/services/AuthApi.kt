package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.auth.LoginRequest
import com.example.readytohelpmobile.model.auth.RegisterRequest
import com.example.readytohelpmobile.model.auth.UserResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

/**
 * Retrofit interface for Authentication endpoints.
 */
interface AuthApi {

    /**
     * Authenticates a user.
     * @param request Contains email and password.
     * @return A Response containing the JWT token string.
     */
    @POST("auth/login/mobile")
    suspend fun login(@Body request: LoginRequest): Response<String>

    /**
     * Registers a new user.
     * @param request Contains name, email, and password.
     * @return A Response containing the created user details.
     */
    @POST("user/register")
    suspend fun register(@Body request: RegisterRequest): Response<UserResponse>

    /**
     * Logs out the user on the server side (if applicable).
     * @return An empty Response.
     */
    @POST("auth/logout")
    suspend fun logout(): Response<Void>
}