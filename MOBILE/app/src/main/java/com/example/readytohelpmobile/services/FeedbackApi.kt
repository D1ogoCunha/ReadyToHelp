package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Feedback
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

/**
 * Retrofit interface for Feedback endpoints.
 */
interface FeedbackApi {

    /**
     * Sends a feedback/confirmation for an occurrence.
     * @param feedback The feedback object containing occurrenceId, userId, etc.
     */
    @POST("feedback")
    suspend fun createFeedback(@Body feedback: Feedback): Response<Feedback>
}