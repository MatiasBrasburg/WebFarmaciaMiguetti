using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace WebFarmaciaMiguetti.Models
{
    public class Liquidaciones
    {
        [Key]
           [JsonProperty]
        public int IdLiquidaciones { get; set; }

   [JsonProperty]
        public int IdMandataria { get; set; }

           [JsonProperty]
        [DataType(DataType.Date)]
        public DateTime? FechaPresentacion { get; set; }
    
    [JsonProperty]
        public string? Periodo { get; set; }

           [JsonProperty]
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] 
        public decimal TotalPresentado { get; set; }
    
           [JsonProperty]
        public string? Estado { get; set; }

           [JsonProperty]
        public string? Observaciones { get; set; }
        
        public Liquidaciones() { }
    }
}