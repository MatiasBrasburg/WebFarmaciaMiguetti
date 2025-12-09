using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebFarmaciaMiguetti.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;        


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
        // NO ESTÁ LOGUEADO: Lo mandamos a la puerta (Login)
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
    public IActionResult ModificarMandataria (int IdMandataria, string QueToco, string? NombreMandataria, long Cuit, string? Descripcion, string? Direccion)
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
                // Nombre inválido
                return View("Error", new ErrorViewModel { RequestId = "Llene el nombre de la mandataria, por favor." });
            }

            if(ObjtMandataria != null)
            {
                BD.ModificarMandataria(IdMandataria, nuevoNombreMandataria, Cuit, Descripcion, Direccion);
            }
            else
            {
                // No se encontró la mandataria
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
                // No se encontró la mandataria
                return View("Error", new ErrorViewModel { RequestId = "No se encontró la mandataria con ese nombre." });
            }
        }
        else
        {
            BD.EliminarMandataria(IdMandataria);
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

public IActionResult ABM_OS (int IdOS, string QueToco, int? IdMandataria, string? NombreOS, int? CodigoBonificacion, string? NombreCodigoBonificacion, bool? EsPrepaga, bool? Activa)
    {
        int nuevoIdMandataria = 0;
string nuevoNombreOS = "";
            ObrasSociales ObjtOSS = BD.TraerOSPorId(IdOS);

        if(QueToco == "Modificar")
        {

           if(!string.IsNullOrEmpty(NombreOS))
            {
                nuevoNombreOS = NombreOS;
            }
            else
            {
                // Nombre inválido
                return View("Error", new ErrorViewModel { RequestId = "Llene el nombre de la mandataria, por favor." });
            }

            if(ObjtOSS != null)
            {
                if(IdMandataria.HasValue)
                {
                    nuevoIdMandataria = IdMandataria.Value;
                     BD.ModificarOS(IdOS, nuevoNombreOS,nuevoIdMandataria);
                }
                else
                {
                   ObrasSociales OS = BD.TraerOSPorId(IdOS);
                BD.ModificarOS(IdOS, nuevoNombreOS, OS.IdMandataria);
                    
                }
            }
            else
            {
                // No se encontró la mandataria
                return View("Error", new ErrorViewModel { RequestId = "No se encontró la Obra Social." });
            }
        }
        else if(QueToco == "Agregar" )
        {
            

            if(ObjtOSS != null)
            {
Mandatarias mandataria = BD.TraerMandatariaPorId(IdMandataria.Value);
               BD.AgregarOS(IdOS, mandataria.IdMandatarias, NombreOS,EsPrepaga, Activa);
            }
            else
            {
                // No se encontró la mandataria
                return View("Error", new ErrorViewModel { RequestId = "No se encontró la mandataria con ese nombre." });
            }
        }
        else if(QueToco == "Planes")
        {
            if(ObjtOSS.CodigoBonificacion > 0 && ObjtOSS.NombreCodigoBonificacion != null)
            {
                BD.ModificarBonificaciones(IdOS ,CodigoBonificacion,NombreCodigoBonificacion);
            }
            else
            {
                BD.AgregarBonificaciones(IdOS, NombreCodigoBonificacion, CodigoBonificacion);
            }
        }
        else
        {
            BD.EliminarOS(IdOS);
        }
    
        return RedirectToAction("IrAAltasModificacionesOS", "Home");
    }
























































































}
