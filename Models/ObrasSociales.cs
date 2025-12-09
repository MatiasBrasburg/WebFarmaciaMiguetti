using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class ObrasSociales
{

    [JsonProperty]
    public int IdObrasSociales { get; private set; }

    [JsonProperty]
    public int IdMandataria { get; set; }
    
    [JsonProperty]
    public string Nombre { get; set; }

  [JsonProperty]
    public int CodigoBonificacion { get; set; }

    [JsonProperty]
    public bool? EsPrepaga { get; set; }
     [JsonProperty]
    public bool Activa { get; set; }

    [JsonProperty]
    public string NombreCodigoBonificacion { get; set; }
    public ObrasSociales()
    {
        
    }
}