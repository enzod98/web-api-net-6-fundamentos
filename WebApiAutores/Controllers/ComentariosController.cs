using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController: ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public ComentariosController(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var comentarios = await context.Comentarios
                                    .Where(x => x.LibroId == libroId)
                                    .ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioDTO);
            comentario.LibroId = libroId;

            context.Add(comentario);
            await context.SaveChangesAsync();
            return Ok();

        }
    }
}
