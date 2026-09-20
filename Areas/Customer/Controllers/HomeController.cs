using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Areas.Customer.Controllers;

[Area(AreaConstants.CUSTOMER_AREA)]
public class HomeController : Controller
{
    private readonly IRepository<Movie> _movieRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Cinema> _cinemaRepository;
    private readonly IBulkRepository<MovieActor> _movieActorRepository;
    private readonly IRepository<Actor> _actorRepository;

    public HomeController(IRepository<Movie> movieRepository,
        IRepository<Category> categoryRepository,
        IRepository<Cinema> cinemaRepository,
        IBulkRepository<MovieActor> movieActorRepository,
        IRepository<Actor> actorRepository)
    {
        _movieRepository = movieRepository;
        _categoryRepository = categoryRepository;
        _cinemaRepository = cinemaRepository;
        _movieActorRepository = movieActorRepository;
        _actorRepository = actorRepository;
    }

    public IActionResult Index(MovieFilterVM movieFilterVM, int page = 1, int size = 8)
    {
        var movies = _movieRepository.Get(e => e.Status, includes: [e => e.Category, e => e.Cinema]);

        // Filter

        if (movieFilterVM.name is not null)
            movies = movies.Where(e => e.Name.ToLower().Contains(movieFilterVM.name.ToLower()));

        if (movieFilterVM.minPrice is not null)
            movies = movies.Where(e => e.Price >= movieFilterVM.minPrice);

        if (movieFilterVM.maxPrice is not null)
            movies = movies.Where(e => e.Price < movieFilterVM.maxPrice);

        if (movieFilterVM.categoryId is not null)
            movies = movies.Where(e => e.CategoryId == movieFilterVM.categoryId);

        if (movieFilterVM.cinemaId is not null)
            movies = movies.Where(e => e.CinemaId == movieFilterVM.cinemaId);

        // Pagination

        var totalPages = Math.Ceiling(movies.Count() / (double)size);
        movies = movies.Skip((page - 1) * size).Take(size);

        var categories = _categoryRepository.Get(e => e.Status).ToList();
        var cinemas = _cinemaRepository.Get(e => e.Status).ToList();

        return View(new MovieWithFilterVM
        {
            Movies = movies.ToList(),
            Categories = categories,
            Cinemas = cinemas,
            Name = movieFilterVM.name ?? "",
            MinPrice = movieFilterVM.minPrice,
            MaxPrice = movieFilterVM.maxPrice,
            CategoryId = movieFilterVM.categoryId,
            CinemaId = movieFilterVM.cinemaId,
            TotalPages = totalPages,
            CurrentPage = page,
        });
    }

    public IActionResult Details(int id)
    {
        var movie = _movieRepository.GetOne(e => e.Id == id && e.Status,
            includes: [e => e.Category, e => e.Cinema, e => e.SubImages],
            tracked: false);

        if (movie is null)
            return RedirectToAction(nameof(NotFoundPage));

        var actorIds = _movieActorRepository.Get(e => e.MovieId == movie.Id).Select(e => e.ActorId).ToList();
        var actors = _actorRepository.Get(e => actorIds.Contains(e.Id)).ToList();

        var relatedMovies = _movieRepository
            .Get(e => e.CategoryId == movie.CategoryId && e.Id != movie.Id && e.Status, includes: [e => e.Category])
            .Take(4);

        return View(new MovieWithRelatedVM
        {
            Movie = movie,
            Actors = actors,
            RelatedMovies = relatedMovies,
        });
    }

    public IActionResult NotFoundPage()
    {
        return View();
    }
}
