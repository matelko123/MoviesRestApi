using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Movies.MinimalApi.Auth;
using Movies.MinimalApi.Mapping;

namespace Movies.MinimalApi.Endpoints.Movies;

public static class GetAllMovieEndpoint
{
    public const string Name = "GetAllMovies";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
            [AsParameters] GetAllMoviesRequest request, IMovieService movieService, 
            HttpContext context, CancellationToken token) =>
            {
                Guid? userId = context.GetUserId();
                GetAllMoviesOptions options = request.MapToOptions().WithUser(userId);
                IEnumerable<Movie> movies = await movieService.GetAllAsync(options, token);
                int movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                MoviesResponse response = movies.MapToResponse(
                    request.Page.GetValueOrDefault(PagedRequest.DefaultPage), 
                    request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize), 
                    movieCount);
                return TypedResults.Ok(response);
            })
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .WithName($"{Name}V1")
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters] GetAllMoviesRequest request, IMovieService movieService, 
                HttpContext context, CancellationToken token) =>
            {
                Guid? userId = context.GetUserId();
                GetAllMoviesOptions options = request.MapToOptions().WithUser(userId);
                IEnumerable<Movie> movies = await movieService.GetAllAsync(options, token);
                int movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                MoviesResponse response = movies.MapToResponse(
                    request.Page.GetValueOrDefault(PagedRequest.DefaultPage), 
                    request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize), 
                    movieCount);
                return TypedResults.Ok(response);
            })
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .WithName($"{Name}V2")
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(2.0)
            .CacheOutput("MovieCache");
        return app;
    }
}