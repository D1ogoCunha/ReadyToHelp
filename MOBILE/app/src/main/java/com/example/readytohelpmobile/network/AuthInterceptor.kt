package com.example.readytohelpmobile.network

import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        val myToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiQ0lUSVpFTiIsImVtYWlsIjoiY2FybG9zQGV4YW1wbGUuY29tIiwic3ViIjoiNSIsImp0aSI6IjZmMTVhOWJjOThhZjRmYmRiZjk2ZjA2YWJmNTQ0YzIwIiwibmJmIjoxNzYzOTM4OTc4LCJleHAiOjE3NjQwMjUzNzgsImlhdCI6MTc2MzkzODk3OCwiaXNzIjoiUmVhZHlUb0hlbHBBUEkiLCJhdWQiOiJSZWFkeVRvSGVscENsaWVudHMifQ.08ZHQyZrhd_C7yx5lcWxWeD_DVUEdxo8WYQDCwisM0s"

        val newRequest = originalRequest.newBuilder()
            .addHeader("Authorization", "Bearer $myToken")
            .build()

        return chain.proceed(newRequest)
    }
}