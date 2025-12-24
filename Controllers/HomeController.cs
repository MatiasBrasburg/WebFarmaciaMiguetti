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
        var usuarioLogueado = HttpContext.Session.GetString("Usuario");
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
    // ACCIÓN PRINCIPAL: GUARDAR Y ELIMINAR (Fusionado)
    // =========================================================================
    [HttpPost]
    public IActionResult MostrarModificaciones(
        string QueToco, 
        int? IdLiquidacion, 
        int? IdMandatarias, 
        DateTime? Fecha, 
        string? Observaciones, 
        string? DetallesJson 
    )
    {
        try 
        {
            // -----------------------------------------------------------------
            // CASO 1: GUARDAR (TU CÓDIGO ORIGINAL - RESPETADO AL 100%)
            // -----------------------------------------------------------------
            if (QueToco == "Guardar")
            {
                // 1. Validaciones
                if (IdMandatarias == null || IdMandatarias == 0) 
                    return Json(new { success = false, message = "Debe seleccionar una Mandataria." });

                if (string.IsNullOrEmpty(DetallesJson) || DetallesJson == "[]")
                    return Json(new { success = false, message = "La liquidación está vacía." });

                // 2. Deserializar
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<LiquidacionDetalle> listaDetalles; // Ojo: Uso tu clase en singular
                try {
                    listaDetalles = System.Text.Json.JsonSerializer.Deserialize<List<LiquidacionDetalle>>(DetallesJson, opciones);
                } catch {
                    return Json(new { success = false, message = "Error leyendo los detalles JSON." });
                }

                // 3. Calcular Total General
                decimal totalPresentacion = listaDetalles.Sum(x => x.TotalBruto); 

                // 4. Crear Objeto Cabecera
                Liquidaciones nuevaLiq = new Liquidaciones
                {
                    IdMandatarias = IdMandatarias.Value,
                    FechaPresentacion = Fecha ?? DateTime.Now,
                    Observaciones = Observaciones,
                    TotalPresentado = totalPresentacion,
                    Periodo = (Fecha ?? DateTime.Now).ToString("MM-yyyy")
                };

                // 5. GUARDAR EN BD
                int nuevoId = BD.InsertarLiquidacionCabecera(nuevaLiq);

                // B) Guardamos cada detalle
                foreach (var item in listaDetalles)
                {
                    BD.InsertarLiquidacionDetalle(
                        nuevoId, 
                        item.IdObrasSociales, 
                        item.IdPlanBonificacion, 
                        item.CantidadRecetas, 
                        item.TotalBruto, 
                        item.MontoCargoOS, 
                        item.MontoBonificacion
                    );
                }

                return Json(new { success = true, message = $"¡Liquidación N° {nuevoId} guardada correctamente!" });
            }
            
            // -----------------------------------------------------------------
            // CASO 2: ELIMINAR (LA PARTE NUEVA QUE NECESITABAS)
            // -----------------------------------------------------------------
            else if (QueToco == "Eliminar")
            {
                if (IdLiquidacion.HasValue && IdLiquidacion.Value > 0)
                {
                    // Llamamos a la BD para borrar hijos y padre
                    BD.EliminarLiquidacion(IdLiquidacion.Value);
                    return Json(new { success = true, message = "Liquidación eliminada correctamente." });
                }
                else
                {
                    return Json(new { success = false, message = "ID inválido para eliminar." });
                }
            }

            return Json(new { success = false, message = "Acción no reconocida." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en Gestión Liquidaciones");
            return Json(new { success = false, message = "Error grave: " + ex.Message });
        }
    }

    // =========================================================================
    // NUEVO ENDPOINT PARA "VER" (El JS lo llama para llenar el modal)
    // =========================================================================
    [HttpGet]
    public IActionResult ObtenerDetallesDeLiquidacion(int idLiquidacion)
    {
        try
        {
            // Busca todos los items de esa liquidación
            List<LiquidacionDetalle> detalles = BD.TraerDetallesPorIdLiquidacion(idLiquidacion);
            return Json(new { success = true, data = detalles });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    // =========================================================================
    // NUEVO: ENDPOINT PARA BUSCAR (AJAX)
    // =========================================================================
    [HttpGet]
    public IActionResult BuscarLiquidacionesAjax(int? id, DateTime? desde, DateTime? hasta, int? mandataria)
    {
        try
        {
            // Llamamos al nuevo método de BD
            var lista = BD.BuscarLiquidaciones(id, desde, hasta, mandataria);
            
            // Retornamos JSON para que JS dibuje la tabla
            return Json(new { success = true, data = lista });
        }
        catch (Exception ex)
        {
             return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


[HttpGet]
    public IActionResult ObtenerItemPorId(int idDetalle)
    {
        try
        {
            var item = BD.TraerDetallePorId(idDetalle);
            if (item == null) return Json(new { success = false, message = "Ítem no encontrado" });
            return Json(new { success = true, data = item });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult GuardarItemIndividual(int IdLiquidacionPadre, int IdItem, int IdOS, int IdPlan, int Recetas, decimal Total, decimal CargoOS, decimal Bonificacion)
    {
        try
        {
            // Validaciones básicas
            if (IdOS == 0) return Json(new { success = false, message = "Debe elegir Obra Social" });
            if (Total <= 0) return Json(new { success = false, message = "El total debe ser mayor a 0" });

            LiquidacionDetalle item = new LiquidacionDetalle
            {
                IdLiquidacionDetalle = IdItem,
                IdLiquidaciones = IdLiquidacionPadre,
                IdObrasSociales = IdOS,
                IdPlanBonificacion = IdPlan,
                CantidadRecetas = Recetas,
                TotalBruto = Total,
                MontoCargoOS = CargoOS,
                MontoBonificacion = Bonificacion
            };

            if (IdItem == 0)
            {
                // INSERTAR
                BD.AgregarItemIndividual(item);
                return Json(new { success = true, message = "Ítem AGREGADO correctamente." });
            }
            else
            {
                // MODIFICAR
                BD.ModificarItemIndividual(item);
                return Json(new { success = true, message = "Ítem MODIFICADO correctamente." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult EliminarItemIndividual(int idItem, int idLiquidacionPadre)
    {
        try
        {
            BD.EliminarItemIndividual(idItem, idLiquidacionPadre);
            return Json(new { success = true, message = "Ítem eliminado." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }







    // =========================================================================
    // GESTIÓN DE EDICIÓN DE CABECERA (SOLO DATOS GENERALES)
    // =========================================================================

    [HttpGet]
    public IActionResult ObtenerCabeceraLiquidacion(int id)
    {
        try
        {
            var liq = BD.TraerLiquidacionPorId(id);
            if (liq == null) return Json(new { success = false, message = "No encontrada" });
            return Json(new { success = true, data = liq });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult GuardarEdicionCabecera(int IdLiquidacion, int IdMandataria, DateTime Fecha, string Observaciones)
    {
        try
        {
            BD.ModificarLiquidacionCabecera(IdLiquidacion, IdMandataria, Fecha, Observaciones ?? "");
            return Json(new { success = true, message = "Datos de liquidación actualizados." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpGet]
public IActionResult BuscarDetallesGlobalesAjax(DateTime? desde, DateTime? hasta, int? idMandataria, int? idOS)
{
    try
    {
        var lista = BD.BuscarDetallesGlobales(desde, hasta, idMandataria, idOS);
        return Json(new { success = true, data = lista });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = ex.Message });
    }
}

//Configuracion de usuario

 public IActionResult IrAConfiguracion(){
        string? usuarioJson = HttpContext.Session.GetString("Usuario"); // Crear el usuario session
        if (string.IsNullOrEmpty(usuarioJson))
        {
            return RedirectToAction("Index", "Account");
        }
        Usuario userDeSesion = Objeto.StringToObject<Usuario>(usuarioJson);
       
     
        Usuario usuarioCompleto = BD.TraerUsuarioPorId(userDeSesion.IdUsuario);


        if (usuarioCompleto == null)
        {
            return RedirectToAction("CerrarSesion", "Index");
        }
        ViewBag.usuario = usuarioCompleto;

        return View("Configuracion");
    }

 public IActionResult Configuracion(string? Contraseña, string? RazonSocial, string? Domicilio, long? Cuit, string? Iva){
        string? usuarioJson = HttpContext.Session.GetString("Usuario"); // Crear el usuario session
        if (string.IsNullOrEmpty(usuarioJson))
        {
            return RedirectToAction("Index", "Account");
        }
        Usuario userDeSesion = Objeto.StringToObject<Usuario>(usuarioJson);
       
     
        Usuario usuarioCompleto = BD.TraerUsuarioPorId(userDeSesion.IdUsuario);


        if (usuarioCompleto == null)
        {
            return RedirectToAction("CerrarSesion", "Index");
        }
   

BD.ModificarUsuario(userDeSesion.IdUsuario, Contraseña ?? usuarioCompleto.Contraseña, RazonSocial ?? usuarioCompleto.RazonSocial, Domicilio ?? usuarioCompleto.Domicilio, Cuit ?? usuarioCompleto.Cuit, Iva ?? usuarioCompleto.Iva);


        return RedirectToAction("IrAConfiguracion", "Home");
    }

 public IActionResult IrAGestionCobros(){
        string? usuarioJson = HttpContext.Session.GetString("Usuario"); // Crear el usuario session
        if (string.IsNullOrEmpty(usuarioJson))
        {
            return RedirectToAction("Index", "Account");
        }
        Usuario userDeSesion = Objeto.StringToObject<Usuario>(usuarioJson);
       
     
        Usuario usuarioCompleto = BD.TraerUsuarioPorId(userDeSesion.IdUsuario);


        if (usuarioCompleto == null)
        {
            return RedirectToAction("CerrarSesion", "Index");
        }
       
       ViewBag.ObrasSociales = BD.TraerListaOS();
       ViewBag.Mandatarias = BD.TraerListaMandatarias();
       ViewBag.TiposPago = new List<string> { "Transferencia", "Efectivo", "Cheque", "Depósito" };

        return View("GestionCobros");
    }


public class CobrosDetalleRequest
    {
        // Datos del Maestro (Cabecera)
        public DateTime FechaCobroMaestro { get; set; }
        public string NumeroComprobanteMaestro { get; set; }
        public int IdMandatariasMaestro { get; set; }

        // Datos del Detalle (Item)
        public int IdObrasSociales { get; set; }
        public DateTime? FechaCobroDetalle { get; set; } // Fecha individual del pago
        public string TipoPago { get; set; }             // Tipo individual del pago
        public decimal ImporteCobrado { get; set; }
        public decimal MontoDebito { get; set; }
        public string MotivoDebito { get; set; }
        public int? IdLiquidacionDetalle { get; set; }
    }

    // ====================================================================
    // MÉTODOS DE LECTURA Y FILTROS
    // ====================================================================

    [HttpGet]
    public IActionResult TraerObrasSocialesPorMandataria(int idMandataria)
    {
        try
        {
            var lista = BD.TraerOSPorIdMandataria(idMandataria);
            return Json(new { success = true, data = lista });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult TraerLiquidacionesPendientesPorOS(int idObraSocial)
    {
        try
        {
            var lista = BD.TraerLiquidacionesPendientesPorOS(idObraSocial);
            return Json(new { success = true, data = lista });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult BuscarCobrosAjax(string? numeroComprobante, DateTime? desde, DateTime? hasta, int? IdObraSocial, int? IdMandataria)
    {
        try
        {
            // Usamos la búsqueda que agrupa Padre+Hijos y suma totales
            var lista = BD.BuscarCobros(desde, hasta, IdMandataria, IdObraSocial);
            
            // Filtro en memoria opcional por comprobante
            if (!string.IsNullOrEmpty(numeroComprobante))
            {
                lista = lista.Where(x => x.NumeroComprobante.ToString().Contains(numeroComprobante)).ToList();
            }

            return Json(new { success = true, data = lista });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

   [HttpPost]
public IActionResult GuardarCobro(int IdCobro, int? IdLiquidacion, int? IdObraSocial, int? IdMandataria, DateTime? FechaCobro, decimal ImporteCobrado, string NumeroComprobante, string? TipoPago, decimal MontoDebitos, string? MotivoDebito, int? IdLiquidacionDetalle) // <--- Agregamos este parámetro al final
{
    try
    {
        if (IdCobro == 0)
        {
            // NUEVO PAGO
            if (IdMandataria == null || IdMandataria == 0) 
                return Json(new { success = false, message = "Falta la Mandataria." });

            // Buscamos o creamos el padre (Lote)
            int? idPadre = BD.BuscarIdPadre(NumeroComprobante, IdMandataria.Value);

            if (idPadre == null || idPadre == 0)
            {
                idPadre = BD.AgregarCobroCabecera(IdMandataria.Value, FechaCobro ?? DateTime.Now, NumeroComprobante, IdLiquidacion);
            }

            // AQUI ESTA LA MAGIA: Pasamos IdLiquidacionDetalle a la BD para que impute la deuda
            BD.AgregarCobroDetalle(
                idPadre.Value, 
                IdObraSocial.Value, 
                FechaCobro ?? DateTime.Now, 
                ImporteCobrado, 
                TipoPago, 
                MontoDebitos, 
                MotivoDebito,
                IdLiquidacionDetalle // <--- Pasamos el ID de la deuda
            );

            return Json(new { success = true, message = "Pago registrado e imputado correctamente." });
        }
        else
        {
            // EDICIÓN (Por ahora no reimputamos deuda al editar para no complicar, solo actualizamos datos básicos)
            BD.ModificarCobroDetalle(IdCobro, IdObraSocial.Value, FechaCobro ?? DateTime.Now, ImporteCobrado, TipoPago, MontoDebitos, MotivoDebito);
            return Json(new { success = true, message = "Pago modificado." });
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error: " + ex.Message });
    }
}
    [HttpPost]
    public IActionResult EliminarCobro(int idCobro)
    {
        try
        {
            // Recibimos el ID del Detalle
            BD.EliminarCobroDetalle(idCobro);
            return Json(new { success = true, message = "Cobro eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ObtenerCobroPorId(int idCobro)
    {
        try
        {
            // Buscamos en la tabla Detalle
            var cobroDetalle = BD.TraerCobroDetallePorId(idCobro);
            if (cobroDetalle == null) return Json(new { success = false, message = "No encontrado" });
            
            return Json(new { success = true, data = cobroDetalle });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // --- MÉTODOS PARA VISUALIZACIÓN DE DETALLES ---

[HttpGet]
    public IActionResult TraerCobrosPorIdPadre(int idCobroPadre)
    {
        try
        {
            if (idCobroPadre <= 0) 
                return Json(new { success = false, message = "ID de Lote inválido" });

            // Ahora llamamos a la BD pasando el ID (int), que es único y rápido
            var lista = BD.TraerCobrosDelMismoLote(idCobroPadre); 
            
            return Json(new { success = true, data = lista });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

   

    // --- MÉTODOS DE MANTENIMIENTO ---

    [HttpPost]
    public IActionResult EliminarLoteCompleto(int idCobroPadre)
    {
        try
        {
            BD.EliminarLoteCompleto(idCobroPadre);
            return Json(new { success = true, message = "✅ Lote eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult ModificarCabeceraLote(int IdCobro, int IdMandataria, DateTime Fecha, string Comprobante)
    {
        try
        {
            BD.ModificarCobroCabecera(IdCobro, IdMandataria, Fecha, Comprobante);
            return Json(new { success = true, message = "✅ Cabecera actualizada." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    // --- GUARDADO MASIVO (NUEVO LOTE) ---
    [HttpPost]
public IActionResult GuardarLoteCobros([FromBody] List<CobrosDetalleRequest> lote)
{
    if (lote == null || !lote.Any()) return Json(new { success = false, message = "Lista vacía." });

    try
    {
        var primerItem = lote.First();
        
        // 1. Crear Padre
        int idPadre = BD.AgregarCobroCabecera(
            primerItem.IdMandatariasMaestro, 
            primerItem.FechaCobroMaestro, 
            primerItem.NumeroComprobanteMaestro, 
            null
        );

        int guardados = 0;

        // 2. Crear Hijos
        foreach (var item in lote)
        {
            if (item.IdObrasSociales == 0) continue;

            BD.AgregarCobroDetalle(
                idPadre, 
                item.IdObrasSociales, 
                item.FechaCobroDetalle ?? DateTime.Now, 
                item.ImporteCobrado, 
                item.TipoPago, 
                item.MontoDebito, 
                item.MotivoDebito,
                item.IdLiquidacionDetalle // <--- Pasamos la deuda vinculada
            );
            guardados++;
        }

        return Json(new { success = true, message = $"✅ Lote procesado: {guardados} pagos registrados." });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error: " + ex.Message });
    }
}


[HttpGet]
public IActionResult TraerDeudasPendientes(int idObraSocial)
{
    try
    {
        var lista = BD.TraerDeudasPendientesPorOS(idObraSocial);
        return Json(new { success = true, data = lista });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}















































    
}








































































