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
 public static void ModificarOS  (int IdOS, string nuevoNombre, int IdMandatarias, string nombreMandataria, bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE ObrasSociales SET IdMandatarias = @pIdMandatarias, Nombre = @pNuevoNombre, NombreMandataria = @pNombreMandataria, EsPrepaga = @pEsPrepaga, Activa = @pActiva where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pNombreMandataria = nombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
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

  public static void AgregarOS  ( int IdMandataria, string Nombre, string NombreMandataria, bool? EsPrepaga, bool? Activa)
    {
     ObrasSociales ObjOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ObrasSociales ( IdMandatarias, Nombre, NombreMandataria, EsPrepaga, Activa) VALUES (@pIdMandataria, @pNombre, @pNombreMandataria, @pEsPrepaga, @pActiva)"; 
            connection.Execute(query, new {pIdMandataria = IdMandataria, pNombre = Nombre, pNombreMandataria = NombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa});
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

 // Models/BD.cs

public static void AgregarBonificaciones(int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
{
  
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
       
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
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE ObrasSociales SET CodigoBonificacion = @pCodigoBonificacion, NombreCodigoBonificacion = @pNombreCodigoBonificacion where IdObrasSociales = @pIdOS"; 
            connection.Execute(query, new {pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion});
         }
    
    }

public static List<PlanBonificacion> TraerPlanBonificacionPorId  (int IdOS)
    {
     List<PlanBonificacion> ObjPlanBoni = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM PlanBonificacion where IdObrasSociales = @pIdOS"; 
            ObjPlanBoni = connection.Query<PlanBonificacion>(query, new {pIdOS = IdOS}).ToList();
         }

         return ObjPlanBoni;
    }

public static void InsertarPlan(PlanBonificacion plan)
        {
            // Nivel Técnico: Usamos parámetros (@NombrePlan) para evitar SQL Injection.
            string query = "INSERT INTO PlanBonificacion (IdObrasSociales, NombrePlan, Bonificacion, NumeroBonificacion) " +
                           "VALUES (@IdObraSocial, @NombrePlan, @Bonificacion, @NumeroBonificacion)";
            
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // Dapper mapea automáticamente las propiedades del objeto 'plan' a los parámetros @
                    connection.Execute(query, plan); 
                }
                catch (SqlException ex)
                {
                    // Manejo de error de base de datos. Deberías loggear esto.
                    throw new Exception("Error al insertar el plan de bonificación en la base de datos.", ex);
                }
            }
        }

                public static void ActualizarPlan(PlanBonificacion plan)
        {
            // Nivel Técnico: La cláusula WHERE usa el PK para garantizar que solo se actualice una fila.
            string query = "UPDATE PlanBonificacion SET " +
                           "IdObrasSociales = @IdObraSocial, " +
                           "NombrePlan = @NombrePlan, " +
                           "Bonificacion = @Bonificacion, " +
                           "NumeroBonificacion = @NumeroBonificacion " +
                           "WHERE IdPlanBonificacion = @IdPlanBonificacion";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // Dapper encuentra las propiedades del objeto 'plan' (incluyendo el PK para el WHERE)
                    connection.Execute(query, plan); 
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al actualizar el plan de bonificación en la base de datos.", ex);
                }
            }
        }

        
        public static void EliminarPlan(int idPlan)
        {
            // Nivel Técnico: Delete simple con parámetro para el Id.
            string query = "DELETE FROM PlanBonificacion WHERE IdPlanBonificacion = @IdPlan";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // Pasamos un objeto anónimo para el parámetro @IdPlan
                    connection.Execute(query, new { IdPlan = idPlan }); 
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al eliminar el plan de bonificación en la base de datos.", ex);
                }
            }
        }





//-- Codigo Liquidaciones --///
      public static List<Liquidaciones> TraerListaLiquidacionesCompleta()
    {
        List<Liquidaciones> ListLiquidaciones = new List<Liquidaciones>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Liquidaciones";
            ListLiquidaciones = connection.Query<Liquidaciones>(query).ToList();
        }
        return ListLiquidaciones;
    }


public static int InsertarLiquidacionCabecera(Liquidaciones liq)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Usamos SCOPE_IDENTITY() para recuperar el ID autogenerado
        string query = @"
            INSERT INTO Liquidaciones (IdMandatarias, FechaPresentacion, TotalPresentado, Observaciones, Estado, Periodo) 
            VALUES (@IdMandatarias, @FechaPresentacion, @TotalPresentado, @Observaciones, 'PRESENTADA', @Periodo);
            SELECT CAST(SCOPE_IDENTITY() as int);";
        
        return connection.QuerySingle<int>(query, liq);
    }
}

// 2. INSERTAR DETALLE
public static void InsertarLiquidacionDetalle(int idLiquidacion, int idOS, int idPlan, int recetas, decimal bruto, decimal cargoOS, decimal bonificacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            INSERT INTO LiquidacionesDetalles (IdLiquidacion, IdObraSocial, IdPlan, CantidadRecetas, TotalBruto, MontoCargoOS, MontoBonificacion)
            VALUES (@pIdLiq, @pIdOS, @pIdPlan, @pRecetas, @pBruto, @pCargoOS, @pBoni)";

        connection.Execute(query, new { 
            pIdLiq = idLiquidacion, 
            pIdOS = idOS, 
            pIdPlan = idPlan,
            pRecetas = recetas,
            pBruto = bruto,
            pCargoOS = cargoOS,
            pBoni = bonificacion
        });
    }
}

// 3. BUSCAR CON FILTROS (MANDATARIA, FECHAS, ID)
public static List<Liquidaciones> BuscarLiquidaciones(int? idLiq, DateTime? desde, DateTime? hasta, int? idMandataria)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Construcción dinámica de la query (básica pero segura)
        string query = "SELECT * FROM Liquidaciones WHERE 1=1 ";
        
        if (idLiq.HasValue && idLiq.Value > 0)
            query += " AND IdLiquidaciones = @pIdLiq";
        
        if (desde.HasValue)
            query += " AND FechaPresentacion >= @pDesde";
            
        if (hasta.HasValue)
            query += " AND FechaPresentacion <= @pHasta";
            
        if (idMandataria.HasValue && idMandataria.Value > 0)
            query += " AND IdMandatarias = @pIdMand";

        query += " ORDER BY FechaPresentacion DESC";

        return connection.Query<Liquidaciones>(query, new { 
            pIdLiq = idLiq, 
            pDesde = desde, 
            pHasta = hasta, 
            pIdMand = idMandataria 
        }).ToList();
    }
}






































































}