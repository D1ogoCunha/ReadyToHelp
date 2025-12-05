package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.report.Report
import com.example.readytohelpmobile.model.report.ReportResponse
import retrofit2.http.Body
import retrofit2.http.POST

/**
 * Retrofit interface for Report endpoints.
 */
interface ReportApi {

    /**
     * Submits a new report to the system.
     * @param dto The report data (title, description, location, etc.).
     * @return The server response containing the created Report details.
     */
    @POST("reports")
    suspend fun createReport(
        @Body dto: Report
    ): ReportResponse
}