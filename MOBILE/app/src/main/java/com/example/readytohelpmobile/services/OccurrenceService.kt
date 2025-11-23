package com.example.readytohelpmobile.services
import com.example.readytohelpmobile.model.Occurrence
import com.example.readytohelpmobile.network.NetworkClient
import retrofit2.Response
import retrofit2.http.GET

private interface OccurrenceApi {
    @GET("occurrence/active")
    suspend fun getActiveOccurrences(): Response<List<Occurrence>>
}

object OccurrenceService {
    private val api = NetworkClient.retrofit.create(OccurrenceApi::class.java)

    suspend fun getActiveOccurrences(): List<Occurrence>? {
        return try {
            val response = api.getActiveOccurrences()
            if (response.isSuccessful) {
                response.body()
            } else {
                null
            }
        } catch (e: Exception) {

            e.printStackTrace()
            null
        }
    }
}