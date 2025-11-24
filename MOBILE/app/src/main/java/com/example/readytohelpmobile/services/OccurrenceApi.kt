package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Occurrence
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Query

interface OccurrenceApi {
    @GET("occurrence")
    suspend fun getAllOccurrences(
        @Query("pageNumber") pageNumber: Int = 1,
        @Query("pageSize") pageSize: Int = 100
    ): Response<List<Occurrence>>
}
