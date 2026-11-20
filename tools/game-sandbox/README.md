# Game sandbox

A one-page stand-in for a game, to try the game API without building a game. It signs a
player in with a registered game's Keycloak client ("Sign in with PokeHub") and calls
`/api/game/...`: the player's save, catching, and releasing.

It is also the smallest working example of what a browser game needs to do.

## Run it

1. Register the sandbox as a game (needs an administrator's token; see `register.http`
   or the Swagger page). Use slug `sandbox`, so its client id is `game-sandbox`, and
   `http://localhost:5180` as URL and origin.
2. Serve this folder on port 5180:

   ```bash
   python3 -m http.server 5180 --directory tools/game-sandbox
   ```

3. Open http://localhost:5180 and sign in with a PokeHub account.

Settings can be changed with query parameters: `?clientId=game-other&api=http://localhost:5100/api/`.
