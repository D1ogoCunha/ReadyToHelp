package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Feedback
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface FeedbackApi {
    @POST("feedback")
    suspend fun createFeedback(@Body feedback: Feedback): Response<Feedback>
}