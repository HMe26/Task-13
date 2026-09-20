namespace CinemaApp.ViewModels;

public class MovieWithRelatedVM
{
    public Movie Movie { get; set; } = null!;
    public IEnumerable<Actor> Actors { get; set; } = new List<Actor>();
    public IEnumerable<Movie> RelatedMovies { get; set; } = new List<Movie>();
}
