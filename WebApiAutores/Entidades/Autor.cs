using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor : IValidatableObject
    {
        //VALIDACIONES POR ATRIBUTO
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:5, ErrorMessage ="El campo {0} no debe tener más de {1} caracteres")]
        //[PrimeraLetraMayuscula]   //Validación personalizada
        public string Nombre { get; set; }
        //[Range(18,99)]
        //[NotMapped]//Propiedades que no corresponden a la tabla en BD
        //public int Edad { get; set; }
        //[CreditCard]//Valida numeración de la TC
        //public string TarjetaCredito { get; set; }
        //[Url]//Valida que sea URL válida
        //public string URL { get; set; }
        public List<Libro> Libros { get; set; }
        public int Menor { get; set; }
        public int Mayor{ get; set; }


        //VALIDACIONES POR MODELO
        //Para que corran las validaciones por modelo, primero deben pasar todas las validaciones por atributo
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {

                var primeraLetra = Nombre[0].ToString();
                if (primeraLetra != primeraLetra.ToUpper())
                    yield return new ValidationResult("La primera letra debe estar en mayúscula", 
                            new string[] {nameof(Nombre)}
                        );

            }
            if(Menor > Mayor)
                yield return new ValidationResult("Este valor no puede ser más grande que el campo Mayor",
                            new string[] { nameof(Menor) }
                        );
        }
    }
}
