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
    public string Cuit { get; set; }

    [JsonProperty]
    public bool Activa { get; set; }

    
    public Mandatarias()
    {
        
    }
}