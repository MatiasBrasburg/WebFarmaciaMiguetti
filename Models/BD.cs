using Npgsql; // Usamos el driver de Postgres
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace WebFarmaciaMiguetti.Models;

public static class BD
{
    // Método CRÍTICO: Obtiene la conexión construyéndola a partir de variables de entorno.
    private static string GetConnectionString()
    {
        // Leemos las variables que Railway nos inyecta (PGHOST, etc.)
        string host = Environment.GetEnvironmentVariable("PGHOST");
        string port = Environment.GetEnvironmentVariable("PGPORT");
        string user = Environment.GetEnvironmentVariable("PGUSER");
        string password = Environment.GetEnvironmentVariable("PGPASSWORD");
        string database = Environment.GetEnvironmentVariable("PGDATABASE");

        // Construimos la cadena con SSL requerido para servicios cloud como Railway
        return $"Host={host};Port={port};Database={database};Username={user};Password={password};Pooling=true;Timeout=15;SSL Mode=Require;Trust Server Certificate=true;";
    }

    // -- HELPER PARA OBTENER CONEXIÓN --
    private static IDbConnection GetConnection()
    {
        return new NpgsqlConnection(GetConnectionString());
    }

    // =================================================================================
    // MÉTODOS DE DATOS: (TODAS LAS CONSULTAS USAN COMILLAS DOBLES)
    // =================================================================================

    //-- Codigo Mandatarias --///
    public static Mandatarias TraerMandatariaPorNombre(string nombreMandataria)
    {
        using (var connection = GetConnection())
        {
            // Nota: Se usan comillas dobles en "Mandatarias" y en "RazonSocial"
            string query = "SELECT * FROM \"Mandatarias\" WHERE \"RazonSocial\" = @pNombremandataria";
            return connection.QueryFirstOrDefault<Mandatarias>(query, new { pNombremandataria = nombreMandataria });
        }
    }

    public static Mandatarias TraerMandatariaPorId(int IdMandatarias)
    {
        Mandatarias ObjMandataria = null;
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columna
            string query = "SELECT * FROM \"Mandatarias\" WHERE \"IdMandatarias\" = @pIdMandatarias"; 
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new {pIdMandatarias = IdMandatarias});
        }
        return ObjMandataria;
    }

    public static void ModificarMandataria(int IdMandatarias, string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y todas las columnas
            string query = "UPDATE \"Mandatarias\" SET \"RazonSocial\" = @pNuevoNombre, \"Cuit\" = @pCuit, \"Descripcion\" = @pDescripcion, \"Direccion\" = @pDireccion WHERE \"IdMandatarias\" = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
        }
    }

    public static void AgregarMandataria(string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y todas las columnas
            string query = "INSERT INTO \"Mandatarias\" (\"RazonSocial\", \"Cuit\", \"Descripcion\", \"Direccion\") VALUES (@pNuevoNombre, @pCuit, @pDescripcion, @pDireccion)"; 
            connection.Execute(query, new {pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion});
        }
    }

    public static void EliminarMandataria(int IdMandatarias)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columna
            string query = "DELETE FROM \"Mandatarias\" WHERE \"IdMandatarias\" = @pIdMandatarias"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias});
        }
    }

    public static List<Mandatarias> TraerListaMandatarias()
    {
        List<Mandatarias> ListMandatarias = new List<Mandatarias>();
        using (var connection = GetConnection())
        {
            // Nota: Comillas en la tabla
            string query = "SELECT * FROM \"Mandatarias\"";
            ListMandatarias = connection.Query<Mandatarias>(query).ToList();
        }
        return ListMandatarias;
    }

    //-- Codigo Obras Sociales --///
    public static List<ObrasSociales> TraerListaOS()
    {
        List<ObrasSociales> ListOS = new List<ObrasSociales>();
        using (var connection = GetConnection())
        {
            // Nota: Comillas en la tabla
            string query = "SELECT * FROM \"ObrasSociales\"";
            ListOS = connection.Query<ObrasSociales>(query).ToList();
        }
        return ListOS;
    }

    public static void ModificarOS(int IdOS, string nuevoNombre, int IdMandatarias, bool? EsPrepaga, bool? Activa)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y todas las columnas
            string query = "UPDATE \"ObrasSociales\" SET \"IdMandatarias\" = @pIdMandatarias, \"Nombre\" = @pNuevoNombre, \"EsPrepaga\" = @pEsPrepaga, \"Activa\" = @pActiva WHERE \"IdObrasSociales\" = @pIdOS"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
        }
    }

    public static ObrasSociales TraerOSPorId(int IdOS)
    {
        ObrasSociales ObjOS = null;
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columna
            string query = "SELECT * FROM \"ObrasSociales\" WHERE \"IdObrasSociales\" = @pIdOS"; 
            ObjOS = connection.QueryFirstOrDefault<ObrasSociales>(query, new {pIdOS = IdOS});
        }
        return ObjOS;
    }

    public static void AgregarOS(int IdMandataria, string Nombre, string NombreMandataria, bool? EsPrepaga, bool? Activa)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y todas las columnas
            string query = "INSERT INTO \"ObrasSociales\" (\"IdMandatarias\", \"Nombre\", \"NombreMandataria\", \"EsPrepaga\", \"Activa\") VALUES (@pIdMandataria, @pNombre, @pNombreMandataria, @pEsPrepaga, @pActiva)"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria, pNombre = Nombre, pNombreMandataria = NombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa});
        }
    }

    public static void EliminarOS(int IdOS)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columna
            string query = "DELETE FROM \"ObrasSociales\" WHERE \"IdObrasSociales\" = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS});
        }
    }

    public static void AgregarBonificaciones(int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columnas
            string query = "UPDATE \"ObrasSociales\" SET \"NombreCodigoBonificacion\" = @pNombreCodigoBonificacion, \"CodigoBonificacion\" = @pCodigoBonificacion WHERE \"IdObrasSociales\" = @pIdOS"; 
            connection.Execute(query, new { 
                pNombreCodigoBonificacion = NombreCodigoBonificacion, 
                pCodigoBonificacion = CodigoBonificacion, 
                pIdOS = IdOS 
            });
        }
    }

    public static void ModificarBonificaciones(int IdOS, int? CodigoBonificacion, string? NombreCodigoBonificacion)
    {
        using (var connection = GetConnection())
        {
            // Nota: Comillas en tabla y columnas
            string query = "UPDATE \"ObrasSociales\" SET \"CodigoBonificacion\" = @pCodigoBonificacion, \"NombreCodigoBonificacion\" = @pNombreCodigoBonificacion WHERE \"IdObrasSociales\" = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion});
        }
    }
}