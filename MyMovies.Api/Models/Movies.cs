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