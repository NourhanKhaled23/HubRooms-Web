# AdvancedChat Web Application

## Overview
AdvancedChat is a real-time web-based communication platform built with ASP.NET Core MVC and SignalR. It enables users to create chat rooms, broadcast messages to public channels, and send direct private messages to individual users.

## Features
- **Real-time Messaging:** Powered by SignalR with automatic reconnect capabilities.
- **Rooms Management:** Create, delete, and join specific chat rooms.
- **Direct Messaging:** Send private messages directly to active users.
- **RESTful API:** Integrated Swagger/OpenAPI for easy exploration of backend endpoints.
- **Modern UI:** Clean, responsive workspace layout tailored for efficient communication.

## Tech Stack
- **Backend:** C# / ASP.NET Core MVC 8.0
- **Real-time:** SignalR
- **Database:** SQLite with Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript (Vanilla)
- **Identity:** ASP.NET Core Identity (Default)

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run Locally
1. Clone the repository.
2. Navigate to the project directory: \cd AdvancedChat.Web\`n3. Restore dependencies and run: \dotnet run\`n4. Open your browser and navigate to \https://localhost:<port>\.

### API Documentation
Once running in the Development environment, navigate to \/swagger\ to view and interact with the RESTful API endpoints via Swagger UI.
