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

  // GET /api/movies — Laravel equivalent: axios.get('/api/movies')
  getMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(this.apiUrl);
  }

  // GET /api/movies/:id
  getMovie(id: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.apiUrl}/${id}`);
  }

  // POST /api/movies
  addMovie(movie: Omit<Movie, 'id'>): Observable<Movie> {
    return this.http.post<Movie>(this.apiUrl, movie);
  }

  // DELETE /api/movies/:id
  deleteMovie(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}