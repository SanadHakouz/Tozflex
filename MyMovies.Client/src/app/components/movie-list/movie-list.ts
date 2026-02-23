import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MovieService } from '../../services/movie.service';
import { Movie } from '../../models/movie';

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './movie-list.html',
  styleUrl: './movie-list.css'
})
export class MovieListComponent implements OnInit {
  movies: Movie[] = [];

  // Form fields (like $request->title, $request->genre in Laravel)
  newMovie = {
    title: '',
    genre: '',
    year: new Date().getFullYear(),
    rating: 0
  };

  constructor(private movieService: MovieService) { }

  // Runs when component loads (like Laravel's controller index method)
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
      this.movies.push(movie);       // Add to the list without reloading
      this.newMovie = {              // Reset the form
        title: '',
        genre: '',
        year: new Date().getFullYear(),
        rating: 0
      };
    });
  }

  deleteMovie(id: number): void {
    this.movieService.deleteMovie(id).subscribe(() => {
      this.movies = this.movies.filter(m => m.id !== id);  // Remove from list
    });
  }
}