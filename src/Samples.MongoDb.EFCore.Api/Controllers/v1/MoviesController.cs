﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api.Dtos;
using Samples.MongoDb.EFCore.Api.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using MassTransit;
using Samples.MongoDb.EFCore.Api.Events;

namespace Samples.MongoDb.EFCore.Api.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        readonly MediaLibraryDbContext _dbContext;
        readonly IMapper _mapper;
        readonly StackExchange.Redis.IDatabase _redisDatabase;
        readonly IBus _bus;
        public MoviesController(
            MediaLibraryDbContext dbContext,
            IMapper mapper,
            StackExchange.Redis.IDatabase redisDatabase,
            IBus bus)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _redisDatabase = redisDatabase;
            _bus = bus;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "List movies")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            var movies = await _dbContext.Movies.AsNoTracking().ToArrayAsync();
            var movieViewModels = _mapper.Map<IEnumerable<MovieViewModel>>(movies);
            return Ok(movieViewModels);
        }

        [HttpGet("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "Retrieve movie details")]
        public async Task<ActionResult<IEnumerable<MovieViewModel>>> GetMovie(long id)
        {
            var movie = await _dbContext.Movies.AsNoTracking().SingleAsync(m => m._id == id);
            var movieViewModel = _mapper.Map<MovieViewModel>(movie);
            return Ok(movieViewModel);
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Type = typeof(long), Description = "Add movie")]
        public async Task<ActionResult<long>> AddMovie(
            [FromBody] MovieAddModel movieAddModel
            )
        {
            var movie = _mapper.Map<Movie>(movieAddModel);
            movie._id = await _redisDatabase.StringIncrementAsync("movie_id_sequence");
            await _dbContext.Movies.AddAsync(movie);
            await _dbContext.SaveChangesAsync();
            await _bus.Publish(new MovieAddedEvent(movie._id));
            return CreatedAtAction(actionName: nameof(GetMovie),
                                   routeValues: new { id = movie._id },
                                   value: movie._id);
        }

        [HttpDelete("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(void), Description = "Delete movie")]
        public async Task<ActionResult> DeleteMovie(long id)
        {
            var movie = await _dbContext.Movies.SingleAsync(m => m._id == id);

            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}