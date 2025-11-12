using System;
using System.Windows;

using SistemaPV.Services;
using SistemaPV.View_Model;

namespace SistemaPV.View
{
    public partial class Inventario : Window
    {
        public Inventario()
        {
            InitializeComponent();

            
            IDialogService dialogService = new DialogService();

            
            var viewModel = new InventarioViewModel(dialogService);

            
            this.DataContext = viewModel;
        }
    }
}