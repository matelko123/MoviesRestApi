using Movies.Contracts.Responses;
using Refit;

namespace Movies.Api.Sdk;

public interface IMoviesApi
{
    [Get(ApiEndpoints.Movies.Get)]
    Task<MovieResponse> GetMoviesAsync(string idOrSlug);
}