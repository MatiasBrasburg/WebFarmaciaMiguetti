using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Cobros
{

    [JsonProperty]
    public int IdCobros { get; private set; }

    [JsonProperty]
    public int? IdLiquidaciones { get; set; }
    [JsonProperty]
    public int? IdObrasSociales { get;  set; }
    [JsonProperty]
    [DataType(DataType.Date)]
    public DateTime? FechaCobros { get; set; }

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal Precio { get; set; }

    [JsonProperty]
    public string NumeroComprobante { get; set; }
    [JsonProperty]
    public string? TipoPago { get;  set; }

    [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal MontoDebitos { get; set; }
    [JsonProperty]
    public string? MotivoDebito { get;  set; }
    

    public Cobros()
    {
        
    }
}