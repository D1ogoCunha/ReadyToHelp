package com.example.readytohelpmobile.network

import com.example.readytohelpmobile.utils.TokenManager
import okhttp3.Interceptor
import okhttp3.Response

/**
 * An OkHttp Interceptor that automatically adds the "Authorization" header
 * to outgoing requests if a valid token exists.
 *
 * @property tokenManager Manager class to retrieve the stored JWT token.
 */
class AuthInterceptor(private val tokenManager: TokenManager) : Interceptor {

    /**
     * Intercepts the HTTP request chain.
     *
     * @param chain The chain of interceptors provided by OkHttp.
     * @return The response from the server.
     */
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        // Retrieve the JWT token from shared preferences
        val token = tokenManager.getToken()

        // If the token exists, create a new request builder and add the header.
        // Otherwise, proceed with the original request (e.g., for Login/Register).
        val newRequest = if (token != null) {
            originalRequest.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
        } else {
            originalRequest
        }

        // Proceed with the request
        return chain.proceed(newRequest)
    }
}