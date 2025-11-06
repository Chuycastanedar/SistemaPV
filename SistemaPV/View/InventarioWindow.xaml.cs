using System;
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
            this.Loaded += Inventario_Loaded;
        }

        private void Inventario_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInventario();
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
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            AgregarProductoWindow formNuevoProducto = new AgregarProductoWindow();
            bool? resultado = formNuevoProducto.ShowDialog();

            if (resultado == true)
            {
                CargarInventario();
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            object itemSeleccionado = dgvInventario.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para eliminar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView rowView = (DataRowView)itemSeleccionado;


            int idParaEliminar = (int)rowView["ID_PRODUCTO"];
            string nombreProducto = rowView["NOMBRE_PRODUCTO"].ToString();

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar el producto: '{nombreProducto}'?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;


                string query = "DELETE FROM PRODUCTO WHERE ID_PRODUCTO = @id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", idParaEliminar);
                        connection.Open();
                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Producto eliminado exitosamente.");
                            CargarInventario();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo eliminar el producto.");
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

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            object itemSeleccionado = dgvInventario.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para editar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView rowView = (DataRowView)itemSeleccionado;


            AgregarProductoWindow formEditar = new AgregarProductoWindow(rowView);

            bool? resultado = formEditar.ShowDialog();

            if (resultado == true)
            {
                CargarInventario();
            }
        }

        // --- LÓGICA DE USUARIOS ---

        private void RbInventario_Checked(object sender, RoutedEventArgs e)
        {
            if (dgvInventario == null) return;

            dgvInventario.Visibility = Visibility.Visible;
            panelBotonesProductos.Visibility = Visibility.Visible;

            dgvUsuarios.Visibility = Visibility.Collapsed;
            panelBotonesUsuarios.Visibility = Visibility.Collapsed;
        }

        private void RbUsuarios_Checked(object sender, RoutedEventArgs e)
        {
            if (dgvInventario == null) return;

            dgvInventario.Visibility = Visibility.Collapsed;
            panelBotonesProductos.Visibility = Visibility.Collapsed;

            dgvUsuarios.Visibility = Visibility.Visible;
            panelBotonesUsuarios.Visibility = Visibility.Visible;

            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                // CAMBIO: Añadimos "u.ID_ROL" a la consulta
                string query = "SELECT u.ID_USUARIO, u.NOMBRE_USUARIO, r.NOMBRE_ROL, u.ID_ROL " +
                               "FROM USUARIO u " +
                               "JOIN ROL r ON u.ID_ROL = r.ID_ROL";

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


        private void BtnAgregarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // 1. Creamos la nueva ventana
            AgregarUsuarioWindow formNuevoUsuario = new AgregarUsuarioWindow();

            // 2. La mostramos como diálogo
            bool? resultado = formNuevoUsuario.ShowDialog();

            if (resultado == true)
            {
                CargarUsuarios();
            }
        }


        private void BtnEliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verificar si hay un usuario seleccionado
            object itemSeleccionado = dgvUsuarios.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la lista para eliminar.",
                                "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Obtener los datos del usuario
            DataRowView rowView = (DataRowView)itemSeleccionado;
            int idParaEliminar = (int)rowView["ID_USUARIO"];
            string nombreUsuario = rowView["NOMBRE_USUARIO"].ToString();

            // 3. Comprobación de seguridad crítica

            if (nombreUsuario.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("No se puede eliminar al usuario administrador principal.",
                                "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4. Pedir confirmación
            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar al usuario: '{nombreUsuario}'?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No)
            {
                return;
            }

            // 5. Ejecutar el comando SQL DELETE
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "DELETE FROM USUARIO WHERE ID_USUARIO = @id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", idParaEliminar);

                        connection.Open();
                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Usuario eliminado exitosamente.");

                            // 6. Refrescar la tabla de usuarios
                            CargarUsuarios();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario: " + ex.Message,
                                "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnEditarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // 1. Obtener el usuario seleccionado
            object itemSeleccionado = dgvUsuarios.SelectedItem;

            if (itemSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la lista para editar.",
                                "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView rowView = (DataRowView)itemSeleccionado;

            // 2. Abrir la ventana de edición USANDO EL NUEVO CONSTRUCTOR

            AgregarUsuarioWindow formEditar = new AgregarUsuarioWindow(rowView);

            // 3. Mostrar como diálogo y refrescar si el resultado es "true"
            bool? resultado = formEditar.ShowDialog();

            if (resultado == true)
            {
                CargarUsuarios();
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