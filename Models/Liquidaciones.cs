using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Liquidaciones
{

    [JsonProperty]
    public int IdLiquidaciones { get; private set; }

    [JsonProperty]
    public int IdMandataria { get; set; }
    
  
    [JsonProperty]
    [DataType(DataType.Date)]
    public DateTime? FechaPresentacion { get; set; }


    [JsonProperty]
    public string? Periodo { get; set; }

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal TotalPresentado { get; set; }

    [JsonProperty]
    public string? Estado { get; set; }
    
    public Liquidaciones()
    {
        
    }
}