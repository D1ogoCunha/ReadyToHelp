package com.example.readytohelpmobile

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.readytohelpmobile.ui.screens.auth.LoginScreen
import com.example.readytohelpmobile.viewmodel.AuthUiState
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import io.mockk.*
import kotlinx.coroutines.flow.MutableStateFlow
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class LoginScreenTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    // Mocks
    private val mockViewModel = mockk<AuthViewModel>(relaxed = true)
    private val uiStateFlow = MutableStateFlow<AuthUiState>(AuthUiState.Idle) // Estado inicial

    @Test
    fun login_screen_displays_static_elements_correctly() {
        // Preparar o Mock: O ViewModel deve devolver o nosso StateFlow simulado
        every { mockViewModel.uiState } returns uiStateFlow

        // Carregar o ecrã isolado
        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = mockViewModel
            )
        }

        // VERIFICAÇÕES:
        // 1. Título de boas-vindas
        composeTestRule.onNodeWithText("Welcome Back!").assertIsDisplayed()

        // 2. Botão de Login
        composeTestRule.onNodeWithText("Sign In").assertIsDisplayed()

        // 3. Texto de navegação para registo
        composeTestRule.onNodeWithText("No account? Sign up here").assertIsDisplayed()
    }

    @Test
    fun user_can_enter_credentials_and_click_login() {
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = mockViewModel
            )
        }

        // 1. Escrever Email
        composeTestRule.onNodeWithText("Email")
            .performTextInput("teste@exemplo.com")

        // 2. Escrever Password
        composeTestRule.onNodeWithText("Password")
            .performTextInput("123456")

        // 3. Clicar no botão Login
        composeTestRule.onNodeWithText("Sign In").performClick()

        // 4. Verificar se a função login do ViewModel foi chamada com os dados corretos
        verify { mockViewModel.login("teste@exemplo.com", "123456") }
    }

    @Test
    fun login_screen_shows_loading_indicator_when_state_is_loading() {
        // Simular estado de Loading
        uiStateFlow.value = AuthUiState.Loading
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = mockViewModel
            )
        }

        // Verificar que o botão desapareceu e o Loading apareceu
        // O CircularProgressIndicator não tem texto, procuramos pela semântica ou pela ausência do botão
        composeTestRule.onNodeWithText("Sign In").assertDoesNotExist()
    }

    @Test
    fun login_screen_shows_error_message_on_failure() {
        // Simular estado de Erro
        val errorMsg = "Credenciais Inválidas"
        uiStateFlow.value = AuthUiState.Error(errorMsg)
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = {},
                viewModel = mockViewModel
            )
        }

        // Verificar se a mensagem de erro é exibida
        composeTestRule.onNodeWithText(errorMsg).assertIsDisplayed()
    }
    @Test
    fun login_success_triggers_navigation_callback() {
        // 1. Simular o estado de SUCESSO
        // (Ajusta o construtor se o teu Success aceitar um token, ex: AuthUiState.Success("token"))
        uiStateFlow.value = AuthUiState.Success
        every { mockViewModel.uiState } returns uiStateFlow

        var callbackCalled = false

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = { callbackCalled = true }, // Capturar o callback
                onNavigateToRegister = {},
                viewModel = mockViewModel
            )
        }

        // 2. Verificar se o LaunchedEffect disparou o callback
        assert(callbackCalled) { "A navegação onLoginSuccess deveria ter sido chamada" }
    }

    @Test
    fun clicking_register_text_triggers_navigation() {
        every { mockViewModel.uiState } returns uiStateFlow

        var registerClicked = false

        composeTestRule.setContent {
            LoginScreen(
                onLoginSuccess = {},
                onNavigateToRegister = { registerClicked = true }, // Capturar o callback
                viewModel = mockViewModel
            )
        }

        // 1. Encontrar o botão de texto e clicar
        composeTestRule.onNodeWithText("No account? Sign up here").performClick()

        // 2. Verificar se a navegação ocorreu
        assert(registerClicked) { "A navegação onNavigateToRegister deveria ter sido chamada" }
    }

}