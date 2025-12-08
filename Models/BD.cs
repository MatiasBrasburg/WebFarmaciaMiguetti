using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;
namespace WebFarmaciaMiguetti.Models;
public static class BD
{
private static string _connectionString = @"Server=.\SQLEXPRESS;DataBase=FarmaciaNet;Integrated Security=True;TrustServerCertificate=True;";





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

     public static Mandatarias TraerMandatariaPorId  (int IdMandataria)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Mandatarias where IdMandataria = @pIdMandataria"; 
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new {pIdMandataria = IdMandataria});
         }
    
         return ObjMandataria;
    }
 public static void ModificarMandataria  (int IdMandataria, string nuevoNombre)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE Mandatarias SET RazonSocial = @pNuevoNombre where IdMandataria = @pIdMandataria"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria, pNuevoNombre = nuevoNombre});
         }
    
    }
     public static void AgregarMandataria  (string nuevoNombre)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Mandatarias (RazonSocial) VALUES (@pNuevoNombre)"; 
            connection.Execute(query, new {pNuevoNombre = nuevoNombre});
         }
    
    }

      public static void EliminarMandataria  (int IdMandataria)
    {
     Mandatarias ObjMandataria = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM Mandatarias WHERE IdMandataria = @pIdMandataria"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria});
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

}