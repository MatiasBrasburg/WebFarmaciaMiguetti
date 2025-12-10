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

public IActionResult ABM_OS(int IdOS, string QueToco, int? IdMandatarias, string? NombreOS, int? CodigoBonificacion, string? NombreCodigoBonificacion, bool? EsPrepaga, bool? Activa)
{
    
    bool valorEsPrepaga = EsPrepaga ?? false;
    bool valorActiva = Activa ?? true; 
    int nuevoIdMandataria = 0;
    string nuevoNombreOS = "";

   
    ObrasSociales ObjtOSS = BD.TraerOSPorId(IdOS);

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
                BD.ModificarOS(IdOS, nuevoNombreOS, nuevoIdMandataria, valorEsPrepaga, valorActiva);
            }
            else
            {
                // Si no cambió la mandataria, mantenemos la que tenía
                BD.ModificarOS(IdOS, nuevoNombreOS, ObjtOSS.IdMandataria, valorEsPrepaga, valorActiva);
            }
        }
        else
        {
            return View("Error", new ErrorViewModel { RequestId = "No se encontró la Obra Social para modificar." });
        }
    }
    else if (QueToco == "Agregar")
    {
        // === CORRECCIÓN DEL ERROR NULLREFERENCE ===
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
                // Aquí usamos mandataria.IdMandatarias SOLO si estamos seguros que no es null
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
    else if (QueToco == "Planes")
    {
        if (ObjtOSS == null)
        {
            return View("Error", new ErrorViewModel { RequestId = "No se encontró la Obra Social para cargar planes." });
        }
        else
        {
            // Verificamos si ya tiene plan o es nuevo
            if (ObjtOSS.CodigoBonificacion > 0 || !string.IsNullOrEmpty(ObjtOSS.NombreCodigoBonificacion))
            {
                BD.ModificarBonificaciones(IdOS, CodigoBonificacion, NombreCodigoBonificacion);
            }
            else
            {
                BD.AgregarBonificaciones(IdOS, NombreCodigoBonificacion, CodigoBonificacion);
            }
        }
    }
    else
    {
        // Eliminar
        BD.EliminarOS(IdOS);
    }

    return RedirectToAction("IrAAltasModificacionesOS", "Home");
}























































































}
