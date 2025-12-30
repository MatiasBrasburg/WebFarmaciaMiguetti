using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Identity.Client;
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

     // EN: Models/BD.cs

public static void EliminarMandataria(int IdMandatarias)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
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
                    DELETE FROM LiquidacionDetalle 
                    WHERE IdLiquidaciones IN (SELECT IdLiquidaciones FROM Liquidaciones WHERE IdMandatarias = @pId)";
                connection.Execute(sqlLiqDetalles, new { pId = IdMandatarias }, transaction);

                // 1.2 Borrar Cabeceras de Liquidaciones
                string sqlLiqCabeceras = "DELETE FROM Liquidaciones WHERE IdMandatarias = @pId";
                connection.Execute(sqlLiqCabeceras, new { pId = IdMandatarias }, transaction);

                // ==============================================================================
                // RAMA 2: OBRAS SOCIALES Y SUS DEPENDENCIAS (Aquí estaba el error)
                // ==============================================================================

                // 2.1 Borrar COBROS DETALLE (Nietos de Obras Sociales)
                // Borramos los detalles de cobros cuyos padres (Cobros) pertenecen a Obras Sociales de esta Mandataria
                string sqlCobrosDetalles = @"
                    DELETE FROM CobrosDetalle 
                    WHERE IdCobros IN (
                        SELECT IdCobros 
                        FROM Cobros 
                        WHERE IdObrasSociales IN (SELECT IdObrasSociales FROM ObrasSociales WHERE IdMandatarias = @pId)
                    )";
                connection.Execute(sqlCobrosDetalles, new { pId = IdMandatarias }, transaction);

                // 2.2 Borrar COBROS (Hijos de Obras Sociales)
                // CORRECCIÓN CRÍTICA: Borramos por IdObrasSociales, no por IdMandatarias directo.
                // Esto asegura que si el Cobro tiene IdMandataria NULL pero apunta a la OS, se borre igual.
                string sqlCobrosCabeceras = @"
                    DELETE FROM Cobros 
                    WHERE IdObrasSociales IN (SELECT IdObrasSociales FROM ObrasSociales WHERE IdMandatarias = @pId)";
                connection.Execute(sqlCobrosCabeceras, new { pId = IdMandatarias }, transaction);

                // 2.3 Borrar Planes de Bonificación (Hijos de Obras Sociales)
                string sqlPlanes = @"
                    DELETE FROM PlanBonificacion 
                    WHERE IdObrasSociales IN (SELECT IdObrasSociales FROM ObrasSociales WHERE IdMandatarias = @pId)";
                connection.Execute(sqlPlanes, new { pId = IdMandatarias }, transaction);

                // 2.4 Borrar Facturas Cabecera y Detalle (SI APLICARA)
                // OJO: Revisando tu esquema, FacturaCabecera también depende de ObraSocial. 
                // Si hay facturas, esto también fallará. Agrego la limpieza de facturas por seguridad.
                
                // Borrar Detalles de Facturas de las OOSS de esta Mandataria
                string sqlFacturaDetalle = @"
                    DELETE FROM FacturaDetalle 
                    WHERE IdFacturaCabecera IN (
                        SELECT IdFacturaCabecera 
                        FROM FacturaCabecera 
                        WHERE IdObrasSociales IN (SELECT IdObrasSociales FROM ObrasSociales WHERE IdMandatarias = @pId)
                    )";
                connection.Execute(sqlFacturaDetalle, new { pId = IdMandatarias }, transaction);

                // Borrar Cabeceras de Facturas
                string sqlFacturaCabecera = @"
                    DELETE FROM FacturaCabecera 
                    WHERE IdObrasSociales IN (SELECT IdObrasSociales FROM ObrasSociales WHERE IdMandatarias = @pId)";
                connection.Execute(sqlFacturaCabecera, new { pId = IdMandatarias }, transaction);

                // 2.5 AHORA SÍ: Borrar Obras Sociales (Hijos Directos)
                string sqlOOSS = "DELETE FROM ObrasSociales WHERE IdMandatarias = @pId";
                connection.Execute(sqlOOSS, new { pId = IdMandatarias }, transaction);

                // ==============================================================================
                // RAMA 3: EL PADRE
                // ==============================================================================

                // 3.1 Finalmente borrar la Mandataria
                string sqlMandataria = "DELETE FROM Mandatarias WHERE IdMandatarias = @pId";
                connection.Execute(sqlMandataria, new { pId = IdMandatarias }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Tip de Arquitecto: Loguea 'ex' aquí si tienes un logger configurado antes de lanzar.
                throw; 
            }
        }
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
        string query = "SELECT *, IdMandatarias AS IdMandataria FROM ObrasSociales";
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
 public static ObrasSociales TraerOSPorId(int IdOS)
{
    ObrasSociales ObjOS = null;
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
    
        string query = "SELECT *, IdMandatarias AS IdMandataria FROM ObrasSociales where IdObrasSociales = @pIdOS"; 
        ObjOS = connection.QueryFirstOrDefault<ObrasSociales>(query, new {pIdOS = IdOS});
    }

    return ObjOS;
}
 public static List<ObrasSociales> TraerOSPorIdMandataria(int IdMandataria)
{
    List<ObrasSociales> listaOS = null;
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // CORRECCIÓN: Alias aplicado para asegurar el filtro correcto
        string query = "SELECT *, IdMandatarias AS IdMandataria FROM ObrasSociales where IdMandatarias = @pIdMandataria"; 
        listaOS = connection.Query<ObrasSociales>(query, new {pIdMandataria = IdMandataria}).ToList();
    }

    return listaOS;
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


        public static void EliminarOS(int IdOS)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        // Usamos transacción para que si falla algo, no borre nada a medias.
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. Borrar PLANES DE BONIFICACIÓN (Hijos directos)
                string sqlPlanes = "DELETE FROM PlanBonificacion WHERE IdObrasSociales = @pId";
                connection.Execute(sqlPlanes, new { pId = IdOS }, transaction);

                // 2. Borrar DETALLES DE LIQUIDACIONES (Historial de presentaciones)
                // Borramos los ítems donde aparece esta Obra Social en liquidaciones
                string sqlLiqDet = "DELETE FROM LiquidacionDetalle WHERE IdObrasSociales = @pId";
                connection.Execute(sqlLiqDet, new { pId = IdOS }, transaction);

                // 3. Borrar FACTURACIÓN (Ventas)
                // Primero borramos el DETALLE de las facturas de esta Obra Social
                string sqlFacturaDet = @"
                    DELETE FROM FacturaDetalle 
                    WHERE IdFacturaCabecera IN (SELECT IdFacturaCabecera FROM FacturaCabecera WHERE IdObrasSociales = @pId)";
                connection.Execute(sqlFacturaDet, new { pId = IdOS }, transaction);

                // Luego borramos la CABECERA de las facturas
                string sqlFacturaCab = "DELETE FROM FacturaCabecera WHERE IdObrasSociales = @pId";
                connection.Execute(sqlFacturaCab, new { pId = IdOS }, transaction);

                // 4. Borrar COBROS (Pagos)
                // A) Primero: Detalles de cobros que imputan específicamente a esta OS
                string sqlCobroDet1 = "DELETE FROM CobrosDetalle WHERE IdObrasSociales = @pId";
                connection.Execute(sqlCobroDet1, new { pId = IdOS }, transaction);

                // B) Segundo: Detalles de cobros donde la CABECERA del cobro pertenece a esta OS
                // (Por si quedaron huérfanos de la consulta anterior)
                string sqlCobroDet2 = @"
                    DELETE FROM CobrosDetalle 
                    WHERE IdCobros IN (SELECT IdCobros FROM Cobros WHERE IdObrasSociales = @pId)";
                connection.Execute(sqlCobroDet2, new { pId = IdOS }, transaction);

                // C) Tercero: La Cabecera del Cobro
                string sqlCobros = "DELETE FROM Cobros WHERE IdObrasSociales = @pId";
                connection.Execute(sqlCobros, new { pId = IdOS }, transaction);

                // 5. FINALMENTE: Borrar la OBRA SOCIAL PADRE
                string sqlOS = "DELETE FROM ObrasSociales WHERE IdObrasSociales = @pId";
                connection.Execute(sqlOS, new { pId = IdOS }, transaction);

                // Si llegamos hasta acá, confirmamos la muerte de los datos
                transaction.Commit();
            }
            catch (Exception)
            {
                // Si algo explota, deshacemos todo
                transaction.Rollback();
                throw; // Re-lanzamos el error para que lo vea el Controller
            }
        }
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
// 2. INSERTAR DETALLE (CORREGIDO: Tabla en Singular)
public static void InsertarLiquidacionDetalle(int idLiquidacion, int idOS, int idPlan, int recetas, decimal bruto, decimal cargoOS, decimal bonificacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // CAMBIO: El SaldoPendiente inicial es (cargoOS - bonificacion)
        decimal saldoInicial = cargoOS - bonificacion;

        string query = @"INSERT INTO LiquidacionDetalle 
                        (IdLiquidaciones, IdObrasSociales, IdPlanBonificacion, CantidadRecetas, TotalBruto, MontoCargoOS, MontoBonificacion, SaldoPendiente, Pagado)
                        VALUES 
                        (@pIdLiq, @pIdOS, @pIdPlan, @pRecetas, @pBruto, @pCargoOS, @pBoni, @pSaldo, 0)";
        
        connection.Execute(query, new { 
            pIdLiq = idLiquidacion, 
            pIdOS = idOS, 
            pIdPlan = idPlan, 
            pRecetas = recetas, 
            pBruto = bruto, 
            pCargoOS = cargoOS, 
            pBoni = bonificacion,
            pSaldo = saldoInicial // <--- AQUÍ SE INICIA LA DEUDA REAL
        });
    }
}

// 3. BUSCAR CON FILTROS (MANDATARIA, FECHAS, ID)
public static List<dynamic> BuscarLiquidaciones(int? id, DateTime? desde, DateTime? hasta, int? mandataria)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                L.IdLiquidaciones,
                L.FechaPresentacion,
                L.Periodo,
                L.Observaciones,
                L.IdMandatarias,
                M.RazonSocial as NombreMandataria,
                ISNULL(SUM(LD.MontoCargoOS - LD.MontoBonificacion), 0) as TotalReal,
                ISNULL(SUM(LD.SaldoPendiente), 0) as SaldoPendiente,

                -- NUEVO: Buscamos la fecha del último cobro registrado para esta liquidación
                (SELECT MAX(CD.FechaCobroDetalle) 
                 FROM CobrosDetalle CD 
                 INNER JOIN LiquidacionDetalle Det ON CD.IdLiquidacionDetalle = Det.IdLiquidacionDetalle
                 WHERE Det.IdLiquidaciones = L.IdLiquidaciones
                ) as FechaCancelacion

            FROM Liquidaciones L
            INNER JOIN Mandatarias M ON L.IdMandatarias = M.IdMandatarias
            LEFT JOIN LiquidacionDetalle LD ON L.IdLiquidaciones = LD.IdLiquidaciones
            WHERE 1=1 ";

        if (id.HasValue) query += " AND L.IdLiquidaciones = @pId";
        if (desde.HasValue) query += " AND L.FechaPresentacion >= @pDesde";
        if (hasta.HasValue) query += " AND L.FechaPresentacion <= @pHasta";
        if (mandataria.HasValue && mandataria.Value > 0) query += " AND L.IdMandatarias = @pMand";

        query += @" 
            GROUP BY L.IdLiquidaciones, L.FechaPresentacion, L.Periodo, L.Observaciones, L.IdMandatarias, M.RazonSocial
            ORDER BY L.FechaPresentacion DESC";

        return connection.Query<dynamic>(query, new { pId = id, pDesde = desde, pHasta = hasta, pMand = mandataria }).ToList();
    }
}

      // EN Models/BD.cs

// 1. PARA VER: Trae TODA la lista de items de una liquidación
// EN Models/BD.cs

// A. PARA EL BOTÓN VER (Trae la lista entera)
// EN: Models/BD.cs

// CAMBIO: Devolvemos List<dynamic> para poder traer la columna extra 'FechaCancelacion'
// Vuelve a ser List<LiquidacionDetalle> (Tipado Fuerte)
public static List<LiquidacionDetalle> TraerDetallesPorIdLiquidacion(int idLiquidacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                LD.*, 
                
                -- Traemos el nombre del Plan (Dapper lo guarda en LD.NombrePlan)
                PB.NombrePlan,

                -- Traemos el nombre de la OS y le decimos 'Guárdalo en NombreObraSocial'
                OS.Nombre AS NombreObraSocial,

                -- Subconsulta para saber cuándo se pagó (si aplica)
                (SELECT MAX(FechaCobroDetalle) 
                 FROM CobrosDetalle 
                 WHERE IdLiquidacionDetalle = LD.IdLiquidacionDetalle
                ) as FechaCancelacion

            FROM LiquidacionDetalle LD
            -- LEFT JOIN: Si no tiene Plan o no encuentra la OS, trae los datos igual (no explota)
            LEFT JOIN PlanBonificacion PB ON LD.IdPlanBonificacion = PB.IdPlanBonificacion
            LEFT JOIN ObrasSociales OS ON LD.IdObrasSociales = OS.IdObrasSociales
            
            WHERE LD.IdLiquidaciones = @pIdLiq";
            
        // Mapeo directo y limpio
        return connection.Query<LiquidacionDetalle>(query, new { pIdLiq = idLiquidacion }).ToList();
    }
}
// B. PARA EL BOTÓN ELIMINAR (Borra todo en orden)
public static void EliminarLiquidacion(int idLiquidacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var pId = new { Id = idLiquidacion };

                // ---------------------------------------------------------
                // PASO 1: Identificar qué Cobros se verán afectados
                // ---------------------------------------------------------
                // CORRECCIÓN: Hacemos JOIN porque CobrosDetalle no tiene IdLiquidaciones directo.
                // Buscamos los cobros que apuntan a los HIJOS (Detalles) de esta liquidación.
                string queryGetCobros = @"
                    SELECT DISTINCT CD.IdCobros 
                    FROM CobrosDetalle CD
                    INNER JOIN LiquidacionDetalle LD ON CD.IdLiquidacionDetalle = LD.IdLiquidacionDetalle
                    WHERE LD.IdLiquidaciones = @Id";
                
                var listaCobrosAfectados = connection.Query<int>(queryGetCobros, pId, transaction).ToList();

                // ---------------------------------------------------------
                // PASO 2: Eliminar el vínculo en CobrosDetalle
                // ---------------------------------------------------------
                // CORRECCIÓN: Borramos usando una subquery que busca los IDs de los detalles hijos.
                string deleteCobrosDet = @"
                    DELETE FROM CobrosDetalle 
                    WHERE IdLiquidacionDetalle IN (
                        SELECT IdLiquidacionDetalle 
                        FROM LiquidacionDetalle 
                        WHERE IdLiquidaciones = @Id
                    )";
                connection.Execute(deleteCobrosDet, pId, transaction);

                // ---------------------------------------------------------
                // PASO 3: Recalcular y Actualizar las Cabeceras de Cobros
                // ---------------------------------------------------------
                foreach (var idCobro in listaCobrosAfectados)
                {
                    // A. Calculamos el nuevo total sumando los detalles restantes.
                    string queryRecalculo = @"
                        SELECT ISNULL(SUM(ImporteCobrado), 0)  -- Ojo: ImporteCobrado (según tu modelo)
                        FROM CobrosDetalle 
                        WHERE IdCobros = @IdCobro";
                    
                    decimal nuevoTotal = connection.ExecuteScalar<decimal>(queryRecalculo, new { IdCobro = idCobro }, transaction);

                    // B. Actualizamos la cabecera
                    // NOTA: Asegúrate que la tabla 'Cobros' tenga columna 'Total' o la que uses para el monto.
                    // Si no tienes columna Total en Cobros y se calcula al vuelo, este paso sobra, 
                    // pero asumo que guardas el histórico.
                    /* Si tu tabla Cobros no tiene columna 'Total', comenta estas 2 lineas. 
                       Si la tiene, déjalas.
                    */
                    // string updateCabecera = "UPDATE Cobros SET Total = @Total WHERE IdCobros = @IdCobro";
                    // connection.Execute(updateCabecera, new { Total = nuevoTotal, IdCobro = idCobro }, transaction);
                }

                // ---------------------------------------------------------
                // PASO 4: Eliminar LiquidacionDetalle (Hijos propios)
                // ---------------------------------------------------------
                string deleteLiqDet = "DELETE FROM LiquidacionDetalle WHERE IdLiquidaciones = @Id";
                connection.Execute(deleteLiqDet, pId, transaction);

                // ---------------------------------------------------------
                // PASO 5: Eliminar Liquidaciones (Padre propio)
                // ---------------------------------------------------------
                string deleteLiqCab = "DELETE FROM Liquidaciones WHERE IdLiquidaciones = @Id";
                connection.Execute(deleteLiqCab, pId, transaction);

                transaction.Commit();
            }
            catch (Exception ex) // Agregamos ex para ver el error si pasa algo más
            {
                transaction.Rollback();
                throw; 
            }
        }
    }
}

// 1. Traer un solo ítem para editarlo
public static LiquidacionDetalle TraerDetallePorId(int idDetalle)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = "SELECT * FROM LiquidacionDetalle WHERE IdLiquidacionDetalle = @pId";
        return connection.QueryFirstOrDefault<LiquidacionDetalle>(query, new { pId = idDetalle });
    }
}

// 2. Agregar ítem suelto (y actualizar total del padre)
// EN: Models/BD.cs -> Método AgregarItemIndividual

public static void AgregarItemIndividual(LiquidacionDetalle item)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        
        // A. Insertar el ítem (AGREGAMOS SaldoPendiente y Pagado)
        string query = @"
            INSERT INTO LiquidacionDetalle (
                IdLiquidaciones, IdObrasSociales, IdPlanBonificacion, 
                CantidadRecetas, TotalBruto, MontoCargoOS, MontoBonificacion,
                SaldoPendiente, Pagado  -- <--- COLUMNAS NUEVAS
            )
            VALUES (
                @IdLiquidaciones, @IdObrasSociales, @IdPlanBonificacion, 
                @CantidadRecetas, @TotalBruto, @MontoCargoOS, @MontoBonificacion,
                @SaldoPendiente, 0      -- <--- VALORES NUEVOS (0 es false para Pagado)
            )";
            
        connection.Execute(query, item);

        // B. Actualizar el Total de la Liquidación Padre
        ActualizarTotalCabecera(item.IdLiquidaciones, connection);
    }
}

// 3. Modificar ítem suelto
public static void ModificarItemIndividual(LiquidacionDetalle item)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();

        // PASO 1: Actualizamos los datos y RECALCULAMOS EL SALDO en el mismo acto.
        // La fórmula es: (Nuevo Cargo - Nueva Bonif) - (Todo lo que ya me pagaron en CobrosDetalle)
        string query = @"
            UPDATE LiquidacionDetalle 
            SET IdObrasSociales = @IdObrasSociales, 
                IdPlanBonificacion = @IdPlanBonificacion, 
                CantidadRecetas = @CantidadRecetas, 
                TotalBruto = @TotalBruto, 
                MontoCargoOS = @MontoCargoOS, 
                MontoBonificacion = @MontoBonificacion,
                
                -- MAGIA AQUÍ: Restamos los pagos existentes al nuevo total
                SaldoPendiente = (@MontoCargoOS - @MontoBonificacion) - ISNULL((
                    SELECT SUM(ImporteCobrado + MontoDebito) 
                    FROM CobrosDetalle 
                    WHERE IdLiquidacionDetalle = @IdLiquidacionDetalle
                ), 0)

            WHERE IdLiquidacionDetalle = @IdLiquidacionDetalle;

            -- PASO 2 (CRÍTICO): Actualizar el estado 'Pagado'
            -- Si bajaste el precio y el saldo quedó en 0 (o negativo), el sistema debe marcarlo como PAGADO.
            UPDATE LiquidacionDetalle
            SET Pagado = CASE WHEN SaldoPendiente <= 1.00 THEN 1 ELSE 0 END
            WHERE IdLiquidacionDetalle = @IdLiquidacionDetalle;
        ";
        
        connection.Execute(query, item);
        
        // PASO 3: Actualizar el Total de la Liquidación Padre (Cabecera)
        ActualizarTotalCabecera(item.IdLiquidaciones, connection);
    }
}

public static void EliminarItemIndividual(int idDetalle, int idLiquidacionPadre)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // ---------------------------------------------------------
                // PASO 1: Identificar Cobros Afectados (Antes de tocar nada)
                // ---------------------------------------------------------
                // Buscamos si esta Liquidación (el Padre) está metida en algún Cobro.
                string queryGetCobros = @"
                    SELECT DISTINCT IdCobros 
                    FROM CobrosDetalle 
                    WHERE IdLiquidaciones = @IdPadre";
                
                var listaCobrosAfectados = connection.Query<int>(queryGetCobros, new { IdPadre = idLiquidacionPadre }, transaction).ToList();

                // ---------------------------------------------------------
                // PASO 2: Eliminar el Ítem (Hijo)
                // ---------------------------------------------------------
                string deleteItem = "DELETE FROM LiquidacionDetalle WHERE IdLiquidacionDetalle = @IdDetalle";
                connection.Execute(deleteItem, new { IdDetalle = idDetalle }, transaction);

                // ---------------------------------------------------------
                // PASO 3: Recalcular Total de la Liquidación (Padre)
                // ---------------------------------------------------------
                // Sumamos lo que quedó vivo. Si no quedó nada, es 0.
                string querySumLiq = "SELECT ISNULL(SUM(Importe), 0) FROM LiquidacionDetalle WHERE IdLiquidaciones = @IdPadre";
                decimal nuevoTotalLiquidacion = connection.ExecuteScalar<decimal>(querySumLiq, new { IdPadre = idLiquidacionPadre }, transaction);

                // Actualizamos la cabecera de la Liquidación
                string updateLiq = "UPDATE Liquidaciones SET Total = @Total WHERE IdLiquidaciones = @IdPadre";
                connection.Execute(updateLiq, new { Total = nuevoTotalLiquidacion, IdPadre = idLiquidacionPadre }, transaction);

                // ---------------------------------------------------------
                // PASO 4: Propagar el cambio a los Cobros (El Abuelo)
                // ---------------------------------------------------------
                if (listaCobrosAfectados.Any())
                {
                    // A. Actualizar el renglón específico en CobrosDetalle
                    // El importe en el detalle del cobro debe coincidir con el nuevo total de la liquidación
                    string updateCobrosDetalle = @"
                        UPDATE CobrosDetalle 
                        SET Importe = @NuevoImporte 
                        WHERE IdLiquidaciones = @IdPadre";
                    
                    connection.Execute(updateCobrosDetalle, new { NuevoImporte = nuevoTotalLiquidacion, IdPadre = idLiquidacionPadre }, transaction);

                    // B. Recalcular las Cabeceras de los Cobros afectados
                    foreach (var idCobro in listaCobrosAfectados)
                    {
                        // Sumar todos los detalles de ese cobro
                        string querySumCobro = "SELECT ISNULL(SUM(Importe), 0) FROM CobrosDetalle WHERE IdCobros = @IdCobro";
                        decimal nuevoTotalCobro = connection.ExecuteScalar<decimal>(querySumCobro, new { IdCobro = idCobro }, transaction);

                        // Actualizar cabecera Cobros
                        string updateCobroCab = "UPDATE Cobros SET Total = @Total WHERE IdCobros = @IdCobro";
                        connection.Execute(updateCobroCab, new { Total = nuevoTotalCobro, IdCobro = idCobro }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

// MÉTODO PRIVADO PARA RECALCULAR TOTALES (Evita inconsistencias)
private static void ActualizarTotalCabecera(int idLiq, SqlConnection conn)
{
    string sqlUpdate = @"
        UPDATE Liquidaciones 
        SET TotalPresentado = (SELECT COALESCE(SUM(TotalBruto), 0) FROM LiquidacionDetalle WHERE IdLiquidaciones = @pId)
        WHERE IdLiquidaciones = @pId";
    conn.Execute(sqlUpdate, new { pId = idLiq });
}

public static Liquidaciones TraerLiquidacionPorId(int id)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = "SELECT * FROM Liquidaciones WHERE IdLiquidaciones = @pId";
        return connection.QueryFirstOrDefault<Liquidaciones>(query, new { pId = id });
    }
}

// 2. Guardar cambios de la cabecera (Fecha, Mandataria, Obs)
public static void ModificarLiquidacionCabecera(int id, int idMandataria, DateTime fecha, string obs)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            UPDATE Liquidaciones 
            SET IdMandatarias = @pMand, FechaPresentacion = @pFecha, Observaciones = @pObs, Periodo = @pPeriodo
            WHERE IdLiquidaciones = @pId";
            
        connection.Execute(query, new { 
            pId = id, 
            pMand = idMandataria, 
            pFecha = fecha, 
            pObs = obs,
            pPeriodo = fecha.ToString("MM-yyyy")
        });
    }
}
// =============================================================================
// BÚSQUEDA GLOBAL DE DETALLES (Para la vista "Por Obra Social")
// =============================================================================
// EN Models/BD.cs

public static List<dynamic> BuscarDetallesGlobales(DateTime? desde, DateTime? hasta, int? idMandataria, int? idOS)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                LD.IdLiquidacionDetalle,
                LD.IdLiquidaciones,
                L.FechaPresentacion,
                L.Periodo,
                M.RazonSocial as NombreMandataria,
                OS.Nombre as NombreObraSocial,
                PB.NombrePlan,
                LD.CantidadRecetas,
                LD.TotalBruto as BrutoOriginal,
                LD.MontoCargoOS,
                LD.MontoBonificacion,
                (LD.MontoCargoOS - LD.MontoBonificacion) as TotalBruto,
                LD.SaldoPendiente,
                LD.Pagado,

                -- NUEVO: Fecha del último pago de ESTE ítem
                (SELECT MAX(FechaCobroDetalle) 
                 FROM CobrosDetalle 
                 WHERE IdLiquidacionDetalle = LD.IdLiquidacionDetalle
                ) as FechaCancelacion

            FROM LiquidacionDetalle LD
            INNER JOIN Liquidaciones L ON LD.IdLiquidaciones = L.IdLiquidaciones
            INNER JOIN Mandatarias M ON L.IdMandatarias = M.IdMandatarias
            INNER JOIN ObrasSociales OS ON LD.IdObrasSociales = OS.IdObrasSociales
            LEFT JOIN PlanBonificacion PB ON LD.IdPlanBonificacion = PB.IdPlanBonificacion
            WHERE 1=1 ";

        if (desde.HasValue) query += " AND L.FechaPresentacion >= @pDesde";
        if (hasta.HasValue) query += " AND L.FechaPresentacion <= @pHasta";
        if (idMandataria.HasValue && idMandataria.Value > 0) query += " AND M.IdMandatarias = @pMand";
        if (idOS.HasValue && idOS.Value > 0) query += " AND OS.IdObrasSociales = @pOS";

        query += " ORDER BY L.FechaPresentacion DESC, M.RazonSocial, OS.Nombre";

        return connection.Query<dynamic>(query, new { 
            pDesde = desde, pHasta = hasta, pMand = idMandataria, pOS = idOS 
        }).ToList();
    }
}


public static Usuario TraerUsuarioPorId(int idUsuario)
    {
     Usuario ObjUsuario = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuario where IdUsuario = @pIdUsuario"; 
            ObjUsuario = connection.QueryFirstOrDefault<Usuario>(query, new {pIdUsuario = idUsuario});
         }
    
         return ObjUsuario;
    }
    


   public static void ModificarUsuario(int IdUsuario, string? Contraseña, string? RazonSocial, string? Domicilio, long? Cuit, string? Iva)
    {
     Usuario ObjUsuario = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE Usuario SET Contraseña = @pContraseña, RazonSocial = @pRazonSocial, Domicilio = @pDomicilio, Cuit = @pCuit, Iva = @pIva where IdUsuario = @pIdUsuario"; 
            connection.Execute(query, new {pIdUsuario = IdUsuario, pContraseña = Contraseña, pRazonSocial = RazonSocial, pDomicilio = Domicilio, pCuit = Cuit, pIva = Iva});
         }
    
    }

  // =============================================================================
// SECCIÓN COBROS (FALTABA TODO ESTO)
// =============================================================================

// =============================================================================
    // TRAER DEUDAS (Con lógica de Fallback para datos viejos)
    // =============================================================================
   public static List<dynamic> TraerDeudasPendientesPorOS(int idObraSocial)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                LD.IdLiquidacionDetalle,
                L.Periodo,
                L.FechaPresentacion,
                LD.TotalBruto as ImporteOriginal,
                
                -- CORRECCIÓN: Si es 0, respetamos el 0. Solo si es NULL asumimos deuda total.
                ISNULL(LD.SaldoPendiente, LD.TotalBruto) as SaldoPendiente

            FROM LiquidacionDetalle LD
            INNER JOIN Liquidaciones L ON LD.IdLiquidaciones = L.IdLiquidaciones
            WHERE LD.IdObrasSociales = @pIdOS 
              -- IMPORTANTE: Filtramos lo que ya está pagado para que no aparezca en la lista
              AND (LD.Pagado = 0 OR LD.Pagado IS NULL) 
              -- Y por seguridad, que el saldo sea mayor a 0.05 (tolerancia de centavos)
              AND ISNULL(LD.SaldoPendiente, LD.TotalBruto) > 0.05
            ORDER BY L.FechaPresentacion ASC";

        return connection.Query<dynamic>(query, new { pIdOS = idObraSocial }).ToList();
    }
}
// 2. Traer Liquidaciones donde participa esa Obra Social (Para imputar pagos)
public static List<Liquidaciones> TraerLiquidacionesPendientesPorOS(int idObraSocial)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Buscamos liquidaciones que tengan detalles de esa OS
        string query = @"
            SELECT DISTINCT L.IdLiquidaciones, L.FechaPresentacion, L.TotalPresentado, L.Periodo
            FROM Liquidaciones L
            INNER JOIN LiquidacionDetalle LD ON L.IdLiquidaciones = LD.IdLiquidaciones
            WHERE LD.IdObrasSociales = @pIdOS
            ORDER BY L.FechaPresentacion DESC";
            
        return connection.Query<Liquidaciones>(query, new { pIdOS = idObraSocial }).ToList();
    }
}

// 3. Buscar Cobros (El motor del buscador principal)
// EN Models/BD.cs

// CAMBIO: Devolvemos 'dynamic' para poder incluir columnas extra (NombreMandataria)
public static List<dynamic> BuscarCobros(DateTime? desde, DateTime? hasta, int? idMandataria, int? idObraSocial)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                C.IdCobros,
                C.NumeroComprobante,
                CAST(C.FechaCobro AS DATE) as FechaCobro,
                M.RazonSocial as NombreMandataria,
                M.IdMandatarias,
                COUNT(CD.IdCobrosDetalle) as CantidadItems,
                COALESCE(SUM(CD.ImporteCobrado), 0) as TotalImporte,
                COALESCE(SUM(CD.MontoDebito), 0) as TotalDebitos
            FROM Cobros C
            INNER JOIN Mandatarias M ON C.IdMandatarias = M.IdMandatarias
            LEFT JOIN CobrosDetalle CD ON C.IdCobros = CD.IdCobros
            WHERE 1=1 ";

        if (desde.HasValue) query += " AND C.FechaCobro >= @pDesde";
        if (hasta.HasValue) query += " AND C.FechaCobro <= @pHasta";
        if (idMandataria.HasValue && idMandataria.Value > 0) query += " AND M.IdMandatarias = @pIdMand";
        
        // Filtro especial: Si busca por OS, mostramos los lotes que contengan esa OS
        if (idObraSocial.HasValue && idObraSocial.Value > 0) 
        {
            query += " AND EXISTS (SELECT 1 FROM CobrosDetalle sub WHERE sub.IdCobros = C.IdCobros AND sub.IdObrasSociales = @pIdOS)";
        }

        query += @" 
            GROUP BY C.IdCobros, C.NumeroComprobante, CAST(C.FechaCobro AS DATE), M.RazonSocial, M.IdMandatarias
            ORDER BY C.FechaCobro DESC";

        return connection.Query<dynamic>(query, new { pDesde = desde, pHasta = hasta, pIdMand = idMandataria, pIdOS = idObraSocial }).ToList();
    }
}
// -----------------------------------------------------------------------------------
// 2. GUARDAR CABECERA (PADRE) - Retorna el ID generado
// -----------------------------------------------------------------------------------
public static int AgregarCobroCabecera(int idMandataria, DateTime fecha, string comprobante, int? idLiquidacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Sin TipoPago, como pediste.
        string query = @"
            INSERT INTO Cobros (IdMandatarias, FechaCobro, NumeroComprobante, IdLiquidaciones)
            OUTPUT INSERTED.IdCobros
            VALUES (@pMand, @pFecha, @pComp, @pLiq)";

        return connection.QuerySingle<int>(query, new {
            pMand = idMandataria,
            pFecha = fecha,
            pComp = comprobante,
            pLiq = idLiquidacion
        });
    }
}
public static void EliminarLoteCompleto(int idCobroPadre)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. Antes de borrar, restauramos la deuda de TODOS los hijos que tengan imputación
                // Usamos una query masiva para no ir uno por uno
                string sqlRestaurarMasivo = @"
                    UPDATE L
                    SET L.SaldoPendiente = L.SaldoPendiente + C.ImporteCobrado,
                        L.Pagado = 0
                    FROM LiquidacionDetalle L
                    INNER JOIN CobrosDetalle C ON L.IdLiquidacionDetalle = C.IdLiquidacionDetalle
                    WHERE C.IdCobros = @pIdPadre";

                connection.Execute(sqlRestaurarMasivo, new { pIdPadre = idCobroPadre }, transaction);

                // 2. Ahora sí, borramos todos los detalles
                connection.Execute("DELETE FROM CobrosDetalle WHERE IdCobros = @pId", new { pId = idCobroPadre }, transaction);

                // 3. Y finalmente el padre
                connection.Execute("DELETE FROM Cobros WHERE IdCobros = @pId", new { pId = idCobroPadre }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

public static void ModificarCobroCabecera(int idCobro, int idMandataria, DateTime fecha, string comprobante)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            UPDATE Cobros 
            SET IdMandatarias = @pMand, 
                FechaCobro = @pFecha, 
                NumeroComprobante = @pComp
            WHERE IdCobros = @pId";

        connection.Execute(query, new { 
            pId = idCobro, 
            pMand = idMandataria, 
            pFecha = fecha, 
            pComp = comprobante 
        });
    }
}
// -----------------------------------------------------------------------------------
// 3. AGREGAR DETALLE (HIJO)
// -----------------------------------------------------------------------------------
public static void AgregarCobroDetalle(int idCobroPadre, int idObraSocial, DateTime fecha, decimal importe, string tipo, decimal debito, string motivo, int? idLiquidacionDetalle = null)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. Insertamos el Cobro 
                string sqlInsert = @"INSERT INTO CobrosDetalle 
                                   (IdCobros, IdObrasSociales, FechaCobroDetalle, ImporteCobrado, TipoPago, MontoDebito, MotivoDebito, IdLiquidacionDetalle)
                                   VALUES 
                                   (@pIdPadre, @pOS, @pFecha, @pImporte, @pTipo, @pDebito, @pMotivo, @pIdLiqDet)";
                
                connection.Execute(sqlInsert, new { 
                    pIdPadre = idCobroPadre, 
                    pOS = idObraSocial, 
                    pFecha = fecha, 
                    pImporte = importe, 
                    pTipo = tipo, 
                    pDebito = debito, 
                    pMotivo = motivo,
                    pIdLiqDet = idLiquidacionDetalle 
                }, transaction);

                // 2. LOGICA DE IMPUTACIÓN CORREGIDA
                if (idLiquidacionDetalle.HasValue && idLiquidacionDetalle.Value > 0)
                {
                    // CAMBIO CRÍTICO: Restamos (Importe + Debito). 
                    // El débito también reduce la deuda pendiente porque es un monto que ya no vamos a cobrar.
                    string sqlUpdateLiq = @"
                        UPDATE LiquidacionDetalle 
                        SET SaldoPendiente = SaldoPendiente - (@pPago + @pDeb)
                        WHERE IdLiquidacionDetalle = @pIdLiqDet";
                    
                    connection.Execute(sqlUpdateLiq, new { 
                        pPago = importe, 
                        pDeb = debito, // <--- Pasamos el débito para restarlo también
                        pIdLiqDet = idLiquidacionDetalle 
                    }, transaction);

                    // Verificamos si se saldó (tolerancia 0.05)
                    string sqlCheckPagado = @"
                        UPDATE LiquidacionDetalle 
                        SET Pagado = 1 
                        WHERE IdLiquidacionDetalle = @pIdLiqDet AND SaldoPendiente <= 0.05";
                    
                    connection.Execute(sqlCheckPagado, new { pIdLiqDet = idLiquidacionDetalle }, transaction);
                }

                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }
    }
}

// -----------------------------------------------------------------------------------
// 4. MODIFICAR DETALLE (Solo editamos el hijo, el padre raramente se toca desde aquí)
// -----------------------------------------------------------------------------------
public static void ModificarCobroDetalle(int idCobroDetalle, int idObraSocial, DateTime fecha, decimal importe, string tipo, decimal debitos, string motivo)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. OBTENER ESTADO ANTERIOR
                var anterior = connection.QueryFirstOrDefault<dynamic>(
                    "SELECT ImporteCobrado, MontoDebito, IdLiquidacionDetalle FROM CobrosDetalle WHERE IdCobrosDetalle = @id",
                    new { id = idCobroDetalle },
                    transaction
                );

                // 2. REVERTIR IMPACTO EN LA DEUDA (Devolver plata a la deuda original)
                if (anterior != null && anterior.IdLiquidacionDetalle != null)
                {
                    string sqlRevertir = @"
                        UPDATE LiquidacionDetalle 
                        SET SaldoPendiente = SaldoPendiente + (@imp + @deb), 
                            Pagado = 0 
                        WHERE IdLiquidacionDetalle = @idLiq";
                    connection.Execute(sqlRevertir, new { 
                        imp = anterior.ImporteCobrado, 
                        deb = anterior.MontoDebito, 
                        idLiq = anterior.IdLiquidacionDetalle 
                    }, transaction);
                }

                // 3. ACTUALIZAR EL COBRO
                string sqlUpdate = @"UPDATE CobrosDetalle SET 
                        IdObrasSociales = @idOS, 
                        FechaCobroDetalle = @fecha, 
                        ImporteCobrado = @imp, 
                        TipoPago = @tipo, 
                        MontoDebito = @deb, 
                        MotivoDebito = @mot 
                       WHERE IdCobrosDetalle = @id";
                       
                connection.Execute(sqlUpdate, new { 
                    id = idCobroDetalle, 
                    idOS = idObraSocial, 
                    fecha = fecha, 
                    imp = importe, 
                    tipo = tipo, 
                    deb = debitos, 
                    mot = motivo ?? "" 
                }, transaction);

                // 4. APLICAR NUEVO IMPACTO A LA DEUDA
                if (anterior != null && anterior.IdLiquidacionDetalle != null)
                {
                    string sqlAplicar = @"
                        UPDATE LiquidacionDetalle 
                        SET SaldoPendiente = SaldoPendiente - (@imp + @deb)
                        WHERE IdLiquidacionDetalle = @idLiq";

                    connection.Execute(sqlAplicar, new { 
                        imp = importe, 
                        deb = debitos, 
                        idLiq = anterior.IdLiquidacionDetalle 
                    }, transaction);

                    // Chequeo de Pagado (Tolerancia $1)
                    string sqlCheck = @"
                        UPDATE LiquidacionDetalle 
                        SET Pagado = 1 
                        WHERE IdLiquidacionDetalle = @idLiq AND SaldoPendiente <= 1.00";
                    connection.Execute(sqlCheck, new { idLiq = anterior.IdLiquidacionDetalle }, transaction);
                }

                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }
    }
}
// -----------------------------------------------------------------------------------
// 5. ELIMINAR DETALLE (Y limpiar Padre si queda vacío)
// -----------------------------------------------------------------------------------
public static void EliminarCobroDetalle(int idCobroDetalle)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. Obtenemos datos del cobro INCLUYENDO EL DÉBITO
                var info = connection.QueryFirstOrDefault<dynamic>(
                    "SELECT ImporteCobrado, MontoDebito, IdLiquidacionDetalle FROM CobrosDetalle WHERE IdCobrosDetalle = @id", 
                    new { id = idCobroDetalle }, 
                    transaction
                );

                if (info != null && info.IdLiquidacionDetalle != null)
                {
                    // 2. RESTAURAMOS LA DEUDA EN LA LIQUIDACIÓN
                    // Sumamos (Importe + Debito) al SaldoPendiente
                    string sqlRestaurar = @"UPDATE LiquidacionDetalle 
                                            SET SaldoPendiente = SaldoPendiente + (@imp + @deb), 
                                                Pagado = 0 
                                            WHERE IdLiquidacionDetalle = @idLiq";
                    
                    connection.Execute(sqlRestaurar, new { 
                        imp = info.ImporteCobrado, 
                        deb = info.MontoDebito, // <--- Restauramos el débito también
                        idLiq = info.IdLiquidacionDetalle 
                    }, transaction);
                }

                // 3. Borramos el cobro físico
                connection.Execute("DELETE FROM CobrosDetalle WHERE IdCobrosDetalle = @id", new { id = idCobroDetalle }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

// -----------------------------------------------------------------------------------
// 6. TRAER HIJOS DE UN LOTE (Por Comprobante) - Para el Modal VER
// -----------------------------------------------------------------------------------
public static List<dynamic> TraerCobrosDelMismoLote(int idCobroPadre)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                CD.*, 
                OS.Nombre as NombreObraSocial
            FROM CobrosDetalle CD
            INNER JOIN ObrasSociales OS ON CD.IdObrasSociales = OS.IdObrasSociales
            WHERE CD.IdCobros = @pId
            ORDER BY CD.IdCobrosDetalle DESC";
        return connection.Query<dynamic>(query, new { pId = idCobroPadre }).ToList();
    }
}
// 7. Traer un Detalle por ID (Para editar)
public static dynamic TraerCobroDetallePorId(int idCobroDetalle)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Retornamos dynamic para mapear fácil al JSON
        string query = "SELECT * FROM CobrosDetalle WHERE IdCobrosDetalle = @pId";
        return connection.QueryFirstOrDefault<dynamic>(query, new { pId = idCobroDetalle });
    }
}

// 8. Buscar si existe un Padre (Para agregar un pago a un lote existente)
public static int? BuscarIdPadre(string comprobante, int idMandataria)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Asumimos que Comprobante + Mandataria definen un único Lote
        string query = "SELECT IdCobros FROM Cobros WHERE NumeroComprobante = @pComp AND IdMandatarias = @pMand";
        return connection.QueryFirstOrDefault<int?>(query, new { pComp = comprobante, pMand = idMandataria });
    }
}


// TRAER LOS COBROS ASOCIADOS A UN ÍTEM DE LIQUIDACIÓN
public static List<dynamic> ObtenerCobrosPorLiquidacionDetalle(int idLiquidacionDetalle)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            SELECT 
                C.FechaCobro,
                C.NumeroComprobante,
                CD.ImporteCobrado,
                CD.MontoDebito,
                CD.MotivoDebito
            FROM CobrosDetalle CD
            INNER JOIN Cobros C ON CD.IdCobros = C.IdCobros
            WHERE CD.IdLiquidacionDetalle = @pId
            ORDER BY C.FechaCobro DESC";

        return connection.Query<dynamic>(query, new { pId = idLiquidacionDetalle }).ToList();
    }
}


// Modifica o inserta un cobro detalle dependiendo si viene con ID
public static void GuardarCobroDetalle(int idCobroDetalle, int idCobroPadre, int idOS, DateTime fecha, string tipo, decimal importe, decimal debito, string motivo, int? idLiqDetalle)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query;
        if (idCobroDetalle > 0)
        {
            // UPDATE (Editar existente)
            query = @"UPDATE CobrosDetalle SET 
                        IdObrasSociales = @pOS,
                        FechaCobroDetalle = @pFecha,
                        TipoPago = @pTipo,
                        ImporteCobrado = @pImp,
                        MontoDebito = @pDeb,
                        MotivoDebito = @pMot
                      WHERE IdCobrosDetalle = @pId";
        }
        else
        {
            // INSERT (Nuevo)
            query = @"INSERT INTO CobrosDetalle (IdCobros, IdObrasSociales, FechaCobroDetalle, TipoPago, ImporteCobrado, MontoDebito, MotivoDebito, IdLiquidacionDetalle)
                      VALUES (@pPadre, @pOS, @pFecha, @pTipo, @pImp, @pDeb, @pMot, @pLiq)";
        }

        connection.Execute(query, new { 
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