using Movies.MinimalApi.Endpoints.Movies;
using Movies.MinimalApi.Endpoints.Ratings;

namespace Movies.MinimalApi.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        app.MapRatingEndpoints();
        return app;
    }
}