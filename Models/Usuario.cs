using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models;

public class Usuario
{

    [JsonProperty]
    public int IdUsuario { get; private set; }

    [JsonProperty]
    public string Contraseña { get; set; }
    
    [JsonProperty]
    public string RazonSocial { get; set; }
 [JsonProperty]
    public string Domicilio { get; set; }
  

    [JsonProperty]
    public long Cuit { get; set; }
     [JsonProperty]
    public string? Iva { get; set; }

        public Usuario()
    {
        
    }
}