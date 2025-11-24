package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.network.NetworkClient

object OccurrenceService {
    private val api = NetworkClient.retrofit?.create(OccurrenceApi::class.java)

    suspend fun getAllOccurrences(): List<Occurrence>? {
        return try {
            val response = api?.getAllOccurrences()
            response?.let {
                if (it.isSuccessful) {
                    response.body()
                } else {
                    null
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
}