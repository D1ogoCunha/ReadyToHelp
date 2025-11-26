package com.example.readytohelpmobile.model.report

/**
 * API response model after submitting a report.
 *
 * @property reportId The ID of the created report.
 * @property occurrenceId The ID of the generated occurrence (if applicable).
 * @property occurrenceStatus The initial status of the occurrence (e.g., "WAITING").
 * @property responsibleEntity Information about the notified responsible entity (if exists).
 */
data class ReportResponse(
    val reportId: Int,
    val occurrenceId: Int,
    val occurrenceStatus: String,
    val responsibleEntity: ResponsibleEntityContact?
)