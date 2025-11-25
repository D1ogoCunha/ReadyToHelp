package com.example.readytohelpmobile.services

import com.example.readytohelpmobile.model.report.Report
import com.example.readytohelpmobile.model.report.ReportResponse
import retrofit2.http.Body
import retrofit2.http.POST

interface ReportApi {
    @POST("reports")
    suspend fun createReport(
        @Body dto: Report
    ): ReportResponse
}