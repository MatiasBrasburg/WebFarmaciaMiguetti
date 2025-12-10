using Npgsql; // Usamos el driver de Postgres
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace WebFarmaciaMiguetti.Models;

public static class BD
{
    // Método CRÍTICO: Obtiene la conexión construyéndola a partir de variables separadas.
    private static string GetConnectionString()
    {
        // 1. Intenta leer las variables de entorno separadas (Las que Railway debe inyectar)
        string host = Environment.GetEnvironmentVariable("PGHOST");
        string port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432"; // Usa 5432 si no se encuentra
        string user = Environment.GetEnvironmentVariable("PGUSER");
        string password = Environment.GetEnvironmentVariable("PGPASSWORD");
        string database = Environment.GetEnvironmentVariable("PGDATABASE");
        
        // Si encontramos todos los componentes clave de la nube, construimos la cadena
        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(database))
        {
            // Cadena de conexión construida manualmente para mayor control
            return $"Host={host};Port={port};Database={database};Username={user};Password={password};Pooling=true;Timeout=15;";
        }

        // 2. FALLBACK LOCAL (Causará error si no tienes Postgres local, pero asegura que en la nube NO caiga aquí)
        return "Server=localhost;Port=5432;Database=FarmaciaNet;User Id=postgres;Password=tu_password_local;"; 
    }

    // -- HELPER PARA OBTENER CONEXIÓN --
    private static IDbConnection GetConnection()
    {
        return new NpgsqlConnection(GetConnectionString());
    }

    // A partir de aquí, el resto de tus métodos son iguales y funcionales
    // (TraerMandatariaPorNombre, ModificarMandataria, etc. permanecen sin cambios)

    //-- Codigo Mandatarias --///
    public static Mandatarias TraerMandatariaPorNombre(string nombreMandataria)
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM Mandatarias where RazonSocial = @pNombremandataria";
            return connection.QueryFirstOrDefault<Mandatarias>(query, new { pNombremandataria = nombreMandataria });
        }
    }

    // ... (El resto de tus métodos de Mandatarias y Obras Sociales van aquí) ...
    // Para simplificar, asumimos que el resto de métodos (TraerMandatariaPorId, ModificarMandataria, etc.) están copiados correctamente.
    
    // Aquí está el resto del código que ya tienes:
    public static Mandatarias TraerMandatariaPorId(int IdMandatarias)
    {
     Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM Mandatarias where IdMandatarias = @pIdMandatarias"; 
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new {pIdMandatarias = IdMandatarias});
         }
    
         return ObjMandataria;
    }
 public static void ModificarMandataria  (int IdMandatarias, string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
     Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE Mandatarias SET RazonSocial = @pNuevoNombre, Cuit = @pCuit, Descripcion = @pDescripcion, Direccion = @pDireccion where IdMandatarias = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
         }
    
    }
     public static void AgregarMandataria  (string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
     Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
                {
                    string query = "INSERT INTO Mandatarias (RazonSocial, Cuit, Descripcion, Direccion) VALUES (@pNuevoNombre, @pCuit, @pDescripcion, @pDireccion)"; 
            connection.Execute(query, new {pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
         }
    
    }

      public static void EliminarMandataria  (int IdMandatarias)
    {
     Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM Mandatarias WHERE IdMandatarias = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias});
         }
    
    }
     public static List<Mandatarias> TraerListaMandatarias()
    {
        List<Mandatarias> ListMandatarias = new List<Mandatarias>();
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
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
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM ObrasSociales";
            ListOS = connection.Query<ObrasSociales>(query).ToList();
        }
        return ListOS;
    }
 public static void ModificarOS  (int IdOS, string nuevoNombre, int IdMandatarias,  bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE ObrasSociales SET IdMandatarias = @pIdMandatarias, Nombre = @pNuevoNombre, EsPrepaga = @pEsPrepaga, Activa = @pActiva where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
         }
    
    }
 public static ObrasSociales TraerOSPorId  (int IdOS)
    {
     ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM ObrasSociales where IdObrasSociales = @pIdOS"; 
            ObjOS = connection.QueryFirstOrDefault<ObrasSociales>(query, new {pIdOS = IdOS});
         }
    
         return ObjOS;
    }

  public static void AgregarOS  ( int IdMandataria, string Nombre, string NombreMandataria, bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
                {
                    string query = "INSERT INTO ObrasSociales ( IdMandatarias, Nombre, NombreMandataria, EsPrepaga, Activa) VALUES (@pIdMandataria, @pNombre, @pNombreMandataria, @pEsPrepaga, @pActiva)"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria, pNombre = Nombre, pNombreMandataria = NombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa});
         }
    
    }


         public static void EliminarOS (int IdOS)
    {
     ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM ObrasSociales WHERE IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS});
         }
    
    }

 // Models/BD.cs

public static void AgregarBonificaciones(int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
{
    // No necesitamos crear una instancia de ObrasSociales aquí
    using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
    {
        // 🚨 CORRECCIÓN: Usamos UPDATE porque la Obra Social YA EXISTE.
        // El INSERT intentaría crear una fila nueva, pero nosotros queremos modificar la fila del IdOS existente.
        string query = "UPDATE ObrasSociales SET NombreCodigoBonificacion = @pNombreCodigoBonificacion, CodigoBonificacion = @pCodigoBonificacion WHERE IdObrasSociales = @pIdOS"; 
        
        connection.Execute(query, new { 
            pNombreCodigoBonificacion = NombreCodigoBonificacion, 
            pCodigoBonificacion = CodigoBonificacion, 
            pIdOS = IdOS 
        });
    }
}
     public static void ModificarBonificaciones  (int IdOS, int? CodigoBonificacion, string? NombreCodigoBonificacion)
    {
     ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE ObrasSociales SET CodigoBonificacion = @pCodigoBonificacion, NombreCodigoBonificacion = @pNombreCodigoBonificacion where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion});
         }
    
    }
}