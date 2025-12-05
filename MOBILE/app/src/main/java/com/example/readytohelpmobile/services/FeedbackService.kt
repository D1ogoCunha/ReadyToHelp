package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Feedback
import com.example.readytohelpmobile.network.NetworkClient

/**
 * Singleton service to handle Feedback operations.
 */
object FeedbackService {
    // Note: This relies on NetworkClient.retrofit being initialized elsewhere.
    // It's safer to pass context here like in OccurrenceService if possible.
    private val api = NetworkClient.retrofit?.create(FeedbackApi::class.java)

    /**
     * Sends feedback to the API.
     * @return True if the request was successful, False otherwise.
     */
    suspend fun createFeedback(feedback: Feedback): Boolean {
        return try {
            val response = api?.createFeedback(feedback)
            // Return true only if response exists and is successful (2xx)
            response!!.isSuccessful
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
}