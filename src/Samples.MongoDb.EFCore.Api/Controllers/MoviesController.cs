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
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Movie>), Description = "Movies")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            await Task.CompletedTask;
            var movies = _dbContext.Movies.AsNoTracking().AsEnumerable<Movie>();
            return Ok(movies); 
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.NoContent, Type = typeof(IEnumerable<Movie>), Description = "Movies")]
        public async Task<ActionResult<IEnumerable<Movie>>> AddMovie(
            [FromBody] MovieAddModel movieAddModel
            )
        {
            await Task.CompletedTask;
            var movie = _mapper.Map<Movie>(movieAddModel);
            _dbContext.Movies.Add(movie);
            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}
