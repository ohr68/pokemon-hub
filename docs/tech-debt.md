# Technical debt

Known gaps we chose to leave for later. Each entry says what is missing, why it matters
and the direction for fixing it.

## Role granting

**What's missing:** there is no way to give or take a role (Member, Administrator)
through the API or the app. Making someone an administrator means editing
`users.user_roles` in the database by hand.

**Why it matters:** admin-only features (`species:sync`, `pokemon:grant`) can't be
handed out safely, and role changes leave no trace.

**Direction:**
- Endpoints in the Users module to list roles and grant/revoke them, protected by a new
  `users:roles` permission that only administrators have.
- A domain event when roles change, so it is auditable.
- A way to create the first administrator (seeded from configuration on startup, or a
  one-off CLI command), since nobody can grant roles before an admin exists.
- Permissions are read per request from the database today; if they get cached later,
  invalidate the cache on role changes.

## Trainers for accounts created before the Pokédex module

**What's missing:** users registered before the Pokédex module existed never got a
trainer, because their `UserRegistered` events were dropped by the old outbox bug.
They see "Preparing your trainer card..." forever.

**Direction:** a one-off backfill that creates a trainer for every user without one,
safe to run more than once.
