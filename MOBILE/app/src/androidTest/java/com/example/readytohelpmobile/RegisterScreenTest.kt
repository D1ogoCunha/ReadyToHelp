package com.example.readytohelpmobile

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.readytohelpmobile.ui.screens.auth.RegisterScreen
import com.example.readytohelpmobile.viewmodel.AuthUiState
import com.example.readytohelpmobile.viewmodel.AuthViewModel
import io.mockk.*
import kotlinx.coroutines.flow.MutableStateFlow
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class RegisterScreenTest {

    @get:Rule
    val composeTestRule = createComposeRule()

    // Mocks
    private val mockViewModel = mockk<AuthViewModel>(relaxed = true)
    private val uiStateFlow = MutableStateFlow<AuthUiState>(AuthUiState.Idle)

    @Test
    fun register_screen_displays_all_fields_correctly() {
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        // Verificar Textos Estáticos e Campos
        composeTestRule.onNodeWithText("Create Account").assertIsDisplayed()
        composeTestRule.onNodeWithText("Name").assertIsDisplayed()
        composeTestRule.onNodeWithText("Email").assertIsDisplayed()
        // Nota: Temos dois campos de "Password" (label), o Compose pode reclamar se não formos específicos.
        // Como o texto do label é "Password" e "Confirm Password", estamos seguros.
        composeTestRule.onNodeWithText("Password").assertIsDisplayed()
        composeTestRule.onNodeWithText("Confirm Password").assertIsDisplayed()
        composeTestRule.onNodeWithText("Sign Up").assertIsDisplayed()
    }

    @Test
    fun client_validation_shows_error_when_passwords_do_not_match() {
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        // 1. Preencher Password
        composeTestRule.onNodeWithText("Password").performTextInput("123456")

        // 2. Preencher Confirm Password (Diferente)
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("654321")

        // 3. Clicar em Sign Up
        // Usamos performScrollTo para garantir que o botão está visível em ecrãs pequenos
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // 4. VERIFICAR: Mensagem de erro deve aparecer
        composeTestRule.onNodeWithText("Passwords do not match!").assertIsDisplayed()

        // 5. VERIFICAR: O método register do ViewModel NÃO deve ter sido chamado
        verify(exactly = 0) { mockViewModel.register(any(), any(), any()) }
    }

    @Test
    fun successful_input_calls_viewmodel_register() {
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        // 1. Preencher Dados Válidos
        composeTestRule.onNodeWithText("Name").performTextInput("John Doe")
        composeTestRule.onNodeWithText("Email").performTextInput("john@test.com")
        composeTestRule.onNodeWithText("Password").performTextInput("123456")
        composeTestRule.onNodeWithText("Confirm Password").performTextInput("123456")

        // 2. Clicar
        composeTestRule.onNodeWithText("Sign Up").performScrollTo().performClick()

        // 3. VERIFICAR: ViewModel chamado com os dados corretos
        verify { mockViewModel.register("John Doe", "john@test.com", "123456") }
    }

    @Test
    fun register_screen_shows_loading_state() {
        uiStateFlow.value = AuthUiState.Loading
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        // Botão deve desaparecer
        composeTestRule.onNodeWithText("Sign Up").assertDoesNotExist()
    }

    @Test
    fun register_screen_shows_server_error() {
        val errorMsg = "Email already in use"
        uiStateFlow.value = AuthUiState.Error(errorMsg)
        every { mockViewModel.uiState } returns uiStateFlow

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        composeTestRule.onNodeWithText(errorMsg).assertIsDisplayed()
    }

    @Test
    fun registration_success_triggers_callback() {
        // Simular sucesso
        uiStateFlow.value = AuthUiState.Success
        every { mockViewModel.uiState } returns uiStateFlow

        var successCallbackCalled = false

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = { successCallbackCalled = true }, // Capturar
                onNavigateToLogin = {},
                viewModel = mockViewModel
            )
        }

        // Verificar callback
        assert(successCallbackCalled) { "Callback onRegisterSuccess devia ter sido chamado" }
    }

    @Test
    fun clicking_login_link_navigates_back() {
        every { mockViewModel.uiState } returns uiStateFlow

        var navigateLoginCalled = false

        composeTestRule.setContent {
            RegisterScreen(
                onRegisterSuccess = {},
                onNavigateToLogin = { navigateLoginCalled = true }, // Capturar
                viewModel = mockViewModel
            )
        }

        // Clicar no link de texto
        composeTestRule.onNodeWithText("Already have an account? Sign In").performScrollTo().performClick()

        // Verificar callback
        assert(navigateLoginCalled) { "Callback onNavigateToLogin devia ter sido chamado" }
    }
}