using System.Windows;
using System.Windows.Input;

using SistemaPV.View_Model;

namespace SistemaPV.View
{
    public partial class AgregarProductoWindow : Window
    {
        private readonly AgregarProductoViewModel _viewModel;

       
        public AgregarProductoWindow(AgregarProductoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;

            // Suscribirse al evento de cierre del ViewModel
            _viewModel.RequestClose += (dialogResult) =>
            {
                this.DialogResult = dialogResult;
                this.Close();
            };
        }

        

        private void TxtPrecio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }
            string currentText = ((System.Windows.Controls.TextBox)sender).Text + e.Text;
            if (currentText.Split('.').Length > 2)
            {
                e.Handled = true;
            }
        }

        private void TxtStock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
           
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        
    }
}