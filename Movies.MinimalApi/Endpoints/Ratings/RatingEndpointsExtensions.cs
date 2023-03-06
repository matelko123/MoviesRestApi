namespace Movies.MinimalApi.Endpoints.Ratings;

public static class RatingEndpointsExtensions
{
    public static IEndpointRouteBuilder MapRatingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRateMovie();
        app.MapDeleteRating();
        app.MapGetUserRatings();
        return app;
    }
}