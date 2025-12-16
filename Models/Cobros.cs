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
    public string NumeroComprobante { get; set; }
    

    public Cobros()
    {
        
    }
}