using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Mandatarias
{

    [JsonProperty]
    public int IdMandatarias { get; private set; }

    [JsonProperty]
    public string RazonSocial { get; set; }
    
   [JsonProperty]
    public long Cuit { get; set; }

    [JsonProperty]
    public bool Activa { get; set; }

 [JsonProperty]
    public string? Descripcion { get; set; }
    
     [JsonProperty]
    public string? Direccion { get; set; }
    public Mandatarias()
    {
        
    }
}