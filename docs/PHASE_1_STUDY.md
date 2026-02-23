# Phase 1 — Complete Study Guide
### "The Foundation" — ELI5 Edition for Aspiring Giga-Chad Developers

> **How to use this document:**
> Read it top to bottom, once. Then open each file side-by-side and re-read.
> At the bottom there are self-assessment questions. Answer them before moving to Phase 2.
> If you can answer all of them without looking — you own Phase 1.

---

## 🏗️ The Big Picture First

Before touching any code, understand **what we're building architecturally**.

```
┌─────────────────────────────────────────────────────────┐
│                    YOUR COMPUTER                        │
│                                                         │
│   ┌──────────────┐   HTTP    ┌──────────────────────┐  │
│   │   ANGULAR    │ ◄──────► │    .NET WEB API      │  │
│   │  (Frontend)  │  JSON     │    (Backend)         │  │
│   │  Port 4200   │           │    Port 5277         │  │
│   └──────────────┘           └──────────┬───────────┘  │
│                                         │ SQL           │
│                                         ▼               │
│                               ┌──────────────────────┐  │
│                               │       MySQL           │  │
│                               │    Port 3306         │  │
│                               └──────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### The Restaurant Analogy (ELI5)

Think of your app as a restaurant:

| Restaurant           | Your App                          |
|---------------------|-----------------------------------|
| The dining room     | Angular (what the user sees)      |
| The kitchen         | .NET Web API (where logic happens)|
| The recipe files    | MySQL Database (where data lives) |
| The waiter          | The HTTP request (carries orders) |
| The menu            | The API endpoints (`/api/movies`) |
| The order ticket    | JSON (the language they speak)    |
| The building permit | CORS (permission to communicate)  |

**The customer (browser) sits in the dining room (Angular).**
**They place an order (HTTP request) through the waiter (JSON over HTTP).**
**The kitchen (.NET) prepares it from the recipe files (MySQL) and sends it back.**
**The dining room (Angular) presents it beautifully.**

The customer never enters the kitchen. The kitchen never talks directly to the customer.
They communicate ONLY through the waiter. This is the entire architecture of a modern web app.

---

## 🔴 PART 1 — The .NET Backend (`MyMovies.Api/`)

### 1.1 — `MyMovies.Api.csproj` — The Project's Identity Card

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.3" />
    <PackageReference Include="MySql.EntityFrameworkCore" Version="10.0.1" />
  </ItemGroup>

</Project>
```

**ELI5:** This is like the `package.json` in Node.js or `composer.json` in Laravel/PHP.
It tells .NET: "This project runs on .NET 10, and it needs these three external libraries."

**Line by line:**

| Line | What it means |
|------|--------------|
| `Sdk="Microsoft.NET.Sdk.Web"` | "This is a web project, not a console app or desktop app" |
| `TargetFramework net10.0` | "Run this on .NET version 10" |
| `Nullable enable` | "Be strict — warn me if I might get a null reference crash" |
| `ImplicitUsings enable` | "Auto-import common namespaces so I don't type `using System;` everywhere" |
| `Microsoft.AspNetCore.OpenApi` | Swagger UI — auto-generated API documentation |
| `Microsoft.EntityFrameworkCore.Design` | EF Core tools — needed to run `dotnet ef migrations` commands |
| `MySql.EntityFrameworkCore` | EF Core's driver to speak MySQL (like a language translator) |

**Laravel equivalent:** `composer.json` with `"laravel/framework"` and other dependencies.

**How to add a package:**
```bash
dotnet add package PackageName
```
This is exactly like `composer require vendor/package` or `npm install package`.

---

### 1.2 — `appsettings.json` + `appsettings.Development.json` — The Config File

```json
// appsettings.json (base config — safe to commit)
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

// appsettings.Development.json (local only — NEVER commit passwords)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mymovies;User=root;Password=...;Port=3306"
  }
}
```

**ELI5:** Like a `.env` file in Laravel. It stores settings that change between environments
(dev machine vs production server).

**The hierarchy works like this (each overrides the previous):**
```
appsettings.json               ← always loaded (base)
  ↓ overridden by
appsettings.Development.json   ← loaded when ASPNETCORE_ENVIRONMENT=Development
  ↓ overridden by
Environment Variables           ← loaded in production/Docker
  ↓ overridden by
User Secrets                   ← local dev only, stored outside the project folder
```

**🚨 SECURITY WARNING — IMPORTANT:**
Your `appsettings.Development.json` has a real database password in it.
If you push this to GitHub, anyone can see it.
The correct approach:

```bash
# Run this from MyMovies.Api/ — stores secrets outside the project
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=mymovies;User=root;Password=YourPassword;Port=3306"
```

Then add `appsettings.Development.json` to `.gitignore`.

**Reading config in code:**
```csharp
// .NET automatically reads this anywhere you inject IConfiguration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Laravel equivalent:
// $connectionString = env('DB_CONNECTION');
```

---

### 1.3 — `Program.cs` — The Startup File (The Kitchen Setup)

```csharp
using Microsoft.EntityFrameworkCore;
using MyMovies.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ──── SERVICES ────
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!)
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ──── MIDDLEWARE ────
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**ELI5:** This is the first file that runs when your API starts.
It does two things in order:
1. **Register services** — "Here are all the tools my app will use"
2. **Set up the pipeline** — "Here is the order requests flow through"

Think of it as the manager arriving before the restaurant opens:
1. First, they stock the kitchen with ingredients (services)
2. Then, they set up the workflow — who greets, who takes orders, who cooks (middleware)

#### The Two Phases Explained

**PHASE 1 — `builder.Services.*` (Registration)**

This is the **Dependency Injection container**. Think of it as a registry.
You register things ONCE. .NET then automatically gives them to whoever needs them.

```csharp
builder.Services.AddControllers();
```
"Register all my Controllers so .NET knows they exist and can route HTTP requests to them."

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString)
);
```
"Register my database connection. Whenever any class asks for `AppDbContext`, give them one connected to MySQL."

**Laravel equivalent:**
```php
// Laravel does this behind the scenes in config/database.php
// You never write this manually — .env handles it
DB::connection('mysql');
```

```csharp
builder.Services.AddCors(options => { ... });
```
"Register CORS rules. A browser trying to call this API from a different port
needs permission."

---

**PHASE 2 — `app.Use*` / `app.Map*` (The Middleware Pipeline)**

After `var app = builder.Build()`, you define the **pipeline** — the order
that every HTTP request travels through, like a conveyor belt.

```
Incoming HTTP Request
        │
        ▼
   ┌─────────┐
   │  CORS   │  "Does this request have permission to be here?"
   └────┬────┘
        │
        ▼
   ┌───────────────┐
   │ Authorization │  "Is this user logged in?" (we'll add this in Phase 2)
   └──────┬────────┘
          │
          ▼
   ┌──────────────┐
   │  Controllers │  "Route to the right controller method"
   └──────────────┘
```

**Order matters here.** CORS must come BEFORE authorization, and authorization
must come BEFORE controllers. If you flip them, things break in subtle ways.

```csharp
app.UseCors("AllowAngular");   // Step 1 — check CORS
app.UseAuthorization();        // Step 2 — check auth
app.MapControllers();          // Step 3 — route to controller
app.Run();                     // Start the web server, begin listening
```

**`app.Run()` is blocking.** The app stays alive here, listening for requests,
until you press Ctrl+C.

---

### 1.4 — `Models/Movies.cs` — The Database Blueprint

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMovies.Api.Models;

public class Movie
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Rating { get; set; }
}
```

**ELI5:** This class IS your database table. EF Core reads this class and
creates the `movies` table automatically. Every property = one column.

**Laravel equivalent:**
```php
// In Laravel you have BOTH a migration AND a model separately:

// Migration (creates the table):
Schema::create('movies', function (Blueprint $table) {
    $table->id();
    $table->string('title');
    $table->string('genre');
    $table->integer('year');
    $table->decimal('rating', 3, 1);
});

// Model (interacts with the table):
class Movie extends Model {
    protected $fillable = ['title', 'genre', 'year', 'rating'];
}
```

In .NET, **one class does both jobs**. The model IS the migration definition.

**Line by line:**

| Code | What it means | SQL equivalent |
|------|--------------|----------------|
| `namespace MyMovies.Api.Models` | "This class lives in this folder/group" | N/A |
| `public class Movie` | "This is a table called Movie (EF pluralises it to 'Movies')" | `CREATE TABLE Movies` |
| `[Key]` | "This property is the primary key" | `PRIMARY KEY` |
| `[DatabaseGenerated(Identity)]` | "Database auto-generates this value" | `AUTO_INCREMENT` |
| `public int Id` | Integer column named Id | `id INT` |
| `public string Title { get; set; }` | A property with get and set — EF can read and write it | `title VARCHAR(...)` |
| `= string.Empty` | Default value is "" (never null) | `DEFAULT ''` |

**What is `{ get; set; }`?**
In C#, class properties aren't just variables. They have a getter (read) and setter (write).
```csharp
// This is shorthand (auto-property):
public string Title { get; set; } = string.Empty;

// It's equivalent to this (full form):
private string _title = string.Empty;
public string Title
{
    get { return _title; }
    set { _title = value; }
}
```
For now, use auto-properties everywhere. The full form is for when you need custom logic.

**What is `namespace`?**
Think of it like a PHP namespace or a folder path. It prevents naming conflicts
and organizes code. `MyMovies.Api.Models.Movie` is the full name of this class.
Just like `App\Models\Movie` in Laravel.

---

### 1.5 — `Data/AppDbContext.cs` — The Database Bridge

```csharp
using Microsoft.EntityFrameworkCore;
using MyMovies.Api.Models;

namespace MyMovies.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
}
```

**ELI5:** This is EF Core's control room. It knows about your database
AND about all your models/tables. It is the bridge between C# objects and SQL rows.

**Laravel equivalent:**
```php
// In Laravel, this doesn't exist as an explicit class.
// Eloquent's base Model class and config/database.php together do this job.
// The closest thing is a Repository or DB::table('movies')
```

**Line by line:**

```csharp
public class AppDbContext : DbContext
```
Our custom context inherits from EF Core's base `DbContext`.
`DbContext` knows how to speak SQL. We extend it with our specific tables.

```csharp
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
```
This is the **constructor** — runs when .NET creates this class.
The `options` contains the connection string (from `Program.cs`'s `AddDbContext` call).
`: base(options)` passes it to the parent `DbContext` class.

**In plain English:** "When someone creates me, pass them the database settings I was given."

```csharp
public DbSet<Movie> Movies { get; set; }
```
This says: "I have a table called `Movies` that contains `Movie` objects."
`DbSet<Movie>` is like a C# representation of your `movies` MySQL table.
You query it with LINQ instead of writing SQL.

```csharp
// SQL: SELECT * FROM Movies
await _context.Movies.ToListAsync();

// SQL: SELECT * FROM Movies WHERE Id = 5
await _context.Movies.FindAsync(5);

// SQL: INSERT INTO Movies (Title, ...) VALUES ('Inception', ...)
_context.Movies.Add(movie);
await _context.SaveChangesAsync();

// SQL: DELETE FROM Movies WHERE Id = 5
_context.Movies.Remove(movie);
await _context.SaveChangesAsync();
```

**EF Core Migrations — how the table actually gets created:**
```bash
# 1. Tell EF Core "I added a new model, create a migration file"
dotnet ef migrations add InitialCreate

# This creates: Migrations/20240218_InitialCreate.cs
# (a C# file that translates to SQL CREATE TABLE statements)

# 2. Apply the migration to the actual MySQL database
dotnet ef database update
```

---

### 1.6 — `Controllers/MoviesController.cs` — The API Endpoints

This is the longest file. Read it carefully — every HTTP verb is represented.

```csharp
[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MoviesController(AppDbContext context)
    {
        _context = context;
    }
```

**`[ApiController]`** — An attribute (like a PHP annotation `#[...]`).
It activates several automatic behaviors:
- Auto-returns 400 if model validation fails
- Auto-reads request body as JSON
- Auto-routes based on HTTP verbs

**`[Route("api/[controller]")]`** — Defines the URL prefix.
`[controller]` is replaced by the class name minus "Controller".
`MoviesController` → `movies` → full URL: `/api/movies`

**Laravel equivalent:**
```php
// In routes/api.php:
Route::apiResource('movies', MoviesController::class);
// Maps to: GET /api/movies, POST /api/movies, etc.
```

**The constructor — Dependency Injection in action:**
```csharp
public MoviesController(AppDbContext context)
{
    _context = context;
}
```

You're saying "I need an `AppDbContext` to do my job."
.NET sees this, looks in the DI container (registered in `Program.cs`),
finds the `AppDbContext` registration, creates one, and hands it to you.

You never write `new AppDbContext()`. .NET does it. This is **Dependency Injection**.

**Why DI?** Testability. When writing tests, you can hand in a fake database
instead of the real one. The controller doesn't care — it just uses whatever it receives.

---

#### The 5 CRUD Endpoints

**GET ALL — `GET /api/movies`**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
{
    return await _context.Movies.ToListAsync();
}
```

`async Task<ActionResult<IEnumerable<Movie>>>` — looks scary, let's break it down:
- `async` → this method uses async/await (explained below)
- `Task<>` → the return type of any async method
- `ActionResult<>` → can return the data OR an HTTP error (404, 400, etc.)
- `IEnumerable<Movie>` → a collection of Movie objects (like an array)

`_context.Movies.ToListAsync()` — EF Core translates this to `SELECT * FROM Movies`
and returns all rows as a `List<Movie>`.

**GET ONE — `GET /api/movies/5`**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Movie>> GetMovie(int id)
{
    var movie = await _context.Movies.FindAsync(id);

    if (movie == null)
    {
        return NotFound();  // HTTP 404
    }

    return movie;           // HTTP 200 with JSON
}
```

`{id}` in the route maps to the `int id` parameter automatically.
`FindAsync` searches by primary key. Returns `null` if not found.
`return NotFound()` → sends HTTP 404 response.
`return movie` → serializes `Movie` to JSON, sends HTTP 200 response.

**CREATE — `POST /api/movies`**
```csharp
[HttpPost]
public async Task<ActionResult<Movie>> CreateMovie(Movie movie)
{
    _context.Movies.Add(movie);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
}
```

`Movie movie` — .NET reads the JSON request body and deserializes it into a `Movie` object.
`_context.Movies.Add(movie)` — stages the insert (like `git add`)
`_context.SaveChangesAsync()` — commits to DB (like `git commit`) — runs the SQL INSERT
`CreatedAtAction` — returns HTTP 201 with a `Location` header pointing to `/api/movies/{id}`

**UPDATE — `PUT /api/movies/5`**
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateMovie(int id, Movie movie)
{
    if (id != movie.Id)
    {
        return BadRequest();  // HTTP 400 — URL id doesn't match body id
    }

    _context.Entry(movie).State = EntityState.Modified;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!await _context.Movies.AnyAsync(m => m.Id == id))
            return NotFound();
        throw;
    }

    return NoContent();  // HTTP 204 — success, nothing to return
}
```

`_context.Entry(movie).State = EntityState.Modified` — tells EF Core:
"This object already has an Id. Don't INSERT it, UPDATE the existing row instead."

`DbUpdateConcurrencyException` — what if two people try to update the same record
at the same time? This catches that race condition.

**DELETE — `DELETE /api/movies/5`**
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteMovie(int id)
{
    var movie = await _context.Movies.FindAsync(id);

    if (movie == null)
    {
        return NotFound();
    }

    _context.Movies.Remove(movie);
    await _context.SaveChangesAsync();

    return NoContent();  // HTTP 204
}
```

Simple: find it → remove it → save → return 204.

---

### 1.7 — Understanding `async` / `await`

This is **critical**. Read this twice.

**ELI5:** Imagine you're a waiter. You take an order, submit it to the kitchen, and
WAIT for it to be ready before doing anything else. That's **synchronous** (blocking).

A **smart waiter** takes the order, submits it to the kitchen, then goes to serve
other tables while the food cooks. When the food is ready, they deliver it.
That's **asynchronous** (non-blocking).

```csharp
// WRONG (synchronous — blocks the thread, kills performance)
public Movie GetMovie(int id)
{
    return _context.Movies.Find(id);  // Thread is frozen here until DB responds
}

// CORRECT (asynchronous — thread is free while DB does its work)
public async Task<Movie?> GetMovieAsync(int id)
{
    return await _context.Movies.FindAsync(id);  // "Go do other work, come back when ready"
}
```

**Rules:**
1. If a method uses `await`, it MUST be marked `async`
2. An `async` method MUST return `Task` or `Task<T>` (not `void`, not the raw type)
3. Only `await` things that are truly asynchronous: DB calls, HTTP calls, file I/O

**Laravel equivalent:**
Laravel handles this automatically with queues and jobs. You rarely write async
code manually in PHP. In .NET, you manage it explicitly — which gives you more control.

---

## 🔵 PART 2 — The Angular Frontend (`MyMovies.Client/`)

### 2.1 — `main.ts` — The Front Door

```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
```

**ELI5:** This is the very first file that runs in the browser.
It has one job: start the Angular application.

`bootstrapApplication(App, appConfig)` — "Take the root component `App`,
apply the configuration from `appConfig`, and start Angular."

**Laravel equivalent:** `public/index.php` — the single entry point for all requests.

After Angular starts, it takes over the entire browser window and manages all
navigation client-side (no more full page reloads like in traditional PHP apps).

---

### 2.2 — `app.config.ts` — The Angular Providers (DI Container)

```typescript
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
  ]
};
```

**ELI5:** This is Angular's equivalent of `Program.cs`'s service registration section.
It registers the core tools Angular needs.

**Line by line:**

| Provider | What it does | .NET equivalent |
|----------|-------------|-----------------|
| `provideBrowserGlobalErrorListeners()` | Catches uncaught JS errors | Global exception handling middleware |
| `provideRouter(routes)` | "Here are my URL routes, use them" | `app.MapControllers()` |
| `provideHttpClient()` | "Register `HttpClient` for making API calls" | `builder.Services.AddHttpClient()` |

Once registered here, you can **inject** these services anywhere in the app.
Angular's DI container works the same way as .NET's.

---

### 2.3 — `app.ts` — The Root Shell Component

```typescript
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('MyMovies.Client');
}
```

**ELI5:** This is the outermost container of your entire app.
Like the `<body>` tag, but managed by Angular. Everything else lives inside it.

**`@Component` decorator** — Similar to C#'s `[ApiController]` attribute.
It tells Angular "this class is a component" and gives it metadata.

| Property | What it means |
|----------|--------------|
| `selector: 'app-root'` | The HTML tag this component uses: `<app-root></app-root>` (in `index.html`) |
| `imports: [RouterOutlet]` | "I use `<router-outlet>` in my template, import it" |
| `templateUrl: './app.html'` | "My HTML is in this file" |
| `styleUrl: './app.css'` | "My scoped CSS is in this file" |

**`signal('MyMovies.Client')`** — A **Signal** is Angular's reactive state primitive (new in Angular 16+).
When a signal's value changes, Angular automatically updates the UI wherever it's used.

```typescript
// Create a signal
title = signal('Hello');

// Read it (note the () — it's a function call)
console.log(this.title()); // "Hello"

// Update it
this.title.set('World');

// In template:
// {{ title() }}  ← always call it like a function
```

**Old-style vs Signals:**
```typescript
// OLD (still works, but signals are the future)
title = 'MyMovies.Client';  // Just a property — Angular detects changes with Zone.js magic

// NEW (signals — explicit, performant, predictable)
title = signal('MyMovies.Client');  // Angular only updates when you explicitly change it
```

---

### 2.4 — `app.routes.ts` — The URL Map

```typescript
import { Routes } from '@angular/router';
import { MovieListComponent } from './components/movie-list/movie-list';

export const routes: Routes = [
  { path: '', component: MovieListComponent },
];
```

**ELI5:** When the user types a URL in the browser, this file decides
which component to show.

```
User visits: http://localhost:4200/
→ path: ''  matches
→ Angular renders: MovieListComponent
```

**Laravel equivalent:**
```php
// routes/web.php
Route::get('/', [MovieController::class, 'index']);
```

**Later it will look like:**
```typescript
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'movies', component: MoviesComponent, canActivate: [AuthGuard] },
  { path: '**', component: NotFoundComponent },  // 404 catch-all
];
```

**`<router-outlet>`** in `app.html` is the placeholder where the matched component renders.
It's like `@yield('content')` in Laravel Blade.

---

### 2.5 — `models/movie.ts` — The TypeScript Contract

```typescript
export interface Movie {
  id: number;
  title: string;
  genre: string;
  year: number;
  rating: number;
}
```

**ELI5:** This is a **contract** — a description of what a Movie object looks like.
TypeScript uses this to catch mistakes at compile time (before the code runs).

**It is NOT a class.** An `interface` has no code, no methods, no implementation.
It's just a shape definition.

```typescript
// CORRECT — matches the interface
const movie: Movie = { id: 1, title: 'Inception', genre: 'Sci-Fi', year: 2010, rating: 8.8 };

// WRONG — TypeScript will give you a red underline and refuse to compile
const movie: Movie = { id: 1, title: 'Inception' };
// Error: Property 'genre', 'year', 'rating' are missing
```

**Why does this matter?** Without TypeScript, you'd write JavaScript and only discover
typos or missing fields when the app crashes in the browser. TypeScript catches them
while you're writing the code.

**It must mirror the backend DTO exactly:**
```csharp
// Backend (C#)                      // Frontend (TypeScript)
public class Movie {                  export interface Movie {
    public int Id { get; set; }         id: number;
    public string Title { get; set; }   title: string;
    public string Genre { get; set; }   genre: string;
    public int Year { get; set; }       year: number;
    public decimal Rating { get; set; } rating: number;
}                                     }
```

Note: C# properties are PascalCase (`Id`, `Title`).
JSON serialization automatically converts them to camelCase (`id`, `title`).
TypeScript uses camelCase. They match automatically.

---

### 2.6 — `services/movie.service.ts` — The HTTP Communicator

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movie } from '../models/movie';

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  private apiUrl = 'http://localhost:5277/api/movies';

  constructor(private http: HttpClient) { }

  getMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(this.apiUrl);
  }

  getMovie(id: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.apiUrl}/${id}`);
  }

  addMovie(movie: Omit<Movie, 'id'>): Observable<Movie> {
    return this.http.post<Movie>(this.apiUrl, movie);
  }

  deleteMovie(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

**ELI5:** This service is the waiter who talks to the kitchen (.NET API).
No component ever makes HTTP calls directly — they all go through this service.

**`@Injectable({ providedIn: 'root' })`** — Register this service in Angular's DI container.
`providedIn: 'root'` means it's a **singleton** — one shared instance for the whole app.

**Laravel equivalent:**
```php
// Like a Repository class that wraps Guzzle HTTP client
class MovieRepository {
    public function all(): Collection { ... }
    public function find(int $id): Movie { ... }
}
```

**`Observable<Movie[]>` — What is an Observable?**

An Observable is like a subscription to something that will happen in the future.
It's Angular's version of a Promise (but more powerful).

```typescript
// Observable is LAZY — nothing happens until you subscribe
const movies$ = this.movieService.getMovies();  // No HTTP call yet

// subscribe() triggers the HTTP call and handles the response
movies$.subscribe(movies => {
  this.movies = movies;  // This runs when the response arrives
});
```

Think of it like a Netflix subscription:
- Creating the Observable is like signing up for Netflix
- `.subscribe()` is like actually pressing Play
- Nothing happens until you press Play

**`Omit<Movie, 'id'>`** — A TypeScript utility type.
```typescript
addMovie(movie: Omit<Movie, 'id'>)
// Means: a Movie object with all fields EXCEPT 'id'
// Because when creating a new movie, the DB assigns the id — we don't send it
```

**HTTP methods map to CRUD:**
```typescript
this.http.get<Movie[]>(url)         // READ — GET
this.http.post<Movie>(url, body)    // CREATE — POST
this.http.put<Movie>(url, body)     // UPDATE — PUT
this.http.delete<void>(url)         // DELETE — DELETE
```

---

### 2.7 — `components/movie-list/movie-list.ts` — The Component

```typescript
@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './movie-list.html',
  styleUrl: './movie-list.css'
})
export class MovieListComponent implements OnInit {
  movies: Movie[] = [];

  newMovie = {
    title: '',
    genre: '',
    year: new Date().getFullYear(),
    rating: 0
  };

  constructor(private movieService: MovieService) { }

  ngOnInit(): void {
    this.loadMovies();
  }

  loadMovies(): void {
    this.movieService.getMovies().subscribe(movies => {
      this.movies = movies;
    });
  }

  addMovie(): void {
    this.movieService.addMovie(this.newMovie).subscribe(movie => {
      this.movies.push(movie);
      this.newMovie = { title: '', genre: '', year: new Date().getFullYear(), rating: 0 };
    });
  }

  deleteMovie(id: number): void {
    this.movieService.deleteMovie(id).subscribe(() => {
      this.movies = this.movies.filter(m => m.id !== id);
    });
  }
}
```

**ELI5:** This is the component that renders the movie list UI.
It holds the data (state) and handles user actions (events).

**`standalone: true`** — Modern Angular (v17+) doesn't need NgModules.
Components can import what they need directly in `imports: []`.

**`implements OnInit`** — This component uses Angular's lifecycle hook `ngOnInit`.

**Angular Lifecycle Hooks:**
```
Component Created
      │
      ▼
  ngOnInit()       ← Runs once after component is created — LOAD DATA HERE
      │
      ▼
  (user interacts, state changes, Angular re-renders)
      │
      ▼
  ngOnDestroy()    ← Runs when component is removed — CLEAN UP HERE
```

**`ngOnInit()` vs constructor:**
```typescript
// Constructor — runs first, but DOM is not ready, inputs not set
// Use for: DI only (assigning services to private fields)
constructor(private movieService: MovieService) { }

// ngOnInit — component is ready, Angular inputs are set
// Use for: loading data, setting up subscriptions
ngOnInit(): void {
    this.loadMovies();  // CORRECT — load data here
}
```

**`imports: [CommonModule, FormsModule]`**
- `CommonModule` — provides `@if`, `@for` directives (rendering logic)
- `FormsModule` — provides `[(ngModel)]` for two-way data binding

---

### 2.8 — `components/movie-list/movie-list.html` — The Template

```html
<div class="max-w-4xl mx-auto p-6">
  <h1 class="text-3xl font-bold mb-6">🎬 My Movies</h1>

  <!-- Add Movie Form -->
  <div class="bg-gray-100 rounded-lg p-6 mb-8">
    <input [(ngModel)]="newMovie.title" placeholder="Title" />
    <input [(ngModel)]="newMovie.genre" placeholder="Genre" />
    <input [(ngModel)]="newMovie.year" type="number" />
    <input [(ngModel)]="newMovie.rating" type="number" step="0.1" />
    <button (click)="addMovie()">Add Movie</button>
  </div>

  <!-- Movie List -->
  <div class="space-y-3">
    @for (movie of movies; track movie.id) {
      <div>
        <span>{{ movie.title }}</span>
        <span>({{ movie.year }})</span>
        <button (click)="deleteMovie(movie.id)">✕</button>
      </div>
    } @empty {
      <p>No movies yet. Add your first one above!</p>
    }
  </div>
</div>
```

**Angular Template Syntax — The Key Bindings:**

| Syntax | Name | What it does | Example |
|--------|------|-------------|---------|
| `{{ value }}` | Interpolation | Render a value as text | `{{ movie.title }}` |
| `[property]="value"` | Property binding | Set HTML property from TS | `[disabled]="isLoading"` |
| `(event)="method()"` | Event binding | Call TS method on event | `(click)="addMovie()"` |
| `[(ngModel)]="field"` | Two-way binding | Sync input ↔ TS property | `[(ngModel)]="newMovie.title"` |

**`[(ngModel)]` — Two-way binding explained:**
```
User types in input → newMovie.title updates automatically
newMovie.title changes in TS → input value updates automatically
```

The `[()]` "banana in a box" syntax is shorthand for:
```html
<!-- This:  -->
[(ngModel)]="newMovie.title"

<!-- Is the same as this: -->
[ngModel]="newMovie.title" (ngModelChange)="newMovie.title = $event"
```

**`@for` — The new Angular control flow (Angular 17+):**
```html
@for (movie of movies; track movie.id) {
  <!-- render each movie -->
} @empty {
  <!-- render this if movies array is empty -->
}
```

`track movie.id` — tells Angular to track list items by `id` for efficient re-renders.
Without `track`, Angular re-renders the entire list on every change.
With `track`, it only re-renders the items that actually changed.

**Laravel/Blade equivalent:**
```blade
@foreach($movies as $movie)
    <div>{{ $movie->title }}</div>
@endforeach

@forelse($movies as $movie)
    <div>{{ $movie->title }}</div>
@empty
    <p>No movies yet</p>
@endforelse
```

---

## 🟡 PART 3 — How Frontend & Backend Connect

### 3.1 — CORS (Cross-Origin Resource Sharing)

**ELI5:** Browsers have a security rule: "You can only make HTTP requests to
the same domain you loaded the page from."

Your Angular app loads from `http://localhost:4200`.
It tries to call `http://localhost:5277/api/movies`.
Different port = different origin = browser BLOCKS the request.

CORS is the permission system that lets your API say: "Hey browser, it's OK to
let `localhost:4200` talk to me."

```csharp
// In Program.cs — the API gives permission to Angular:
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // "Allow this origin"
              .AllowAnyHeader()                      // "Allow any HTTP headers"
              .AllowAnyMethod();                     // "Allow GET, POST, PUT, DELETE, etc."
    });
});

app.UseCors("AllowAngular");  // Apply the policy
```

**In production**, `WithOrigins` should be your actual domain:
```csharp
policy.WithOrigins("https://mymovies.yourdomain.com")
```

### 3.2 — The Full Request Cycle (End to End)

When you click "Add Movie", here's EXACTLY what happens:

```
1. User clicks "Add Movie" button in browser

2. Angular template: (click)="addMovie()" → calls addMovie() in MovieListComponent

3. MovieListComponent.addMovie() calls:
   this.movieService.addMovie(this.newMovie)

4. MovieService.addMovie() calls:
   this.http.post<Movie>('http://localhost:5277/api/movies', movie)

5. Angular's HttpClient:
   - Serializes the Movie object to JSON: {"title":"Inception","genre":"Sci-Fi",...}
   - Sends HTTP POST request to http://localhost:5277/api/movies
   - With header: Content-Type: application/json

6. Browser checks CORS:
   - Sends OPTIONS preflight request to API
   - API responds with CORS headers: "Access-Control-Allow-Origin: http://localhost:4200"
   - Browser: "OK, the API allows this origin, proceed"

7. .NET receives the request:
   - UseCors middleware → checks origin → allowed → proceed
   - UseAuthorization middleware → no auth required yet → proceed
   - MapControllers → route "POST /api/movies" → MoviesController.CreateMovie()

8. MoviesController.CreateMovie(Movie movie):
   - .NET deserializes JSON body → Movie C# object
   - _context.Movies.Add(movie) → stage for INSERT
   - _context.SaveChangesAsync() → SQL: INSERT INTO Movies (Title, Genre, ...) VALUES (...)
   - Database responds with new Id (e.g., 42)
   - Returns: HTTP 201 + JSON: {"id":42,"title":"Inception","genre":"Sci-Fi",...}

9. HTTP response travels back to browser

10. Angular's HttpClient receives 201 response:
    - Deserializes JSON → Movie TypeScript object
    - Calls the .subscribe() callback: movie => { this.movies.push(movie); }

11. Angular detects movies array changed → re-renders the list
    → New movie appears on screen
```

**The whole round trip typically takes 5-50ms on localhost.**

---

## 🟢 PART 4 — Key Concepts Glossary

### Dependency Injection (DI)

**ELI5:** Instead of a class creating the tools it needs, someone else creates
them and hands them in. The class just says "I need X" and it appears.

```csharp
// WITHOUT DI (bad — tightly coupled, hard to test)
public class MoviesController
{
    private AppDbContext _context = new AppDbContext(); // Hard-coded dependency
}

// WITH DI (good — loosely coupled, testable)
public class MoviesController
{
    private readonly AppDbContext _context;

    public MoviesController(AppDbContext context) // .NET hands this in
    {
        _context = context;
    }
}
```

### REST API

A **REST API** is a convention for structuring HTTP endpoints:

| HTTP Verb | URL                | Action         | Returns  |
|-----------|-------------------|----------------|----------|
| GET       | `/api/movies`      | Get all movies | 200 + [] |
| GET       | `/api/movies/5`    | Get one movie  | 200 + {} |
| POST      | `/api/movies`      | Create a movie | 201 + {} |
| PUT       | `/api/movies/5`    | Update a movie | 204      |
| DELETE    | `/api/movies/5`    | Delete a movie | 204      |

### HTTP Status Codes (the important ones)

| Code | Name | When used |
|------|------|-----------|
| 200 | OK | Successful GET |
| 201 | Created | Successful POST |
| 204 | No Content | Successful PUT/DELETE (nothing to return) |
| 400 | Bad Request | Invalid input |
| 401 | Unauthorized | Not logged in |
| 403 | Forbidden | Logged in but no permission |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Bug in your code |

### Observable vs Promise

| | Promise | Observable |
|--|---------|-----------|
| Values emitted | One | One or many |
| Lazy? | No (starts immediately) | Yes (starts on subscribe) |
| Cancellable? | No | Yes (unsubscribe) |
| Operators? | .then() | .pipe(map, filter, debounce...) |
| Angular uses | HttpClient returns Observable | Everywhere |

For simple HTTP calls, they behave nearly the same. Observables become powerful
for things like: real-time data, auto-complete (debounce), combining multiple requests.

### EF Core vs Raw SQL

```csharp
// EF Core (what you're using):
var movies = await _context.Movies
    .Where(m => m.Year > 2000)
    .OrderBy(m => m.Title)
    .Take(10)
    .ToListAsync();

// Raw SQL equivalent:
// SELECT * FROM Movies WHERE Year > 2000 ORDER BY Title LIMIT 10
```

EF Core translates LINQ → SQL automatically. It also handles:
- Migrations (schema management)
- Change tracking (knows what changed)
- Relationships (joins)
- Parameterization (prevents SQL injection automatically)

---

## 🔴 CRITICAL — Security Issue to Fix Now

Your `appsettings.Development.json` contains a real password.
**Never commit passwords to git.**

**Fix it:**
```bash
# From MyMovies.Api/ directory:
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=mymovies;User=root;Password=YourActualPassword;Port=3306"
```

Then update `appsettings.Development.json` to remove the password:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

And add it to `.gitignore`:
```
appsettings.Development.json
```

User secrets are stored in:
`C:\Users\YourName\AppData\Roaming\Microsoft\UserSecrets\{project-id}\secrets.json`
Outside your project — safe from git.

---

## 📝 PART 5 — Self-Assessment Questions

Answer these without looking. If you struggle on any, re-read that section.

**Architecture:**
1. What are the three main layers of this application and what does each do?
2. Why can't Angular call the .NET API without CORS configured?
3. What travels between Angular and .NET? (what format, what protocol)

**.NET Backend:**
4. What is `Program.cs` responsible for? Name its two main phases.
5. What is Dependency Injection and why do we use it?
6. What is `DbContext` and what does it represent?
7. What does `_context.SaveChangesAsync()` actually do to the database?
8. Why is `async`/`await` important? What happens if you use `.Result` instead?
9. What HTTP status code does `return NotFound()` produce?
10. What does `[Route("api/[controller]")]` resolve to for `MoviesController`?
11. What does `dotnet ef migrations add MyMigration` do? And `dotnet ef database update`?

**Angular Frontend:**
12. What is `main.ts` responsible for?
13. What is `app.config.ts` equivalent to in .NET?
14. What is the difference between a TypeScript `interface` and a `class`?
15. What is an `Observable` and when does the HTTP request actually fire?
16. What is `[(ngModel)]` and what does the `[()]` syntax mean?
17. What is `ngOnInit()` and why do you load data there instead of in the constructor?
18. What does `@for (movie of movies; track movie.id)` do? Why the `track`?
19. What is a Signal and how does it differ from a plain property?
20. Why does `MovieService` use `private apiUrl = 'http://localhost:5277/api/movies'`
    and where should this URL come from in a production app?

**Bonus (think, don't look):**
21. If you add a new property to the `Movie` class in C# but don't create a migration,
    what happens when you run the app?
22. If you add a `posterUrl` field to the TypeScript `Movie` interface but the backend
    doesn't send it, what value will it have?
23. What would break if you moved `app.UseCors("AllowAngular")` AFTER `app.MapControllers()`?

---

## ✅ Phase 1 Completion Checklist

Before moving to Phase 2, confirm:

- [ ] I can explain what `Program.cs` does in both phases (services vs middleware)
- [ ] I understand what `DbContext` is and why it has `DbSet<Movie>`
- [ ] I understand why `async`/`await` exists and how to use it correctly
- [ ] I understand what an `Observable` is and why `.subscribe()` triggers the HTTP call
- [ ] I understand what `[(ngModel)]` does and why `FormsModule` is needed
- [ ] I understand why CORS exists and how the policy is configured
- [ ] I have fixed the password in `appsettings.Development.json`
- [ ] I can trace the full path of a "Add Movie" click from button to database and back
- [ ] I can answer at least 17 of the 20 self-assessment questions without looking

**When all boxes are checked, you are ready for Phase 2: Authentication & Security.**
