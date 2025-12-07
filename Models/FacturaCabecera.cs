using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class FacturaCabecera
{

    [JsonProperty]
    public int IdFacturaCabecera { get; private set; }

    [JsonProperty]
    public DateTime? FechaHora { get; set; }
    [JsonProperty]
    public int IdObrasSociales { get;  set; }

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal TotalVenta { get; set; }


         [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(5,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal ProcentajeCobertura { get; set; }



         [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal MontoACargoSocio { get; set; }



         [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal MontoACargoOS { get; set; }

    [JsonProperty]
    public string? Estado { get; set; }
    [JsonProperty]
    public string? Observaciones { get;  set; }
  [JsonProperty]
    public string? UsuarioCarga { get;  set; }


    public FacturaCabecera()
    {
        
    }
}