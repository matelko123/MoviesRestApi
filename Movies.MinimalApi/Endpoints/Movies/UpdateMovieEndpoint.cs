using Microsoft.AspNetCore.OutputCaching;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Movies.MinimalApi.Auth;
using Movies.MinimalApi.Mapping;

namespace Movies.MinimalApi.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";
    
    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
                Guid id, UpdateMovieRequest request, IMovieService movieService, 
            IOutputCacheStore outputCacheStore, HttpContext context, CancellationToken token) =>
        {
            Guid? userId = context.GetUserId();
            Movie movie = request.MapToMovie(id);
            Movie? updatedMovie = await movieService.UpdateAsync(movie, userId, token);
            if (updatedMovie is null)
            {
                return Results.NotFound();
            }

            await outputCacheStore.EvictByTagAsync("movies", token);
            MovieResponse response = updatedMovie.MapToResponse();
            return TypedResults.Ok(response);
        })
        .WithName(Name)
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);
        return app;
    }
}