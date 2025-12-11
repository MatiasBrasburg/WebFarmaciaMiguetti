using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace WebFarmaciaMiguetti.Models
{
    public class PlanBonificacion
    {
        [Key]
            [JsonProperty]
        public int IdPlanBonificacion { get; set; }

    [JsonProperty]
        public int IdObraSocial { get; set; }

    [JsonProperty]
        [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
        [StringLength(50)]
        public string NombrePlan { get; set; }

    [JsonProperty]
        [Required(ErrorMessage = "La bonificación es obligatoria.")]
        [Range(0.0, 100.0, ErrorMessage = "La bonificación debe estar entre 0 y 100.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bonificacion { get; set; } 

    [JsonProperty]
        [StringLength(50)]
        public string NumeroBonificacion { get; set; }

        public PlanBonificacion() { }
    }
}