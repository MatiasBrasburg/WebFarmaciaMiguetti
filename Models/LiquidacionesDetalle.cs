using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class LiquidacionesDetalle
{
    [JsonProperty]
    public int IdLiquidacionesDetalle { get; private set; }

    [JsonProperty]
    public int IdLiquidaciones { get; private set; }

    [JsonProperty]
    public int IdFacturaCabecera { get; set; }
    
  
    

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
    public decimal ImporteReclamado { get; set; }

    
    public LiquidacionesDetalle()
    {
        
    }
}