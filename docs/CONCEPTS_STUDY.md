# Core Concepts Study Guide
### OOP · Sessions & Tokens · .NET Structure · Git Strategy

> This document focuses on concepts, not features. Master these and every framework
> becomes just syntax. These are the ideas that don't change between languages.

---

## PART 1 — Object-Oriented Programming (OOP)

### What is OOP? (ELI5)

Programming is about organizing code so humans can understand and change it.
OOP is one philosophy for doing that: **model your code around real-world things**.

A "thing" in OOP is called an **object**. Objects have:
- **Data** (what they know about themselves) — called *properties* or *fields*
- **Behavior** (what they can do) — called *methods*

A **class** is the blueprint. An **object** is the thing built from that blueprint.

```
Blueprint (Class)          Built thing (Object / Instance)
─────────────────          ──────────────────────────────
Movie (the concept)   →    new Movie { Title = "Inception", Year = 2010 }
Car (the concept)     →    new Car { Brand = "BMW", Color = "Black" }
User (the concept)    →    new User { Email = "you@gmail.com" }
```

**In your code right now:**
```csharp
// This IS the class — the blueprint
public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
}

// This is an OBJECT — built from the blueprint
var movie = new Movie();          // Create an instance
movie.Title = "Inception";       // Set a property
movie.Genre = "Sci-Fi";

// Or with object initializer syntax (same thing, shorter):
var movie = new Movie { Title = "Inception", Genre = "Sci-Fi" };
```

**PHP/Laravel equivalent:**
```php
// PHP class
class Movie extends Model {
    public $title;
    public $genre;
}

// PHP object
$movie = new Movie();
$movie->title = "Inception";
```

The concept is identical. Only the syntax differs.

---

### 1.1 — Encapsulation — "Hide the internals, expose what's needed"

**ELI5:** A TV remote. You press Play and the movie starts.
You don't need to know about infrared signals, capacitors, or circuit boards.
The remote *hides* the complexity and *exposes* a simple button.

Encapsulation = hiding internal data and providing controlled access via methods.

**Access Modifiers — The Gates:**

| Keyword | Who can access it |
|---------|------------------|
| `public` | Anyone, from anywhere |
| `private` | Only this class itself |
| `protected` | This class + classes that inherit from it |
| `internal` | Only within the same project (assembly) |

```csharp
public class BankAccount
{
    // PRIVATE — nobody outside can touch this directly
    private decimal _balance = 0;

    // PUBLIC — controlled way to deposit (add validation)
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit must be positive");

        _balance += amount;  // Only way to change balance — goes through our rule
    }

    // PUBLIC — controlled way to read balance (read-only from outside)
    public decimal GetBalance()
    {
        return _balance;
    }
}

// Usage:
var account = new BankAccount();
account.Deposit(100);             // OK — goes through the rule
account._balance = 99999;         // ERROR — private, can't touch it
```

Without encapsulation:
```csharp
// BAD — no encapsulation
public class BankAccount
{
    public decimal Balance = 0;   // Anyone can do: account.Balance = -99999
}
```

**In your codebase:** The `private readonly AppDbContext _context` in your controller
is encapsulation. Nobody outside the controller touches the database directly.

**C# Properties are encapsulation built in:**
```csharp
// A property is a public getter + setter wrapping a private field:
public class Movie
{
    private int _year;

    public int Year
    {
        get { return _year; }
        set
        {
            if (value < 1888 || value > 2100)
                throw new ArgumentOutOfRangeException("Invalid year");
            _year = value;
        }
    }
}

// Auto-property (shorthand for when you don't need custom logic):
public int Year { get; set; }

// Read-only property (can set in constructor, never again):
public int Year { get; init; }
```

---

### 1.2 — Inheritance — "Children get everything the parent has"

**ELI5:** You inherit your parents' traits — eyes, nose, maybe height.
You also have your own traits they don't have.
A child class inherits everything from the parent and can add its own stuff.

```csharp
// PARENT CLASS (base class)
public class MediaItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }

    public string GetDisplayName()
    {
        return $"{Title} ({Year})";
    }
}

// CHILD CLASS — gets EVERYTHING from MediaItem, plus its own stuff
public class Movie : MediaItem           // ← the colon means "inherits from"
{
    public int RuntimeMinutes { get; set; }  // Movie-specific
    public string Director { get; set; } = string.Empty;
}

// ANOTHER CHILD CLASS
public class Series : MediaItem
{
    public int TotalSeasons { get; set; }   // Series-specific
    public bool IsCompleted { get; set; }
}
```

Usage:
```csharp
var movie = new Movie();
movie.Title = "Inception";        // Inherited from MediaItem ✅
movie.Year = 2010;                // Inherited from MediaItem ✅
movie.RuntimeMinutes = 148;       // Movie's own property ✅
movie.TotalSeasons = 3;           // ERROR — that's on Series, not Movie ❌

var name = movie.GetDisplayName(); // Inherited method → "Inception (2010)" ✅
```

**This is exactly what EF Core uses in your future models:**
```
MediaItem (id, title, year, posterUrl)
    ├── Movie (runtimeMinutes, director)
    └── Series (totalSeasons, seasons[])
            └── Season (seasonNumber, episodes[])
                    └── Episode (episodeNumber, duration)
```

**The `base` keyword — call parent's code:**
```csharp
public class Series : MediaItem
{
    public int TotalSeasons { get; set; }

    // Override parent method and extend it:
    public override string GetDisplayName()
    {
        return base.GetDisplayName() + $" — {TotalSeasons} Seasons";
        // base.GetDisplayName() → "Friends (1994)"
        // Result → "Friends (1994) — 10 Seasons"
    }
}
```

**Rules:**
- A class can only inherit from ONE class in C# (no multiple inheritance)
- But a class can implement MANY interfaces (see below)
- `sealed` keyword prevents a class from being inherited

---

### 1.3 — Polymorphism — "Same action, different behavior"

**ELI5:** "Play" means different things to different things.
- Tell a speaker to Play → it plays audio
- Tell a video player to Play → it plays video
- Tell a game to Play → it starts a game

Same word. Same call. Different behavior depending on what you're calling it on.
**Poly** = many, **morph** = form. Many forms of the same thing.

**Two types in C#:**

#### Type 1 — Method Overriding (Runtime Polymorphism)

```csharp
public class MediaItem
{
    public virtual string GetInfo()   // virtual = "children CAN override this"
    {
        return $"Media: {Title}";
    }
}

public class Movie : MediaItem
{
    public override string GetInfo()  // override = "I'm replacing the parent version"
    {
        return $"Movie: {Title} ({RuntimeMinutes} min)";
    }
}

public class Series : MediaItem
{
    public override string GetInfo()
    {
        return $"Series: {Title} — {TotalSeasons} seasons";
    }
}
```

The magic happens here:
```csharp
// A list that holds anything that IS a MediaItem
List<MediaItem> library = new List<MediaItem>();
library.Add(new Movie { Title = "Inception", RuntimeMinutes = 148 });
library.Add(new Series { Title = "Friends", TotalSeasons = 10 });
library.Add(new Movie { Title = "Interstellar", RuntimeMinutes = 169 });

// Same method call, different output based on actual type:
foreach (var item in library)
{
    Console.WriteLine(item.GetInfo());
}
// Output:
// Movie: Inception (148 min)
// Series: Friends — 10 seasons
// Movie: Interstellar (169 min)
```

.NET figures out at runtime which version of `GetInfo()` to call based on the
actual type of the object. You write one loop, it works for everything.

#### Type 2 — Method Overloading (Compile-time Polymorphism)

Same method name, different parameters:
```csharp
public class SearchService
{
    // Search by title (string)
    public List<Movie> Search(string title) { ... }

    // Search by year (int)
    public List<Movie> Search(int year) { ... }

    // Search by title AND year
    public List<Movie> Search(string title, int year) { ... }
}

// Usage — C# picks the right one based on what you pass:
service.Search("Inception");          // calls first version
service.Search(2010);                 // calls second version
service.Search("Inception", 2010);    // calls third version
```

---

### 1.4 — Abstraction — "Show what matters, hide what doesn't"

**ELI5:** When you drive a car, you use the steering wheel, pedals, and dashboard.
You don't see the engine, transmission, or fuel injection system.
The car *abstracts* the complexity. You interact with a simplified interface.

In code: define WHAT something does, not HOW it does it.

**Two tools for abstraction in C#:**

#### Abstract Classes — "Partial blueprint, must be completed"

```csharp
// Abstract class — cannot be instantiated directly
// Forces children to implement certain methods
public abstract class MediaItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Concrete method — has implementation, children inherit it
    public string GetDisplayName() => $"{Title} ({GetType().Name})";

    // Abstract method — NO implementation, children MUST override it
    public abstract int GetDurationInMinutes();
    public abstract string GetSummary();
}

public class Movie : MediaItem
{
    public int RuntimeMinutes { get; set; }

    // MUST implement all abstract methods or compiler screams
    public override int GetDurationInMinutes() => RuntimeMinutes;
    public override string GetSummary() => $"A {RuntimeMinutes}-minute film";
}

public class Series : MediaItem
{
    public int EpisodesCount { get; set; }

    public override int GetDurationInMinutes() => EpisodesCount * 45; // estimate
    public override string GetSummary() => $"A series with {EpisodesCount} episodes";
}

// This is ILLEGAL — you can't create an abstract class directly:
var item = new MediaItem();  // ❌ ERROR: Cannot create instance of abstract class
var movie = new Movie();     // ✅ Fine — Movie is concrete
```

#### Interfaces — "A contract. No code. Just promises."

```csharp
// Interface — pure contract, zero implementation
// Starts with capital I by convention
public interface IStreamable
{
    string GetStreamUrl();           // Must be implemented
    bool IsAvailable();              // Must be implemented
    int GetBitrateKbps();           // Must be implemented
}

public interface ICacheable
{
    string GetCacheKey();
    TimeSpan GetCacheDuration();
}

// A class can implement MULTIPLE interfaces (unlike inheritance — only 1 parent)
public class Episode : MediaItem, IStreamable, ICacheable
{
    public string StreamUrl { get; set; } = string.Empty;

    // IStreamable implementation:
    public string GetStreamUrl() => StreamUrl;
    public bool IsAvailable() => !string.IsNullOrEmpty(StreamUrl);
    public int GetBitrateKbps() => 4500;

    // ICacheable implementation:
    public string GetCacheKey() => $"episode_{Id}";
    public TimeSpan GetCacheDuration() => TimeSpan.FromHours(24);

    // MediaItem abstract implementation:
    public override int GetDurationInMinutes() => 45;
    public override string GetSummary() => $"Episode {Id}";
}
```

**Abstract Class vs Interface — when to use which:**

| | Abstract Class | Interface |
|--|---------------|-----------|
| Can have implementation? | Yes (some methods) | No (C# 8+ has default, but avoid it) |
| Can have fields? | Yes | No |
| Multiple allowed? | No (single inheritance) | Yes (many interfaces) |
| Use when | Objects share code + are related | Objects just share a contract |
| Example | `MediaItem` → `Movie`, `Series` | `IStreamable`, `ICacheable`, `IDisposable` |

**In your project later:**
```csharp
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<string> GenerateAccessTokenAsync(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(int userId);
}

// Now you can swap implementations easily:
public class AuthService : IAuthService { ... }          // Real implementation
public class MockAuthService : IAuthService { ... }     // Fake for tests
```

The controller depends on `IAuthService`, not `AuthService`.
Tests inject `MockAuthService`. Production injects `AuthService`.
Controller doesn't know or care. That's abstraction + DI together.

---

### 1.5 — Constructors — "The setup that runs when you create an object"

```csharp
public class User
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Parameterless constructor
    public User()
    {
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    // Parameterized constructor
    public User(string email, string passwordHash)
    {
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}

// Usage:
var user1 = new User();                            // parameterless
var user2 = new User("a@b.com", "hashedpass");    // parameterized

// Object initializer (works with parameterless):
var user3 = new User { Email = "a@b.com" };
// BUT: PasswordHash has private set, so you CAN'T do this:
var user4 = new User { PasswordHash = "..." };    // ❌ ERROR
```

**Constructor chaining (`: this(...)`):**
```csharp
public class Movie
{
    public string Title { get; set; }
    public int Year { get; set; }
    public decimal Rating { get; set; }

    public Movie() : this("Unknown", DateTime.Now.Year)  // calls second constructor
    {
    }

    public Movie(string title, int year)
    {
        Title = title;
        Year = year;
        Rating = 0;
    }
}
```

---

### 1.6 — Static vs Instance Members

```csharp
public class Counter
{
    // INSTANCE field — each object has its own copy
    public int Count { get; private set; } = 0;

    // STATIC field — shared across ALL objects of this class
    public static int TotalCreated { get; private set; } = 0;

    public Counter()
    {
        TotalCreated++;  // Increments the shared counter every time any Counter is created
    }

    // INSTANCE method — operates on this specific object
    public void Increment() => Count++;

    // STATIC method — doesn't need an object, can't use 'this'
    public static void ResetTotal() => TotalCreated = 0;
}

var c1 = new Counter();
var c2 = new Counter();
c1.Increment();
c1.Increment();
c2.Increment();

Console.WriteLine(c1.Count);         // 2 — c1's own count
Console.WriteLine(c2.Count);         // 1 — c2's own count
Console.WriteLine(Counter.TotalCreated); // 2 — shared, called on the CLASS not an object
```

**When you see `static` on utility classes:**
```csharp
// Common pattern — utility class with only static methods
public static class PasswordHelper
{
    public static string Hash(string password) => BCrypt.HashPassword(password);
    public static bool Verify(string password, string hash) => BCrypt.Verify(password, hash);
}

// Usage — no new, call directly on the class:
var hash = PasswordHelper.Hash("mypassword");
bool valid = PasswordHelper.Verify("mypassword", hash);
```

---

### 1.7 — Generics — "Type-safe templates"

**ELI5:** A Tupperware container works for soup, pasta, or cereal.
The container doesn't care what's inside — it just holds things.
Generics let you write code that works with any type, but still type-safely.

```csharp
// WITHOUT generics — you'd write this for every type
public class MovieList
{
    private List<Movie> _items = new();
    public void Add(Movie item) { _items.Add(item); }
    public Movie Get(int index) { return _items[index]; }
}

public class SeriesList { ... }  // Same code, different type. Ugly.

// WITH generics — write once, use with any type
public class Repository<T>
{
    private List<T> _items = new();
    public void Add(T item) { _items.Add(item); }
    public T Get(int index) { return _items[index]; }
}

var movieRepo = new Repository<Movie>();
var seriesRepo = new Repository<Series>();
var userRepo = new Repository<User>();
```

You already use generics everywhere:
```csharp
List<Movie>                   // List of Movies
Task<ActionResult<Movie>>     // Task that returns ActionResult of Movie
DbSet<Movie>                  // EF Core set of Movies
Observable<Movie[]>           // Angular Observable of Movie array
```

The `<T>` is the placeholder. When you write `List<Movie>`, `T` becomes `Movie`.

---

## PART 2 — Sessions vs Tokens (The Authentication Battle)

### 2.1 — The Stateless Problem

**HTTP is stateless.** Every request is independent. The server remembers nothing.

```
Request 1: "Hi, I'm user@gmail.com with password abc123"
Server: "OK, logged in. Here's your movies list."

Request 2: "Give me my profile"
Server: "Who are you? I don't know you."
```

The server forgot. Every request starts from zero.
We need a mechanism to say "remember that I'm logged in" across requests.

Two solutions: **Sessions** (old way) and **Tokens** (modern way).

---

### 2.2 — Sessions (The Old Way — Stateful)

```
┌──────────┐                        ┌──────────────────────────────┐
│ Browser  │                        │          Server              │
│          │  POST /login           │                              │
│          │ ──────────────────────►│ 1. Verify email + password   │
│          │                        │ 2. Create a session in DB:   │
│          │                        │    sessions table:           │
│          │                        │    { id: "abc123",           │
│          │                        │      userId: 42,             │
│          │                        │      expiresAt: tomorrow }   │
│          │◄─────────────────────── │ 3. Set cookie:              │
│          │  Set-Cookie:           │    PHPSESSID=abc123          │
│          │  PHPSESSID=abc123      │                              │
│          │                        │                              │
│          │  GET /profile          │                              │
│          │  Cookie: abc123        │                              │
│          │ ──────────────────────►│ 4. Look up "abc123" in DB   │
│          │                        │ 5. Found: userId=42          │
│          │◄─────────────────────── │ 6. Return profile           │
└──────────┘                        └──────────────────────────────┘
```

**Key point:** The server stores the session. The browser only gets a session ID (a random string like `abc123`). Every request, the server looks up that ID in the database.

**Problems:**
- Every request = a DB lookup to validate the session (slow at scale)
- If you have 3 servers behind a load balancer, server 2 doesn't have server 1's sessions (sticky sessions or shared session store needed)
- Hard to use with mobile apps, APIs, microservices

**Laravel uses sessions by default** — `$request->session()->get('user')`.

---

### 2.3 — Tokens / JWT (The Modern Way — Stateless)

JWT = **JSON Web Token**. A token is a self-contained, signed package of data.

**The idea:** Instead of storing "this session belongs to user 42" on the server,
put "this token belongs to user 42" IN the token itself, and prove it's genuine with a signature.

**A JWT has 3 parts, separated by dots:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9    ← Header (base64)
.eyJ1c2VySWQiOjQyLCJlbWFpbCI6ImFAYi5jb20iLCJleHAiOjE3MDAwMDAwMDB9    ← Payload (base64)
.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c    ← Signature (HMAC-SHA256)
```

**Decoded Payload (the data inside):**
```json
{
  "userId": 42,
  "email": "you@gmail.com",
  "role": "user",
  "exp": 1700000000,
  "iat": 1699999000
}
```

These values are called **Claims** — claims the token makes about the user.

**The Signature is the security:**
```
Signature = HMAC_SHA256(
    base64(header) + "." + base64(payload),
    SECRET_KEY        ← Only the server knows this
)
```

If anyone tampers with the payload (e.g., changes `userId: 42` to `userId: 1`),
the signature no longer matches. The server detects this immediately and rejects it.

**The flow:**
```
┌──────────┐                        ┌──────────────────────────────┐
│ Browser  │                        │          Server              │
│          │  POST /login           │                              │
│          │ ──────────────────────►│ 1. Verify email + password   │
│          │                        │ 2. Create JWT:               │
│          │                        │    payload: { userId: 42 }   │
│          │                        │    sign with SECRET_KEY      │
│          │◄─────────────────────── │ 3. Return JWT string        │
│          │  { accessToken: "..." }│                              │
│          │                        │                              │
│          │  GET /profile          │                              │
│          │  Authorization:        │                              │
│          │  Bearer eyJ...         │                              │
│          │ ──────────────────────►│ 4. Validate JWT signature    │
│          │                        │    (no DB lookup needed!)    │
│          │                        │ 5. Extract userId from JWT   │
│          │◄─────────────────────── │ 6. Return profile           │
└──────────┘                        └──────────────────────────────┘
```

**Key point:** The server stores NOTHING. The JWT is self-validating.
Step 4 is just a cryptographic check — fast, no DB required.

**Advantages over sessions:**
- No DB lookup per request (just crypto verification)
- Works across multiple servers — they all share the same SECRET_KEY
- Works with mobile apps, SPAs, third-party clients
- The token carries user data — no extra DB query for basic info

---

### 2.4 — The Access Token + Refresh Token Pattern

**Problem:** JWTs can't be invalidated. If someone steals your JWT, they have access until it expires. So we make JWTs expire fast (15 minutes). But then users have to re-login every 15 minutes. Bad UX.

**Solution:** Two tokens.

```
ACCESS TOKEN (JWT)               REFRESH TOKEN
─────────────────                ─────────────
Short-lived (15 min)             Long-lived (7 days)
Stored in memory (JS)            Stored in HttpOnly cookie
Used for every API request       Used only to get a new access token
Self-validating (no DB)          Validated against DB
Stateless                        Stateful (stored in DB, can be revoked)
```

**The Dance:**
```
1. Login → Server returns:
   - Access token (15 min) → stored in JS memory
   - Refresh token (7 days) → stored in HttpOnly cookie

2. API call → send Access token in Authorization header
   → Server validates signature → no DB needed → fast

3. Access token expires after 15 min
   → Next API call fails with 401

4. Angular interceptor catches 401:
   → Automatically calls POST /auth/refresh
   → Sends refresh token (browser auto-sends the cookie)
   → Server validates refresh token against DB
   → Server returns new access token (15 min)
   → Retry the original request with new token
   → User never noticed anything

5. Logout → server marks refresh token as revoked in DB
   → Even if someone stole the access token, it expires in max 15 min
   → Refresh token is dead → can't get new access tokens
```

---

### 2.5 — Cookie Types (since we're talking about HttpOnly)

| Cookie Type | JS can read it? | Sent automatically? | Use for |
|-------------|----------------|---------------------|---------|
| Normal cookie | Yes (`document.cookie`) | Yes (same-origin) | Non-sensitive data |
| HttpOnly cookie | **No** | Yes (same-origin) | Refresh tokens, session IDs |
| Secure cookie | Depends on HttpOnly | Yes (HTTPS only) | Anything in production |
| SameSite=Strict | Depends | No (cross-site) | CSRF protection |

**Why HttpOnly for refresh token?**
JavaScript cannot access HttpOnly cookies. Even if an attacker injects malicious JS into your page (XSS attack), they cannot steal the refresh token. The browser just sends it automatically with requests — your JS code never even sees it.

**Why memory (not localStorage) for access token?**
`localStorage` survives browser restarts and is readable by ANY JavaScript on the page.
If a malicious script is injected, it can `localStorage.getItem('accessToken')` and steal it.
Memory (a JS variable) is cleared when the tab closes and cannot be accessed by injected scripts.

---

### 2.6 — Rate Limiting — "Slow down spammers"

Without rate limiting, an attacker can try 1,000,000 passwords per second on `/api/auth/login`.
Rate limiting says: "Max 5 attempts per 60 seconds per IP address."

```csharp
// In Program.cs — Phase 2:
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", config =>
    {
        config.Window = TimeSpan.FromSeconds(60);  // 60-second window
        config.PermitLimit = 5;                    // Max 5 requests per window
        config.QueueLimit = 0;                     // Don't queue — reject immediately
    });
});

// On the endpoint:
[HttpPost("login")]
[EnableRateLimiting("login")]
public async Task<IActionResult> Login(LoginDto dto) { ... }
```

**Response when rate limited:** HTTP 429 Too Many Requests.

---

## PART 3 — .NET Folder Structure (The Professional Layout)

### 3.1 — Why Structure Matters

Bad structure = you can't find anything = you write the same thing twice = bugs.
Good structure = a new developer opens the project and knows exactly where to look.

The structure we're building toward for `MyMovies.Api/`:

```
MyMovies.Api/
│
├── Controllers/              ← HTTP layer ONLY. No business logic.
│   ├── AuthController.cs     ← POST /auth/login, /auth/register, /auth/refresh
│   ├── MoviesController.cs   ← GET/POST/PUT/DELETE /movies
│   ├── SeriesController.cs
│   └── ProfilesController.cs
│
├── Services/                 ← Business logic. The brain of the app.
│   ├── Interfaces/           ← Contracts first, then implementations
│   │   ├── IAuthService.cs
│   │   ├── IMovieService.cs
│   │   └── ITmdbService.cs
│   ├── AuthService.cs        ← Implements IAuthService
│   ├── MovieService.cs       ← Implements IMovieService
│   └── TmdbService.cs        ← Implements ITmdbService
│
├── Models/                   ← EF Core entities (= database tables)
│   ├── User.cs
│   ├── Profile.cs
│   ├── RefreshToken.cs
│   ├── MediaItem.cs          ← Base class
│   ├── Movie.cs              ← Inherits MediaItem
│   ├── Series.cs             ← Inherits MediaItem
│   ├── Season.cs
│   ├── Episode.cs
│   └── MediaSource.cs        ← URL or file path for actual video
│
├── DTOs/                     ← What the API receives and returns (NOT entities)
│   ├── Auth/
│   │   ├── RegisterDto.cs    ← { Email, Password, ConfirmPassword }
│   │   ├── LoginDto.cs       ← { Email, Password }
│   │   └── AuthResponseDto.cs ← { AccessToken, User }
│   ├── Movies/
│   │   ├── MovieDto.cs       ← What the API returns to the client
│   │   └── CreateMovieDto.cs ← What the client sends to create a movie
│   └── Profiles/
│       ├── ProfileDto.cs
│       └── CreateProfileDto.cs
│
├── Data/                     ← Database configuration
│   ├── AppDbContext.cs       ← The EF Core bridge
│   └── Seed/                 ← (later) default data for fresh installs
│       └── DatabaseSeeder.cs
│
├── Migrations/               ← AUTO-GENERATED by EF Core. Never edit manually.
│   ├── 20260218_CreateMovies.cs
│   └── AppDbContextModelSnapshot.cs
│
├── Middleware/               ← Custom HTTP pipeline components
│   └── ErrorHandlingMiddleware.cs ← Catch all unhandled exceptions, return clean JSON
│
├── Validators/               ← FluentValidation rules (Phase 2)
│   ├── RegisterDtoValidator.cs
│   └── LoginDtoValidator.cs
│
├── Extensions/               ← IServiceCollection helpers (keeps Program.cs clean)
│   ├── AuthExtensions.cs     ← builder.Services.AddJwtAuthentication(config)
│   └── CorsExtensions.cs     ← builder.Services.AddCorsPolicy(config)
│
├── Helpers/                  ← Pure utility functions, no DI needed
│   └── JwtHelper.cs
│
├── Program.cs                ← Stays clean: just calls extension methods
├── appsettings.json
├── appsettings.Development.json
└── MyMovies.Api.csproj
```

---

### 3.2 — The Responsibility Rule (Most Important Rule)

Each folder has ONE job. Never mix them.

```
WRONG — Business logic in Controller:
┌─────────────────────────────────────────────────────┐
│ MoviesController.cs                                 │
│                                                     │
│   [HttpPost]                                        │
│   public async Task<IActionResult> Create(Movie m) │
│   {                                                 │
│       // Checking if title exists (BUSINESS LOGIC)  │
│       var exists = await _context.Movies            │
│           .AnyAsync(x => x.Title == m.Title);       │
│       if (exists) return BadRequest("Exists");      │
│                                                     │
│       // Fetching TMDB data (BUSINESS LOGIC)        │
│       var tmdbData = await _tmdbClient.Search(m.Title); │
│                                                     │
│       // Sending email (BUSINESS LOGIC)             │
│       await _emailService.SendAddedNotification();  │
│                                                     │
│       _context.Movies.Add(m);                       │
│       await _context.SaveChangesAsync();            │
│       return Ok(m);                                 │
│   }                                                 │
└─────────────────────────────────────────────────────┘

CORRECT — Controller is a traffic cop only:
┌────────────────────────────────┐    ┌──────────────────────────────┐
│ MoviesController.cs            │    │ MovieService.cs              │
│                                │    │                              │
│   [HttpPost]                   │    │   public async Task<Movie>   │
│   public async Task<IActionResult> │    │   CreateAsync(CreateMovieDto dto) │
│   Create(CreateMovieDto dto)   │───►│   {                          │
│   {                            │    │     // All logic here        │
│     var movie = await          │    │     // Check duplicates      │
│       _movieService            │    │     // Fetch TMDB            │
│       .CreateAsync(dto);       │    │     // Send email            │
│                                │◄───│     // Save to DB            │
│     return CreatedAtAction(... │    │   }                          │
│   }                            │    └──────────────────────────────┘
└────────────────────────────────┘
```

**The test:** Can you unit test your service WITHOUT starting the web server?
Yes → Good structure.
No → You've mixed responsibilities.

---

### 3.3 — DTOs vs Models — Critical Distinction

**Model** = what EF Core puts in the database.
**DTO** (Data Transfer Object) = what travels across the API boundary.

They are NEVER the same class. Here's why:

```csharp
// Model (database) — has everything
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }    // ← NEVER send this to client
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; }  // ← definitely not
    public List<Profile> Profiles { get; set; }
}

// DTO (API response) — only what the client needs
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    // Notice: no PasswordHash, no RefreshTokens, no internal fields
}
```

If you returned the `User` model directly from the API:
- The password hash would be in the JSON response
- Every profile and refresh token would be serialized (potentially circular references)
- Your DB schema leaks to clients (security + coupling issue)

**Mapping between them:**
```csharp
// In the service:
public async Task<UserDto> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);

    // Manual mapping (simple, explicit):
    return new UserDto
    {
        Id = user.Id,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };

    // OR with AutoMapper library (later):
    // return _mapper.Map<UserDto>(user);
}
```

---

### 3.4 — The Migration Files (Don't Touch These)

```
Migrations/
├── 20260218172001_CreateMoviesTable.cs          ← The actual migration
├── 20260218172001_CreateMoviesTable.Designer.cs ← EF Core metadata
└── AppDbContextModelSnapshot.cs                 ← Current state of the schema
```

**`CreateMoviesTable.cs`** — Has two methods:
- `Up()` — apply the migration (CREATE TABLE, ADD COLUMN, etc.)
- `Down()` — roll it back (DROP TABLE, DROP COLUMN, etc.)

```csharp
// What your migration generated (you already have this):
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Movies",
        columns: table => new
        {
            Id = table.Column<int>(nullable: false)
                .Annotation("MySQL:ValueGenerationStrategy", IdentityColumn),
            Title = table.Column<string>(nullable: false),
            Genre = table.Column<string>(nullable: false),
            Year = table.Column<int>(nullable: false),
            Rating = table.Column<decimal>(nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_Movies", x => x.Id)
    );
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "Movies");
}
```

**Rule:** Never edit migration files manually. If you made a mistake, create a new migration that fixes it.

---

## PART 4 — Git & GitHub Strategy for MyMovies

### 4.1 — The Answer to Your Question

**Yes — `git init` in the parent `MyMovies/` directory is the correct approach.**

Your repo is already initialized there correctly. Here's why:

```
MyMovies/                 ← ONE git repo for the whole project ✅
├── .git/                 ← One git history
├── CLAUDE.md
├── MyMovies.slnx
├── MyMovies.Api/         ← Backend in the same repo
└── MyMovies.Client/      ← Frontend in the same repo
```

This is called a **Monorepo** (mono = single, repo = repository). One repository,
multiple projects. This is the right choice when:
- Frontend and backend are always deployed together
- Same team works on both
- You want one pull request to change both sides

The alternative is two separate repos — used when different teams or deployment cycles.
For your learning project: monorepo is perfect.

---

### 4.2 — Create a Proper `.gitignore` (Critical)

The Angular project has a `.gitignore` but there's no root-level one and no .NET one.
You need to fix this NOW before your first commit.

**Create: `MyMovies/.gitignore`** (the root-level catch-all):

```gitignore
# ═══════════════════════════
# .NET / C# - BACKEND
# ═══════════════════════════

# Build output
bin/
obj/

# User secrets (contains real passwords and API keys)
appsettings.Development.json
appsettings.*.json
!appsettings.json

# .NET user-secrets (stored outside project, but just in case)
secrets.json

# Visual Studio / Rider artifacts
.vs/
*.user
*.suo
*.userprefs
.idea/

# NuGet packages (restored via dotnet restore)
packages/
*.nupkg

# EF Core — keep migrations (they're part of the source)
# Do NOT ignore Migrations/

# ═══════════════════════════
# ANGULAR - FRONTEND
# ═══════════════════════════

# Dependencies (restored via npm install)
node_modules/

# Build output
dist/
.angular/

# Environment files
.env
.env.local
.env.development.local
environment.ts
environment.development.ts

# ═══════════════════════════
# GENERAL
# ═══════════════════════════

# OS files
.DS_Store
Thumbs.db
desktop.ini

# IDE
.vscode/
!.vscode/settings.json
!.vscode/extensions.json
.cursor/

# Logs
*.log
logs/

# Docker
.env.docker

# Uploaded files (user content — not source code)
storage/
uploads/
wwwroot/uploads/
```

---

### 4.3 — Git Concepts You Need (ELI5)

**The Three Areas:**

```
Working Directory          Staging Area (Index)       Repository (.git)
──────────────────         ────────────────────       ─────────────────
Where you edit files  →    Where you stage changes →  Where history lives
                     git add                    git commit
```

**Key commands:**

```bash
# What's changed?
git status

# See exact changes:
git diff

# Stage specific files (NEVER use git add . blindly):
git add MyMovies.Api/Controllers/MoviesController.cs
git add MyMovies.Api/Models/Movie.cs

# Stage everything in a folder:
git add MyMovies.Api/

# Create a commit:
git commit -m "feat: add movie CRUD endpoints with EF Core"

# View history:
git log --oneline

# Create a branch (new feature):
git checkout -b feat/authentication

# Switch back to main:
git checkout main

# Push to GitHub:
git push origin main
```

---

### 4.4 — Commit Message Convention (Write This Properly)

Professional teams use **Conventional Commits**:

```
<type>: <short description>

[optional body]
```

**Types:**

| Type | When to use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `refactor` | Code change with no feature or fix |
| `test` | Adding or fixing tests |
| `chore` | Build, deps, config (no production code) |

**Examples:**
```bash
git commit -m "feat: add JWT authentication with refresh tokens"
git commit -m "feat: add multi-profile system per user account"
git commit -m "fix: resolve CORS issue blocking Angular requests"
git commit -m "refactor: move business logic from controller to MovieService"
git commit -m "chore: add .gitignore for .NET and Angular"
git commit -m "docs: add Phase 1 study guide"
```

**Bad commit messages (don't do this):**
```bash
git commit -m "fix stuff"
git commit -m "changes"
git commit -m "asdfgh"
git commit -m "working now"
```

---

### 4.5 — Branching Strategy for This Project

Since you're the only developer, keep it simple:

```
main ─────────────────────────────────────────────────────────► (production-ready)
       │              │                  │
       └── feat/auth  └── feat/profiles  └── feat/tmdb
           (work here)    (work here)        (work here)
           merge when done
```

**Workflow:**
```bash
# Start Phase 2 work:
git checkout -b feat/authentication

# Work, commit frequently...
git add MyMovies.Api/Controllers/AuthController.cs
git commit -m "feat: add user registration endpoint"

git add MyMovies.Api/Services/AuthService.cs
git commit -m "feat: add JWT token generation"

# When Phase 2 is complete and tested:
git checkout main
git merge feat/authentication
git push origin main
```

---

### 4.6 — Setting Up GitHub Remote

```bash
# 1. Create a new EMPTY repo on github.com (no README, no .gitignore — we have our own)

# 2. Add the remote (replace with your actual URL):
git remote add origin https://github.com/yourusername/mymovies.git

# 3. Push for the first time:
git push -u origin main

# After that, just:
git push
```

---

## PART 5 — Self-Assessment Questions

Answer these without looking.

**OOP:**
1. What is the difference between a class and an object (instance)?
2. Explain encapsulation in one sentence and give a real-world example.
3. If `Movie` inherits from `MediaItem`, what does `Movie` get automatically?
4. What is the difference between `virtual`/`override` and `abstract`/`override`?
5. Can a C# class inherit from two classes? Can it implement two interfaces?
6. What is the difference between an `abstract class` and an `interface`?
7. When would you use a `static` method vs an instance method?
8. What does `<T>` mean in `List<T>`?

**Auth:**
9. Why is HTTP "stateless" and why does that create an authentication problem?
10. Where does the server store session data? Where does it store JWT data?
11. What are the three parts of a JWT?
12. Why can't JWT tokens be "revoked" the same way sessions can?
13. Why is the access token short-lived (15 min)?
14. Why is the refresh token stored in an HttpOnly cookie and not localStorage?
15. What is a "claim" in a JWT?
16. What does rate limiting protect against?

**.NET Structure:**
17. What is the ONLY job of a Controller?
18. What is the job of a Service?
19. What is a DTO and why do we never return a Model entity directly from the API?
20. Should you ever edit a migration file manually?
21. What does `bin/` contain and why is it in `.gitignore`?
22. What does `obj/` contain and why is it in `.gitignore`?

**Git:**
23. Is a monorepo the right choice for this project? Why?
24. What is the Staging Area in git?
25. What is the difference between `git add .` and `git add specific-file.cs`?
26. Write a proper commit message for "I added login functionality".
