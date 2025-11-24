package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Occurrence
import retrofit2.Response
import retrofit2.http.GET

interface OccurrenceApi {
    @GET("occurrence/active")
    suspend fun getActiveOccurrences(): Response<List<Occurrence>>
}