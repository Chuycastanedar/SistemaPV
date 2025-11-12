using SistemaPV.View_Model;
using System.Windows;
using System.Windows.Input;

namespace SistemaPV.View
{
    public partial class VentaWindow : Window
    {
        public VentaWindow(int idUsuario)
        {
            
            var viewModel = new VentaViewModel(idUsuario);
            viewModel.RequestLogout += OnRequestLogout;
            // Asignar el DataContext
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void OnRequestLogout()
        {
            ((App)Application.Current).NavigateToLogin();
            this.Close();
        }


        // Validar que solo se ingresen números
        private void TxtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

    }
}