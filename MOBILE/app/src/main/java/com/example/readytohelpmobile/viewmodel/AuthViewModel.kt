package com.example.readytohelpmobile.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.services.AuthService
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

sealed class AuthUiState {
    object Idle : AuthUiState()
    object Loading : AuthUiState()
    object Success : AuthUiState()
    data class Error(val msg: String) : AuthUiState()
}

class AuthViewModel(application: Application) : AndroidViewModel(application) {

    private val service = AuthService(application)

    private val _uiState = MutableStateFlow<AuthUiState>(AuthUiState.Idle)
    val uiState: StateFlow<AuthUiState> = _uiState

    fun login(email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            val result = service.login(email, pass)

            result.onSuccess {
                _uiState.value = AuthUiState.Success
            }.onFailure { e ->
                _uiState.value = AuthUiState.Error(e.message ?: "Erro desconhecido")
            }
        }
    }

    fun register(name: String, email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            val result = service.register(name, email, pass)

            result.onSuccess {
                login(email, pass)
            }.onFailure { e ->
                _uiState.value = AuthUiState.Error(e.message ?: "Erro no registo")
            }
        }
    }

    fun logout(onLogoutComplete: () -> Unit) {
        viewModelScope.launch {
            service.logout()
            onLogoutComplete()
        }
    }
}