using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class CobrosDetalle
{

    [JsonProperty]
    public int IdCobrosDetalle { get; private set; }

    [JsonProperty]
    public int IdCobros { get; set; }
        [JsonProperty]
    public int IdObrasSociales { get; set; }

    [JsonProperty]
    [DataType(DataType.Date)]
    public DateTime? FechaCobroDetalle { get; set; }

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal ImporteCobrado { get; set; }

 
    [JsonProperty]
    public string? TipoPago { get;  set; }

    [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal MontoDebito { get; set; }
    
    public string? MotivoDebito { get;  set; }

    public CobrosDetalle()
    {
        
    }
}