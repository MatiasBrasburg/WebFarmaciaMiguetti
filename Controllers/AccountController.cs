using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebFarmaciaMiguetti.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;        


namespace WebFarmaciaMiguetti.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
            return View("Login");
    }

    public IActionResult LogIn2(string Contraseña)
    {
        Usuario ObjUsuario = BD.TraerUsuarioPorContraseña(Contraseña);
        if (ObjUsuario != null)
        {
            HttpContext.Session.SetString("Usuario", JsonSerializer.Serialize(ObjUsuario));
            return RedirectToAction("Home", "Home"); 
        }
        else
            {
                ViewBag.Mensaje = "Contraseña incorrecta. Inténtelo de nuevo.";
                return View("Login");

            }

    
    }
}
