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

        // Constructor para "Agregar"
        public AgregarProductoWindow()
        {
            InitializeComponent();
        }

        // Constructor para "Editar"
        public AgregarProductoWindow(DataRowView productoAEditar)
        {
            InitializeComponent();

           
            this.productoId = (int)productoAEditar["ID_PRODUCTO"];
            txtNombre.Text = productoAEditar["NOMBRE_PRODUCTO"].ToString();
            txtPrecio.Text = productoAEditar["PRECIO"].ToString();
            txtStock.Text = productoAEditar["CANTIDAD_STOCK"].ToString();
            txtDescripcion.Text = productoAEditar["DESCRIPCION"].ToString();

            this.Title = "Editar Producto";
            btnGuardar.Content = "Actualizar";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtPrecio.Text) || string.IsNullOrEmpty(txtStock.Text))
            {
                MessageBox.Show("Nombre, Precio y Stock son obligatorios.");
                return;
            }

            try
            {
                if (this.productoId == null)
                {
                    GuardarNuevoProducto();
                }
                else
                {
                    ActualizarProductoExistente();
                }

                this.DialogResult = true;
                this.Close();
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

        private void GuardarNuevoProducto()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

            
            string query = "INSERT INTO PRODUCTO (NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION) " +
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

        private void ActualizarProductoExistente()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

            
            string query = "UPDATE PRODUCTO SET NOMBRE_PRODUCTO = @nombre, PRECIO = @precio, " +
                           "CANTIDAD_STOCK = @stock, DESCRIPCION = @descripcion " +
                           "WHERE ID_PRODUCTO = @id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    command.Parameters.AddWithValue("@precio", decimal.Parse(txtPrecio.Text));
                    command.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));
                    command.Parameters.AddWithValue("@descripcion", txtDescripcion.Text);
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