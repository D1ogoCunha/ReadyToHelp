# ReadyToHelp

ReadyToHelp is a collaborative platform designed for the reporting and real-time management of various types of incidents. The core focus of the application is to empower users to report occurrences and visualize them through interactive maps on both mobile and web interfaces.

Beyond reporting, the platform features a crowd-sourced feedback system where users can update the status of active incidents when they are within the required proximity, confirming whether they still persist or have been resolved.

Key features include:
* **Multi-Platform Map Visualization**: Real-time tracking of incidents on Android and Web platforms.
* **Administrative Management**: A dedicated web interface for administrators to manage occurrences and oversee the user base.
* **Automated Notifications**: When a reported incident is validated and its status changes from "Pending" to "Active," the relevant responsible entity is automatically notified.

## Core Technologies

* **Backend**: .NET 8+ (ASP.NET Core) and Entity Framework Core.
* **Web**: Angular 19.
* **Mobile**: Kotlin (Android SDK 36, Min SDK 28).
* **Database**: PostgreSQL.
* **Infrastructure**: Docker support and GitLab CI/CD integration.

## Prerequisites

To set up and run the project components, the following are required:
* **Git**: For version control.
* **.NET SDK (8+)**: To run the Backend API.
* **dotnet-ef tool**: For managing database migrations.
* **PostgreSQL or Docker**: For database hosting.
* **Node.js & npm**: For managing web dependencies.
* **Angular CLI**: To build and serve the web frontend.
* **Android Studio**: For mobile application development.

## Installation and Setup

### Cloning the Repository
```bash
git clone https://gitlab.com/grupo_07-lds-2526/lds_25_26.git
cd lds_25_26
```
### Running the API (Backend)
```bash
cd API/ReadyToHelpAPI
dotnet ef database update
dotnet run
```
### Running the Web Frontend
```bash
cd WEB/readytohelp
npm install
ng serve
```
### Running the Mobile App
1 - Open the MOBILE directory in Android Studio.

2 - Sync the project with Gradle files.

3 - Build and run the application on an emulator or physical device.

### Simulating Responsible Entity notification
```bash
python notifier.py
```
### Testing
```bash
dotnet test
npx cypress run
```

