package com.example.readytohelpmobile.model.report

/**
 * Represents the contact information of a responsible entity (e.g., Police, Fire Department).
 * This information is usually returned by the API when a report is submitted, providing
 * the user with details on who was notified.
 *
 * @property name The name of the entity (e.g., "Lisbon Police Department").
 * @property email The contact email address of the entity.
 * @property address The physical address of the entity's headquarters or station.
 * @property contactPhone The contact phone number of the entity.
 */
data class ResponsibleEntityContact(
    val name: String,
    val email: String,
    val address: String,
    val contactPhone: Int
)