using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using SistemaPV.View_Model;
using System.Globalization;

namespace SistemaPV.View
{
    public partial class AgregarUsuarioWindow : Window
    {
        private readonly AgregarUsuarioViewModel _viewModel;

       

        public AgregarUsuarioWindow(AgregarUsuarioViewModel viewModel)
        {
            
            _viewModel = viewModel;
            this.DataContext = _viewModel; 

            
            InitializeComponent();

            _viewModel.RequestClose += (dialogResult) =>
            {
                this.DialogResult = dialogResult;
                this.Close();
            };
        }

        

        private void TxtSoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
           
            foreach (char c in e.Text)
            {
                if (!char.IsLetter(c) && c != ' ')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TxtNombreUsuario_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            foreach (char c in e.Text)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '.' && c != '_')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        
    }

    public class PasswordBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Devuelve un array de los PasswordBox para el CommandParameter
            return new object[] { values[0], values[1] };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}