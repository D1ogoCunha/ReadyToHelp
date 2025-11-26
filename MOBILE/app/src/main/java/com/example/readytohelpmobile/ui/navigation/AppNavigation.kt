package com.example.readytohelpmobile.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.readytohelpmobile.ui.screens.auth.LoginScreen
import com.example.readytohelpmobile.ui.screens.auth.RegisterScreen
import com.example.readytohelpmobile.ui.screens.map.MapScreen

/**
 * Main navigation component for the application.
 * It sets up the NavHost and defines the navigation graph (routes).
 *
 * @param startDestination The initial route to display when the app starts (e.g., "login" or "map").
 */
@Composable
fun AppNavigation(startDestination: String) {
    // Create the NavController to manage app navigation
    val navController = rememberNavController()

    // Define the navigation host with the start destination
    NavHost(navController = navController, startDestination = startDestination) {

        // Route: Login Screen
        composable("login") {
            LoginScreen(
                onLoginSuccess = {
                    // On successful login, navigate to the map and clear the back stack
                    // so the user cannot go back to the login screen by pressing back.
                    navController.navigate("map") {
                        popUpTo("login") { inclusive = true }
                    }
                },
                onNavigateToRegister = {
                    // Navigate to the registration screen
                    navController.navigate("register")
                }
            )
        }

        // Route: Register Screen
        composable("register") {
            RegisterScreen(
                onRegisterSuccess = {
                    // On successful registration, navigate to the map.
                    // Clear both register and login screens from the back stack.
                    navController.navigate("map") {
                        popUpTo("register") { inclusive = true }
                        popUpTo("login") { inclusive = true }
                    }
                },
                onNavigateToLogin = {
                    // Navigate back to the login screen
                    navController.popBackStack()
                }
            )
        }

        // Route: Map Screen (Main App Interface)
        composable("map") {
            MapScreen(
                onLogout = {
                    // On logout, navigate back to login and clear the entire back stack history
                    navController.navigate("login") {
                        popUpTo(0) { inclusive = true }
                    }
                }
            )
        }
    }
}