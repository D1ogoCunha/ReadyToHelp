package com.example.readytohelpmobile.model.report

data class ReportResponse(
    val reportId: Int,
    val occurrenceId: Int,
    val occurrenceStatus: String,
    val responsibleEntity: ResponsibleEntityContact?
)