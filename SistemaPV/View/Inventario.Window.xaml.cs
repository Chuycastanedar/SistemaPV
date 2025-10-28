using System;
using System.Collections.Generic;
using System.Configuration; // Para App.config
using System.Data;          // Para DataTable
using System.Data.SqlClient; // Para SQL Server
using System.Windows;       // Para WPF

namespace SistemaPV.View
{
    public partial class Inventario : Window
    {
        public Inventario()
        {
            InitializeComponent();

            this.Loaded += InventarioWindow_Loaded;
        }


        private void InventarioWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInventario();
        }

        private void CargarInventario()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "SELECT Identificador, Nombre, Precio, Stock, Descripcion FROM Productos";

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
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            //  Creamos una instancia de nuestra nueva ventana
            AgregarProductoWindow formNuevoProducto = new AgregarProductoWindow();

            //  Usamos ShowDialog() en lugar de Show().
            // ShowDialog() bloquea la ventana principal hasta que el formulario se cierre.
            bool? resultado = formNuevoProducto.ShowDialog();

            // Si el usuario guardó (configuraremos esto en el paso 3),
            // el 'resultado' será 'true'.
            if (resultado == true)
            {
                // Si el resultado es 'true', volvemos a cargar el inventario
                CargarInventario();
            }
        }


        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Verificar si hay un producto seleccionado
            // 'SelectedItem' nos da la fila completa que el usuario seleccionó.
            object itemSeleccionado = dgvInventario.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para eliminar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //  Obtener el ID del producto
            // Como enlazamos un DataTable, el 'SelectedItem' es un 'DataRowView'.
            // Necesitamos "convertirlo" (castearlo) para poder leer sus columnas.
            DataRowView rowView = (DataRowView)itemSeleccionado;

            // Obtenemos el Identificador y el Nombre de la fila seleccionada
            int idParaEliminar = (int)rowView["Identificador"];
            string nombreProducto = rowView["Nombre"].ToString();

            //  ¡Pedir confirmación!
            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar el producto: '{nombreProducto}'?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            // Si el usuario presiona "No", simplemente salimos del método.
            if (confirmacion == MessageBoxResult.No)
            {
                return;
            }

            //  Ejecutar el comando SQL DELETE
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                // Usamos parámetros (@id) para evitar Inyección SQL
                string query = "DELETE FROM Productos WHERE Identificador = @id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Asignamos el valor al parámetro @id
                        command.Parameters.AddWithValue("@id", idParaEliminar);

                        connection.Open();
                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Producto eliminado exitosamente.");

                            // Refrescar la tabla
                            CargarInventario();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo eliminar el producto. Es posible que ya haya sido eliminado.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el producto: " + ex.Message,
                                "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // En Inventario.xaml.cs

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Obtener el producto seleccionado (igual que en "Eliminar")
            object itemSeleccionado = dgvInventario.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para editar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Convertimos el item a DataRowView
            DataRowView rowView = (DataRowView)itemSeleccionado;

            // 2. Abrir la ventana de edición PASÁNDOLE los datos
            // Usamos el NUEVO constructor que creamos
            AgregarProductoWindow formEditar = new AgregarProductoWindow(rowView);

            // 3. Abrir como diálogo y refrescar si el resultado es "true"
            bool? resultado = formEditar.ShowDialog();

            if (resultado == true)
            {
                // Si el usuario guardó/actualizó, refrescamos la tabla
                CargarInventario();
            }
        }

        private void RbInventario_Checked(object sender, RoutedEventArgs e)
        {
            // Asegurarse de que los controles no sean nulos (pasa al iniciar)
            if (dgvInventario == null) return;

            // Mostrar todo lo de INVENTARIO
            dgvInventario.Visibility = Visibility.Visible;
            panelBotonesProductos.Visibility = Visibility.Visible;

            // Ocultar todo lo de USUARIOS
            dgvUsuarios.Visibility = Visibility.Collapsed;
            panelBotonesUsuarios.Visibility = Visibility.Collapsed;
        }

        private void RbUsuarios_Checked(object sender, RoutedEventArgs e)
        {
            // Ocultar todo lo de INVENTARIO
            dgvInventario.Visibility = Visibility.Collapsed;
            panelBotonesProductos.Visibility = Visibility.Collapsed;

            // Mostrar todo lo de USUARIOS
            dgvUsuarios.Visibility = Visibility.Visible;
            panelBotonesUsuarios.Visibility = Visibility.Visible;

            // Cargar los datos de los usuarios (lo crearemos ahora)
            CargarUsuarios();
        }
        private void CargarUsuarios()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                // Ocultamos la contraseña por seguridad. ¡Nunca la muestres en la UI!
                string query = "SELECT Id, NombreUsuario, Rol FROM Usuarios";

                DataTable dtUsuarios = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dtUsuarios);
                    }
                }

                dgvUsuarios.ItemsSource = dtUsuarios.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}