# RSVP Event App

A .NET MAUI mobile app that lets people create an account, log in (or browse as a guest), view events, see event details, add events, and RSVP.

## Current milestone: local data storage (SQLite)

All data is stored on the device in a SQLite database (`RsvpApp.db3`) through a Data Access Layer (`Data/RsvpDatabase.cs`) built on sqlite-net-pcl:

- **Users**: accounts created in the app; passwords are stored as salted PBKDF2 hashes, never in plain text
- **Events**: each event references the user who hosts it
- **Rsvps**: links people (registered users or guests) to events; a unique index on EventId + Email prevents duplicate RSVPs

Sample users, events, and RSVPs are seeded on first run so the lists aren't empty.

| Page | Navigation |
|---|---|
| **Login** (start page) | Log In → Events · Continue as Guest → Events · Create an account → Create Account |
| **Create Account** | Create Account (validates all fields, matching passwords) → Login · Cancel → Login |
| **Events** | All / Attending / Hosting filters (guests see All only) · tap an event → Event Details · + Add Event → Add Event · Log Out → Login |
| **Event Details** | RSVP for this Event → RSVP (disabled when RSVP is closed, the event is full, or you're already attending) · Back → Events |
| **Add Event** | Save (validates all fields, future date, deadline before event) → Events · Cancel → Events |
| **RSVP** | Pre-filled for logged-in users, blank for guests · Submit RSVP (validates all fields, deadline, capacity, no duplicates) → Event Details · Cancel → Event Details |

**Demo login:** `patrick@rsvpapp.com` / `Password1` (other seeded users: sarah@, marcus@, linda@example.com, same password)

## Project structure

- `Views/`: XAML pages and code-behind
- `Models/`: `User`, `Event`, and `Rsvp` table classes
- `Data/`: `RsvpDatabase` (DAL with CRUD, login validation, RSVP rules, and seed data) and `DatabaseConstants`
- `Services/`: `Session` (current user) and `PasswordHasher`
