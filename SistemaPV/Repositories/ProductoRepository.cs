using System;
using System.Collections.Generic;
using System.Linq;
using SistemaPV.Model;
using System.Data;
using System.Data.SqlClient;

namespace SistemaPV.Repositories
{
    public class ProductoRepository : RepositoryBase 
    {
        public List<Producto> GetProductos()
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
                        DESCRIPCION = row["DESCRIPCION"].ToString()
                    });
                }
            }
            return productos;
        }

        public bool DeleteProducto(int id)
        {
            string query = "DELETE FROM PRODUCTO WHERE ID_PRODUCTO = @id";
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }

        public bool AddProducto(Producto producto)
        {
            string query = "INSERT INTO PRODUCTO (NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION) " +
                           "VALUES (@nombre, @precio, @stock, @descripcion)";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nombre", producto.NOMBRE_PRODUCTO);
                command.Parameters.AddWithValue("@precio", producto.PRECIO);
                command.Parameters.AddWithValue("@stock", producto.CANTIDAD_STOCK);
                command.Parameters.AddWithValue("@descripcion", producto.DESCRIPCION);

                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }

        public bool UpdateProducto(Producto producto)
        {
            string query = "UPDATE PRODUCTO SET NOMBRE_PRODUCTO = @nombre, PRECIO = @precio, " +
                           "CANTIDAD_STOCK = @stock, DESCRIPCION = @descripcion " +
                           "WHERE ID_PRODUCTO = @id";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nombre", producto.NOMBRE_PRODUCTO);
                command.Parameters.AddWithValue("@precio", producto.PRECIO);
                command.Parameters.AddWithValue("@stock", producto.CANTIDAD_STOCK);
                command.Parameters.AddWithValue("@descripcion", producto.DESCRIPCION);
                command.Parameters.AddWithValue("@id", producto.ID_PRODUCTO);

                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }
    }
}