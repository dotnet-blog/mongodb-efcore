using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api.Dtos;
using Samples.MongoDb.EFCore.Api.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Samples.MongoDb.EFCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        readonly MediaLibraryDbContext _dbContext;
        readonly IMapper _mapper;
        public MoviesController(
            MediaLibraryDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "List movies")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            var movies = await _dbContext.Movies.AsNoTracking().ToArrayAsync<Movie>();
            var movieViewModels = _mapper.Map<IEnumerable<MovieViewModel>>(movies);
            return Ok(movieViewModels);
        }

        [HttpGet("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MovieViewModel>), Description = "Movie details")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovie(string id)
        {
            var guidId = Guid.Parse(id);
            var movie = await _dbContext.Movies.AsNoTracking().SingleAsync<Movie>(m => m._id == guidId);
            var movieViewModel = _mapper.Map<MovieViewModel>(movie);
            return Ok(movieViewModel);
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Type = typeof(IEnumerable<Movie>), Description = "Add movie")]
        public async Task<ActionResult<IEnumerable<Movie>>> AddMovie(
            [FromBody] MovieAddModel movieAddModel
            )
        {
            var movie = _mapper.Map<Movie>(movieAddModel);
            await _dbContext.Movies.AddAsync(movie);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(actionName: nameof(GetMovie), 
                                   routeValues: new { id = movie._id.ToString("N") }, 
                                   value: new { id = movie._id.ToString("N") });
        }
    }
}
