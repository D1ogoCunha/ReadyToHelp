package com.example.readytohelpmobile.viewmodel

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.example.readytohelpmobile.services.AuthService
import com.example.readytohelpmobile.model.auth.LoginRequest
import com.example.readytohelpmobile.model.auth.RegisterRequest
import com.example.readytohelpmobile.network.NetworkClient
import com.example.readytohelpmobile.utils.TokenManager
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

    private val api = NetworkClient.getRetrofitInstance(application).create(AuthService::class.java)
    private val tokenManager = TokenManager(application)

    private val _uiState = MutableStateFlow<AuthUiState>(AuthUiState.Idle)
    val uiState: StateFlow<AuthUiState> = _uiState

    fun login(email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            try {
                val response = api.login(LoginRequest(email, pass))
                if (response.isSuccessful && response.body() != null) {
                    tokenManager.saveToken(response.body()!!)
                    _uiState.value = AuthUiState.Success
                } else {
                    _uiState.value = AuthUiState.Error("Login falhou: ${response.code()}")
                }
            } catch (e: Exception) {
                _uiState.value = AuthUiState.Error("Erro: ${e.localizedMessage}")
            }
        }
    }

    fun register(name: String, email: String, pass: String) {
        _uiState.value = AuthUiState.Loading
        viewModelScope.launch {
            try {
                val response = api.register(RegisterRequest(name, email, pass))
                if (response.isSuccessful) {
                    login(email, pass)
                } else {
                    _uiState.value = AuthUiState.Error("Registo falhou: ${response.code()}")
                }
            } catch (e: Exception) {
                _uiState.value = AuthUiState.Error("Erro: ${e.localizedMessage}")
            }
        }
    }
}