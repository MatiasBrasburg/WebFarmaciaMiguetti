using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;
namespace WebFarmaciaMiguetti.Models;
public static class BD
{
private static string _connectionString = @"Server=.\SQLEXPRESS01;DataBase=FarmaciaNet;Integrated Security=True;TrustServerCertificate=True;";



//-- Codigo Mandatarias --///

     public static Mandatarias TraerMandatariaPorNombre  (string nombreMandataria)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Mandatarias where RazonSocial = @pNombremandataria"; 
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new {pNombremandataria = nombreMandataria});
         }
    
         return ObjMandataria;
    }

     public static Mandatarias TraerMandatariaPorId  (int IdMandatarias)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Mandatarias where IdMandatarias = @pIdMandatarias"; 
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new {pIdMandatarias = IdMandatarias});
         }
    
         return ObjMandataria;
    }
 public static void ModificarMandataria  (int IdMandatarias, string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE Mandatarias SET RazonSocial = @pNuevoNombre, Cuit = @pCuit, Descripcion = @pDescripcion, Direccion = @pDireccion where IdMandatarias = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
         }
    
    }
     public static void AgregarMandataria  (string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Mandatarias (RazonSocial, Cuit, Descripcion, Direccion) VALUES (@pNuevoNombre, @pCuit, @pDescripcion, @pDireccion)"; 
            connection.Execute(query, new {pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
         }
    
    }

      public static void EliminarMandataria  (int IdMandatarias)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM Mandatarias WHERE IdMandatarias = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias});
         }
    
    }
     public static List<Mandatarias> TraerListaMandatarias()
    {
        List<Mandatarias> ListMandatarias = new List<Mandatarias>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Mandatarias";
            ListMandatarias = connection.Query<Mandatarias>(query).ToList();
        }
        return ListMandatarias;
    }





//-- Codigo Obras Sociales --///
      public static List<ObrasSociales> TraerListaOS()
    {
        List<ObrasSociales> ListOS = new List<ObrasSociales>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM ObrasSociales";
            ListOS = connection.Query<ObrasSociales>(query).ToList();
        }
        return ListOS;
    }
 public static void ModificarOS  (int IdOS, string nuevoNombre, int IdMandatarias,  bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE ObrasSociales SET IdMandatarias = @pIdMandatarias, Nombre = @pNuevoNombre, EsPrepaga = @pEsPrepaga, Activa = @pActiva where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
         }
    
    }
 public static ObrasSociales TraerOSPorId  (int IdOS)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM ObrasSociales where IdObrasSociales = @pIdOS"; 
            ObjOS = connection.QueryFirstOrDefault<ObrasSociales>(query, new {pIdOS = IdOS});
         }
    
         return ObjOS;
    }

  public static void AgregarOS  ( int IdMandataria, string Nombre, bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ObrasSociales ( IdMandatarias, Nombre, EsPrepaga, Activa) VALUES (@pIdMandataria, @pNombre, @pEsPrepaga, @pActiva)"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria, pNombre = Nombre, pEsPrepaga = EsPrepaga, pActiva = Activa});
         }
    
    }


         public static void EliminarOS (int IdOS)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM ObrasSociales WHERE IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS});
         }
    
    }

 public static void AgregarBonificaciones  (int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ObrasSociales (NombreCodigoBonificacion, CodigoBonificacion) VALUES (@pNombreCodigoBonificacion, @pCodigoBonificacion) where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pNombreCodigoBonificacion = NombreCodigoBonificacion, pCodigoBonificacion = CodigoBonificacion, pIdOS = IdOS});
         }
    
    }
     public static void ModificarBonificaciones  (int IdOS, int? CodigoBonificacion, string? NombreCodigoBonificacion)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE ObrasSociales SET CodigoBonificacion = @pCodigoBonificacion, NombreCodigoBonificacion = @pNombreCodigoBonificacion where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion});
         }
    
    }














































































}