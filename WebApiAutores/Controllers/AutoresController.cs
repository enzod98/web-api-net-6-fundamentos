using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    //[Route("api/autores")]  // api/autores => Ruta base
    [Route("api/[controller]")]  // con esta anotación queda igual que la línea de arriba
                                //  ya que agarra el nombgre del controlador
    public class AutoresController : ControllerBase
    {
        private readonly AppDbContext context;

        public AutoresController(AppDbContext context)
        {
            this.context = context;
        }

        // creamos varias rutas para la misma acción
        [HttpGet] //    [GET]/api/autores
        [HttpGet("listado")] //    [GET]/api/autores/listado
        [HttpGet("/listado")]   // [GET]/listado - teniendo la '/' al inicio, omitimos la ruta base
        public async Task<ActionResult<List<Autor>>> Get()
        {
            return await context.Autores.Include(x => x.Libros).ToListAsync();
        }

        [HttpGet("primero")] // [GET]/api/autores/primero
        public async Task<ActionResult<Autor>> PrimerAutor()
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        [HttpGet("{id:int}/{param2=ValorPorDefecto}/{param3?}")]   
        // [GET]/api/autores/1/loQueSea/tambien(el último parámetro siendo opcional)
        public async Task<ActionResult<Autor>> Get(int id, string param2, string param3)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            return autor == null
                ? NotFound()
                : Ok(autor);
        }
        
        [HttpGet("{nombre}")]   // [GET]/api/autores/fulano
        public async Task<ActionResult<Autor>> Get(string nombre)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre));
            return autor == null
                ? NotFound()
                : Ok(autor);
        }

        [HttpPost] //    [POST]/api/autores
        public async Task<ActionResult> Post(Autor autor)
        {
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]   //[PUT] /api/autores/1
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
                return NotFound();

            if (autor.Id != id)
                return BadRequest("El id del autor no coindice con el especificado");

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]    //api/autores/1
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
                return NotFound();

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();

            return Ok();

        }
    }
}
