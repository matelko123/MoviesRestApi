using Microsoft.AspNetCore.OutputCaching;
using Movies.Application.Services;
using Movies.MinimalApi.Auth;

namespace Movies.MinimalApi.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";
    
    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
                Guid id, IMovieService movieService, 
                IOutputCacheStore outputCacheStore, CancellationToken token) =>
            {
                bool deleted = await movieService.DeleteByIdAsync(id, token);
                if (!deleted)
                {
                    return Results.NotFound();
                }

                await outputCacheStore.EvictByTagAsync("movies", token);
                return TypedResults.Ok();
            })
            .WithName(Name)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthConstants.AdminUserPolicyName);
        return app;
    }
}