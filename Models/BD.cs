using Npgsql; // Usamos el driver de Postgres
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace WebFarmaciaMiguetti.Models;

public static class BD
{
    // Esta función decide inteligentemente qué conexión usar
    private static string GetConnectionString()
    {
        // 1. Intenta leer la variable de entorno de Railway
        string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

        // 2. Si no existe (estás en tu PC), usa tu conexión local (aunque idealmente deberías instalar Postgres local también)
        if (string.IsNullOrEmpty(connectionString))
        {
            // OJO: Si vas a probar en local, necesitarás un Postgres local o cambiar esto temporalmente.
            return "Server=localhost;Port=5432;Database=FarmaciaNet;User Id=postgres;Password=tu_password_local;"; 
        }

        return connectionString;
    }

    // -- HELPER PARA OBTENER CONEXIÓN --
    // Usamos NpgsqlConnection en lugar de SqlConnection
    private static IDbConnection GetConnection()
    {
        return new NpgsqlConnection(GetConnectionString());
    }

    //-- Codigo Mandatarias --///
    public static Mandatarias TraerMandatariaPorNombre(string nombreMandataria)
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM Mandatarias where RazonSocial = @pNombremandataria";
            return connection.QueryFirstOrDefault<Mandatarias>(query, new { pNombremandataria = nombreMandataria });
        }
    }

    public static Mandatarias TraerMandatariaPorId(int IdMandatarias)
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM Mandatarias where IdMandatarias = @pIdMandatarias";
            return connection.QueryFirstOrDefault<Mandatarias>(query, new { pIdMandatarias = IdMandatarias });
        }
    }

    public static void ModificarMandataria(int IdMandatarias, string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (var connection = GetConnection())
        {
            string query = "UPDATE Mandatarias SET RazonSocial = @pNuevoNombre, Cuit = @pCuit, Descripcion = @pDescripcion, Direccion = @pDireccion where IdMandatarias = @pIdMandatarias";
            connection.Execute(query, new { pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion });
        }
    }

    public static void AgregarMandataria(string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (var connection = GetConnection())
        {
            // En Postgres, para insertar y devolver ID a veces cambia, pero el INSERT simple funciona igual
            string query = "INSERT INTO Mandatarias (RazonSocial, Cuit, Descripcion, Direccion) VALUES (@pNuevoNombre, @pCuit, @pDescripcion, @pDireccion)";
            connection.Execute(query, new { pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion });
        }
    }

    public static void EliminarMandataria(int IdMandatarias)
    {
        using (var connection = GetConnection())
        {
            string query = "DELETE FROM Mandatarias WHERE IdMandatarias = @pIdMandatarias";
            connection.Execute(query, new { pIdMandatarias = IdMandatarias });
        }
    }

    public static List<Mandatarias> TraerListaMandatarias()
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM Mandatarias";
            return connection.Query<Mandatarias>(query).ToList();
        }
    }

    //-- Codigo Obras Sociales --///
    public static List<ObrasSociales> TraerListaOS()
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM ObrasSociales";
            return connection.Query<ObrasSociales>(query).ToList();
        }
    }

    public static void ModificarOS(int IdOS, string nuevoNombre, int IdMandatarias, bool? EsPrepaga, bool? Activa)
    {
        using (var connection = GetConnection())
        {
            string query = "UPDATE ObrasSociales SET IdMandatarias = @pIdMandatarias, Nombre = @pNuevoNombre, EsPrepaga = @pEsPrepaga, Activa = @pActiva where IdObrasSociales = @pIdOS";
            connection.Execute(query, new { pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
        }
    }

    public static ObrasSociales TraerOSPorId(int IdOS)
    {
        using (var connection = GetConnection())
        {
            string query = "SELECT * FROM ObrasSociales where IdObrasSociales = @pIdOS";
            return connection.QueryFirstOrDefault<ObrasSociales>(query, new { pIdOS = IdOS });
        }
    }

    public static void AgregarOS(int IdMandataria, string Nombre, string NombreMandataria, bool? EsPrepaga, bool? Activa)
    {
        using (var connection = GetConnection())
        {
            string query = "INSERT INTO ObrasSociales ( IdMandatarias, Nombre, NombreMandataria, EsPrepaga, Activa) VALUES (@pIdMandataria, @pNombre, @pNombreMandataria, @pEsPrepaga, @pActiva)";
            connection.Execute(query, new { pIdMandataria = IdMandataria, pNombre = Nombre, pNombreMandataria = NombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa });
        }
    }

    public static void EliminarOS(int IdOS)
    {
        using (var connection = GetConnection())
        {
            string query = "DELETE FROM ObrasSociales WHERE IdObrasSociales = @pIdOS";
            connection.Execute(query, new { pIdOS = IdOS });
        }
    }

    public static void AgregarBonificaciones(int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
    {
        using (var connection = GetConnection())
        {
            string query = "UPDATE ObrasSociales SET NombreCodigoBonificacion = @pNombreCodigoBonificacion, CodigoBonificacion = @pCodigoBonificacion WHERE IdObrasSociales = @pIdOS";
            connection.Execute(query, new { pNombreCodigoBonificacion = NombreCodigoBonificacion, pCodigoBonificacion = CodigoBonificacion, pIdOS = IdOS });
        }
    }

    public static void ModificarBonificaciones(int IdOS, int? CodigoBonificacion, string? NombreCodigoBonificacion)
    {
        using (var connection = GetConnection())
        {
            string query = "UPDATE ObrasSociales SET CodigoBonificacion = @pCodigoBonificacion, NombreCodigoBonificacion = @pNombreCodigoBonificacion where IdObrasSociales = @pIdOS";
            connection.Execute(query, new { pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion });
        }
    }
}