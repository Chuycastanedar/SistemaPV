using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private class DetalleVentaHistorial
        {
            public string NOMBRE_PRODUCTO { get; set; }
            public int CANTIDAD { get; set; }
            public decimal PRECIO_UNITARIO { get; set; }
            public decimal SUBTOTAL => CANTIDAD * PRECIO_UNITARIO;
        }

        private List<Producto> listaProductos = new List<Producto>();
        private List<DetalleVenta> carrito = new List<DetalleVenta>();
        private DataTable dtProductosCompleto; // Para almacenar todos los productos

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
            if (panelVenta != null && panelHistorial != null && panelInventario != null)
            {
                // Configurar visibilidad inicial
                panelVenta.Visibility = Visibility.Visible;
                panelHistorial.Visibility = Visibility.Collapsed;
                panelInventario.Visibility = Visibility.Collapsed;
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

        // MÉTODOS PARA LA BÚSQUEDA EN EL COMBOBOX
        private void CbProductos_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                // Cuando presiona Enter o Tab, buscar y seleccionar el primer resultado
                BuscarYSeleccionarProducto();
                e.Handled = true;
            }
        }

        private void CbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si se selecciona un producto del dropdown, actualizar el texto
            if (cbProductos.SelectedItem != null)
            {
                Producto producto = (Producto)cbProductos.SelectedItem;
                cbProductos.Text = producto.Nombre;
            }
        }

        private void CbProductos_KeyUp(object sender, KeyEventArgs e)
        {
            // Buscar automáticamente mientras el usuario escribe (excepto teclas de navegación)
            if (e.Key != Key.Enter && e.Key != Key.Tab && e.Key != Key.Escape && e.Key != Key.Up && e.Key != Key.Down)
            {
                BuscarYSeleccionarProducto();
            }
        }

        private void BuscarYSeleccionarProducto()
        {
            string textoBusqueda = cbProductos.Text.Trim();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                // Si no hay texto, mostrar todos los productos
                cbProductos.ItemsSource = listaProductos;
                cbProductos.IsDropDownOpen = true;
                return;
            }

            // Buscar en cualquier parte del nombre (case-insensitive)
            var productosFiltrados = listaProductos
                .Where(p => p.Nombre.IndexOf(textoBusqueda, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (productosFiltrados.Any())
            {
                // Mostrar productos filtrados en el dropdown
                cbProductos.ItemsSource = productosFiltrados;
                cbProductos.IsDropDownOpen = true;

                // Si hay exactamente un resultado, seleccionarlo automáticamente
                if (productosFiltrados.Count == 1)
                {
                    cbProductos.SelectedItem = productosFiltrados[0];
                    cbProductos.Text = productosFiltrados[0].Nombre;
                }
            }
            else
            {
                // Si no hay resultados, mantener todos los productos
                cbProductos.ItemsSource = listaProductos;
                cbProductos.IsDropDownOpen = false;
            }
        }

        // MÉTODOS PARA EL CONTROL NUMÉRICO
        private void BtnIncrementar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCantidad.Text, out int cantidad))
            {
                cantidad++;
                txtCantidad.Text = cantidad.ToString();
            }
            else
            {
                txtCantidad.Text = "1";
            }
        }

        private void BtnDecrementar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCantidad.Text, out int cantidad) && cantidad > 1)
            {
                cantidad--;
                txtCantidad.Text = cantidad.ToString();
            }
        }

        // Validar que solo se ingresen números
        private void TxtCantidad_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Solo permitir números
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
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
            txtCantidad.Text = "1";
        }

        // MÉTODO PARA QUITAR PRODUCTO SELECCIONADO
        private void BtnQuitarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgvVenta.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para quitar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetalleVenta productoSeleccionado = (DetalleVenta)dgvVenta.SelectedItem;

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas quitar el producto '{productoSeleccionado.Nombre}' de la venta?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                // Quitar el producto del carrito
                carrito.Remove(productoSeleccionado);

                // Renumerar los productos restantes
                for (int i = 0; i < carrito.Count; i++)
                {
                    carrito[i].Numero = i + 1;
                }

                // Actualizar la DataGrid
                dgvVenta.ItemsSource = null;
                dgvVenta.ItemsSource = carrito;

                // Recalcular el total
                CalcularTotal();
            }
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
                txtCantidad.Text = "1";
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
                        
                        string insertVenta = @"INSERT INTO VENTA (FECHA_VENTA, MONTO_TOTAL, ID_USUARIO, ID_METODO, ID_ESTADO_VENTA) 
                                              OUTPUT INSERTED.ID_VENTA 
                                              VALUES (@fecha, @total, 1, 1, 1)";
                        SqlCommand cmdVenta = new SqlCommand(insertVenta, conn, transaction);
                        cmdVenta.Parameters.AddWithValue("@fecha", calendarioFecha.SelectedDate ?? DateTime.Now);
                        cmdVenta.Parameters.AddWithValue("@total", carrito.Sum(c => c.Subtotal));
                        int idVenta = (int)cmdVenta.ExecuteScalar();

                        
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
                        txtCantidad.Text = "1";
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

       
        private void RbVenta_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta != null && panelHistorial != null && panelInventario != null)
            {
                // Mostrar panel de venta
                panelVenta.Visibility = Visibility.Visible;
                panelHistorial.Visibility = Visibility.Collapsed;
                panelInventario.Visibility = Visibility.Collapsed;

                // Ocultar panel de detalles al cambiar a venta
                panelDetallesVenta.Visibility = Visibility.Collapsed;
            }
        }

        private void RbHistorial_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta != null && panelHistorial != null && panelInventario != null)
            {
                // Ocultar panel de venta y mostrar historial
                panelVenta.Visibility = Visibility.Collapsed;
                panelHistorial.Visibility = Visibility.Visible;
                panelInventario.Visibility = Visibility.Collapsed;

                // Cargar historial de ventas
                CargarHistorialVentas();

                // Ocultar panel de detalles inicialmente
                panelDetallesVenta.Visibility = Visibility.Collapsed;
            }
        }

        private void RbInventario_Checked(object sender, RoutedEventArgs e)
        {
            // Verificar que los controles existen
            if (panelVenta != null && panelHistorial != null && panelInventario != null)
            {
                // Ocultar panel de venta y mostrar inventario
                panelVenta.Visibility = Visibility.Collapsed;
                panelHistorial.Visibility = Visibility.Collapsed;
                panelInventario.Visibility = Visibility.Visible;

                // Cargar inventario
                CargarInventario();

                // Ocultar panel de detalles al cambiar a inventario
                panelDetallesVenta.Visibility = Visibility.Collapsed;
            }
        }

        private void CargarHistorialVentas()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                
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

        // --- MÉTODOS PARA VER DETALLES DE PRODUCTOS ---
        private void DgvHistorial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ocultar detalles cuando se cambia la selección
            panelDetallesVenta.Visibility = Visibility.Collapsed;
        }

        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            if (dgvHistorial.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona una venta del historial para ver sus detalles.",
                                "Ninguna venta seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedVenta = (DataRowView)dgvHistorial.SelectedItem;
            int idVenta = (int)selectedVenta["ID_VENTA"];

            CargarDetallesVenta(idVenta);
        }

        private void CargarDetallesVenta(int idVenta)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = @"SELECT 
                                p.NOMBRE_PRODUCTO,
                                dv.CANTIDAD,
                                dv.PRECIO_UNITARIO_AL_VENDER as PRECIO_UNITARIO
                                FROM DETALLE_VENTA dv
                                JOIN PRODUCTO p ON dv.ID_PRODUCTO = p.ID_PRODUCTO
                                WHERE dv.ID_VENTA = @idVenta";

                List<DetalleVentaHistorial> detalles = new List<DetalleVentaHistorial>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idVenta", idVenta);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add(new DetalleVentaHistorial
                            {
                                NOMBRE_PRODUCTO = reader.GetString(0),
                                CANTIDAD = reader.GetInt32(1),
                                PRECIO_UNITARIO = reader.GetDecimal(2)
                            });
                        }
                    }
                }

                dgvDetallesVenta.ItemsSource = detalles;
                panelDetallesVenta.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los detalles de la venta: " + ex.Message);
            }
        }

        // --- CANCELAR VENTA DEL HISTORIAL ---
        private void BtnCancelarVentaHistorial_Click(object sender, RoutedEventArgs e)
        {
            if (dgvHistorial.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona una venta del historial para cancelar.",
                                "Ninguna venta seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedVenta = (DataRowView)dgvHistorial.SelectedItem;
            int idVenta = (int)selectedVenta["ID_VENTA"];
            string estadoActual = selectedVenta["ESTADO_VENTA"].ToString();

            // Verificar si la venta ya está cancelada
            if (estadoActual == "Cancelada")
            {
                MessageBox.Show("Esta venta ya está cancelada.",
                                "Venta ya cancelada", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas cancelar la venta #{idVenta}?\n\nEsta acción devolverá todos los productos al inventario.",
                "Confirmar Cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                CancelarVenta(idVenta);
            }
        }

        private void CancelarVenta(int idVenta)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 1. Obtener ID del estado "Cancelada"
                        string queryEstado = "SELECT ID_ESTADO_VENTA FROM ESTADO_VENTA WHERE NOMBRE_ESTADO_VENTA = 'Cancelada'";
                        SqlCommand cmdEstado = new SqlCommand(queryEstado, conn, transaction);
                        int idEstadoCancelada = (int)cmdEstado.ExecuteScalar();

                        // 2. Actualizar el estado de la venta a "Cancelada"
                        string updateVenta = @"UPDATE VENTA 
                                             SET ID_ESTADO_VENTA = @idEstadoCancelada 
                                             WHERE ID_VENTA = @idVenta";
                        SqlCommand cmdUpdateVenta = new SqlCommand(updateVenta, conn, transaction);
                        cmdUpdateVenta.Parameters.AddWithValue("@idEstadoCancelada", idEstadoCancelada);
                        cmdUpdateVenta.Parameters.AddWithValue("@idVenta", idVenta);
                        cmdUpdateVenta.ExecuteNonQuery();

                        // 3. Obtener todos los productos de la venta para devolver al stock
                        string queryDetalles = @"SELECT ID_PRODUCTO, CANTIDAD 
                                               FROM DETALLE_VENTA 
                                               WHERE ID_VENTA = @idVenta";
                        SqlCommand cmdDetalles = new SqlCommand(queryDetalles, conn, transaction);
                        cmdDetalles.Parameters.AddWithValue("@idVenta", idVenta);

                        List<(int idProducto, int cantidad)> productosDevolver = new List<(int, int)>();

                        using (SqlDataReader reader = cmdDetalles.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productosDevolver.Add((reader.GetInt32(0), reader.GetInt32(1)));
                            }
                        }

                        // 4. Devolver los productos al stock
                        foreach (var (idProducto, cantidad) in productosDevolver)
                        {
                            string updateStock = @"UPDATE PRODUCTO 
                                                 SET CANTIDAD_STOCK = CANTIDAD_STOCK + @cantidad 
                                                 WHERE ID_PRODUCTO = @idProducto";
                            SqlCommand cmdUpdateStock = new SqlCommand(updateStock, conn, transaction);
                            cmdUpdateStock.Parameters.AddWithValue("@cantidad", cantidad);
                            cmdUpdateStock.Parameters.AddWithValue("@idProducto", idProducto);
                            cmdUpdateStock.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Venta cancelada exitosamente. Los productos han sido devueltos al inventario.",
                                        "Cancelación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Recargar el historial y el inventario
                        CargarHistorialVentas();
                        CargarProductos();

                        // Ocultar detalles después de cancelar
                        panelDetallesVenta.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error al cancelar la venta: " + ex.Message,
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarInventario()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION FROM PRODUCTO";

                dtProductosCompleto = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dtProductosCompleto);
                    }
                }

                dgvInventario.ItemsSource = dtProductosCompleto.DefaultView;
                txtBuscarInventario.Clear(); // Limpiar búsqueda al cargar
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el inventario: " + ex.Message);
            }
        }

        // --- BÚSQUEDA EN INVENTARIO ---
        private void TxtBuscarInventario_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarInventario();
        }

        private void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarInventario.Clear();
            FiltrarInventario();
        }

        private void FiltrarInventario()
        {
            if (dtProductosCompleto == null) return;

            string filtro = txtBuscarInventario.Text.Trim();

            if (string.IsNullOrEmpty(filtro))
            {
                // Mostrar todos los productos si no hay filtro
                dgvInventario.ItemsSource = dtProductosCompleto.DefaultView;
            }
            else
            {
                // Aplicar filtro por nombre o descripción
                DataView vistaFiltrada = new DataView(dtProductosCompleto);
                vistaFiltrada.RowFilter = $"NOMBRE_PRODUCTO LIKE '%{filtro}%' OR DESCRIPCION LIKE '%{filtro}%'";
                dgvInventario.ItemsSource = vistaFiltrada;
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