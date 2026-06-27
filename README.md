# Advanced Chat

A real-time chat platform with a web frontend and a WPF desktop client, built on ASP.NET Core 8 and SignalR. Users can create chat rooms, invite members, send public and private messages, and collaborate in real time.

## Architecture

```
AdvancedChat.sln
├── AdvancedChat.Web          — ASP.NET Core 8 MVC app with SignalR
│   ├── Controllers/          — MVC + API controllers
│   ├── Hubs/                 — SignalR hub (real-time messaging)
│   ├── Services/             — Business logic layer
│   ├── Models/               — Domain models and view models
│   ├── Data/                 — EF Core DbContext + migrations
│   ├── Areas/Identity/       — ASP.NET Core Identity UI
│   └── Views/                — Razor views (landing, dashboard, rooms)
└── AdvancedChat.Desktop      — WPF desktop client (.NET 8 Windows)
    └── MainWindow.xaml       — XAML UI with SignalR connectivity
```

## Features

- **User authentication** — Register and sign in via ASP.NET Core Identity
- **Chat rooms** — Create, delete, and manage rooms with role-based access
- **Member management** — Invite users by email to join rooms
- **Real-time messaging** — Instant message delivery via SignalR WebSockets
- **Private messages** — Direct messaging between users
- **Desktop client** — WPF application with full chat functionality
- **Dashboard** — Unified console for room management and messaging

## Tech stack

| Layer          | Technology                                    |
|----------------|-----------------------------------------------|
| Runtime        | .NET 8                                        |
| Web framework  | ASP.NET Core MVC + SignalR                    |
| Desktop client | WPF (.NET 8, Windows)                         |
| Database       | SQLite via Entity Framework Core 8            |
| Authentication | ASP.NET Core Identity                         |
| Frontend       | Bootstrap 5, Bootstrap Icons, Inter font      |
| Client-side    | jQuery, jQuery Validation, SignalR JS client  |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Desktop client only) Windows 10/11

### Run the web app

```bash
cd AdvancedChat.Web
dotnet run --launch-profile http
```

The app starts at `http://localhost:5017`. Open the URL in a browser, register an account, and start chatting.

### Run the desktop client

The desktop client connects to the same backend. Update the server URL in the UI (default: `http://localhost:5017`).

```bash
cd AdvancedChat.Desktop
dotnet run
```

### Database

The app uses SQLite with EF Core migrations. The database is created automatically on first run. To rebuild from scratch:

```bash
cd AdvancedChat.Web
dotnet ef database drop
dotnet ef database update
```

## Project structure

### Web app (`AdvancedChat.Web/`)

- **`Program.cs`** — Application entry point, DI registration, middleware pipeline
- **`Controllers/ApiAuthController.cs`** — JSON auth endpoints (register, login, logout)
- **`Controllers/ApiRoomsController.cs`** — JSON room management API
- **`Controllers/RoomsController.cs`** — MVC controller for room CRUD and member management
- **`Controllers/HomeController.cs`** — Landing page and error handling
- **`Hubs/ChatHub.cs`** — SignalR hub: room join/leave, public/private messaging
- **`Services/ChatRoomService.cs`** — Room and message business logic
- **`Models/`** — Domain entities (`ChatRoom`, `ChatMessage`, `ChatRoomMember`) and view models
- **`Data/ApplicationDbContext.cs`** — EF Core context with Fluent API configuration
- **`Areas/Identity/`** — Scaffolded Identity pages (login, register)
- **`Views/`** — Razor views with custom CSS styling

### Desktop client (`AdvancedChat.Desktop/`)

- **`MainWindow.xaml`** — XAML layout with connection panel, room list, and chat area
- **`MainWindow.xaml.cs`** — SignalR connection management, room loading, message handling

## API endpoints

### Auth (`/api/auth`)

| Method | Path            | Description        |
|--------|-----------------|--------------------|
| POST   | `/api/auth/register` | Create a new user |
| POST   | `/api/auth/login`    | Sign in           |
| POST   | `/api/auth/logout`   | Sign out (auth)   |

### Rooms (`/api/rooms`)

| Method | Path                    | Description         |
|--------|-------------------------|---------------------|
| GET    | `/api/rooms`            | List user's rooms   |
| POST   | `/api/rooms`            | Create a room       |
| POST   | `/api/rooms/{id}/users` | Add user to room    |

## SignalR hub (`/chatHub`)

| Method              | Description                       |
|---------------------|-----------------------------------|
| `JoinRoom`          | Subscribe to room messages        |
| `SendMessage`       | Send a message to a room          |
| `SendPrivateMessage`| Send a direct message to a user   |
| `GetRecentMessages` | Retrieve the last 50 messages     |

Clients receive events: `ReceiveMessage`, `ReceivePrivateMessage`, `SystemMessage`.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -am 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a pull request

## License

This project is licensed under the MIT License.
