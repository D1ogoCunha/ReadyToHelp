package com.example.readytohelpmobile.services

import android.content.Context
import com.example.readytohelpmobile.model.report.Report
import com.example.readytohelpmobile.model.report.ReportResponse
import com.example.readytohelpmobile.network.NetworkClient

/**
 * Singleton service helper for creating reports.
 */
object ReportService {

    /**
     * Calls the API to create a new report.
     *
     * @param context Application context to ensure Retrofit initialization.
     * @param dto The Report data object.
     * @return A Result wrapper containing either the ReportResponse (Success) or an Exception (Failure).
     */
    suspend fun createReport(context: Context, dto: Report): Result<ReportResponse> {
        return try {
            // Initialize API on demand using context
            val api = NetworkClient.getRetrofitInstance(context).create(ReportApi::class.java)

            val response = api.createReport(dto)
            Result.success(response)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}