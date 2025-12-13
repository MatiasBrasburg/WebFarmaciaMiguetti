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
 public static List<ObrasSociales> TraerOSPorIdMandataria  (int IdMandataria)
    {
     List<ObrasSociales> listaOS = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM ObrasSociales where IdMandatarias = @pIdMandataria"; 
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
// 2. INSERTAR DETALLE (CORREGIDO: Tabla en Singular)
public static void InsertarLiquidacionDetalle(int idLiquidacion, int idOS, int idPlan, int recetas, decimal bruto, decimal cargoOS, decimal bonificacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // CORRECCIÓN FINAL: La columna en BD se llama IdLiquidaciones (Plural)
        string query = @"
            INSERT INTO LiquidacionDetalle (IdLiquidaciones, IdObrasSociales, IdPlanBonificacion, CantidadRecetas, TotalBruto, MontoCargoOS, MontoBonificacion)
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


      // EN Models/BD.cs

// 1. PARA VER: Trae TODA la lista de items de una liquidación
// EN Models/BD.cs

// A. PARA EL BOTÓN VER (Trae la lista entera)
public static List<LiquidacionDetalle> TraerDetallesPorIdLiquidacion(int idLiquidacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Traemos todos los detalles que pertenezcan a esa Liquidacion (FK)
        string query = "SELECT * FROM LiquidacionDetalle WHERE IdLiquidaciones = @pIdLiq";
        return connection.Query<LiquidacionDetalle>(query, new { pIdLiq = idLiquidacion }).ToList();
    }
}

// B. PARA EL BOTÓN ELIMINAR (Borra todo en orden)
public static void EliminarLiquidacion(int idLiquidacion)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        // Usamos transacción para asegurar que no quede basura si algo falla
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 1. Borrar Hijos (Detalles)
                string queryDetalles = "DELETE FROM LiquidacionDetalle WHERE IdLiquidaciones = @pId";
                connection.Execute(queryDetalles, new { pId = idLiquidacion }, transaction);

                // 2. Borrar Padre (Cabecera)
                string queryCabecera = "DELETE FROM Liquidaciones WHERE IdLiquidaciones = @pId";
                connection.Execute(queryCabecera, new { pId = idLiquidacion }, transaction);

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
public static void AgregarItemIndividual(LiquidacionDetalle item)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        
        // A. Insertar el ítem
        string query = @"
            INSERT INTO LiquidacionDetalle (IdLiquidaciones, IdObrasSociales, IdPlanBonificacion, CantidadRecetas, TotalBruto, MontoCargoOS, MontoBonificacion)
            VALUES (@IdLiquidaciones, @IdObrasSociales, @IdPlanBonificacion, @CantidadRecetas, @TotalBruto, @MontoCargoOS, @MontoBonificacion)";
        connection.Execute(query, item);

        // B. Actualizar el Total de la Liquidación Padre (Magia automática)
        ActualizarTotalCabecera(item.IdLiquidaciones, connection);
    }
}

// 3. Modificar ítem suelto
public static void ModificarItemIndividual(LiquidacionDetalle item)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();

        string query = @"
            UPDATE LiquidacionDetalle 
            SET IdObrasSociales = @IdObrasSociales, 
                IdPlanBonificacion = @IdPlanBonificacion, 
                CantidadRecetas = @CantidadRecetas, 
                TotalBruto = @TotalBruto, 
                MontoCargoOS = @MontoCargoOS, 
                MontoBonificacion = @MontoBonificacion
            WHERE IdLiquidacionDetalle = @IdLiquidacionDetalle";
        
        connection.Execute(query, item);
        
        // Actualizar Total Padre
        ActualizarTotalCabecera(item.IdLiquidaciones, connection);
    }
}

// 4. Eliminar ítem suelto
public static void EliminarItemIndividual(int idDetalle, int idLiquidacionPadre)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        
        string query = "DELETE FROM LiquidacionDetalle WHERE IdLiquidacionDetalle = @pId";
        connection.Execute(query, new { pId = idDetalle });

        // Actualizar Total Padre
        ActualizarTotalCabecera(idLiquidacionPadre, connection);
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
public static List<Cobros> BuscarCobros(string numeroComprobante, DateTime? desde, DateTime? hasta, int? idObraSocial, int? idMandataria)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Hacemos JOIN con Obras Sociales para poder buscar por Mandataria también
        string query = @"
            SELECT C.*, OS.Nombre as NombreObraSocial 
            FROM Cobros C
            INNER JOIN ObrasSociales OS ON C.IdObrasSociales = OS.IdObrasSociales
            WHERE 1=1 ";

        if (!string.IsNullOrEmpty(numeroComprobante))
            query += " AND C.NumeroComprobante LIKE @pComp";

        if (desde.HasValue)
            query += " AND C.FechaCobro >= @pDesde";

        if (hasta.HasValue)
            query += " AND C.FechaCobro <= @pHasta";

        if (idObraSocial.HasValue && idObraSocial.Value > 0)
            query += " AND C.IdObrasSociales = @pIdOS";

        if (idMandataria.HasValue && idMandataria.Value > 0)
            query += " AND OS.IdMandatarias = @pIdMand"; // Filtro por el padre

        query += " ORDER BY C.FechaCobro DESC";

        return connection.Query<Cobros>(query, new { 
            pComp = $"%{numeroComprobante}%", 
            pDesde = desde, 
            pHasta = hasta, 
            pIdOS = idObraSocial,
            pIdMand = idMandataria
        }).ToList();
    }
}

// 4. Agregar Cobro
public static void AgregarCobro(int? idLiquidacion, int? idObraSocial, DateTime? fecha, decimal importe, string comprobante, string tipo, decimal debitos, string motivoDebito)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            INSERT INTO Cobros (IdLiquidaciones, IdObrasSociales, FechaCobro, ImporteCobrado, NumeroComprobante, TipoPago, MontoDebitos, MotivoDebito)
            VALUES (@pLiq, @pOS, @pFecha, @pImporte, @pComp, @pTipo, @pDebitos, @pMotivo)";

        connection.Execute(query, new {
            pLiq = (idLiquidacion == 0 ? null : idLiquidacion), // Si es 0 guardamos NULL
            pOS = idObraSocial,
            pFecha = fecha,
            pImporte = importe,
            pComp = comprobante,
            pTipo = tipo,
            pDebitos = debitos,
            pMotivo = motivoDebito
        });
    }
}

// 5. Modificar Cobro
public static void ModificarCobro(int idCobro, int? idLiquidacion, int? idObraSocial, DateTime? fecha, decimal importe, string comprobante, string tipo, decimal debitos, string motivoDebito)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = @"
            UPDATE Cobros 
            SET IdLiquidaciones = @pLiq, 
                IdObrasSociales = @pOS, 
                FechaCobro = @pFecha, 
                ImporteCobrado = @pImporte, 
                NumeroComprobante = @pComp, 
                TipoPago = @pTipo, 
                MontoDebitos = @pDebitos, 
                MotivoDebito = @pMotivo
            WHERE IdCobros = @pId";

        connection.Execute(query, new {
            pId = idCobro,
            pLiq = (idLiquidacion == 0 ? null : idLiquidacion),
            pOS = idObraSocial,
            pFecha = fecha,
            pImporte = importe,
            pComp = comprobante,
            pTipo = tipo,
            pDebitos = debitos,
            pMotivo = motivoDebito
        });
    }
}

// 6. Eliminar Cobro
public static void EliminarCobro(int idCobro)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = "DELETE FROM Cobros WHERE IdCobros = @pId";
        connection.Execute(query, new { pId = idCobro });
    }
}

// 7. Traer un Cobro por ID (Para editar)
public static Cobros TraerCobroPorId(int idCobro)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        string query = "SELECT * FROM Cobros WHERE IdCobros = @pId";
        return connection.QueryFirstOrDefault<Cobros>(query, new { pId = idCobro });
    }
}

// 8. Traer Mandataria dueña de una OS (Para que el combo se seleccione solo al editar)
public static Mandatarias TraerMandatariaPorIdOS(int idObraSocial)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        // Join para buscar al padre
        string query = @"
            SELECT M.* FROM Mandatarias M
            INNER JOIN ObrasSociales OS ON M.IdMandatarias = OS.IdMandatarias
            WHERE OS.IdObrasSociales = @pIdOS";
            
        return connection.QueryFirstOrDefault<Mandatarias>(query, new { pIdOS = idObraSocial });
    }
}































}