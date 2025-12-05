package com.example.readytohelpmobile.ui.screens.auth

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.readytohelpmobile.R
import com.example.readytohelpmobile.viewmodel.AuthUiState
import com.example.readytohelpmobile.viewmodel.AuthViewModel

private val BrandBlue = Color(0xFF4253AF)

/**
 * Composable function for the User Registration Screen.
 * Handles new user sign-up form, including basic validation (matching passwords).
 *
 * @param onRegisterSuccess Callback function executed when registration is successful.
 * @param onNavigateToLogin Callback function to navigate back to the login screen.
 * @param viewModel The ViewModel that handles registration logic.
 */
@Composable
fun RegisterScreen(
    onRegisterSuccess: () -> Unit,
    onNavigateToLogin: () -> Unit,
    viewModel: AuthViewModel = viewModel()
) {
    // Local state for form inputs
    var name by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }

    // Local state for client-side validation errors (e.g. passwords don't match)
    var validationError by remember { mutableStateOf<String?>(null) }

    // Observe UI state from ViewModel
    val state by viewModel.uiState.collectAsState()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(BrandBlue)
            .padding(24.dp)
            .verticalScroll(rememberScrollState()), // Make screen scrollable for smaller devices
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        // Logo
        Image(
            painter = painterResource(id = R.drawable.readytohelp_logo),
            contentDescription = "ReadyToHelp Logo",
            modifier = Modifier
                .height(100.dp)
                .fillMaxWidth(),
            contentScale = ContentScale.Fit
        )

        Spacer(modifier = Modifier.height(24.dp))

        Text(
            text = "Create Account",
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            color = Color.White
        )
        Text(
            text = "Join the ReadyToHelp community",
            style = MaterialTheme.typography.bodyMedium,
            color = Color.White.copy(alpha = 0.8f),
            textAlign = TextAlign.Center
        )

        Spacer(modifier = Modifier.height(32.dp))

        // Name Field
        OutlinedTextField(
            value = name,
            onValueChange = { name = it },
            label = { Text("Name", color = Color.White) },
            leadingIcon = { Icon(imageVector = Icons.Default.Person, contentDescription = null, tint = Color.White) },
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            singleLine = true,
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = Color.White,
                unfocusedBorderColor = Color.White,
                focusedTextColor = Color.White,
                unfocusedTextColor = Color.White,
                cursorColor = Color.White
            )
        )

        Spacer(modifier = Modifier.height(16.dp))

        // Email Field
        OutlinedTextField(
            value = email,
            onValueChange = { email = it },
            label = { Text("Email", color = Color.White) },
            leadingIcon = { Icon(imageVector = Icons.Default.Email, contentDescription = null, tint = Color.White) },
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            singleLine = true,
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = Color.White,
                unfocusedBorderColor = Color.White,
                focusedTextColor = Color.White,
                unfocusedTextColor = Color.White,
                cursorColor = Color.White
            )
        )

        Spacer(modifier = Modifier.height(16.dp))

        // Password Field
        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            label = { Text("Password", color = Color.White) },
            leadingIcon = { Icon(imageVector = Icons.Default.Lock, contentDescription = null, tint = Color.White) },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            singleLine = true,
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = Color.White,
                unfocusedBorderColor = Color.White,
                focusedTextColor = Color.White,
                unfocusedTextColor = Color.White,
                cursorColor = Color.White
            )
        )

        Spacer(modifier = Modifier.height(16.dp))

        // Confirm Password Field
        OutlinedTextField(
            value = confirmPassword,
            onValueChange = { confirmPassword = it },
            label = { Text("Confirm Password", color = Color.White) },
            leadingIcon = { Icon(imageVector = Icons.Default.Lock, contentDescription = null, tint = Color.White) },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            singleLine = true,
            isError = validationError != null, // Highlight error state
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = Color.White,
                unfocusedBorderColor = Color.White,
                focusedTextColor = Color.White,
                unfocusedTextColor = Color.White,
                cursorColor = Color.White,
                errorBorderColor = Color(0xFFFFCDD2),
                errorLabelColor = Color(0xFFFFCDD2)
            )
        )

        // Show validation error message if applicable
        if (validationError != null) {
            Text(
                text = validationError!!,
                color = Color(0xFFFFCDD2),
                style = MaterialTheme.typography.bodySmall,
                modifier = Modifier.padding(top = 4.dp)
            )
        }

        Spacer(modifier = Modifier.height(24.dp))

        if (state is AuthUiState.Loading) {
            CircularProgressIndicator(color = Color.White)
        } else {
            // Sign Up Button
            Button(
                onClick = {
                    // Client-side validation
                    if (password.isBlank()) {
                        validationError = "Password cannot be empty"
                    } else if (password != confirmPassword) {
                        validationError = "Passwords do not match!"
                    } else {
                        validationError = null
                        // Proceed to register via ViewModel
                        viewModel.register(name, email, password)
                    }
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp),
                shape = RoundedCornerShape(12.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color.White,
                    contentColor = BrandBlue
                )
            ) {
                Text(text = "Sign Up", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Navigation back to Login
            TextButton(onClick = onNavigateToLogin) {
                Text(
                    text = "Already have an account? Sign In",
                    color = Color.White
                )
            }
        }

        // Display server-side errors
        if (state is AuthUiState.Error) {
            Spacer(modifier = Modifier.height(16.dp))
            Text(
                text = (state as AuthUiState.Error).msg,
                color = Color(0xFFFFCDD2),
                style = MaterialTheme.typography.bodyMedium
            )
        }

        // Handle successful registration (triggers navigation)
        LaunchedEffect(state) {
            if (state is AuthUiState.Success) {
                onRegisterSuccess()
            }
        }
    }
}