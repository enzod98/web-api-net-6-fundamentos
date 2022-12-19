using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers
{
    [ApiController]
    //[Route("api/autores")]  // api/autores => Ruta base
    [Route("api/[controller]")]  // con esta anotación queda igual que la línea de arriba
                                //  ya que agarra el nombgre del controlador
    public class AutoresController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public AutoresController(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet("configuraciones")]
        public ActionResult<string> obtenerConfig()
        {
            return configuration["apellido"];
        }


        // creamos varias rutas para la misma acción
        //[HttpGet("listado")] //    [GET]/api/autores/listado
        //[HttpGet("/listado")]   // [GET]/listado - teniendo la '/' al inicio, omitimos la ruta base
        [HttpGet] //    [GET]/api/autores
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            var autores = await context.Autores.ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpGet("{id:int}", Name = "obtenerAutor")]   
        // [GET]/api/autores/1/loQueSea/tambien(el último parámetro siendo opcional)
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(y => y.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            return autor == null
                ? NotFound()
                : Ok(mapper.Map<AutorDTOConLibros>(autor));
        }

        [HttpGet("{nombre}")]   // [GET]/api/autores/fulano
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute]string nombre)
        {
            var autores = await context.Autores
                            .Where(x => x.Nombre.Contains(nombre))
                            .ToListAsync();

            return autores == null
                ? NotFound()
                : Ok(mapper.Map<List<AutorDTO>>(autores));
        }

        [HttpPost] //    [POST]/api/autores
        public async Task<ActionResult> Post([FromBody]AutorCreacionDTO autorCreacionDTO)
        {

            var existeNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);
            if (existeNombre)
                return BadRequest("El nombre ingresado ya existe.");

            //Mapeo automático de clases
            var autorDB = mapper.Map<Autor>(autorCreacionDTO);

            context.Add(autorDB);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autorDB);

            //Con createdAsRoute devolvemos la ruta de creación del nuevo recurso en la cabecera de la 
            //respuesta HTTP
            return CreatedAtRoute("obtenerAutor", new { id= autorDB.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]   //[PUT] /api/autores/1
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
                return NotFound();

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]    //api/autores/1
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
                return NotFound();

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();

            return NoContent();

        }
    }
}
