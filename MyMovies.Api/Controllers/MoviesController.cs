using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMovies.Api.Data;
using MyMovies.Api.Models;

namespace MyMovies.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // This becomes /api/movies
public class MoviesController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructor — .NET INJECTS the database context automatically
    // Laravel equivalent: You never see this because Eloquent just "knows" the DB
    public MoviesController(AppDbContext context)
    {
        _context = context;
    }

    // ──── GET /api/movies ────
    // Laravel equivalent: public function index() { return Movie::all(); }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
    {
        return await _context.Movies.ToListAsync();
    }

    // ──── GET /api/movies/5 ────
    // Laravel equivalent: public function show($id) { return Movie::findOrFail($id); }
    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound();   // Returns 404 (like abort(404) in Laravel)
        }

        return movie;
    }

    // ──── POST /api/movies ────
    // Laravel equivalent: public function store(Request $request) { return Movie::create($request->all()); }
    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovie(Movie movie)
    {
        _context.Movies.Add(movie);              // Stage it (like git add)
        await _context.SaveChangesAsync();       // Commit it (like git commit)

        // Returns 201 Created + the movie with its new Id
        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
    }

    // ──── PUT /api/movies/5 ────
    // Laravel equivalent: public function update(Request $request, $id) { ... }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMovie(int id, Movie movie)
    {
        if (id != movie.Id)
        {
            return BadRequest();   // Returns 400 (like abort(400))
        }

        _context.Entry(movie).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Movies.AnyAsync(m => m.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();   // Returns 204 (update successful, nothing to return)
    }

    // ──── DELETE /api/movies/5 ────
    // Laravel equivalent: public function destroy($id) { Movie::findOrFail($id)->delete(); }
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

        return NoContent();
    }
}