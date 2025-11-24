package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Feedback
import com.example.readytohelpmobile.network.NetworkClient
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

private interface FeedbackApi {
    @POST("feedback")
    suspend fun createFeedback(@Body feedback: Feedback): Response<Feedback>
}

object FeedbackService {
    private val api = NetworkClient.retrofit?.create(FeedbackApi::class.java)

    suspend fun createFeedback(feedback: Feedback): Boolean {
        return try {
            val response = api?.createFeedback(feedback)
            response!!.isSuccessful
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
}