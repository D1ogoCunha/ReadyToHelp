package com.example.readytohelpmobile.services

import android.content.Context
import android.util.Log
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.network.NetworkClient

object OccurrenceService {
    suspend fun getAllOccurrences(context: Context): List<Occurrence>? {
        return try {
            val retrofit = NetworkClient.getRetrofitInstance(context)
            val api = retrofit.create(OccurrenceApi::class.java)

            val response = api.getAllOccurrences()
            if (response.isSuccessful) {
                Log.d("OccurrenceService", "Success: ${response.body()?.size} occurrences found")
                response.body()
            } else {
                Log.e("OccurrenceService", "API Error: ${response.code()} - ${response.errorBody()?.string()}")
                null
            }
        } catch (e: Exception) {
            Log.e("OccurrenceService", "Exception fetching occurrences", e)
            e.printStackTrace()
            null
        }
    }
}