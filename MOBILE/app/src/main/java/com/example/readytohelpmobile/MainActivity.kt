package com.example.readytohelpmobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.example.readytohelpmobile.ui.navigation.AppNavigation
import com.example.readytohelpmobile.ui.theme.ReadyToHelpMobileTheme
import com.example.readytohelpmobile.utils.TokenManager

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val tokenManager = TokenManager(this)
        tokenManager.clearToken()
        val startDest = if (tokenManager.getToken() != null) "map" else "login"

        setContent {
            ReadyToHelpMobileTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    AppNavigation(startDestination = startDest)
                }
            }
        }
    }
}