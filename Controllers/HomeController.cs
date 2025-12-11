using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebFarmaciaMiguetti.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq; 
using System.Text.Json; // <--- OBLIGATORIO PARA LEER EL JSON

namespace WebFarmaciaMiguetti.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        var usuarioLogueado = HttpContext.Session.GetString("UsuarioAutorizado");
        if (string.IsNullOrEmpty(usuarioLogueado))
        {
            return RedirectToAction("Index", "Account"); 
        }
        return View(); 
    }
    
    public IActionResult Home()
    {
        return View("Index");
    }
    
    public IActionResult IrAAltasModificacionesMandatarias()
    {
        List<Mandatarias> ListMandatarias = BD.TraerListaMandatarias();
        ViewBag.ListaMandatarias = ListMandatarias;
        return View("AltasModificacionesMandatarias");
    }
    
    public IActionResult ModificarMandataria(int IdMandataria, string QueToco, string? NombreMandataria, long Cuit, string? Descripcion, string? Direccion)
    {
        string nuevoNombreMandataria = "";
        
        if(QueToco == "Modificar")
        {
            Mandatarias ObjtMandataria = BD.TraerMandatariaPorId(IdMandataria);

            if(!string.IsNullOrEmpty(NombreMandataria))
            {
                nuevoNombreMandataria = NombreMandataria;
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "Llene el nombre de la mandataria, por favor." });
            }

            if(ObjtMandataria != null)
            {
                BD.ModificarMandataria(IdMandataria, nuevoNombreMandataria, Cuit, Descripcion, Direccion);
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "No se encontró la mandataria." });
            }
        }
        else if(QueToco == "Agregar" )
        {
            if(!string.IsNullOrEmpty(NombreMandataria))
            {
                BD.AgregarMandataria(NombreMandataria, Cuit, Descripcion, Direccion);
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "Llene el nombre de la mandataria, por favor." });
            }
        }
        else if(QueToco == "Eliminar")
        {
            BD.EliminarMandataria(IdMandataria);
        }
        else
        {
            return View("Error", new ErrorViewModel { RequestId = "Operación no definida." });
        }
    
        return RedirectToAction("IrAAltasModificacionesMandatarias", "Home");
    }


    public IActionResult IrAAltasModificacionesOS()
    {
        List<ObrasSociales> ListOS = BD.TraerListaOS();
        ViewBag.ListaObrasSociales = ListOS;

        List<Mandatarias> ListMandatarias = BD.TraerListaMandatarias();
        ViewBag.ListaMandatarias = ListMandatarias;
        
        return View("AltasModificacionesObrasSociales");
    }

    // ==================================================================
    // ACCIÓN ABM DE OBRAS SOCIALES (LIMPIA - SOLO OOSS)
    // ==================================================================
    public IActionResult ABM_OS(int IdOS, string QueToco, int? IdMandatarias, string? NombreOS, bool? EsPrepaga, bool? Activa)
    {
        bool valorEsPrepaga = EsPrepaga ?? false;
        bool valorActiva = Activa ?? false; 
        int nuevoIdMandataria = 0;
        string nuevoNombreOS = "";

        ObrasSociales ObjtOSS = null;
        if (IdOS > 0)
        {
            ObjtOSS = BD.TraerOSPorId(IdOS);
        }

        if (QueToco == "Modificar")
        {
            if (!string.IsNullOrEmpty(NombreOS))
            {
                nuevoNombreOS = NombreOS;
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "El nombre de la Obra Social no puede estar vacío." });
            }

            if (ObjtOSS != null)
            {
                if (IdMandatarias.HasValue && IdMandatarias.Value > 0)
                {
                    nuevoIdMandataria = IdMandatarias.Value;
                    Mandatarias mandataria = BD.TraerMandatariaPorId(nuevoIdMandataria);
                    
                    BD.ModificarOS(IdOS, nuevoNombreOS, nuevoIdMandataria, mandataria.RazonSocial, valorEsPrepaga, valorActiva);
                }
                else
                {
                    Mandatarias mandataria = BD.TraerMandatariaPorId(ObjtOSS.IdMandataria);
                    BD.ModificarOS(IdOS, nuevoNombreOS, ObjtOSS.IdMandataria, mandataria.RazonSocial, valorEsPrepaga, valorActiva);
                }
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "No se encontró la Obra Social para modificar." });
            }
        }
        else if (QueToco == "Agregar")
        {
            int idMand = IdMandatarias ?? 0;
            string nombre = NombreOS ?? "";

            if (string.IsNullOrEmpty(nombre))
            {
                return View("Error", new ErrorViewModel { RequestId = "Debe escribir un nombre para la Obra Social." });
            }

            if (idMand > 0)
            {
                Mandatarias mandataria = BD.TraerMandatariaPorId(idMand);
                if (mandataria != null)
                {
                    BD.AgregarOS( mandataria.IdMandatarias, nombre, mandataria.RazonSocial, valorEsPrepaga, valorActiva);
                }
                else
                {
                    return View("Error", new ErrorViewModel { RequestId = "La Mandataria seleccionada no existe en la Base de Datos." });
                }
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "Debe seleccionar una Mandataria válida." });
            }
        }
        else if (QueToco == "Eliminar")
        {
            BD.EliminarOS(IdOS);
        }
       
        else
        {
            return View("Error", new ErrorViewModel { RequestId = "Operación no definida." });
        }

        return RedirectToAction("IrAAltasModificacionesOS", "Home");
    }


    // -----------------------------------------------------------------------------------
    // NUEVOS ENDPOINTS AJAX PARA PLANES DE BONIFICACIÓN (FETCH API)
    // -----------------------------------------------------------------------------------

    [HttpGet]
    public IActionResult ObtenerPlanesPorObrasSociales(int idObraSocial)
    {
        try
        {
            List<PlanBonificacion> planes = BD.TraerPlanBonificacionPorId(idObraSocial);
            return Json(new { success = true, data = planes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener planes para ObrasSociales ID: {Id}", idObraSocial);
            return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
        }
    }
    
    [HttpPost]
    public IActionResult GuardarPlanBonificacion([FromBody] PlanBonificacion plan)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "Datos del plan inválidos.", errors = errors });
        }

        try
        {
            if (plan.IdPlanBonificacion == 0) 
            {
                 BD.InsertarPlan(plan);
                 return Json(new { success = true, message = "¡Plan CREADO con éxito!" });
            }
            else 
            {
                BD.ActualizarPlan(plan);
                return Json(new { success = true, message = "¡Plan MODIFICADO con éxito!" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar PlanBonificacion.");
            return StatusCode(500, new { success = false, message = "Error crítico al guardar el plan: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult EliminarPlanBonificacion([FromBody] int idPlan)
    {
        try
        {
            BD.EliminarPlan(idPlan);
            return Json(new { success = true, message = "¡Plan eliminado con éxito!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar PlanBonificacion ID: {Id}", idPlan);
            return StatusCode(500, new { success = false, message = "Error interno al eliminar: " + ex.Message });
        }
    }



    public IActionResult IrAMostrarModificaciones()
    {
        ViewBag.Mandatarias = BD.TraerListaMandatarias();
        ViewBag.ObrasSociales = BD.TraerListaOS();
        ViewBag.ListaLiquidaciones = BD.TraerListaLiquidacionesCompleta();
        return View("MostrarModificaciones");
    }


  // =========================================================================
    // FUNCIÓN ÚNICA PARA GESTIÓN DE LIQUIDACIONES (Show, Guardar, Eliminar)
    // =========================================================================
    public IActionResult MostrarModificaciones(
        string QueToco, 
        int? IdLiquidacion, 
        int? IdMandataria, 
        DateTime? Fecha, 
        string? Observaciones, 
        string? DetallesJson // <--- Aquí llega la lista de Obras Sociales convertida en texto
    )
    {
        try 
        {
            // -----------------------------------------------------------------
            // CASO 1: GUARDAR (Alta o Edición) - Recibe JSON y guarda todo
            // -----------------------------------------------------------------
            if (QueToco == "Guardar")
            {
                // 1. Validaciones básicas
                if (IdMandataria == null || IdMandataria == 0) 
                    return Json(new { success = false, message = "Debe seleccionar una Mandataria." });

                if (string.IsNullOrEmpty(DetallesJson) || DetallesJson == "[]")
                    return Json(new { success = false, message = "La liquidación debe tener al menos una Obra Social." });

                // 2. Deserializar el JSON (Convertir texto a Objetos C#)
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<LiquidacionDetalleDTO> listaDetalles;
                
                try {
                    listaDetalles = JsonSerializer.Deserialize<List<LiquidacionDetalleDTO>>(DetallesJson, opciones);
                } catch {
                    return Json(new { success = false, message = "Error al leer los detalles de la liquidación." });
                }

                // 3. Llamar a la Base de Datos (Lógica pendiente en BD.cs)
                // Aquí deberías tener un método como: BD.GuardarLiquidacion(IdLiquidacion, IdMandataria, Fecha, Observaciones, listaDetalles);
                
                // --- SIMULACIÓN DE GUARDADO (Para que pruebes la vista) ---
                // TODO: Reemplazar esto por tu llamada real a BD.cs
                bool guardadoExitoso = true; 

                if (guardadoExitoso)
                {
                    return Json(new { success = true, message = "¡Presentación guardada correctamente!" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Error en Base de Datos." });
                }
            }

            // -----------------------------------------------------------------
            // CASO 2: ELIMINAR UNA LIQUIDACIÓN ENTERA
            // -----------------------------------------------------------------
            else if (QueToco == "Eliminar")
            {
                if (IdLiquidacion > 0)
                {
                    // TODO: Implementar BD.EliminarLiquidacion(IdLiquidacion.Value);
                    return Json(new { success = true, message = "Liquidación eliminada." });
                }
                return Json(new { success = false, message = "ID de liquidación inválido." });
            }

            // -----------------------------------------------------------------
            // CASO 3 (Default): CARGAR LA VISTA INICIAL (Buscadores y Filtros)
            // -----------------------------------------------------------------
            else 
            {
                // Cargar las listas para los combos (Selects)
                ViewBag.Mandatarias = BD.TraerListaMandatarias();
                ViewBag.ObrasSociales = BD.TraerListaOS();
                
                return View("MostrarModificaciones");
            }
        }
        catch (Exception ex)
        {
            // Captura cualquier error fatal y avisa al frontend
            _logger.LogError(ex, "Error en MostrarModificaciones");
            return Json(new { success = false, message = "Error interno del servidor: " + ex.Message });
        }
    }

    // =========================================================================
    // CLASES AUXILIARES (DTO) - Pégalas dentro de la clase HomeController 
    // o al final del archivo (fuera de la clase pero dentro del namespace)
    // =========================================================================
    
    public class LiquidacionDetalleDTO
    {
        // Estos nombres deben coincidir con lo que enviamos en el JSON del Javascript
        public int IdObraSocial { get; set; }
        public int IdPlan { get; set; }
        public int CantidadRecetas { get; set; }
        public decimal TotalBruto { get; set; } 
        public decimal MontoCargoOS { get; set; } 
        public decimal MontoBonificacion { get; set; } // El Neto calculado
    }



















}