using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFarmaciaMiguetti.Models
{
    public class LiquidacionesDetalle
    {
        [Key]
            [JsonProperty]
        public int IdLiquidacionDetalle { get; set; }

            [JsonProperty]
        public int IdLiquidaciones { get; set; } 

        // Columnas de Relación
            [JsonProperty]
        public int IdObrasSociales { get; set; }
            [JsonProperty]
        public int IdPlanBonificacion { get; set; } 
    [JsonProperty]
        public int CantidadRecetas { get; set; }
    [JsonProperty]
        // Columnas de Importes (Con formato Moneda)
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] // ✅ RESTAURADO
        public decimal TotalBruto { get; set; } 
    [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] // ✅ RESTAURADO
        public decimal MontoCargoOS { get; set; }
    [JsonProperty]
        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)] // ✅ RESTAURADO
        public decimal MontoBonificacion { get; set; } // (Neto)

        public LiquidacionesDetalle() { }
    }
}