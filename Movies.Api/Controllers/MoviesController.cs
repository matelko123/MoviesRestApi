using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, 
        CancellationToken token)
    {
        Movie movie = request.MapToMovie();
        bool result = await _movieService.CreateAsync(movie, token);
        return CreatedAtAction(nameof(GetV1), new { idOrSlug = movie.Id}, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug, 
        [FromServices] LinkGenerator linkGenerator,
        CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        
        Movie? movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
        
        if (movie is null)
        {
            return NotFound();
        }

        MovieResponse response = movie.MapToResponse();
        var movieObj = new { id = movie.Id };
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV1), values: new { idOrSlug = movie.Id }),
            Rel = "self",
            Type = "GET"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: movieObj),
            Rel = "self",
            Type = "PUT"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: movieObj),
            Rel = "self",
            Type = "DELETE"
        });
        
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request,
        CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        GetAllMoviesOptions options = request.MapToOptions().WithUser(userId);
        IEnumerable<Movie> movies = await _movieService.GetAllAsync(options, token);
        int movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
        MoviesResponse response = movies.MapToResponse(options.Page, options.PageSize, movieCount);
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, 
        [FromBody] UpdateMovieRequest request, 
        CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        Movie movie = request.MapToMovie(id);
        Movie? updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
        if (updatedMovie is null)
        {
            return NotFound();
        }

        MovieResponse response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, 
        CancellationToken token)
    {
        bool deleted = await _movieService.DeleteByIdAsync(id, token);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}