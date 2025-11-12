using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaPV.Model;
using System.Data;
using System.Data.SqlClient;

namespace SistemaPV.Repositories
{
    public class VentaRepository : RepositoryBase
    {
        
        public List<Producto> GetTodosProductos()
        {
            var productos = new List<Producto>();
            string query = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION FROM PRODUCTO";

            using (var connection = GetConnection())
            using (var adapter = new SqlDataAdapter(query, connection))
            {
                var dtProductos = new DataTable();
                adapter.Fill(dtProductos);

                foreach (DataRow row in dtProductos.Rows)
                {
                    productos.Add(new Producto
                    {
                        ID_PRODUCTO = (int)row["ID_PRODUCTO"],
                        NOMBRE_PRODUCTO = row["NOMBRE_PRODUCTO"].ToString(),
                        PRECIO = (decimal)row["PRECIO"],
                        CANTIDAD_STOCK = (int)row["CANTIDAD_STOCK"],
                        DESCRIPCION = row["DESCRIPCION"]?.ToString()
                    });
                }
            }
            return productos;
        }

        
        public List<VentaHistorial> GetHistorialVentas()
        {
            var historial = new List<VentaHistorial>();
            string query = @"SELECT 
                                v.ID_VENTA, 
                                v.FECHA_VENTA, 
                                v.MONTO_TOTAL,
                                u.NOMBRE + ' ' + u.APATERNO as CAJERO,
                                mp.NOMBRE_METODO as METODO_PAGO,
                                ev.NOMBRE_ESTADO_VENTA as ESTADO_VENTA,
                                ev.ID_ESTADO_VENTA,
                                (SELECT SUM(dv.CANTIDAD) FROM DETALLE_VENTA dv WHERE dv.ID_VENTA = v.ID_VENTA) as CANTIDAD_PRODUCTOS
                                FROM VENTA v
                                JOIN USUARIO u ON v.ID_USUARIO = u.ID_USUARIO
                                JOIN METODO_PAGO mp ON v.ID_METODO = mp.ID_METODO
                                JOIN ESTADO_VENTA ev ON v.ID_ESTADO_VENTA = ev.ID_ESTADO_VENTA
                                ORDER BY v.FECHA_VENTA DESC";

            using (var connection = GetConnection())
            using (var adapter = new SqlDataAdapter(query, connection))
            {
                var dtHistorial = new DataTable();
                adapter.Fill(dtHistorial);

                foreach (DataRow row in dtHistorial.Rows)
                {
                    historial.Add(new VentaHistorial
                    {
                        ID_VENTA = (int)row["ID_VENTA"],
                        FECHA_VENTA = (DateTime)row["FECHA_VENTA"],
                        MONTO_TOTAL = (decimal)row["MONTO_TOTAL"],
                        CAJERO = row["CAJERO"]?.ToString(),
                        METODO_PAGO = row["METODO_PAGO"]?.ToString(),
                        ESTADO_VENTA = row["ESTADO_VENTA"]?.ToString(),
                        ID_ESTADO_VENTA = (int)row["ID_ESTADO_VENTA"],
                        CANTIDAD_PRODUCTOS = row["CANTIDAD_PRODUCTOS"] != DBNull.Value ? (int)row["CANTIDAD_PRODUCTOS"] : 0
                    });
                }
            }
            return historial;
        }

        
        public List<VentaHistorialDetalle> GetDetallesDeVenta(int idVenta)
        {
            var detalles = new List<VentaHistorialDetalle>();
            string query = @"SELECT 
                                p.NOMBRE_PRODUCTO,
                                dv.CANTIDAD,
                                dv.PRECIO_UNITARIO_AL_VENDER as PRECIO_UNITARIO
                                FROM DETALLE_VENTA dv
                                JOIN PRODUCTO p ON dv.ID_PRODUCTO = p.ID_PRODUCTO
                                WHERE dv.ID_VENTA = @idVenta";

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detalles.Add(new VentaHistorialDetalle
                        {
                            NOMBRE_PRODUCTO = reader.GetString(0),
                            CANTIDAD = reader.GetInt32(1),
                            PRECIO_UNITARIO = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return detalles;
        }

        public List<MetodoPago> GetMetodosPago()
        {
            var metodos = new List<MetodoPago>();
            string query = "SELECT ID_METODO, NOMBRE_METODO FROM METODO_PAGO";

            using (var connection = GetConnection())
            using (var adapter = new SqlDataAdapter(query, connection))
            {
                var dtMetodos = new DataTable();
                adapter.Fill(dtMetodos);

                foreach (DataRow row in dtMetodos.Rows)
                {
                    metodos.Add(new MetodoPago
                    {
                        ID_METODO = (int)row["ID_METODO"],
                        NOMBRE_METODO = row["NOMBRE_METODO"].ToString()
                    });
                }
            }
            return metodos;
        }

        public bool RegistrarVenta(List<VentaDetalleItem> carrito, DateTime fecha, int idUsuarioLogueado, int idMetodoPago)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {

                    int idEstadoVenta = 1;

                    string insertVenta = @"INSERT INTO VENTA (FECHA_VENTA, MONTO_TOTAL, ID_USUARIO, ID_METODO, ID_ESTADO_VENTA) 
                                           OUTPUT INSERTED.ID_VENTA 
                                           VALUES (@fecha, @total, @idUsuario, @idMetodo, @idEstado)";

                    var cmdVenta = new SqlCommand(insertVenta, conn, transaction);
                    cmdVenta.Parameters.AddWithValue("@fecha", fecha);
                    cmdVenta.Parameters.AddWithValue("@total", carrito.Sum(c => c.Subtotal));
                    cmdVenta.Parameters.AddWithValue("@idUsuario", idUsuarioLogueado);
                    cmdVenta.Parameters.AddWithValue("@idMetodo", idMetodoPago);
                    cmdVenta.Parameters.AddWithValue("@idEstado", idEstadoVenta);

                    int idVenta = (int)cmdVenta.ExecuteScalar();

                    foreach (var item in carrito)
                    {
                        string insertDetalle = "INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER) " +
                                               "VALUES (@idVenta, @idProducto, @cantidad, @precio)";
                        var cmdDetalle = new SqlCommand(insertDetalle, conn, transaction);
                        cmdDetalle.Parameters.AddWithValue("@idVenta", idVenta);
                        cmdDetalle.Parameters.AddWithValue("@idProducto", item.ProductoId);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio", item.Precio);
                        cmdDetalle.ExecuteNonQuery();

                        string updateStock = "UPDATE PRODUCTO SET CANTIDAD_STOCK = CANTIDAD_STOCK - @cantidad WHERE ID_PRODUCTO = @idProducto";
                        var cmdUpdate = new SqlCommand(updateStock, conn, transaction);
                        cmdUpdate.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdUpdate.Parameters.AddWithValue("@idProducto", item.ProductoId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }



        public bool CancelarVenta(int idVenta)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string queryEstado = "SELECT ID_ESTADO_VENTA FROM ESTADO_VENTA WHERE NOMBRE_ESTADO_VENTA = 'Cancelada'";
                    var cmdEstado = new SqlCommand(queryEstado, conn, transaction);
                    int idEstadoCancelada = (int)cmdEstado.ExecuteScalar();

                    string updateVenta = "UPDATE VENTA SET ID_ESTADO_VENTA = @idEstadoCancelada WHERE ID_VENTA = @idVenta";
                    var cmdUpdateVenta = new SqlCommand(updateVenta, conn, transaction);
                    cmdUpdateVenta.Parameters.AddWithValue("@idEstadoCancelada", idEstadoCancelada);
                    cmdUpdateVenta.Parameters.AddWithValue("@idVenta", idVenta);
                    cmdUpdateVenta.ExecuteNonQuery();

                    string queryDetalles = "SELECT ID_PRODUCTO, CANTIDAD FROM DETALLE_VENTA WHERE ID_VENTA = @idVenta";
                    var cmdDetalles = new SqlCommand(queryDetalles, conn, transaction);
                    cmdDetalles.Parameters.AddWithValue("@idVenta", idVenta);

                    var productosDevolver = new List<(int idProducto, int cantidad)>();
                    using (var reader = cmdDetalles.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productosDevolver.Add((reader.GetInt32(0), reader.GetInt32(1)));
                        }
                    }

                    foreach (var (idProducto, cantidad) in productosDevolver)
                    {
                        string updateStock = "UPDATE PRODUCTO SET CANTIDAD_STOCK = CANTIDAD_STOCK + @cantidad WHERE ID_PRODUCTO = @idProducto";
                        var cmdUpdateStock = new SqlCommand(updateStock, conn, transaction);
                        cmdUpdateStock.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdUpdateStock.Parameters.AddWithValue("@idProducto", idProducto);
                        cmdUpdateStock.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}