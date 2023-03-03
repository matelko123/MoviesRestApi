using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        Movie movie = request.MaptoMovie();
        bool result = await _movieRepository.CreateAsync(movie);
        return CreatedAtAction(nameof(Get), new { id = movie.Id}, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        Movie? movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        MovieResponse response = movie.MapToResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Movie> movies = await _movieRepository.GetAllAsync();
        MoviesResponse response = movies.MapToResponse();
        return Ok(response);
    }
}