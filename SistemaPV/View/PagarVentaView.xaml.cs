using SistemaPV.View_Model;
using System.Text.RegularExpressions; // validación de decimales
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaPV.View
{
    public partial class PagarVentaView : Window
    {
        public PagarVentaView(PagarVentaViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;

            viewModel.RequestClose += (sender, e) => this.Close();
            viewModel.RequestDialogResult += (sender, result) =>
            {
                if (this.IsActive)
                {
                    this.DialogResult = result;
                }
            };
        }

        // Lógica para validar que solo se ingresen números y un solo punto/coma
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+"); // Permite números y un punto
            if (regex.IsMatch(e.Text) || ((TextBox)sender).Text.Contains(".") && e.Text == ".")
            {
                e.Handled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
    }
}