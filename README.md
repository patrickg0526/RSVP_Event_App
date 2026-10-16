# RSVP Event App

A .NET MAUI mobile app that lets people create an account, log in (or browse as a guest), view events, see event details, add events, and RSVP.

## Current milestone: UI and page navigation

All pages and screen-to-screen navigation are in place. Data is hard-coded for now. Saving users, events, and RSVPs comes in later milestones.

| Page | Navigation |
|---|---|
| **Login** (start page) | Log In → Events · Continue as Guest → Events · Create an account → Create Account |
| **Create Account** | Create Account (validates all fields, matching passwords) → Login · Cancel → Login |
| **Events** | All / Attending / Hosting filters (guests see All only) · tap an event → Event Details · + Add Event → Add Event · Log Out → Login |
| **Event Details** | RSVP for this Event → RSVP (disabled when RSVP is closed, the event is full, or you're already attending) · Back → Events |
| **Add Event** | Save (validates all fields, future date, deadline before event) → Events · Cancel → Events |
| **RSVP** | Pre-filled for logged-in users, blank for guests · Submit RSVP (validates all fields) → Event Details · Cancel → Event Details |

**Demo login:** `patrick@rsvpapp.com` / `Password1`

## Project structure

- `Views/`: XAML pages and code-behind
- `Models/`: `User` and `Event`
- `Services/AppData.cs`: hard-coded sample data and the current session
