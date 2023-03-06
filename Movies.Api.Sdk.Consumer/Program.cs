using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Refit;

ServiceCollection services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(x => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async () => await x.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(x =>
        x.BaseAddress = new Uri("https://localhost:5001"));

ServiceProvider provider = services.BuildServiceProvider();

IMoviesApi moviesApi = provider.GetRequiredService<IMoviesApi>();
MovieResponse movie = await moviesApi.GetMovieAsync("nick-the-greek-2023");

MovieResponse newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Spiderman 2",
    YearOfRelease = 2002,
    Genres = new[] { "Action" }
});

await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
    Title = "Spiderman 2",
    YearOfRelease = 2003,
    Genres = new[] { "Action" }
});

await moviesApi.DeleteMovieAsync(newMovie.Id);

GetAllMoviesRequest request = new GetAllMoviesRequest()
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
};

MoviesResponse movies = await moviesApi.GetMoviesAsync(request);

foreach (MovieResponse movieResponse in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}
