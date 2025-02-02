﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api.Dtos;
using Samples.MongoDb.EFCore.Api.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using MassTransit;
using Samples.MongoDb.EFCore.Api.Events;
using Samples.MongoDb.EFCore.Api.Dtos.MediaLibrary;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace Samples.MongoDb.EFCore.Api.Controllers.v1
{
    /// <summary>
    /// Movie library demo
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(ErrorModel), Description = "Validation error details")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(ErrorModel), Description = "Application error details")]
    public class MoviesController : ControllerBase
    {
        readonly MediaLibraryDbContext _dbContext;
        readonly IMapper _mapper;
        readonly StackExchange.Redis.IDatabase _redisDatabase;
        readonly IBus _bus;
        readonly IValidator<MovieAddModel> _movieAddModelValidator;
        readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;
        public MoviesController(
            MediaLibraryDbContext dbContext,
            IMapper mapper,
            StackExchange.Redis.IDatabase redisDatabase,
            IBus bus,
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IValidator<MovieAddModel> movieAddModelValidator)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _redisDatabase = redisDatabase;
            _bus = bus;
            _apiBehaviorOptions = apiBehaviorOptions;
            _movieAddModelValidator = movieAddModelValidator;
        }

        /// <summary>
        /// Find movies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "List movies")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            var movies = await _dbContext.Movies.AsNoTracking().ToArrayAsync();
            var movieViewModels = _mapper.Map<IEnumerable<MovieViewModel>>(movies);
            return Ok(movieViewModels);
        }

        /// <summary>
        /// Get movie
        /// </summary>
        /// <param name="id">Movie id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "Retrieve movie details")]
        public async Task<IActionResult> GetMovie(long id)
        {
            var movie = await _dbContext.Movies.AsNoTracking().SingleAsync(m => m._id == id);
            var movieViewModel = _mapper.Map<MovieViewModel>(movie);
            return Ok(movieViewModel);
        }

        /// <summary>
        /// Adds new movie to library
        /// </summary>
        /// <param name="movieAddModel"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Type = typeof(long), Description = "Add movie")]
        public async Task<IActionResult> AddMovie(
            [FromBody] MovieAddModel movieAddModel
        )
        {
            FluentValidation.Results.ValidationResult validationResult = await _movieAddModelValidator.ValidateAsync(movieAddModel);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
            }

            var movie = _mapper.Map<Movie>(movieAddModel);
            movie._id = await _redisDatabase.StringIncrementAsync("movie_id_sequence");
            movie.DateTimeCreated = DateTime.UtcNow;
            await _dbContext.Movies.AddAsync(movie);
            await _dbContext.SaveChangesAsync();
            await _bus.Publish(new MovieAddedEvent(movie._id));
            return CreatedAtAction(actionName: nameof(GetMovie),
                                   routeValues: new { id = movie._id },
                                   value: movie._id);
        }

        /// <summary>
        /// Removes movie from the library
        /// </summary>
        /// <param name="id">Movie id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(void), Description = "Delete movie")]
        public async Task<IActionResult> DeleteMovie(long id)
        {
            var movie = await _dbContext.Movies.SingleAsync(m => m._id == id);

            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
