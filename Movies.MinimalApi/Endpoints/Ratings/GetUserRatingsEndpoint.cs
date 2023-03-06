using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Responses;
using Movies.MinimalApi.Auth;
using Movies.MinimalApi.Mapping;

namespace Movies.MinimalApi.Endpoints.Ratings;

public static class GetUserRatingsEndpoint
{
    public const string Name = "GetUserRatings";

    public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Rate, async (
                HttpContext context, 
                IRatingService ratingService, CancellationToken token) =>
            {
                Guid? userId = context.GetUserId();
                IEnumerable<MovieRating> ratings = await ratingService.GetRatingsForUserAsync(userId!.Value, token);
                IEnumerable<MovieRatingResponse> ratingsResponse = ratings.MapToResponse();
                return TypedResults.Ok(ratingsResponse);
            })
            .WithName(Name)
            .Produces<IEnumerable<MovieRatingResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization();
        return app;
    }
}