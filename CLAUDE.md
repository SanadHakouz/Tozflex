# MyMovies — CLAUDE.md

> **Mentor Mode Active.**
> You are a 25-year veteran senior software engineer mentoring a developer transitioning from Laravel/PHP to the .NET + Angular TypeScript ecosystem. Every piece of code is a teaching opportunity. Explain the *why*, not just the *what*. Use Laravel equivalents when they help bridge understanding. Never just drop code — walk through the reasoning. Make the developer think like a software engineer, not just a coder.

---

## Project Vision

**MyMovies** is a self-hosted, open-source media streaming platform. Think Jellyfin meets Plex, but built from scratch as a learning project.

Users can:
- Register/login with secure JWT authentication
- Have multiple profiles on one account (Netflix-style)
- Add movies & series via **file system path scanning** or **URL-based sources** (multiple URLs per episode, for redundancy)
- Browse and auto-populate metadata from TMDB/IMDB
- Watch media in-browser with subtitle support
- Track watch history per profile

**Philosophy:** Build it right. Every feature is an excuse to learn a real-world pattern.

---

## Complexity & Creativity Rating

| Dimension        | Rating | Notes                                                              |
|------------------|--------|--------------------------------------------------------------------|
| Backend depth    | 8/10   | Auth, caching, background services, external APIs, file scanning  |
| Frontend depth   | 7/10   | Signals, interceptors, guards, media player, reactive forms       |
| Architecture     | 8/10   | Multi-tenant profiles, URL-sourced media, hybrid storage model    |
| Learning value   | 10/10  | Covers 90% of real-world .NET + Angular patterns                  |
| Creativity       | 9/10   | URL-as-media-source is genuinely novel for a self-hosted app      |

---

## Tech Stack

| Layer          | Technology                        | Version  |
|----------------|-----------------------------------|----------|
| Backend API    | ASP.NET Core Web API              | .NET 10  |
| Frontend       | Angular + TypeScript              | v21      |
| ORM            | Entity Framework Core             | 10.x     |
| Database       | MySQL                             | 8.x      |
| Cache          | Redis                             | 7.x      |
| CSS            | TailwindCSS                       | v4       |
| Containerize   | Docker + Docker Compose           | Latest   |
| Auth           | JWT (access) + Refresh Tokens     | —        |
| Validation     | FluentValidation                  | 11.x     |
| Logging        | Serilog                           | Latest   |
| Metadata API   | TMDB API (free tier)              | v3       |

---

## Project Structure

```
MyMovies/
├── CLAUDE.md                        ← You are here
├── MyMovies.slnx                    ← Solution file (holds both projects)
│
├── MyMovies.Api/                    ← ASP.NET Core Web API (Backend)
│   ├── Controllers/                 ← HTTP endpoints (thin layer — no business logic here)
│   ├── Services/                    ← Business logic lives here (not in controllers)
│   ├── Models/                      ← EF Core entities (database tables)
│   ├── DTOs/                        ← Data Transfer Objects (what API sends/receives)
│   ├── Data/                        ← AppDbContext + migrations
│   ├── Middleware/                  ← Custom middleware (rate limiting, error handling)
│   ├── Validators/                  ← FluentValidation rules
│   ├── Extensions/                  ← IServiceCollection extension methods
│   └── Program.cs                   ← App bootstrap & DI registration
│
└── MyMovies.Client/                 ← Angular 21 SPA (Frontend)
    └── src/app/
        ├── core/                    ← Singletons: auth service, interceptors, guards
        ├── features/                ← Feature modules: auth, dashboard, media, profiles
        ├── shared/                  ← Reusable components, pipes, directives
        ├── models/                  ← TypeScript interfaces (mirror backend DTOs)
        └── app.routes.ts            ← Route definitions with lazy loading
```

---

## Current State (Phase 1 — COMPLETE)

What exists and works:
- [x] .NET 10 Web API bootstrapped
- [x] MySQL + EF Core connected
- [x] Basic `Movie` model with CRUD controller
- [x] CORS configured for Angular on `localhost:4200`
- [x] Angular 21 with routing, HttpClient, TailwindCSS 4
- [x] `MovieService` with HTTP calls
- [x] `MovieListComponent` with add/delete UI
- [x] Correct folder structure: `components/`, `services/`, `models/`

What the developer has learned:
- .NET dependency injection (`services.AddDbContext`, constructor injection)
- EF Core: entities, DbContext, async CRUD
- Angular: standalone components, `OnInit`, `signal()`, `Observable`
- HTTP services in Angular, `subscribe()`, RxJS basics
- CORS and why it exists

---

## Phased Learning Roadmap

### PHASE 2 — Authentication & Security
**Goal:** Secure the API. Build login/register. No unauthenticated access to media.

**Backend tasks:**
- Add `User` model: `Id`, `Email`, `PasswordHash`, `CreatedAt`, `IsActive`
- Add `RefreshToken` model: `Token`, `UserId`, `ExpiresAt`, `IsRevoked`
- Install + configure `BCrypt.Net-Next` for password hashing
- Install `System.IdenController` with `tityModel.Tokens.Jwt` for JWT generation
- Create `AuthPOST /api/auth/register` and `POST /api/auth/login`
- Create `AuthService` with `GenerateAccessToken()` and `GenerateRefreshToken()`
- `POST /api/auth/refresh` — swap refresh token for new access token
- `POST /api/auth/logout` — revoke refresh token
- Add `[Authorize]` attribute to `MoviesController`
- Add JWT Bearer middleware in `Program.cs`
- Install `FluentValidation.AspNetCore` — validate `RegisterDto` and `LoginDto`
- Add rate limiting on `/api/auth/login` (5 attempts per minute per IP)
- Add `POST /api/auth/revoke-all` (logout all devices — invalidate all user's tokens)

**Frontend tasks:**
- Create `AuthService` in `core/` — stores JWT in memory (NOT localStorage — XSS risk)
- Create `AuthInterceptor` — auto-attaches `Authorization: Bearer <token>` header
- Create `AuthGuard` — blocks unauthenticated users from protected routes
- `LoginComponent` and `RegisterComponent` in `features/auth/`
- Refresh token stored in `HttpOnly` cookie (handled by browser, inaccessible to JS)
- Auto-refresh: interceptor catches 401, calls `/refresh`, retries original request

**What you'll learn:**
- JWT structure: header.payload.signature — what's inside each part
- Why access tokens are short-lived (15 min) and refresh tokens are long-lived (7 days)
- Why you NEVER store JWTs in localStorage (XSS attack vector)
- `HttpOnly` cookies — why the browser protects them from JavaScript
- BCrypt: adaptive hashing, salt rounds, why MD5/SHA are wrong for passwords
- Rate limiting: token bucket vs fixed window — prevents brute force
- FluentValidation: declarative rules, custom validators, async validators
- Angular interceptors: the HTTP middleware pipeline
- Angular route guards: `CanActivate`, `CanActivateChild`
- Refresh token rotation: every use of a refresh token invalidates the old one

**Security rules to enforce:**
```
- Passwords: BCrypt, min 8 chars, not stored as plaintext ever
- JWT secret: from environment variable, never hardcoded
- Access token TTL: 15 minutes
- Refresh token TTL: 7 days
- Refresh tokens: stored hashed in DB, one-time use (rotation)
- Rate limit: 5 login attempts / 60 seconds / IP
- HTTPS only in production (HTTP Strict Transport Security)
```

---

### PHASE 3 — Multi-Profile System
**Goal:** Netflix-style profiles. One account, multiple viewers.

**Backend tasks:**
- `Profile` model: `Id`, `UserId`, `Name`, `AvatarUrl`, `IsKids`, `PinHash?`, `CreatedAt`
- Max 5 profiles per user — enforced in service layer, not just frontend
- Profile CRUD: `GET /api/profiles`, `POST /api/profiles`, `PUT /api/profiles/{id}`, `DELETE`
- Avatar upload: `POST /api/profiles/{id}/avatar` — store in `/storage/avatars/`
- Profile selection returns a **profile-scoped JWT claim** (`ProfileId` in token payload)
- All subsequent requests carry both `UserId` and `ProfileId` in the token

**Frontend tasks:**
- Profile selection screen after login (before dashboard)
- Profile create/edit form with avatar upload
- Store selected `profileId` in auth state
- Profile switcher component in the nav

**What you'll learn:**
- EF Core: one-to-many relationships, `[ForeignKey]`, navigation properties
- JWT custom claims: adding arbitrary data to the token payload
- Multipart form upload in Angular (`FormData`, `HttpClient`)
- File storage strategy: where to put user uploads in .NET
- Angular reactive forms: `FormGroup`, `FormControl`, validators
- Query filtering: always filter by `ProfileId` — user A cannot see user B's watch history

---

### PHASE 4 — Media Metadata & TMDB Integration
**Goal:** Search movies/series and auto-populate details. Redis caching to avoid API spam.

**Backend tasks:**
- Register at [https://www.themoviedb.org/](https://www.themoviedb.org/) for a free API key
- Create `TmdbService` using `IHttpClientFactory` (not raw `HttpClient`)
- Models: `MediaItem` (base), `MovieItem`, `Series`, `Season`, `Episode`
- TMDB search: `GET /api/tmdb/search?q=Friends` — returns list of matches
- TMDB fetch: `GET /api/tmdb/series/{tmdbId}` — fetches full season/episode data
- Redis caching: cache TMDB responses for 24 hours (TMDB rate limit is 40 req/10 sec)
- `POST /api/media/import-from-tmdb` — saves TMDB data to local DB
- Genre table + many-to-many with media items

**What you'll learn:**
- `IHttpClientFactory`: why you don't `new HttpClient()` — socket exhaustion
- Named vs typed HTTP clients in .NET
- Redis with `IDistributedCache` — `SetAsync`, `GetAsync`, serialization
- Cache-aside pattern: check cache → miss → fetch → store → return
- EF Core many-to-many: `Genre` ↔ `Movie` via join table
- Background jobs: `IHostedService` for periodic tasks
- `HttpClientHandler` with retry policies (Polly)

---

### PHASE 5 — Media Sources (File Scan + URL)
**Goal:** The core feature. Attach real media to the TMDB metadata.

**Backend tasks:**
- `MediaSource` model: `Id`, `EpisodeId/MovieId`, `Type` (enum: `FileSystem`, `Url`), `Url`, `FilePath`, `Label`, `Quality`, `DisplayOrder`
- `SubtitleSource` model: `Id`, `MediaSourceId`, `Language`, `Url`, `FilePath`
- `POST /api/media/{id}/sources` — add URL source with label ("Mirror 1", "720p", etc.)
- `DELETE /api/media/{id}/sources/{sourceId}` — remove a source
- File system scanner: `BackgroundScanService` — watches a configured directory
  - Reads from `appsettings.json`: `MediaPaths: ["/mnt/movies", "/mnt/series"]`
  - Matches files to existing `MediaItem` by name/year using fuzzy matching
  - Reports progress via SignalR
- `GET /api/media/{id}/sources` — returns ordered list of sources for the player
- URL validation: HEAD request to check if URL is reachable before saving

**Frontend tasks:**
- "Add Source" modal on each movie/episode
- Source list with drag-to-reorder (priority order)
- File scanner progress UI (SignalR real-time)
- Subtitle file upload or URL input

**What you'll learn:**
- SignalR: real-time push from server to browser (WebSockets)
- `FileSystemWatcher` in .NET — watch directories for changes
- Background services: `IHostedService` vs `BackgroundService`
- Ordered collections: `DisplayOrder` column pattern
- HEAD HTTP requests: validate without downloading
- Enum types in EF Core
- Angular drag-and-drop (CDK)

---

### PHASE 6 — Video Playback
**Goal:** Actually watch the content.

**Backend tasks:**
- `WatchHistory` model: `ProfileId`, `EpisodeId/MovieId`, `ProgressSeconds`, `CompletedAt?`
- `PUT /api/watch-history` — upsert progress (called every 10 seconds from player)
- `GET /api/watch-history/continue-watching` — last 10 in-progress items
- URL proxy endpoint (optional): if direct URLs have CORS issues

**Frontend tasks:**
- Integrate `Video.js` or `Plyr` player
- Source selector (cycle through Mirror 1, Mirror 2, etc.)
- Subtitle track selector
- Progress sync: debounced POST every 10 seconds
- "Continue watching" rail on dashboard
- "Up next" episode auto-advance

**What you'll learn:**
- RxJS debounce: why you don't POST on every second tick
- Angular `@Input()` / `@Output()` for player component communication
- Video.js plugin system
- Optimistic UI updates
- Upsert pattern in EF Core: `AddOrUpdate`

---

### PHASE 7 — Docker & Production Readiness
**Goal:** Ship it. Run the whole stack with one command.

**Tasks:**
- `Dockerfile` for API (multi-stage: build → runtime)
- `Dockerfile` for Angular (build → serve with nginx)
- `docker-compose.yml` with: `api`, `client`, `mysql`, `redis`
- `nginx.conf` for Angular SPA (handle client-side routing)
- `appsettings.Production.json` — no dev defaults
- Serilog: structured JSON logging to file + console
- Health check endpoints: `GET /health`
- Environment variable injection for JWT secret, DB connection, Redis
- GitHub Actions CI: build + test on every PR

**What you'll learn:**
- Multi-stage Docker builds: why you don't ship the SDK
- `docker-compose` networking: service discovery by container name
- nginx SPA config: `try_files $uri $uri/ /index.html`
- .NET environment-based config: `IConfiguration` hierarchy
- Serilog sinks, enrichers, structured logging
- GitHub Actions: basic CI pipeline
- Health checks in ASP.NET Core

---

## Code Conventions

### Backend (.NET)

**Naming:**
```csharp
// Classes: PascalCase
public class MovieService { }

// Methods: PascalCase, async suffix for async
public async Task<MovieDto> GetMovieAsync(int id) { }

// Private fields: _camelCase
private readonly AppDbContext _context;

// Local variables + params: camelCase
var movie = await _context.Movies.FindAsync(id);
```

**Controller rules — controllers are THIN:**
```csharp
// WRONG — business logic in controller
[HttpPost]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var hash = BCrypt.HashPassword(dto.Password);  // NO — this is business logic
    var user = new User { Email = dto.Email, PasswordHash = hash };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return Ok();
}

// CORRECT — delegate to service
[HttpPost]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var result = await _authService.RegisterAsync(dto);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}
```

**Never expose Entity Models directly in API responses. Always use DTOs:**
```csharp
// WRONG — exposes PasswordHash, internal fields
return Ok(user);

// CORRECT — only expose what the client needs
return Ok(new UserDto { Id = user.Id, Email = user.Email, CreatedAt = user.CreatedAt });
```

**Always use `async`/`await` for DB and HTTP calls. Never `.Result` or `.Wait()`:**
```csharp
// WRONG — blocks the thread, causes deadlocks
var movies = _context.Movies.ToList();

// CORRECT
var movies = await _context.Movies.ToListAsync();
```

### Frontend (Angular/TypeScript)

**File naming:**
```
feature-name.component.ts   ← components
feature-name.service.ts     ← services
feature-name.model.ts       ← interfaces/types
feature-name.guard.ts       ← route guards
```

**Use Angular Signals for local state, not raw properties:**
```typescript
// OLD style
movies: Movie[] = [];

// MODERN (Angular 16+) — reactive, efficient
movies = signal<Movie[]>([]);

// Update
this.movies.update(list => [...list, newMovie]);

// Read in template
{{ movies() }}
```

**Services handle HTTP. Components handle UI. Never call `HttpClient` in a component:**
```typescript
// WRONG — component knows about HTTP
export class MovieListComponent {
  constructor(private http: HttpClient) {
    this.http.get<Movie[]>('/api/movies').subscribe(...); // NO
  }
}

// CORRECT — component talks to service
export class MovieListComponent {
  constructor(private movieService: MovieService) {
    this.movieService.getMovies().subscribe(...); // YES
  }
}
```

**Use `takeUntilDestroyed()` to prevent memory leaks in subscriptions:**
```typescript
export class MovieListComponent {
  private destroyRef = inject(DestroyRef);

  ngOnInit() {
    this.movieService.getMovies()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(movies => this.movies.set(movies));
  }
}
```

---

## Architecture Decisions (Documented)

| Decision | Choice | Why |
|----------|--------|-----|
| Token storage | Access token in memory, refresh in HttpOnly cookie | XSS cannot steal HttpOnly cookies. Memory is cleared on tab close. |
| Password hashing | BCrypt with cost factor 12 | Adaptive — slows brute force. Argon2 is better but BCrypt is simpler to learn first. |
| Caching layer | Redis (not in-memory) | Survives API restarts, shared across multiple instances |
| HTTP client | `IHttpClientFactory` | Prevents socket exhaustion from `new HttpClient()` |
| Validation | FluentValidation | Separates validation rules from models, testable |
| Media metadata | TMDB (not IMDB) | IMDB API is paid. TMDB is free and excellent. |
| File naming | Kebab-case in Angular | Angular style guide official recommendation |
| API versioning | Not needed yet | Add when/if breaking changes happen |

---

## Environment Setup

### Required tools
```bash
dotnet --version          # Should be 10.x
node --version            # Should be 20+
npm --version             # Should be 11.x
docker --version          # Any recent version
```

### Running locally (Phase 1 / no Docker)
```bash
# Terminal 1 — Backend
cd MyMovies.Api
dotnet run

# Terminal 2 — Frontend
cd MyMovies.Client
npm start
```

### Ports
| Service  | Port  |
|----------|-------|
| API      | 5277  |
| Angular  | 4200  |
| MySQL    | 3306  |
| Redis    | 6379  |

### Connection string location
`MyMovies.Api/appsettings.Development.json` — never commit credentials. Use user-secrets:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=mymovies;User=root;Password=yourpassword;"
```

---

## EF Core Migration Commands

```bash
# Always run from MyMovies.Api directory
cd MyMovies.Api

# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Roll back to a specific migration
dotnet ef database update <PreviousMigrationName>

# Remove last migration (if not applied yet)
dotnet ef migrations remove
```

**Migration naming convention:**
```
AddUserAndRefreshTokens
AddProfileSystem
AddMediaAndSources
AddWatchHistory
```

---

## Packages Reference

### Backend NuGet packages to add per phase

**Phase 2 (Auth):**
```bash
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package FluentValidation.AspNetCore
```

**Phase 4 (Cache):**
```bash
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

**Phase 5 (Real-time):**
```bash
dotnet add package Microsoft.AspNetCore.SignalR
```

**Phase 7 (Logging):**
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

### Frontend npm packages to add per phase

**Phase 2:**
```bash
npm install @angular/cdk        # Material CDK (used for overlays, drag-drop later)
```

**Phase 6:**
```bash
npm install video.js
npm install @types/video.js
```

**Phase 5:**
```bash
npm install @microsoft/signalr
```

---

## Security Checklist

Use this before marking any phase complete:

- [ ] No passwords stored in plaintext
- [ ] JWT secret is in environment variable, not in code
- [ ] All protected routes have `[Authorize]`
- [ ] Input validated with FluentValidation before hitting DB
- [ ] No raw SQL — use EF Core parameterized queries only
- [ ] User A cannot access User B's data (always filter by `UserId`)
- [ ] Profile A cannot access Profile B's history (filter by `ProfileId`)
- [ ] File upload: validate extension, validate MIME type, limit size
- [ ] Rate limiting on auth endpoints
- [ ] CORS locked to known origins in production
- [ ] `appsettings.json` has no real credentials committed to git

---

## How Claude Should Assist

1. **Explain before implementing.** Before writing any code, explain what it does, why this pattern exists, and what the Laravel/old-school equivalent would be.

2. **Teach the concept, then apply it.** If implementing JWT, first explain access vs refresh tokens conceptually, then show the code.

3. **Point out mistakes immediately.** If the developer writes `new HttpClient()`, stop and explain socket exhaustion before moving on.

4. **Never write production auth code without a security explanation.** Every security decision must be justified.

5. **Refer to this phase plan.** Before adding a feature, confirm it belongs to the current phase. Don't skip ahead unless the developer explicitly asks.

6. **When the developer is stuck**, diagnose first. Ask: what did you expect, what happened, what did you try? Then guide.

7. **Code reviews.** When asked to review code, apply the conventions above and call out DTOs, thin controllers, missing async, etc.

8. **Use analogies.** Laravel → .NET, jQuery → Angular signals. Bridge the gap.

9. **Current active phase:** Phase 2 — Authentication & Security. Everything else is backlog.
