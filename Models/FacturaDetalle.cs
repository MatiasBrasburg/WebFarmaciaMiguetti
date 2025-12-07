using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class FacturaDetalle
{


    [JsonProperty]
    public int IdFacturDetalle{ get; private set; }

    [JsonProperty]
    public int IdFacturaCabecera { get; private set; }

    [JsonProperty]
    public int IdMedicamentos { get; set; }
    [JsonProperty]
    public int Cantidad { get;  set; }

        [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal PrecioUnitarioHistorico { get; set; }

         [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18,2)")] // <--- OBLIGATORIO PARA SQL
        [DataType(DataType.Currency)] // Para que se vea con el signo $ en la vista
       public decimal Subtotal { get; set; }

    

    public FacturaDetalle()
    {
        
    }
}