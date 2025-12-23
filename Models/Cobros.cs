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
    [Required]
    [DataType(DataType.Date)]
    public DateTime FechaCobro { get; set; }
    [JsonProperty]
    public string NumeroComprobante { get; set; }
    
[JsonProperty]
    public int? IdMandatarias { get; set; }
    public Cobros()
    {
        
    }
}