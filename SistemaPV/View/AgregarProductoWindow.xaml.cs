
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace SistemaPV.View
{
    public partial class AgregarProductoWindow : Window
    {
        private int? productoId = null;


        // Este es el primer constructor
        public AgregarProductoWindow()
        { 
        
        }

        // Este es un segundo constructor que acepta la fila seleccionada.
        public AgregarProductoWindow(DataRowView productoAEditar)
        {
            InitializeComponent();

            // Guardamos el ID del producto
            this.productoId = (int)productoAEditar["Identificador"];

            // Rellenamos los campos con los datos del producto
            txtNombre.Text = productoAEditar["Nombre"].ToString();
            txtPrecio.Text = productoAEditar["Precio"].ToString();
            txtStock.Text = productoAEditar["Stock"].ToString();
            txtDescripcion.Text = productoAEditar["Descripcion"].ToString();

            // Cambiamos la apariencia para el modo "Editar"
            this.Title = "Editar Producto";
            btnGuardar.Content = "Actualizar";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validacion
            if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtPrecio.Text))
            {
                MessageBox.Show("El nombre y el precio son obligatorios.");
                return;

            }

            try
            {

                if (this.productoId == null)
                {
                    // Modo "Agregar" (el ID es nulo)
                    GuardarNuevoProducto();
                }
                else
                {
                    // Modo "Editar" (tenemos un ID)
                    ActualizarProductoExistente();
                }

                //  Obtener la cadena de conexión
                string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

               
               
                
                // Siempre usamos parámetros (ej: @nombre).
                string query = "INSERT INTO Productos (Nombre, Precio, Stock, Descripcion) " +
                               "VALUES (@nombre, @precio, @stock, @descripcion)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Asignamos los valores a los parámetros
                        command.Parameters.AddWithValue("@nombre", txtNombre.Text);

                        // Convertimos los valores a sus tipos correctos
                        command.Parameters.AddWithValue("@precio", decimal.Parse(txtPrecio.Text));
                        command.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));
                        command.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);

                        //Abrimos conexión y ejecutamos
                        connection.Open();

                        // Usamos ExecuteNonQuery() para comandos INSERT, UPDATE, DELETE
                        // Devuelve el número de filas afectadas.
                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("¡Producto guardado exitosamente!");

                            
                            // Le decimos a la ventana principal que "sí se guardó".
                            this.DialogResult = true;

                            //Cerramos este formulario
                            this.Close();
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Error: El precio y el stock deben ser números válidos.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el producto: " + ex.Message);
            }
        }

        // Este método es el código INSERT que ya tenías
        private void GuardarNuevoProducto()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
            string query = "INSERT INTO Productos (Nombre, Precio, Stock, Descripcion) " +
                           "VALUES (@nombre, @precio, @stock, @descripcion)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    command.Parameters.AddWithValue("@precio", decimal.Parse(txtPrecio.Text));
                    command.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));
                    command.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);

                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("¡Producto guardado exitosamente!");
                }
            }
        }

        // ¡Este es el método nuevo para el UPDATE!
        private void ActualizarProductoExistente()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

            // 🛡️ El comando UPDATE usa SET para cambiar valores y WHERE para especificar cuál
            string query = "UPDATE Productos SET Nombre = @nombre, Precio = @precio, " +
                           "Stock = @stock, Descripcion = @descripcion " +
                           "WHERE Identificador = @id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Asignamos todos los valores de los campos...
                    command.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    command.Parameters.AddWithValue("@precio", decimal.Parse(txtPrecio.Text));
                    command.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));
                    command.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);

                    // ¡Y muy importante, le decimos qué ID actualizar!
                    command.Parameters.AddWithValue("@id", this.productoId.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("¡Producto actualizado exitosamente!");
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
