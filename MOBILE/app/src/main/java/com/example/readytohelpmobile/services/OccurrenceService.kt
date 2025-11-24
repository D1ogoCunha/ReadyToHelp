package com.example.readytohelpmobile.services

import android.content.Context
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.network.NetworkClient

class OccurrenceService(context: Context) {
    private val api = NetworkClient.getRetrofitInstance(context).create(OccurrenceApi::class.java)

    suspend fun getActiveOccurrences(): List<Occurrence>? {
        return try {
            val response = api.getActiveOccurrences()
            if (response.isSuccessful) {
                response.body()
            } else {
                null
            }
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
}