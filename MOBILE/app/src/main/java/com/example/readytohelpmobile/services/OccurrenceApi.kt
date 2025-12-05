package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.Occurrence
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Query

/**
 * Retrofit interface for Occurrence endpoints.
 */
interface OccurrenceApi {

    /**
     * Fetches a paginated list of occurrences.
     * @param pageNumber Current page index (default 1).
     * @param pageSize Items per page (default 100).
     */
    @GET("occurrence")
    suspend fun getAllOccurrences(
        @Query("pageNumber") pageNumber: Int = 1,
        @Query("pageSize") pageSize: Int = 100
    ): Response<List<Occurrence>>
}