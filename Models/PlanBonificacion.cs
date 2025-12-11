// Models/PlanBonificacion.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace WebFarmaciaMiguetti.Models
{
    public class PlanBonificacion
    {
        [Key]
        public int IdPlanBonificacion { get; set; }
        public int IdObraSocial { get; set; }

        [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
        [StringLength(50)]
        public string NombrePlan { get; set; }

        [Required(ErrorMessage = "La bonificación es obligatoria.")]
        // CAMBIO AQUÍ: Rango de 0 a 100
        [Range(0.0, 100.0, ErrorMessage = "La bonificación debe estar entre 0 y 100.")]
        public decimal Bonificacion { get; set; } 

        [StringLength(20)]
        public string NumeroBonificacion { get; set; }


        public PlanBonificacion()
        {
        }
    }
}