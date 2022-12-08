using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public LibrosController(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            var libroDB = await context.Libros
                        .Include(x => x.Comentarios)
                        .FirstOrDefaultAsync(x => x.Id == id);

            return libroDB == null
                    ? NotFound()
                    : mapper.Map<LibroDTO>(libroDB);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroDTO)
        {
            //var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            //if (!existeAutor)
            //    return BadRequest("No existe el autor indicado");
            var libroDB = mapper.Map<Libro>(libroDTO);
            context.Add(libroDB);

            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
