# Saled WebAPI

A secure ASP.NET Core 8 Web API for managing saled items with role-based authorization, JWT authentication, file upload support, real-time notifications via SignalR, and JSON file persistence.

## Key Features

- JWT-based authentication and authorization
- Role and clearance-level policies: Admin, Agent, User
- Secure endpoints for users and salad items
- Real-time activity broadcasting using SignalR
- File upload support for salad images
- Custom request logging middleware with asynchronous background processing
- JSON file-based persistence for `Data/User.json` and `Data/Saled.json`
- Swagger / OpenAPI documentation

## Architecture

- `Controllers/` - API controllers exposing REST endpoints
- `Services/` - business logic and persistence
- `Interface/` - service interfaces for dependency injection
- `Models/` - domain entities
- `Hubs/` - SignalR hub for real-time notifications
- `MyLogMiddleware.cs` - custom middleware for request timing
- `Program.cs` - app startup and service registration

## Technologies

- .NET 8 / ASP.NET Core
- JWT Bearer authentication
- SignalR
- Serilog logging
- Swagger / Swashbuckle
- System.Text.Json
- JSON file persistence
- RabbitMQ client package is included in the project dependencies for future messaging support

## Requirements

- .NET 8 SDK
- Windows / Linux / macOS

## How to Run

1. Open the solution in Visual Studio or use the terminal.
2. Restore dependencies:
   ```powershell
   dotnet restore
   ```
3. Run the application:
   ```powershell
   dotnet run
   ```
4. Open Swagger at `http://localhost:5066/swagger` or the configured application URL.

## API Endpoints

### Authentication

- `POST /api/login` - Authenticate a user and receive a JWT token
- `POST /api/user/GenerateBadge` - Admin endpoint to generate an Agent token

### User Management

- `GET /api/user` - Get all users (Admin only)
- `GET /api/user/{id}` - Get a specific user
- `PUT /api/user/{id}` - Update a user (Admin or owner)
- `DELETE /api/user/{id}` - Delete a user and their salad items (Admin only)
- `GET /api/user/me` - Get current authenticated user details
- `POST /api/user` - Create a new user (Admin only)

### Salad Item Management

- `GET /api/item` - Get salad items; regular users only see their own items
- `GET /api/item/{id}` - Get a salad by ID with ownership validation
- `POST /api/item` - Create a salad item with optional image upload
- `PUT /api/item/{id}` - Update a salad item with support for JSON or form data
- `DELETE /api/item/{id}` - Delete a salad item with ownership validation

## Real-time Notifications

- `ActivityHub` handles authenticated SignalR connections.
- Notifies relevant users and admins when salad items are added, updated, or deleted.

## Logging

- Custom middleware logs request duration and metadata.
- Log messages are queued and written to disk by a background worker.

## Configuration

- JWT configuration and issuer are handled in `Services/TokenService.cs`.
- CORS is configured to allow any origin, headers, and methods.
- Swagger is configured to support Bearer authentication.

## Notes

- The project currently uses JSON file persistence for an MVP-style implementation.
- The architecture is designed for easy migration to a database-backed store, including service interface abstractions.
- Environment-specific configuration is handled through ASP.NET Core environment settings.

## Future Improvements

- Replace JSON persistence with a relational database or NoSQL store
- Add unit and integration tests
- Add refresh token workflow for JWT
- Use a distributed cache for scaling and performance
- Introduce RabbitMQ or another message broker for event-driven workflows


