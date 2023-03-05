﻿using System.Data;
using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: token));

        if (result > 0)
        {
            foreach (string genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
            }
        }
        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
            select m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating
            from movies m
            left join ratings r on m.id = r.movieid
            left join ratings myr on m.id = myr.movieid
                and myr.userid = @userId
            where id = @id
            group by id, userrating
        """, new { id, userId }, cancellationToken: token));
        if (movie is null)
        {
            return null;
        }

        IEnumerable<string>? genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
            select name from genres where movieid = @id
            """, new { id }, cancellationToken: token));

        foreach (string genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
            select 
                m.*, 
                round(avg(r.rating), 1) as rating, 
                myr.rating as userrating
            from movies m
            left join ratings r on m.id = r.movieid
            left join ratings myr on m.id = myr.movieid
                and myr.userid = @userId
            where slug = @slug
            group by id, userrating
        """, new { slug, userId }, cancellationToken: token));
        if (movie is null)
        {
            return null;
        }

        IEnumerable<string>? genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
            select name from genres where movieid = @id
            """, new { id = movie.Id }, cancellationToken: token));

        foreach (string genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        var result = await connection.QueryAsync(new CommandDefinition("""
        select 
            m.*, 
            string_agg(g.name, ',') as genres,
            round(avg(r.rating), 1) as rating, 
            myr.rating as userrating
        from movies m 
            left join genres g on m.id = g.movieid
            left join ratings r on m.id = r.movieid
            left join ratings myr on m.id = myr.movieid
                and myr.userid = @userId 
        where (@title is null or m.title like ('%' || @title || '%'))
        and (@yearofrelease is null or m.yearofrelease = @yearofrelease)   
        group by id, userrating
        """,new
        {
            userId = options.UserId,
            title = options.Title,
            yearofrelease = options.YearOfRelease
        }, cancellationToken: token));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        using IDbTransaction transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
            delete from genres where movieid = @id
            """, new { id = movie.Id }, cancellationToken: token));
        
        foreach (string genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
        }

        int result = await connection.ExecuteAsync(new CommandDefinition("""
        update movies set slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
        where id = @Id
        """, movie, cancellationToken: token));
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        using IDbTransaction transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new CommandDefinition("""
            delete from genres where movieid = @id  
            """, new { id }, cancellationToken: token));
        
        int result = await connection.ExecuteAsync(new CommandDefinition("""
            delete from movies where id = @id
            """, new { id }, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
            select count(1) from movies where id = @id
            """, new { id }, cancellationToken: token));
    }
}