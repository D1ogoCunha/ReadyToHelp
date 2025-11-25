package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Feedback
import com.example.readytohelpmobile.network.NetworkClient

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