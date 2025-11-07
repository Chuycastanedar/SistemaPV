using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

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

        // Validar que solo se ingresen números decimales en Precio
        private void TxtPrecio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permitir solo números y un punto decimal
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            // Verificar que no haya más de un punto decimal
            string currentText = ((System.Windows.Controls.TextBox)sender).Text + e.Text;
            if (currentText.Split('.').Length > 2)
            {
                e.Handled = true;
            }
        }

        // Validar que solo se ingresen números enteros en Stock
        private void TxtStock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permitir solo números
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtPrecio.Text) || string.IsNullOrEmpty(txtStock.Text))
            {
                MessageBox.Show("Nombre, Precio y Stock son obligatorios.");
                return;
            }

            // Validar longitud del nombre
            if (txtNombre.Text.Length > 150)
            {
                MessageBox.Show("El nombre no puede tener más de 150 caracteres.");
                return;
            }

            // Validar longitud de la descripción
            if (txtDescripcion.Text.Length > 500)
            {
                MessageBox.Show("La descripción no puede tener más de 500 caracteres.");
                return;
            }

            // Validar formato del precio
            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio debe ser un número decimal válido y mayor o igual a 0.");
                return;
            }

            // Validar formato del stock
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El stock debe ser un número entero válido y mayor o igual a 0.");
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