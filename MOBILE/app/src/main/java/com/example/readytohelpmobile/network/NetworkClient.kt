package com.example.readytohelpmobile.network

import android.content.Context
import com.example.readytohelpmobile.utils.TokenManager
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import retrofit2.converter.scalars.ScalarsConverterFactory
import java.util.concurrent.TimeUnit

/**
 * Singleton object responsible for managing and providing the Retrofit instance.
 * This class handles the configuration of the HTTP client, including timeouts,
 * logging, authentication interception, and JSON parsing.
 */
object NetworkClient {

    // The base URL for the backend API
    private const val BASE_URL = "https://readytohelp-api.up.railway.app/api/"

    // Volatile ensures that the value is read directly from main memory,
    // guaranteeing visibility of changes across threads.
    @Volatile
    var retrofit: Retrofit? = null

    /**
     * Returns the singleton instance of Retrofit.
     * Implements the "Double-Checked Locking" pattern to ensure thread safety
     * and lazy initialization.
     *
     * @param context The application context used to initialize dependencies like TokenManager.
     * @return The configured Retrofit instance.
     */
    fun getRetrofitInstance(context: Context): Retrofit {
        // Check if instance exists; if not, synchronize and check again before creating.
        return retrofit ?: synchronized(this) {
            retrofit ?: buildRetrofit(context.applicationContext).also { retrofit = it }
        }
    }

    /**
     * Builds the Retrofit instance with all necessary configurations.
     *
     * @param context Application context.
     * @return A fully configured Retrofit object.
     */
    private fun buildRetrofit(context: Context): Retrofit {
        // Configure logging to see the body of HTTP requests/responses in Logcat
        val loggingInterceptor = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }

        // Initialize TokenManager to retrieve the saved JWT
        val tokenManager = TokenManager(context)

        // Initialize the interceptor that injects the Authorization header
        val authInterceptor = AuthInterceptor(tokenManager)

        // Configure the OkHttp Client
        val okHttpClient = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor) // Logs network traffic
            .addInterceptor(authInterceptor)    // Injects JWT Token
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .build()

        // Configure Moshi for JSON parsing (Kotlin friendly)
        val moshi = Moshi.Builder()
            .addLast(KotlinJsonAdapterFactory())
            .build()

        // Construct the Retrofit instance
        return Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttpClient)
            // Scalar converter allows returning raw Strings (e.g. for simple tokens)
            .addConverterFactory(ScalarsConverterFactory.create())
            // Moshi converter allows returning Data Classes (JSON mapping)
            .addConverterFactory(MoshiConverterFactory.create(moshi))
            .build()
    }
}