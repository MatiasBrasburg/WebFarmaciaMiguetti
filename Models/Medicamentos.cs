using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Medicamentos
{

    [JsonProperty]
    public int IdMedicamentos { get; private set; }

    [JsonProperty]
    public string NombreComercial { get; set; }
    
      [JsonProperty]
    public string Drogas { get; set; }
  
    [JsonProperty]
    public string? Laboratorio { get; set; }

      [JsonProperty]
    public string? CodigoBarras { get; set; }


        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal PrecioVenta { get; set; }

    [JsonProperty]
    public int? StockActual { get; set; }

      [JsonProperty]
    public bool Activo { get; set; }
    
    public Medicamentos()
    {
        
    }
}