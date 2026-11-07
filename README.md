# RSVP Event App

A .NET MAUI mobile app that lets people create an account, log in (or browse as a guest), view events, see event details, add events, and RSVP, backed by an ASP.NET Core web service.

## Architecture

```
RSVP_Event_App (MAUI)  --HTTP/JSON-->  RsvpApi (ASP.NET Core Web API)  -->  SQLite (RsvpApi.db3)
   Views -> ViewModels -> Data/RsvpDatabase    Controllers -> Data/RsvpRepository
```

- **RsvpApi** owns all data. It stores Users, Events, and Rsvps in SQLite on the server, hashes passwords with salted PBKDF2, and enforces the RSVP rules (deadline, capacity, no duplicate RSVPs).
- **RSVP_Event_App** follows **MVVM**. Views (XAML) contain layout and data binding only, View Models in `ViewModels/` hold the logic and commands, and `Data/RsvpDatabase.cs` is the data access layer that calls the API with `HttpClient` + Newtonsoft.Json. Views, View Models, and the data access class are registered in `MauiProgram` and supplied through dependency injection.
- **Authentication**: user credentials are verified by the web service using **HTTP Basic Authentication** (`GET /api/Users/me`). After login the app keeps the credentials in memory only and sends them with Basic Authentication for user-specific requests (Attending / Hosting lists, adding events, and RSVPing with the user's saved credentials). Guests can browse and RSVP by entering their details.

## API endpoints (http://localhost:5090, Swagger at `/swagger`)

| Method & route | Auth | Purpose |
|---|---|---|
| `POST /api/Users` | none | Create an account (409 if the e-mail already exists) |
| `GET /api/Users/me` | Basic | Log in / return the account |
| `GET /api/Events` | none | All events with attendee counts |
| `GET /api/Events/attending` | Basic | Events the user is attending |
| `GET /api/Events/hosting` | Basic | Events the user is hosting |
| `GET /api/Events/{id}` | none | Event details |
| `GET /api/Events/{id}/attendees` | none | Names of people attending |
| `GET /api/Events/{id}/rsvp` | Basic | Whether the user has already RSVP'd |
| `POST /api/Events` | Basic | Add an event hosted by the user |
| `POST /api/Events/{id}/rsvps` | optional | RSVP as the logged-in user, or as a guest |

## Pages

| Page | Navigation |
|---|---|
| **Login** (start page) | Log In → Events · Continue as Guest → Events · Create an account → Create Account |
| **Create Account** | Create Account → Login · Cancel → Login |
| **Events** | All / Attending / Hosting filters (guests see All only) · tap an event → Event Details · + Add Event · Log Out |
| **Event Details** | RSVP for this Event (disabled when closed, full, or already attending) · Back |
| **Add Event** | Save (validated) · Cancel |
| **RSVP** | Pre-filled for logged-in users, blank for guests · Submit RSVP · Cancel |

## Running

1. Start the web service: `dotnet run --launch-profile http` in `RsvpApi` (sample users, events, and RSVPs are created on first run).
2. Run `RSVP_Event_App` on Windows Machine or the Android emulator. On Android the app reaches the service at `http://10.0.2.2:5090`.

**Test login:** `patrick@rsvpapp.com` / `Password1` (other sample users: sarah@, marcus@, linda@example.com, same password)

## Android package

`dotnet publish RSVP_Event_App/RSVP_Event_App.csproj -f net9.0-android -c Release -p:AndroidPackageFormat=apk`

The web service address is for local development. A production release needs a hosted HTTPS service and a production signing key.
