using System.Data;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql; // Cambio Clave: Usamos el driver de PostgreSQL
using Microsoft.Extensions.Configuration; // Necesario si usaras config, pero usaremos Environment

namespace WebFarmaciaMiguetti.Models;

public static class BD
{
    // Método para construir la cadena de conexión leyendo las variables de Railway
    private static string GetConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        var port = Environment.GetEnvironmentVariable("PGPORT");
        var user = Environment.GetEnvironmentVariable("PGUSER");
        var pass = Environment.GetEnvironmentVariable("PGPASSWORD");
        var db = Environment.GetEnvironmentVariable("PGDATABASE");

        // Si las variables están vacías (entorno local sin configurar), retorna una cadena por defecto o vacía
        if (string.IsNullOrEmpty(host)) 
            return "Server=127.0.0.1;Port=5432;Database=FarmaciaNet;User Id=postgres;Password=admin;";

        // Cadena para Railway con SSL obligatorio
        return $"Host={host};Port={port};Database={db};Username={user};Password={pass};Pooling=true;SSL Mode=Require;Trust Server Certificate=true;";
    }

    //-- Codigo Mandatarias --///

    public static Mandatarias TraerMandatariaPorNombre(string nombreMandataria)
    {
        Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            // OJO: Comillas dobles en la tabla
            string query = "SELECT * FROM \"Mandatarias\" where \"RazonSocial\" = @pNombremandataria";
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new { pNombremandataria = nombreMandataria });
        }

        return ObjMandataria;
    }

    public static Mandatarias TraerMandatariaPorId(int IdMandatarias)
    {
        Mandatarias ObjMandataria = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Mandatarias\" where \"IdMandatarias\" = @pIdMandatarias";
            ObjMandataria = connection.QueryFirstOrDefault<Mandatarias>(query, new { pIdMandatarias = IdMandatarias });
        }

        return ObjMandataria;
    }

    public static void ModificarMandataria(int IdMandatarias, string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE \"Mandatarias\" SET \"RazonSocial\" = @pNuevoNombre, \"Cuit\" = @pCuit, \"Descripcion\" = @pDescripcion, \"Direccion\" = @pDireccion where \"IdMandatarias\" = @pIdMandatarias";
            connection.Execute(query, new { pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion });
        }
    }

    public static void AgregarMandataria(string nuevoNombre, long Cuit, string? Descripcion, string? Direccion)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "INSERT INTO \"Mandatarias\" (\"RazonSocial\", \"Cuit\", \"Descripcion\", \"Direccion\") VALUES (@pNuevoNombre, @pCuit, @pDescripcion, @pDireccion)";
            connection.Execute(query, new { pNuevoNombre = nuevoNombre, pCuit = Cuit, pDescripcion = Descripcion, pDireccion = Direccion });
        }
    }

    // EN: Models/BD.cs

    public static void EliminarMandataria(int IdMandatarias)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // ==============================================================================
                    // RAMA 1: LIQUIDACIONES (Relación Directa)
                    // ==============================================================================

                    // 1.1 Borrar Detalles de Liquidaciones
                    string sqlLiqDetalles = @"
                    DELETE FROM ""LiquidacionDetalle"" 
                    WHERE ""IdLiquidaciones"" IN (SELECT ""IdLiquidaciones"" FROM ""Liquidaciones"" WHERE ""IdMandatarias"" = @pId)";
                    connection.Execute(sqlLiqDetalles, new { pId = IdMandatarias }, transaction);

                    // 1.2 Borrar Cabeceras de Liquidaciones
                    string sqlLiqCabeceras = "DELETE FROM \"Liquidaciones\" WHERE \"IdMandatarias\" = @pId";
                    connection.Execute(sqlLiqCabeceras, new { pId = IdMandatarias }, transaction);

                    // ==============================================================================
                    // RAMA 2: OBRAS SOCIALES Y SUS DEPENDENCIAS
                    // ==============================================================================

                    // 2.1 Borrar COBROS DETALLE
                    string sqlCobrosDetalles = @"
                    DELETE FROM ""CobrosDetalle"" 
                    WHERE ""IdCobros"" IN (
                        SELECT ""IdCobros"" 
                        FROM ""Cobros"" 
                        WHERE ""IdObrasSociales"" IN (SELECT ""IdObrasSociales"" FROM ""ObrasSociales"" WHERE ""IdMandatarias"" = @pId)
                    )";
                    connection.Execute(sqlCobrosDetalles, new { pId = IdMandatarias }, transaction);

                    // 2.2 Borrar COBROS
                    string sqlCobrosCabeceras = @"
                    DELETE FROM ""Cobros"" 
                    WHERE ""IdObrasSociales"" IN (SELECT ""IdObrasSociales"" FROM ""ObrasSociales"" WHERE ""IdMandatarias"" = @pId)";
                    connection.Execute(sqlCobrosCabeceras, new { pId = IdMandatarias }, transaction);

                    // 2.3 Borrar Planes de Bonificación
                    string sqlPlanes = @"
                    DELETE FROM ""PlanBonificacion"" 
                    WHERE ""IdObrasSociales"" IN (SELECT ""IdObrasSociales"" FROM ""ObrasSociales"" WHERE ""IdMandatarias"" = @pId)";
                    connection.Execute(sqlPlanes, new { pId = IdMandatarias }, transaction);

                    // 2.4 Borrar Facturas (Si aplica)
                    string sqlFacturaDetalle = @"
                    DELETE FROM ""FacturaDetalle"" 
                    WHERE ""IdFacturaCabecera"" IN (
                        SELECT ""IdFacturaCabecera"" 
                        FROM ""FacturaCabecera"" 
                        WHERE ""IdObrasSociales"" IN (SELECT ""IdObrasSociales"" FROM ""ObrasSociales"" WHERE ""IdMandatarias"" = @pId)
                    )";
                    connection.Execute(sqlFacturaDetalle, new { pId = IdMandatarias }, transaction);

                    string sqlFacturaCabecera = @"
                    DELETE FROM ""FacturaCabecera"" 
                    WHERE ""IdObrasSociales"" IN (SELECT ""IdObrasSociales"" FROM ""ObrasSociales"" WHERE ""IdMandatarias"" = @pId)";
                    connection.Execute(sqlFacturaCabecera, new { pId = IdMandatarias }, transaction);

                    // 2.5 AHORA SÍ: Borrar Obras Sociales
                    string sqlOOSS = "DELETE FROM \"ObrasSociales\" WHERE \"IdMandatarias\" = @pId";
                    connection.Execute(sqlOOSS, new { pId = IdMandatarias }, transaction);

                    // ==============================================================================
                    // RAMA 3: EL PADRE
                    // ==============================================================================

                    string sqlMandataria = "DELETE FROM \"Mandatarias\" WHERE \"IdMandatarias\" = @pId";
                    connection.Execute(sqlMandataria, new { pId = IdMandatarias }, transaction);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public static List<Mandatarias> TraerListaMandatarias()
    {
        List<Mandatarias> ListMandatarias = new List<Mandatarias>();
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Mandatarias\"";
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
            string query = "SELECT *, \"IdMandatarias\" AS IdMandataria FROM \"ObrasSociales\"";
            ListOS = connection.Query<ObrasSociales>(query).ToList();
        }
        return ListOS;
    }

    public static void ModificarOS(int IdOS, string nuevoNombre, int IdMandatarias, string nombreMandataria, bool? EsPrepaga, bool? Activa)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE \"ObrasSociales\" SET \"IdMandatarias\" = @pIdMandatarias, \"Nombre\" = @pNuevoNombre, \"NombreMandataria\" = @pNombreMandataria, \"EsPrepaga\" = @pEsPrepaga, \"Activa\" = @pActiva where \"IdObrasSociales\" = @pIdOS";
            connection.Execute(query, new { pIdMandatarias = IdMandatarias, pNuevoNombre = nuevoNombre, pNombreMandataria = nombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa, pIdOS = IdOS });
        }
    }

    public static ObrasSociales TraerOSPorId(int IdOS)
    {
        ObrasSociales ObjOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT *, \"IdMandatarias\" AS IdMandataria FROM \"ObrasSociales\" where \"IdObrasSociales\" = @pIdOS";
            ObjOS = connection.QueryFirstOrDefault<ObrasSociales>(query, new { pIdOS = IdOS });
        }
        return ObjOS;
    }

    public static List<ObrasSociales> TraerOSPorIdMandataria(int IdMandataria)
    {
        List<ObrasSociales> listaOS = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT *, \"IdMandatarias\" AS IdMandataria FROM \"ObrasSociales\" where \"IdMandatarias\" = @pIdMandataria";
            listaOS = connection.Query<ObrasSociales>(query, new { pIdMandataria = IdMandataria }).ToList();
        }
        return listaOS;
    }

    public static void AgregarOS(int IdMandataria, string Nombre, string NombreMandataria, bool? EsPrepaga, bool? Activa)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "INSERT INTO \"ObrasSociales\" ( \"IdMandatarias\", \"Nombre\", \"NombreMandataria\", \"EsPrepaga\", \"Activa\") VALUES (@pIdMandataria, @pNombre, @pNombreMandataria, @pEsPrepaga, @pActiva)";
            connection.Execute(query, new { pIdMandataria = IdMandataria, pNombre = Nombre, pNombreMandataria = NombreMandataria, pEsPrepaga = EsPrepaga, pActiva = Activa });
        }
    }

    public static void EliminarOS(int IdOS)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Borrar PLANES DE BONIFICACIÓN
                    string sqlPlanes = "DELETE FROM \"PlanBonificacion\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlPlanes, new { pId = IdOS }, transaction);

                    // 2. Borrar DETALLES DE LIQUIDACIONES
                    string sqlLiqDet = "DELETE FROM \"LiquidacionDetalle\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlLiqDet, new { pId = IdOS }, transaction);

                    // 3. Borrar FACTURACIÓN
                    string sqlFacturaDet = @"
                    DELETE FROM ""FacturaDetalle"" 
                    WHERE ""IdFacturaCabecera"" IN (SELECT ""IdFacturaCabecera"" FROM ""FacturaCabecera"" WHERE ""IdObrasSociales"" = @pId)";
                    connection.Execute(sqlFacturaDet, new { pId = IdOS }, transaction);

                    string sqlFacturaCab = "DELETE FROM \"FacturaCabecera\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlFacturaCab, new { pId = IdOS }, transaction);

                    // 4. Borrar COBROS
                    string sqlCobroDet1 = "DELETE FROM \"CobrosDetalle\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlCobroDet1, new { pId = IdOS }, transaction);

                    string sqlCobroDet2 = @"
                    DELETE FROM ""CobrosDetalle"" 
                    WHERE ""IdCobros"" IN (SELECT ""IdCobros"" FROM ""Cobros"" WHERE ""IdObrasSociales"" = @pId)";
                    connection.Execute(sqlCobroDet2, new { pId = IdOS }, transaction);

                    string sqlCobros = "DELETE FROM \"Cobros\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlCobros, new { pId = IdOS }, transaction);

                    // 5. FINALMENTE: Borrar la OBRA SOCIAL PADRE
                    string sqlOS = "DELETE FROM \"ObrasSociales\" WHERE \"IdObrasSociales\" = @pId";
                    connection.Execute(sqlOS, new { pId = IdOS }, transaction);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public static void AgregarBonificaciones(int IdOS, string? NombreCodigoBonificacion, int? CodigoBonificacion)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE \"ObrasSociales\" SET \"NombreCodigoBonificacion\" = @pNombreCodigoBonificacion, \"CodigoBonificacion\" = @pCodigoBonificacion WHERE \"IdObrasSociales\" = @pIdOS";
            connection.Execute(query, new { pNombreCodigoBonificacion = NombreCodigoBonificacion, pCodigoBonificacion = CodigoBonificacion, pIdOS = IdOS });
        }
    }

    public static void ModificarBonificaciones(int IdOS, int? CodigoBonificacion, string? NombreCodigoBonificacion)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE \"ObrasSociales\" SET \"CodigoBonificacion\" = @pCodigoBonificacion, \"NombreCodigoBonificacion\" = @pNombreCodigoBonificacion where \"IdObrasSociales\" = @pIdOS";
            connection.Execute(query, new { pIdOS = IdOS, pCodigoBonificacion = CodigoBonificacion, pNombreCodigoBonificacion = NombreCodigoBonificacion });
        }
    }

    public static List<PlanBonificacion> TraerPlanBonificacionPorId(int IdOS)
    {
        List<PlanBonificacion> ObjPlanBoni = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            // TRUCO: Usamos 'AS IdObraSocial' para que Dapper llene tu propiedad en singular
            string query = "SELECT *, \"IdObrasSociales\" as IdObraSocial FROM \"PlanBonificacion\" where \"IdObrasSociales\" = @pIdOS";
            ObjPlanBoni = connection.Query<PlanBonificacion>(query, new { pIdOS = IdOS }).ToList();
        }
        return ObjPlanBoni;
    }

    public static void InsertarPlan(PlanBonificacion plan)
    {
        // CORRECCIÓN: En VALUES usamos @IdObraSocial (Singular) que es lo que tiene tu objeto
        string query = "INSERT INTO \"PlanBonificacion\" (\"IdObrasSociales\", \"NombrePlan\", \"Bonificacion\", \"NumeroBonificacion\") " +
                       "VALUES (@IdObraSocial, @NombrePlan, @Bonificacion, @NumeroBonificacion)";

        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            try
            {
                connection.Open();
                connection.Execute(query, plan);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("Error al insertar el plan. Verifique los datos.", ex);
            }
        }
    }

    public static void ActualizarPlan(PlanBonificacion plan)
    {
        // CORRECCIÓN: SET "IdObrasSociales" = @IdObraSocial (Singular)
        string query = "UPDATE \"PlanBonificacion\" SET " +
                       "\"IdObrasSociales\" = @IdObraSocial, " +
                       "\"NombrePlan\" = @NombrePlan, " +
                       "\"Bonificacion\" = @Bonificacion, " +
                       "\"NumeroBonificacion\" = @NumeroBonificacion " +
                       "WHERE \"IdPlanBonificacion\" = @IdPlanBonificacion";

        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            try
            {
                connection.Open();
                connection.Execute(query, plan);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("Error al actualizar el plan.", ex);
            }
        }
    }

    public static void EliminarPlan(int idPlan)
    {
        string query = "DELETE FROM \"PlanBonificacion\" WHERE \"IdPlanBonificacion\" = @IdPlan";
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            try
            {
                connection.Open();
                connection.Execute(query, new { IdPlan = idPlan });
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("Error al eliminar el plan de bonificación en la base de datos.", ex);
            }
        }
    }

    //-- Codigo Liquidaciones --///

    public static List<Liquidaciones> TraerListaLiquidacionesCompleta(int IdUsuario)
    {
        List<Liquidaciones> ListLiquidaciones = new List<Liquidaciones>();
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Liquidaciones\" WHERE \"IdUsuario\" = @IdUsuario";
            ListLiquidaciones = connection.Query<Liquidaciones>(query, new { IdUsuario = IdUsuario }).ToList();
        }
        return ListLiquidaciones;
    }

    public static int InsertarLiquidacionCabecera(Liquidaciones liq, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            // CORRECCIÓN POSTGRES: Usamos RETURNING en lugar de SCOPE_IDENTITY()
            string query = @"
            INSERT INTO ""Liquidaciones"" (""IdMandatarias"", ""FechaPresentacion"", ""TotalPresentado"", ""Observaciones"", ""Estado"", ""Periodo"", ""IdUsuario"") 
            VALUES (@IdMandatarias, @FechaPresentacion, @TotalPresentado, @Observaciones, 'PRESENTADA', @Periodo, @IdUsuario)
            RETURNING ""IdLiquidaciones"";";

            return connection.QuerySingle<int>(query, new
            {
                IdMandatarias = liq.IdMandatarias,
                FechaPresentacion = liq.FechaPresentacion,
                TotalPresentado = liq.TotalPresentado,
                Observaciones = liq.Observaciones,
                Periodo = liq.Periodo,
                IdUsuario = idUsuario
            });
        }
    }

    public static void InsertarLiquidacionDetalle(int idLiquidacion, int idOS, int idPlan, int recetas, decimal bruto, decimal cargoOS, decimal bonificacion, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            decimal saldoInicial = cargoOS - bonificacion;

            string query = @"INSERT INTO ""LiquidacionDetalle"" 
                        (""IdLiquidaciones"", ""IdObrasSociales"", ""IdPlanBonificacion"", ""CantidadRecetas"", ""TotalBruto"", ""MontoCargoOS"", ""MontoBonificacion"", ""SaldoPendiente"", ""Pagado"", ""IdUsuario"")
                        VALUES 
                        (@pIdLiq, @pIdOS, @pIdPlan, @pRecetas, @pBruto, @pCargoOS, @pBoni, @pSaldo, false, @pIdUsuario)";
            // Nota: En Postgres el bit se mapea a boolean (false)

            connection.Execute(query, new
            {
                pIdLiq = idLiquidacion,
                pIdOS = idOS,
                pIdPlan = idPlan,
                pRecetas = recetas,
                pBruto = bruto,
                pCargoOS = cargoOS,
                pBoni = bonificacion,
                pSaldo = saldoInicial,
                pIdUsuario = idUsuario
            });
        }
    }

    public static List<dynamic> BuscarLiquidaciones(int? id, DateTime? desde, DateTime? hasta, int? mandataria, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            // CORRECCIÓN POSTGRES: COALESCE en vez de ISNULL
            string query = @"
            SELECT 
                L.""IdLiquidaciones"",
                L.""FechaPresentacion"",
                L.""Periodo"",
                L.""Observaciones"",
                L.""IdMandatarias"",
                M.""RazonSocial"" as ""NombreMandataria"",
                COALESCE(SUM(LD.""MontoCargoOS"" - LD.""MontoBonificacion""), 0) as ""TotalReal"",
                COALESCE(SUM(LD.""SaldoPendiente""), 0) as ""SaldoPendiente"",

                (SELECT MAX(CD.""FechaCobroDetalle"") 
                 FROM ""CobrosDetalle"" CD 
                 INNER JOIN ""LiquidacionDetalle"" Det ON CD.""IdLiquidacionDetalle"" = Det.""IdLiquidacionDetalle""
                 WHERE Det.""IdLiquidaciones"" = L.""IdLiquidaciones""
                ) as ""FechaCancelacion""

            FROM ""Liquidaciones"" L
            INNER JOIN ""Mandatarias"" M ON L.""IdMandatarias"" = M.""IdMandatarias""
            LEFT JOIN ""LiquidacionDetalle"" LD ON L.""IdLiquidaciones"" = LD.""IdLiquidaciones""
            
            WHERE L.""IdUsuario"" = @pUser ";

            if (id.HasValue) query += " AND L.\"IdLiquidaciones\" = @pId";
            if (desde.HasValue) query += " AND L.\"FechaPresentacion\" >= @pDesde";
            if (hasta.HasValue) query += " AND L.\"FechaPresentacion\" <= @pHasta";
            if (mandataria.HasValue && mandataria.Value > 0) query += " AND L.\"IdMandatarias\" = @pMand";

            query += @" 
            GROUP BY L.""IdLiquidaciones"", L.""FechaPresentacion"", L.""Periodo"", L.""Observaciones"", L.""IdMandatarias"", M.""RazonSocial""
            ORDER BY L.""FechaPresentacion"" DESC";

            return connection.Query<dynamic>(query, new
            {
                pId = id,
                pDesde = desde,
                pHasta = hasta,
                pMand = mandataria,
                pUser = idUsuario
            }).ToList();
        }
    }

    public static List<LiquidacionDetalle> TraerDetallesPorIdLiquidacion(int idLiquidacion, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT 
                LD.*, 
                PB.""NombrePlan"",
                OS.""Nombre"" AS ""NombreObraSocial"",
                (SELECT MAX(""FechaCobroDetalle"") FROM ""CobrosDetalle"" WHERE ""IdLiquidacionDetalle"" = LD.""IdLiquidacionDetalle"") as ""FechaCancelacion""
            FROM ""LiquidacionDetalle"" LD
            INNER JOIN ""Liquidaciones"" L ON LD.""IdLiquidaciones"" = L.""IdLiquidaciones"" 
            LEFT JOIN ""PlanBonificacion"" PB ON LD.""IdPlanBonificacion"" = PB.""IdPlanBonificacion""
            LEFT JOIN ""ObrasSociales"" OS ON LD.""IdObrasSociales"" = OS.""IdObrasSociales""
            WHERE LD.""IdLiquidaciones"" = @pIdLiq AND L.""IdUsuario"" = @pIdUsuario";

            return connection.Query<LiquidacionDetalle>(query, new { pIdLiq = idLiquidacion, pIdUsuario = idUsuario }).ToList();
        }
    }

    public static void EliminarLiquidacion(int idLiquidacion, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var parametros = new { Id = idLiquidacion, IdUsuario = idUsuario };

                    // PASO 1
                    string queryGetCobros = @"
                    SELECT DISTINCT CD.""IdCobros"" 
                    FROM ""CobrosDetalle"" CD
                    INNER JOIN ""LiquidacionDetalle"" LD ON CD.""IdLiquidacionDetalle"" = LD.""IdLiquidacionDetalle""
                    WHERE LD.""IdLiquidaciones"" = @Id AND LD.""IdUsuario"" = @IdUsuario";

                    var listaCobrosAfectados = connection.Query<int>(queryGetCobros, parametros, transaction).ToList();

                    // PASO 2
                    string deleteCobrosDet = @"
                    DELETE FROM ""CobrosDetalle"" 
                    WHERE ""IdLiquidacionDetalle"" IN (
                        SELECT ""IdLiquidacionDetalle"" 
                        FROM ""LiquidacionDetalle"" 
                        WHERE ""IdLiquidaciones"" = @Id AND ""IdUsuario"" = @IdUsuario
                    )";
                    connection.Execute(deleteCobrosDet, parametros, transaction);

                    // PASO 3
                    foreach (var idCobro in listaCobrosAfectados)
                    {
                        string queryRecalculo = @"
                        SELECT COALESCE(SUM(""ImporteCobrado""), 0) 
                        FROM ""CobrosDetalle"" 
                        WHERE ""IdCobros"" = @IdCobro";

                        decimal nuevoTotal = connection.ExecuteScalar<decimal>(queryRecalculo, new { IdCobro = idCobro }, transaction);
                    }

                    // PASO 4
                    string deleteLiqDet = "DELETE FROM \"LiquidacionDetalle\" WHERE \"IdLiquidaciones\" = @Id AND \"IdUsuario\" = @IdUsuario";
                    connection.Execute(deleteLiqDet, parametros, transaction);

                    // PASO 5
                    string deleteLiqCab = "DELETE FROM \"Liquidaciones\" WHERE \"IdLiquidaciones\" = @Id AND \"IdUsuario\" = @IdUsuario";
                    connection.Execute(deleteLiqCab, parametros, transaction);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public static LiquidacionDetalle TraerDetallePorId(int idDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT LD.* FROM ""LiquidacionDetalle"" LD
            INNER JOIN ""Liquidaciones"" L ON LD.""IdLiquidaciones"" = L.""IdLiquidaciones""
            WHERE LD.""IdLiquidacionDetalle"" = @pId AND L.""IdUsuario"" = @pUser";

            return connection.QueryFirstOrDefault<LiquidacionDetalle>(query, new { pId = idDetalle, pUser = idUsuario });
        }
    }

    public static void AgregarItemIndividual(LiquidacionDetalle item, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();

            int existe = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM \"Liquidaciones\" WHERE \"IdLiquidaciones\" = @pLiq AND \"IdUsuario\" = @pUser",
                new { pLiq = item.IdLiquidaciones, pUser = idUsuario }
            );

            if (existe == 0) throw new Exception("No tiene permisos sobre esta liquidación.");

            string query = @"INSERT INTO ""LiquidacionDetalle"" (
                            ""IdLiquidaciones"", ""IdObrasSociales"", ""IdPlanBonificacion"", 
                            ""CantidadRecetas"", ""TotalBruto"", ""MontoCargoOS"", ""MontoBonificacion"", 
                            ""SaldoPendiente"", ""Pagado""
                         )
                         VALUES (
                            @IdLiquidaciones, @IdObrasSociales, @IdPlanBonificacion, 
                            @CantidadRecetas, @TotalBruto, @MontoCargoOS, @MontoBonificacion, 
                            @SaldoPendiente, false
                         )";

            connection.Execute(query, item);

            ActualizarTotalCabecera(item.IdLiquidaciones, connection);
        }
    }

    public static void EliminarItemIndividual(int idDetalle, int idLiquidacionPadre, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var parametros = new { IdDetalle = idDetalle, pUser = idUsuario };

                    string queryGetCobros = @"
                    SELECT DISTINCT CD.""IdCobros"" 
                    FROM ""CobrosDetalle"" CD
                    INNER JOIN ""LiquidacionDetalle"" LD ON CD.""IdLiquidacionDetalle"" = LD.""IdLiquidacionDetalle""
                    INNER JOIN ""Liquidaciones"" L ON LD.""IdLiquidaciones"" = L.""IdLiquidaciones""
                    WHERE LD.""IdLiquidacionDetalle"" = @IdDetalle AND L.""IdUsuario"" = @pUser";

                    var listaCobrosAfectados = connection.Query<int>(queryGetCobros, parametros, transaction).ToList();

                    string deleteCobros = "DELETE FROM \"CobrosDetalle\" WHERE \"IdLiquidacionDetalle\" = @IdDetalle";
                    connection.Execute(deleteCobros, parametros, transaction);

                    string deleteItem = @"
                    DELETE FROM ""LiquidacionDetalle"" 
                    WHERE ""IdLiquidacionDetalle"" = @IdDetalle
                    AND EXISTS (SELECT 1 FROM ""Liquidaciones"" L WHERE L.""IdLiquidaciones"" = ""LiquidacionDetalle"".""IdLiquidaciones"" AND L.""IdUsuario"" = @pUser)";

                    int rows = connection.Execute(deleteItem, parametros, transaction);

                    if (rows > 0)
                    {
                        ActualizarTotalCabecera(idLiquidacionPadre, connection);

                        foreach (var idCobro in listaCobrosAfectados)
                        {
                            string querySumCobro = "SELECT COALESCE(SUM(\"ImporteCobrado\"), 0) FROM \"CobrosDetalle\" WHERE \"IdCobros\" = @IdCobro";
                            decimal nuevoTotalCobro = connection.ExecuteScalar<decimal>(querySumCobro, new { IdCobro = idCobro }, transaction);
                        }
                    }

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    private static void ActualizarTotalCabecera(int idLiq, NpgsqlConnection conn)
    {
        string sql = @"
        UPDATE ""Liquidaciones"" 
        SET ""TotalPresentado"" = (
            SELECT COALESCE(SUM(""MontoCargoOS"" - ""MontoBonificacion""), 0) 
            FROM ""LiquidacionDetalle"" 
            WHERE ""IdLiquidaciones"" = @id
        )
        WHERE ""IdLiquidaciones"" = @id";

        conn.Execute(sql, new { id = idLiq });
    }

    public static void ModificarItemIndividual(LiquidacionDetalle item, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();

            string query = @"
            UPDATE ""LiquidacionDetalle"" 
            SET 
                ""IdObrasSociales"" = @IdObrasSociales, 
                ""IdPlanBonificacion"" = @IdPlanBonificacion, 
                ""CantidadRecetas"" = @CantidadRecetas, 
                ""TotalBruto"" = @TotalBruto, 
                ""MontoCargoOS"" = @MontoCargoOS, 
                ""MontoBonificacion"" = @MontoBonificacion, 
                
                ""SaldoPendiente"" = (@MontoCargoOS - @MontoBonificacion) - COALESCE((
                    SELECT SUM(""ImporteCobrado"" + ""MontoDebito"") 
                    FROM ""CobrosDetalle"" 
                    WHERE ""IdLiquidacionDetalle"" = @IdLiquidacionDetalle
                ), 0)

            WHERE ""IdLiquidacionDetalle"" = @IdLiquidacionDetalle
            AND EXISTS (
                SELECT 1 FROM ""Liquidaciones"" L 
                WHERE L.""IdLiquidaciones"" = ""LiquidacionDetalle"".""IdLiquidaciones"" 
                AND L.""IdUsuario"" = @pUser
            );

            UPDATE ""LiquidacionDetalle""
            SET ""Pagado"" = CASE WHEN ""SaldoPendiente"" <= 1.00 THEN true ELSE false END
            WHERE ""IdLiquidacionDetalle"" = @IdLiquidacionDetalle;
        ";

            int filasAfectadas = connection.Execute(query, new
            {
                item.IdLiquidacionDetalle,
                item.IdObrasSociales,
                item.IdPlanBonificacion,
                item.CantidadRecetas,
                item.TotalBruto,
                item.MontoCargoOS,
                item.MontoBonificacion,
                pUser = idUsuario
            });

            if (filasAfectadas == 0) throw new Exception("No se pudo modificar el ítem (No encontrado o sin permisos).");

            ActualizarTotalCabecera(item.IdLiquidaciones, connection);
        }
    }

    public static Liquidaciones TraerLiquidacionPorId(int id, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Liquidaciones\" WHERE \"IdLiquidaciones\" = @pId AND \"IdUsuario\" = @pUser";
            return connection.QueryFirstOrDefault<Liquidaciones>(query, new { pId = id, pUser = idUsuario });
        }
    }

    public static void ModificarLiquidacionCabecera(int id, int idMandataria, DateTime fecha, string obs, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            UPDATE ""Liquidaciones"" 
            SET ""IdMandatarias"" = @pMand, ""FechaPresentacion"" = @pFecha, ""Observaciones"" = @pObs, ""Periodo"" = @pPeriodo
            WHERE ""IdLiquidaciones"" = @pId AND ""IdUsuario"" = @pUser";

            connection.Execute(query, new
            {
                pId = id,
                pMand = idMandataria,
                pFecha = fecha,
                pObs = obs,
                pPeriodo = fecha.ToString("MM-yyyy"),
                pUser = idUsuario
            });
        }
    }

    // =============================================================================
    // BÚSQUEDA GLOBAL DE DETALLES
    // =============================================================================

    public static List<dynamic> BuscarDetallesGlobales(DateTime? desde, DateTime? hasta, int? idMandataria, int? idOS, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT LD.""IdLiquidacionDetalle"", LD.""IdLiquidaciones"", L.""FechaPresentacion"", L.""Periodo"",
                M.""RazonSocial"" as ""NombreMandataria"", OS.""Nombre"" as ""NombreObraSocial"", PB.""NombrePlan"",
                LD.""CantidadRecetas"", LD.""TotalBruto"" as ""BrutoOriginal"", LD.""MontoCargoOS"", LD.""MontoBonificacion"",
                (LD.""MontoCargoOS"" - LD.""MontoBonificacion"") as ""TotalBruto"", LD.""SaldoPendiente"", LD.""Pagado"",
                (SELECT MAX(""FechaCobroDetalle"") FROM ""CobrosDetalle"" WHERE ""IdLiquidacionDetalle"" = LD.""IdLiquidacionDetalle"") as ""FechaCancelacion""
            FROM ""LiquidacionDetalle"" LD
            INNER JOIN ""Liquidaciones"" L ON LD.""IdLiquidaciones"" = L.""IdLiquidaciones""
            INNER JOIN ""Mandatarias"" M ON L.""IdMandatarias"" = M.""IdMandatarias""
            INNER JOIN ""ObrasSociales"" OS ON LD.""IdObrasSociales"" = OS.""IdObrasSociales""
            LEFT JOIN ""PlanBonificacion"" PB ON LD.""IdPlanBonificacion"" = PB.""IdPlanBonificacion""
            WHERE L.""IdUsuario"" = @pIdUser ";

            if (desde.HasValue) query += " AND L.\"FechaPresentacion\" >= @pDesde";
            if (hasta.HasValue) query += " AND L.\"FechaPresentacion\" <= @pHasta";
            if (idMandataria.HasValue && idMandataria.Value > 0) query += " AND M.\"IdMandatarias\" = @pMand";
            if (idOS.HasValue && idOS.Value > 0) query += " AND OS.\"IdObrasSociales\" = @pOS";

            query += " ORDER BY L.\"FechaPresentacion\" DESC, M.\"RazonSocial\", OS.\"Nombre\"";

            return connection.Query<dynamic>(query, new { pDesde = desde, pHasta = hasta, pMand = idMandataria, pOS = idOS, pIdUser = idUsuario }).ToList();
        }
    }

    public static Usuario TraerUsuarioPorId(int idUsuario)
    {
        Usuario ObjUsuario = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Usuario\" where \"IdUsuario\" = @pIdUsuario";
            ObjUsuario = connection.QueryFirstOrDefault<Usuario>(query, new { pIdUsuario = idUsuario });
        }
        return ObjUsuario;
    }

    public static Usuario TraerUsuarioPorContraseña(string contraseña)
    {
        Usuario ObjUsuario = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT * FROM \"Usuario\" where \"Contraseña\" = @pContraseña";
            ObjUsuario = connection.QueryFirstOrDefault<Usuario>(query, new { pContraseña = contraseña });
        }
        return ObjUsuario;
    }

    public static void ModificarUsuario(int IdUsuario, string? Contraseña, string? RazonSocial, string? Domicilio, long? Cuit, string? Iva)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "UPDATE \"Usuario\" SET \"Contraseña\" = @pContraseña, \"RazonSocial\" = @pRazonSocial, \"Domicilio\" = @pDomicilio, \"Cuit\" = @pCuit, \"Iva\" = @pIva where \"IdUsuario\" = @pIdUsuario";
            connection.Execute(query, new { pIdUsuario = IdUsuario, pContraseña = Contraseña, pRazonSocial = RazonSocial, pDomicilio = Domicilio, pCuit = Cuit, pIva = Iva });
        }
    }

    // =============================================================================
    // SECCIÓN COBROS
    // =============================================================================

    public static List<dynamic> TraerDeudasPendientesPorOS(int idObraSocial, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT 
                LD.""IdLiquidacionDetalle"", L.""Periodo"", L.""FechaPresentacion"", 
                LD.""TotalBruto"" as ""ImporteOriginal"",
                COALESCE(LD.""SaldoPendiente"", LD.""TotalBruto"") as ""SaldoPendiente""
            FROM ""LiquidacionDetalle"" LD
            INNER JOIN ""Liquidaciones"" L ON LD.""IdLiquidaciones"" = L.""IdLiquidaciones""
            WHERE LD.""IdObrasSociales"" = @pIdOS 
              AND L.""IdUsuario"" = @pUser 
              AND (LD.""Pagado"" = false OR LD.""Pagado"" IS NULL) 
              AND COALESCE(LD.""SaldoPendiente"", LD.""TotalBruto"") > 0.05
            ORDER BY L.""FechaPresentacion"" ASC";

            return connection.Query<dynamic>(query, new { pIdOS = idObraSocial, pUser = idUsuario }).ToList();
        }
    }

    public static List<Liquidaciones> TraerLiquidacionesPendientesPorOS(int idObraSocial, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT DISTINCT L.""IdLiquidaciones"", L.""FechaPresentacion"", L.""TotalPresentado"", L.""Periodo""
            FROM ""Liquidaciones"" L
            INNER JOIN ""LiquidacionDetalle"" LD ON L.""IdLiquidaciones"" = LD.""IdLiquidaciones""
            WHERE LD.""IdObrasSociales"" = @pIdOS AND L.""IdUsuario"" = @pUser
            ORDER BY L.""FechaPresentacion"" DESC";

            return connection.Query<Liquidaciones>(query, new { pIdOS = idObraSocial, pUser = idUsuario }).ToList();
        }
    }

    public static List<dynamic> BuscarCobros(DateTime? desde, DateTime? hasta, int? idMandataria, int? idObraSocial, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT 
                C.""IdCobros"", C.""NumeroComprobante"", CAST(C.""FechaCobro"" AS DATE) as ""FechaCobro"",
                M.""RazonSocial"" as ""NombreMandataria"", M.""IdMandatarias"",
                COUNT(CD.""IdCobrosDetalle"") as ""CantidadItems"",
                COALESCE(SUM(CD.""ImporteCobrado""), 0) as ""TotalImporte"",
                COALESCE(SUM(CD.""MontoDebito""), 0) as ""TotalDebitos""
            FROM ""Cobros"" C
            INNER JOIN ""Mandatarias"" M ON C.""IdMandatarias"" = M.""IdMandatarias""
            LEFT JOIN ""CobrosDetalle"" CD ON C.""IdCobros"" = CD.""IdCobros""
            WHERE C.""IdUsuario"" = @pUser ";

            if (desde.HasValue) query += " AND C.\"FechaCobro\" >= @pDesde";
            if (hasta.HasValue) query += " AND C.\"FechaCobro\" <= @pHasta";
            if (idMandataria.HasValue && idMandataria.Value > 0) query += " AND M.\"IdMandatarias\" = @pIdMand";

            if (idObraSocial.HasValue && idObraSocial.Value > 0)
            {
                query += " AND EXISTS (SELECT 1 FROM \"CobrosDetalle\" sub WHERE sub.\"IdCobros\" = C.\"IdCobros\" AND sub.\"IdObrasSociales\" = @pIdOS)";
            }

            query += @" 
            GROUP BY C.""IdCobros"", C.""NumeroComprobante"", CAST(C.""FechaCobro"" AS DATE), M.""RazonSocial"", M.""IdMandatarias""
            ORDER BY C.""FechaCobro"" DESC";

            return connection.Query<dynamic>(query, new { pDesde = desde, pHasta = hasta, pIdMand = idMandataria, pIdOS = idObraSocial, pUser = idUsuario }).ToList();
        }
    }

    public static int AgregarCobroCabecera(int idMandataria, DateTime fecha, string comprobante, int? idLiquidacion, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            // CORRECCIÓN POSTGRES: RETURNING en vez de OUTPUT INSERTED
            string query = @"
            INSERT INTO ""Cobros"" (""IdMandatarias"", ""FechaCobro"", ""NumeroComprobante"", ""IdLiquidaciones"", ""IdUsuario"")
            VALUES (@pMand, @pFecha, @pComp, @pLiq, @pUser)
            RETURNING ""IdCobros""";

            return connection.QuerySingle<int>(query, new
            {
                pMand = idMandataria,
                pFecha = fecha,
                pComp = comprobante,
                pLiq = idLiquidacion,
                pUser = idUsuario
            });
        }
    }

    public static void EliminarLoteCompleto(int idCobroPadre, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int esMio = connection.ExecuteScalar<int>(
                        "SELECT COUNT(1) FROM \"Cobros\" WHERE \"IdCobros\" = @id AND \"IdUsuario\" = @user",
                        new { id = idCobroPadre, user = idUsuario },
                        transaction
                    );

                    if (esMio == 0) throw new Exception("No tiene permisos para eliminar este lote.");

                    // CORRECCIÓN SQL: Postgres usa sintaxis distinta para UPDATE ... FROM
                    string sqlRestaurarMasivo = @"
                    UPDATE ""LiquidacionDetalle"" AS L
                    SET ""SaldoPendiente"" = L.""SaldoPendiente"" + (C.""ImporteCobrado"" + C.""MontoDebito""), 
                        ""Pagado"" = false
                    FROM ""CobrosDetalle"" AS C
                    WHERE L.""IdLiquidacionDetalle"" = C.""IdLiquidacionDetalle""
                    AND C.""IdCobros"" = @pIdPadre";

                    connection.Execute(sqlRestaurarMasivo, new { pIdPadre = idCobroPadre }, transaction);

                    connection.Execute("DELETE FROM \"CobrosDetalle\" WHERE \"IdCobros\" = @pId", new { pId = idCobroPadre }, transaction);
                    connection.Execute("DELETE FROM \"Cobros\" WHERE \"IdCobros\" = @pId", new { pId = idCobroPadre }, transaction);

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    public static void ModificarCobroCabecera(int idCobro, int idMandataria, DateTime fecha, string comprobante, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            UPDATE ""Cobros"" 
            SET ""IdMandatarias"" = @pMand, 
                ""FechaCobro"" = @pFecha, 
                ""NumeroComprobante"" = @pComp
        WHERE ""IdCobros"" = @pId AND ""IdUsuario"" = @pUser";

            connection.Execute(query, new
            {
                pId = idCobro,
                pMand = idMandataria,
                pFecha = fecha,
                pComp = comprobante,
                pUser = idUsuario
            });
        }
    }

    public static void AgregarCobroDetalle(int idCobroPadre, int idObraSocial, DateTime fecha, decimal importe, string tipo, decimal debito, string motivo, int? idLiquidacionDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();

            int esMio = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM \"Cobros\" WHERE \"IdCobros\" = @pId AND \"IdUsuario\" = @pUser",
                new { pId = idCobroPadre, pUser = idUsuario }
            );

            if (esMio == 0) throw new Exception("No tiene permisos sobre este lote de cobros.");

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string sqlInsert = @"INSERT INTO ""CobrosDetalle"" 
                                   (""IdCobros"", ""IdObrasSociales"", ""FechaCobroDetalle"", ""ImporteCobrado"", ""TipoPago"", ""MontoDebito"", ""MotivoDebito"", ""IdLiquidacionDetalle"")
                                   VALUES 
                                   (@pIdPadre, @pOS, @pFecha, @pImporte, @pTipo, @pDebito, @pMotivo, @pIdLiqDet)";

                    connection.Execute(sqlInsert, new
                    {
                        pIdPadre = idCobroPadre,
                        pOS = idObraSocial,
                        pFecha = fecha,
                        pImporte = importe,
                        pTipo = tipo,
                        pDebito = debito,
                        pMotivo = motivo ?? "",
                        pIdLiqDet = idLiquidacionDetalle
                    }, transaction);

                    if (idLiquidacionDetalle.HasValue && idLiquidacionDetalle.Value > 0)
                    {
                        string sqlUpdateLiq = @"UPDATE ""LiquidacionDetalle"" 
                                            SET ""SaldoPendiente"" = ""SaldoPendiente"" - (@pPago + @pDeb)
                                            WHERE ""IdLiquidacionDetalle"" = @pIdLiqDet";

                        connection.Execute(sqlUpdateLiq, new
                        {
                            pPago = importe,
                            pDeb = debito,
                            pIdLiqDet = idLiquidacionDetalle
                        }, transaction);

                        string sqlCheckPagado = @"UPDATE ""LiquidacionDetalle"" 
                                              SET ""Pagado"" = true
                                              WHERE ""IdLiquidacionDetalle"" = @pIdLiqDet AND ""SaldoPendiente"" <= 0.05";

                        connection.Execute(sqlCheckPagado, new { pIdLiqDet = idLiquidacionDetalle }, transaction);
                    }

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    public static void ModificarCobroDetalle(int idCobroDetalle, int idObraSocial, DateTime fecha, decimal importe, string tipo, decimal debitos, string motivo, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int esMio = connection.ExecuteScalar<int>(
                        @"SELECT COUNT(1) 
                      FROM ""CobrosDetalle"" CD 
                      INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros"" 
                      WHERE CD.""IdCobrosDetalle"" = @pId AND C.""IdUsuario"" = @pUser",
                        new { pId = idCobroDetalle, pUser = idUsuario },
                        transaction
                    );

                    if (esMio == 0) throw new Exception("⛔ No tiene permisos para modificar este cobro (o no existe).");

                    var anterior = connection.QueryFirstOrDefault<dynamic>(
                        "SELECT \"ImporteCobrado\", \"MontoDebito\", \"IdLiquidacionDetalle\" FROM \"CobrosDetalle\" WHERE \"IdCobrosDetalle\" = @id",
                        new { id = idCobroDetalle },
                        transaction
                    );

                    if (anterior != null && anterior.IdLiquidacionDetalle != null)
                    {
                        string sqlRevertir = @"
                        UPDATE ""LiquidacionDetalle"" 
                        SET ""SaldoPendiente"" = ""SaldoPendiente"" + (@imp + @deb), 
                            ""Pagado"" = false
                        WHERE ""IdLiquidacionDetalle"" = @idLiq";

                        connection.Execute(sqlRevertir, new
                        {
                            imp = anterior.ImporteCobrado,
                            deb = anterior.MontoDebito,
                            idLiq = anterior.IdLiquidacionDetalle
                        }, transaction);
                    }

                    string sqlUpdate = @"UPDATE ""CobrosDetalle"" SET 
                        ""IdObrasSociales"" = @idOS, 
                        ""FechaCobroDetalle"" = @fecha, 
                        ""ImporteCobrado"" = @imp, 
                        ""TipoPago"" = @tipo, 
                        ""MontoDebito"" = @deb, 
                        ""MotivoDebito"" = @mot 
                        WHERE ""IdCobrosDetalle"" = @id";

                    connection.Execute(sqlUpdate, new
                    {
                        id = idCobroDetalle,
                        idOS = idObraSocial,
                        fecha = fecha,
                        imp = importe,
                        tipo = tipo,
                        deb = debitos,
                        mot = motivo ?? ""
                    }, transaction);

                    if (anterior != null && anterior.IdLiquidacionDetalle != null)
                    {
                        string sqlAplicar = @"
                        UPDATE ""LiquidacionDetalle"" 
                        SET ""SaldoPendiente"" = ""SaldoPendiente"" - (@imp + @deb)
                        WHERE ""IdLiquidacionDetalle"" = @idLiq";

                        connection.Execute(sqlAplicar, new
                        {
                            imp = importe,
                            deb = debitos,
                            idLiq = anterior.IdLiquidacionDetalle
                        }, transaction);

                        string sqlCheck = @"
                        UPDATE ""LiquidacionDetalle"" 
                        SET ""Pagado"" = true
                        WHERE ""IdLiquidacionDetalle"" = @idLiq AND ""SaldoPendiente"" <= 1.00";

                        connection.Execute(sqlCheck, new { idLiq = anterior.IdLiquidacionDetalle }, transaction);
                    }

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    public static void EliminarCobroDetalle(int idCobroDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var info = connection.QueryFirstOrDefault<dynamic>(
                        @"SELECT CD.""ImporteCobrado"", CD.""MontoDebito"", CD.""IdLiquidacionDetalle"" 
                      FROM ""CobrosDetalle"" CD
                      INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros""
                      WHERE CD.""IdCobrosDetalle"" = @id AND C.""IdUsuario"" = @user",
                        new { id = idCobroDetalle, user = idUsuario },
                        transaction
                    );

                    if (info == null) throw new Exception("Cobro no encontrado o sin permisos.");

                    if (info.IdLiquidacionDetalle != null)
                    {
                        string sqlRestaurar = @"
                        UPDATE ""LiquidacionDetalle"" 
                        SET ""SaldoPendiente"" = ""SaldoPendiente"" + (@imp + @deb), 
                            ""Pagado"" = false
                        WHERE ""IdLiquidacionDetalle"" = @idLiq";

                        connection.Execute(sqlRestaurar, new
                        {
                            imp = info.ImporteCobrado,
                            deb = info.MontoDebito,
                            idLiq = info.IdLiquidacionDetalle
                        }, transaction);
                    }

                    connection.Execute("DELETE FROM \"CobrosDetalle\" WHERE \"IdCobrosDetalle\" = @id", new { id = idCobroDetalle }, transaction);

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    public static List<dynamic> TraerCobrosDelMismoLote(int idCobroPadre, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT CD.*, OS.""Nombre"" as ""NombreObraSocial""
            FROM ""CobrosDetalle"" CD
            INNER JOIN ""ObrasSociales"" OS ON CD.""IdObrasSociales"" = OS.""IdObrasSociales""
            INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros"" 
            WHERE CD.""IdCobros"" = @pId AND C.""IdUsuario"" = @pUser 
            ORDER BY CD.""IdCobrosDetalle"" DESC";

            return connection.Query<dynamic>(query, new { pId = idCobroPadre, pUser = idUsuario }).ToList();
        }
    }

    public static dynamic TraerCobroDetallePorId(int idCobroDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT CD.* FROM ""CobrosDetalle"" CD
            INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros""
            WHERE CD.""IdCobrosDetalle"" = @pId AND C.""IdUsuario"" = @pUser";

            return connection.QueryFirstOrDefault<dynamic>(query, new { pId = idCobroDetalle, pUser = idUsuario });
        }
    }

    public static int? BuscarIdPadre(string comprobante, int idMandataria, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = "SELECT \"IdCobros\" FROM \"Cobros\" WHERE \"NumeroComprobante\" = @pComp AND \"IdMandatarias\" = @pMand AND \"IdUsuario\" = @pUser";
            return connection.QueryFirstOrDefault<int?>(query, new { pComp = comprobante, pMand = idMandataria, pUser = idUsuario });
        }
    }

    public static List<dynamic> ObtenerCobrosPorLiquidacionDetalle(int idLiquidacionDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            string query = @"
            SELECT C.""FechaCobro"", C.""NumeroComprobante"", CD.""ImporteCobrado"", CD.""MontoDebito"", CD.""MotivoDebito""
            FROM ""CobrosDetalle"" CD
            INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros""
            WHERE CD.""IdLiquidacionDetalle"" = @pId AND C.""IdUsuario"" = @pUser
            ORDER BY C.""FechaCobro"" DESC";

            return connection.Query<dynamic>(query, new { pId = idLiquidacionDetalle, pUser = idUsuario }).ToList();
        }
    }

    public static void GuardarCobroDetalle(int idCobroDetalle, int idCobroPadre, int idOS, DateTime fecha, string tipo, decimal importe, decimal debito, string motivo, int? idLiqDetalle, int idUsuario)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString()))
        {
            connection.Open();

            string query;

            if (idCobroDetalle > 0)
            {
                // CASO EDICIÓN (UPDATE)
                string sqlCheckOwner = @"
                SELECT COUNT(1) 
                FROM ""CobrosDetalle"" CD 
                INNER JOIN ""Cobros"" C ON CD.""IdCobros"" = C.""IdCobros"" 
                WHERE CD.""IdCobrosDetalle"" = @pId AND C.""IdUsuario"" = @pUser";

                int esMio = connection.ExecuteScalar<int>(sqlCheckOwner, new { pId = idCobroDetalle, pUser = idUsuario });

                if (esMio == 0) throw new Exception("⛔ No tiene permisos para modificar este cobro (o no existe).");

                query = @"UPDATE ""CobrosDetalle"" SET 
                        ""IdObrasSociales"" = @pOS,
                        ""FechaCobroDetalle"" = @pFecha,
                        ""TipoPago"" = @pTipo,
                        ""ImporteCobrado"" = @pImp,
                        ""MontoDebito"" = @pDeb,
                        ""MotivoDebito"" = @pMot
                      WHERE ""IdCobrosDetalle"" = @pId";
            }
            else
            {
                // CASO NUEVO (INSERT)
                string sqlCheckPadre = "SELECT COUNT(1) FROM \"Cobros\" WHERE \"IdCobros\" = @pId AND \"IdUsuario\" = @pUser";

                int esMio = connection.ExecuteScalar<int>(sqlCheckPadre, new { pId = idCobroPadre, pUser = idUsuario });

                if (esMio == 0) throw new Exception("⛔ No tiene permisos sobre el Lote de Cobros seleccionado.");

                query = @"INSERT INTO ""CobrosDetalle"" 
                      (""IdCobros"", ""IdObrasSociales"", ""FechaCobroDetalle"", ""TipoPago"", ""ImporteCobrado"", ""MontoDebito"", ""MotivoDebito"", ""IdLiquidacionDetalle"")
                      VALUES 
                      (@pPadre, @pOS, @pFecha, @pTipo, @pImp, @pDeb, @pMot, @pLiq)";
            }

            connection.Execute(query, new
            {
                pId = idCobroDetalle,
                pPadre = idCobroPadre,
                pOS = idOS,
                pFecha = fecha,
                pTipo = tipo,
                pImp = importe,
                pDeb = debito,
                pMot = motivo ?? "",
                pLiq = idLiqDetalle
            });
        }
    }
}