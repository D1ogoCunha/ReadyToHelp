package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.network.NetworkClient
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Query

private interface OccurrenceApi {
    @GET("occurrence")
    suspend fun getAllOccurrences(
        @Query("pageNumber") pageNumber: Int = 1,
        @Query("pageSize") pageSize: Int = 100
    ): Response<List<Occurrence>>
}

object OccurrenceService {
    private val api = NetworkClient.retrofit.create(OccurrenceApi::class.java)

    suspend fun getAllOccurrences(): List<Occurrence>? {
        return try {
            val response = api.getAllOccurrences()
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