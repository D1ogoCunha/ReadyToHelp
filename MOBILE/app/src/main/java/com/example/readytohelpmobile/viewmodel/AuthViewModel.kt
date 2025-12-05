package com.example.readytohelpmobile.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.services.AuthService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

/**
 * Represents the UI state for Authentication screens (Login/Register).
 */
sealed class AuthUiState {
    /** State when the view model is waiting for user interaction. */
    object Idle : AuthUiState()

    /** State when an authentication request is in progress. */
    object Loading : AuthUiState()

    /** State when the authentication request completed successfully. */
    object Success : AuthUiState()

    /** * State when the authentication request failed.
     * @property msg The error message to display.
     */
    data class Error(val msg: String) : AuthUiState()
}

/**
 * ViewModel responsible for managing Authentication logic (Login, Register, Logout).
 * It holds the UI state and communicates with the [AuthService].
 *
 * @param application The application context used to initialize the service.
 */
class AuthViewModel(application: Application) : AndroidViewModel(application) {

    private val service = AuthService(application)

    // Backing property for UI state
    private val _uiState = MutableStateFlow<AuthUiState>(AuthUiState.Idle)

    /**
     * Public immutable stream of UI state to be observed by the UI.
     */
    val uiState: StateFlow<AuthUiState> = _uiState

    /**
     * Attempts to log the user in with the provided credentials.
     * Updates [uiState] to Loading, then Success or Error based on the result.
     *
     * @param email The user's email.
     * @param pass The user's password.
     */
    fun login(email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            val result = service.login(email, pass)

            result.onSuccess {
                _uiState.value = AuthUiState.Success
            }.onFailure { e ->
                _uiState.value = AuthUiState.Error(e.message ?: "Unknown error")
            }
        }
    }

    /**
     * Attempts to register a new user.
     * If registration is successful, it automatically attempts to log the user in.
     *
     * @param name The user's full name.
     * @param email The user's email.
     * @param pass The user's password.
     */
    fun register(name: String, email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            val result = service.register(name, email, pass)

            result.onSuccess {
                // On successful registration, auto-login the user
                login(email, pass)
            }.onFailure { e ->
                _uiState.value = AuthUiState.Error(e.message ?: "Registration error")
            }
        }
    }

    /**
     * Logs out the current user and executes a callback upon completion.
     *
     * @param onLogoutComplete Callback function to execute after logout (e.g., navigate to login).
     */
    fun logout(onLogoutComplete: () -> Unit) {
        viewModelScope.launch {
            service.logout()
            onLogoutComplete()
        }
    }
}