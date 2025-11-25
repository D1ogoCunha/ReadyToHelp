package com.example.readytohelpmobile.services

import android.content.Context
import com.example.readytohelpmobile.model.report.Report
import com.example.readytohelpmobile.model.report.ReportResponse
import com.example.readytohelpmobile.network.NetworkClient

object ReportService {
    suspend fun createReport(context: Context, dto: Report): Result<ReportResponse> {
        return try {
            val api = NetworkClient.getRetrofitInstance(context).create(ReportApi::class.java)
            val response = api.createReport(dto)
            Result.success(response)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}