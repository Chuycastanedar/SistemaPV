using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaPV.View
{
    public partial class VentaWindow : Window
    {
        private class Producto
        {
            public int ID { get; set; }
            public string Nombre { get; set; }
            public decimal Precio { get; set; }
            public int Stock { get; set; }
        }

        private class DetalleVenta
        {
            public int Numero { get; set; }
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal => Cantidad * Precio;
        }

        private List<Producto> listaProductos = new List<Producto>();
        private List<DetalleVenta> carrito = new List<DetalleVenta>();

        public VentaWindow()
        {
            InitializeComponent();
            CargarProductos();
            calendarioFecha.SelectedDate = DateTime.Now;

            // Inicializar la navegación
            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            // Asegurarse de que los controles existen antes de usarlos
            if (panelVenta != null && dgvHistorial != null && dgvInventario != null)
            {
                // Configurar visibilidad inicial
                panelVenta.Visibility = Visibility.Visible;
                dgvHistorial.Visibility = Visibility.Collapsed;
                dgvInventario.Visibility = Visibility.Collapsed;
            }
        }

        private void CargarProductos()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK FROM PRODUCTO";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        listaProductos.Clear();
                        while (reader.Read())
                        {
                            listaProductos.Add(new Producto
                            {
                                ID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Stock = reader.GetInt32(3)
                            });
                        }
                    }
                }

                cbProductos.ItemsSource = listaProductos;
                cbProductos.DisplayMemberPath = "Nombre";
                cbProductos.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }

        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (cbProductos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto.");
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.");
                return;
            }

            Producto productoSeleccionado = (Producto)cbProductos.SelectedItem;

            if (cantidad > productoSeleccionado.Stock)
            {
                MessageBox.Show("No hay suficiente stock disponible.");
                return;
            }

            int numero = carrito.Count + 1;
            carrito.Add(new DetalleVenta
            {
                Numero = numero,
                Nombre = productoSeleccionado.Nombre,
                Cantidad = cantidad,
                Precio = productoSeleccionado.Precio
            });

            dgvVenta.ItemsSource = null;
            dgvVenta.ItemsSource = carrito;

            CalcularTotal();

            // LIMPIAR EL CAMPO DE CANTIDAD DESPUÉS DE AGREGAR
            txtCantidad.Clear();
        }

        private void CalcularTotal()
        {
            decimal total = carrito.Sum(i => i.Subtotal);
            txtTotal.Text = $"$ {total:F2}";
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Deseas cancelar la venta actual?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                carrito.Clear();
                dgvVenta.ItemsSource = null;
                txtTotal.Text = "$ 0.00";
            }
        }

        private void BtnProceder_Click(object sender, RoutedEventArgs e)
        {
            if (!carrito.Any())
            {
                MessageBox.Show("No hay productos en la venta.");
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // Insertar venta - CORREGIDO para usar los nombres de tu BD
                        string insertVenta = @"INSERT INTO VENTA (FECHA_VENTA, MONTO_TOTAL, ID_USUARIO, ID_METODO, ID_ESTADO_VENTA) 
                                              OUTPUT INSERTED.ID_VENTA 
                                              VALUES (@fecha, @total, 1, 1, 1)";
                        SqlCommand cmdVenta = new SqlCommand(insertVenta, conn, transaction);
                        cmdVenta.Parameters.AddWithValue("@fecha", calendarioFecha.SelectedDate ?? DateTime.Now);
                        cmdVenta.Parameters.AddWithValue("@total", carrito.Sum(c => c.Subtotal));
                        int idVenta = (int)cmdVenta.ExecuteScalar();

                        // Insertar detalles - CORREGIDO para usar PRECIO_UNITARIO_AL_VENDER
                        foreach (var item in carrito)
                        {
                            int idProducto = listaProductos.First(p => p.Nombre == item.Nombre).ID;

                            string insertDetalle = "INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER) " +
                                                   "VALUES (@idVenta, @idProducto, @cantidad, @precio)";
                            SqlCommand cmdDetalle = new SqlCommand(insertDetalle, conn, transaction);
                            cmdDetalle.Parameters.AddWithValue("@idVenta", idVenta);
                            cmdDetalle.Parameters.AddWithValue("@idProducto", idProducto);
                            cmdDetalle.Parameters.AddWithValue("@cantidad", item.Cantidad);
                            cmdDetalle.Parameters.AddWithValue("@precio", item.Precio);
                            cmdDetalle.ExecuteNonQuery();

                            // Actualizar stock
                            string updateStock = "UPDATE PRODUCTO SET CANTIDAD_STOCK = CANTIDAD_STOCK - @cantidad WHERE ID_PRODUCTO = @idProducto";
                            SqlCommand cmdUpdate = new SqlCommand(updateStock, conn, transaction);
                            cmdUpdate.Parameters.AddWithValue("@cantidad", item.Cantidad);
                            cmdUpdate.Parameters.AddWithValue("@idProducto", idProducto);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Venta registrada exitosamente.");
                        carrito.Clear();
                        dgvVenta.ItemsSource = null;
                        txtTotal.Text = "$ 0.00";
                        CargarProductos();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error al registrar la venta: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }

        // --- NUEVOS MÉTODOS PARA NAVEGACIÓN ---

        private void RbVenta_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta == null || dgvHistorial == null || dgvInventario == null)
                return;

            // Mostrar panel de venta
            panelVenta.Visibility = Visibility.Visible;
            dgvHistorial.Visibility = Visibility.Collapsed;
            dgvInventario.Visibility = Visibility.Collapsed;
        }

        private void RbHistorial_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta == null || dgvHistorial == null || dgvInventario == null)
                return;

            // Ocultar panel de venta y mostrar historial
            panelVenta.Visibility = Visibility.Collapsed;
            dgvHistorial.Visibility = Visibility.Visible;
            dgvInventario.Visibility = Visibility.Collapsed;

            // Cargar historial de ventas
            CargarHistorialVentas();
        }

        private void RbInventario_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta == null || dgvHistorial == null || dgvInventario == null)
                return;

            // Ocultar panel de venta y mostrar inventario
            panelVenta.Visibility = Visibility.Collapsed;
            dgvHistorial.Visibility = Visibility.Collapsed;
            dgvInventario.Visibility = Visibility.Visible;

            // Cargar inventario
            CargarInventario();
        }

        private void CargarHistorialVentas()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = @"SELECT v.ID_VENTA, v.FECHA_VENTA, v.MONTO_TOTAL,
                                (SELECT COUNT(*) FROM DETALLE_VENTA dv WHERE dv.ID_VENTA = v.ID_VENTA) as CANTIDAD_PRODUCTOS
                                FROM VENTA v
                                ORDER BY v.FECHA_VENTA DESC";

                DataTable dtHistorial = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dtHistorial);
                    }
                }

                dgvHistorial.ItemsSource = dtHistorial.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial de ventas: " + ex.Message);
            }
        }

        private void CargarInventario()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION FROM PRODUCTO";

                DataTable dtProductos = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dtProductos);
                    }
                }

                dgvInventario.ItemsSource = dtProductos.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el inventario: " + ex.Message);
            }
        }

        // --- CERRAR SESIÓN ---
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?",
                                                     "Cerrar Sesión",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Cerrar la aplicación completamente
                Application.Current.Shutdown();
            }
        }
    }
}