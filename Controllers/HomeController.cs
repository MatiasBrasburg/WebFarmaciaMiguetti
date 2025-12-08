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



}
