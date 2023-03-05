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
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateMovie([FromRoute] Guid id,
        [FromBody] RateMovieRequest request, CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        bool result = await _ratingService.RateMovieAsync(id, request.Rating, userId!.Value, token);
        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRating([FromRoute] Guid id,
        CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        bool result = await _ratingService.DeleteRatingAsync(id, userId!.Value, token);
        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRatings(CancellationToken token)
    {
        Guid? userId = HttpContext.GetUserId();
        IEnumerable<MovieRating> ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);
        IEnumerable<MovieRatingResponse> ratingsResponse = ratings.MapToResponse();
        return Ok(ratingsResponse);
    }
}