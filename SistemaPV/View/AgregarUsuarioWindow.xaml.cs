using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography; // Para el HASH
using System.Text; // Para el HASH
using System.Windows;
using System.Windows.Controls; // Para el ComboBox

namespace SistemaPV.View
{
    public partial class AgregarUsuarioWindow : Window
    {

        private int? usuarioId = null;
        private int? idRolAEditar = null;

        public AgregarUsuarioWindow()
        {
            InitializeComponent();
        }

        public AgregarUsuarioWindow(DataRowView usuarioAEditar)
        {
            InitializeComponent();

            // 1. Guardar los IDs
            this.usuarioId = (int)usuarioAEditar["ID_USUARIO"];
            this.idRolAEditar = (int)usuarioAEditar["ID_ROL"];

            // 2. Rellenar los campos de texto
            txtNombre.Text = usuarioAEditar["NOMBRE"].ToString();
            txtAPaterno.Text = usuarioAEditar["APATERNO"].ToString();
            txtAMaterno.Text = usuarioAEditar["AMATERNO"].ToString();
            txtNombreUsuario.Text = usuarioAEditar["NOMBRE_USUARIO"].ToString();

            // 3. Cambiar la UI para el modo "Editar"


            txtNombreUsuario.IsEnabled = false;

            // Ocultar los campos de contraseña
            lblPassword.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Collapsed;
            rowPassword.Height = new GridLength(0); // Colapsa la fila

            lblPasswordConfirm.Visibility = Visibility.Collapsed;
            txtPasswordConfirm.Visibility = Visibility.Collapsed;
            rowPasswordConfirm.Height = new GridLength(0); // Colapsa la fila

            // Cambiar títulos
            this.Title = "Editar Usuario";
            btnGuardar.Content = "Actualizar";
        }
           

        private void GuardarNuevoUsuario()
        {
            

            string aPaterno = txtAPaterno.Text;
            string aMaterno = string.IsNullOrEmpty(txtAMaterno.Text) ? null : txtAMaterno.Text;
            string nombre = txtNombre.Text;
            string nombreUsuario = txtNombreUsuario.Text;
            int idRol = (int)cmbRol.SelectedValue;
            string passwordPlana = txtPassword.Password;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = @"INSERT INTO USUARIO (APATERNO, AMATERNO, NOMBRE, NOMBRE_USUARIO, PASSWORD_HASH, ID_ROL) 
                         VALUES (@aPaterno, @aMaterno, @nombre, @nombreUsuario, @passwordHash, @idRol)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@aPaterno", aPaterno);
                        command.Parameters.AddWithValue("@aMaterno", (object)aMaterno ?? DBNull.Value);
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                        command.Parameters.AddWithValue("@passwordHash", passwordPlana);
                        command.Parameters.AddWithValue("@idRol", idRol);

                        connection.Open();
                        command.ExecuteNonQuery();

                        MessageBox.Show("¡Usuario creado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    MessageBox.Show("Error: El nombre de usuario '" + nombreUsuario + "' ya existe.", "Error de Duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error de base de datos: " + sqlEx.Message, "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                throw ex; // Relanzamos la excepción para que el BtnGuardar_Click la atrape
            }
        }

        private void ActualizarUsuarioExistente()
        {
            // (Este es el código NUEVO para el UPDATE)

            string aPaterno = txtAPaterno.Text;
            string aMaterno = string.IsNullOrEmpty(txtAMaterno.Text) ? null : txtAMaterno.Text;
            string nombre = txtNombre.Text;
            int idRol = (int)cmbRol.SelectedValue;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

                // Query para actualizar. NO actualizamos el nombre de usuario ni la contraseña.
                string query = @"UPDATE USUARIO 
                         SET APATERNO = @aPaterno, 
                             AMATERNO = @aMaterno, 
                             NOMBRE = @nombre, 
                             ID_ROL = @idRol 
                         WHERE ID_USUARIO = @id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@aPaterno", aPaterno);
                        command.Parameters.AddWithValue("@aMaterno", (object)aMaterno ?? DBNull.Value);
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@idRol", idRol);
                        command.Parameters.AddWithValue("@id", this.usuarioId.Value); // El ID del usuario que estamos editando

                        connection.Open();
                        command.ExecuteNonQuery();

                        MessageBox.Show("¡Usuario actualizado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; // Relanzamos la excepción
            }
        }
        

        //  PASO 1: Cargar los Roles en el ComboBox 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarRoles(); // Esto carga los roles en el ComboBox

            if (this.idRolAEditar != null)
            {
                cmbRol.SelectedValue = this.idRolAEditar;
            }
        }

        private void CargarRoles()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
                string query = "SELECT ID_ROL, NOMBRE_ROL FROM ROL";

                DataTable dtRoles = new DataTable();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dtRoles);
                    }
                }

                // Configuramos el ComboBox
                cmbRol.ItemsSource = dtRoles.DefaultView;
                cmbRol.DisplayMemberPath = "NOMBRE_ROL"; 
                cmbRol.SelectedValuePath = "ID_ROL";     

                // Seleccionar el primer rol por defecto si existe
                if (cmbRol.Items.Count > 0)
                {
                    cmbRol.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los roles: " + ex.Message);
            }
        }

        // PASO 2: Lógica del Botón Guardar 
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones 
            if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtAPaterno.Text) ||
                string.IsNullOrEmpty(txtNombreUsuario.Text))
            {
                MessageBox.Show("Nombre, Apellido Paterno y Nombre de Usuario son obligatorios.", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si estamos en modo "Agregar", también validamos las contraseñas
            if (this.usuarioId == null)
            {
                if (string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("La contraseña es obligatoria.", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (txtPassword.Password != txtPasswordConfirm.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (cmbRol.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un rol para el usuario.", "Error de Rol", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Lógica de decisión 
            try
            {
                
                if (this.usuarioId == null)
                {
                    GuardarNuevoUsuario();
                }
                else
                {
                    ActualizarUsuarioExistente();
                }

                // Si todo sale bien, avisamos y cerramos
                this.DialogResult = true;
                this.Close();
            }
            catch (SqlException sqlEx)
            {
                
                if (sqlEx.Number != 2627 && sqlEx.Number != 2601)
                {
                    MessageBox.Show("Error de base de datos: " + sqlEx.Message, "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
               
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        }

        // --- PASO 3: Lógica de Cancelar ---
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}