using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFarmaciaMiguetti.Models
{
    public class LiquidacionDetalle
    {
        [Key]
        [JsonProperty]
        public int IdLiquidacionDetalle { get; set; }

        [JsonProperty]
        public int IdLiquidaciones { get; set; } 

        [JsonProperty]
        public int IdObrasSociales { get; set; }

        [JsonProperty]
        public int IdPlanBonificacion { get; set; } 

        [NotMapped] 
        [JsonProperty]
        public string? NombrePlan { get; set; }

      
        [NotMapped] 
        [JsonProperty]
        public string? NombreObraSocial { get; set; }
       

        [JsonProperty]
        public int CantidadRecetas { get; set; }

        [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] 
        public decimal TotalBruto { get; set; } 

        [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] 
        public decimal MontoCargoOS { get; set; }

        [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] 
        public decimal MontoBonificacion { get; set; } 

        [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] 
        public decimal SaldoPendiente { get; set; }
        
        [JsonProperty]
        public bool Pagado { get; set; }
   
        public DateTime? FechaCancelacion { get; set; }

        public LiquidacionDetalle() { }
    }
}