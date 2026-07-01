# HubRooms Web Application

HubRooms is a comprehensive real-time chat platform built on ASP.NET Core 8 MVC and SignalR. Create dynamic chat rooms, send private direct messages, and explore the Swagger-powered REST API.

## Features

- Auth via ASP.NET Core Identity (cookie-based)
- Chat rooms with membership enforcement
- Real-time messaging over WebSockets
- Private direct messages between users
- Dashboard with live feed and quick-send controls
- REST API for auth and room management

## Tech stack

| Layer         | Technology                               |
|---------------|------------------------------------------|
| Runtime       | .NET 8                                   |
| Web framework | ASP.NET Core MVC + SignalR               |
| Database      | SQLite / EF Core 8                       |
| Auth          | ASP.NET Core Identity                    |
| Frontend      | Bootstrap 5, Bootstrap Icons, Inter font |

## Quick start

```bash
cd AdvancedChat.Web
dotnet run --launch-profile http
```

Open `http://localhost:5017`, register, and start chatting. SQLite database is created automatically on first run.

## API

### Auth

```
POST /api/auth/register   { email, password }        → 200
POST /api/auth/login      { email, password }        → 200
POST /api/auth/logout     [Auth]                     → 200
```

### Rooms

```
GET   /api/rooms          [Auth]                     → 200 [ rooms ]
POST  /api/rooms          [Auth] { name, description } → 200
POST  /api/rooms/{id}/users [Auth] { email }         → 200
```

## SignalR hub (`/chatHub`)

| Method              | Description                    |
|---------------------|--------------------------------|
| `JoinRoom`          | Subscribe to room messages     |
| `SendMessage`       | Send a message to a room       |
| `SendPrivateMessage`| Send a direct message          |
| `GetRecentMessages` | Get last 50 messages           |

Client events: `ReceiveMessage`, `ReceivePrivateMessage`, `SystemMessage`.

## Project structure

```
Controllers/   — MVC + API controllers
Hubs/          — SignalR hub
Services/      — Business logic
Models/        — Entities + view models
Data/          — DbContext + migrations
Areas/Identity/ — Auth pages (login, register)
Views/         — Razor views
wwwroot/       — Static assets
```

## License

MIT
