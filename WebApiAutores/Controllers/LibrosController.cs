using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libroDB = await context.Libros
                        .Include(x => x.Comentarios)
                        .Include(x => x.AutoresLibros)
                        .ThenInclude(x => x.Autor)
                        .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null) return NotFound();

            libroDB.AutoresLibros = libroDB.AutoresLibros.OrderBy(x => x.Orden).ToList();
            return libroDB == null
                    ? NotFound()
                    : mapper.Map<LibroDTOConAutores>(libroDB);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresId == null || libroCreacionDTO.AutoresId.Count == 0)
                return BadRequest("No se puede crear un libro sin autor");

            var autoresDbId = await context.Autores.Where(x => libroCreacionDTO.AutoresId.Contains(x.Id))
                                .Select(x => x.Id).ToListAsync();
            if (autoresDbId.Count() != libroCreacionDTO.AutoresId.Count())
                return BadRequest("Verifique los ID de autores especificados");

            var libroDB = mapper.Map<Libro>(libroCreacionDTO);

            AsignarOrdenAutores(libroDB);

            context.Add(libroDB);

            await context.SaveChangesAsync();
            var libroDTO = mapper.Map<LibroDTO>(libroDB);

            return CreatedAtRoute("obtenerLibro", new { id= libroDB.Id }, libroDTO);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros.Include(x => x.AutoresLibros)
                            .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null) return NotFound();

            //al usar la misma instancia libroDB, se actualiza directamente en la memoria del EF
            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
                return BadRequest();

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null) return NotFound();

            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);
            patchDocument.ApplyTo(libroDTO, ModelState);
            
            var esValido = TryValidateModel(libroDTO);
                if(!esValido)
                return BadRequest();

            mapper.Map(libroDTO, libroDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);
            if (!existe)
                return NotFound();

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();

            return NoContent();

        }


        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                    libro.AutoresLibros[i].Orden = i;
        }
    }
}
