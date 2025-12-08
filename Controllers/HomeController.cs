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
}
