using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaPV.Model;
using SistemaPV.Services;
using SistemaPV.View_Model; 

namespace SistemaPV.View
{
    public class DialogService : IDialogService
    {
        public bool? ShowAgregarProducto()
        {
            var viewModel = new AgregarProductoViewModel();
            var window = new AgregarProductoWindow(viewModel); 
            return window.ShowDialog();
        }

        public bool? ShowEditarProducto(Producto producto)
        {
            var viewModel = new AgregarProductoViewModel(producto); 
            var window = new AgregarProductoWindow(viewModel);
            return window.ShowDialog();
        }

        public bool? ShowAgregarUsuario()
        {
            var viewModel = new AgregarUsuarioViewModel();
            var window = new AgregarUsuarioWindow(viewModel);
            return window.ShowDialog();
        }

        public bool? ShowEditarUsuario(Usuario usuario)
        {
            
            var viewModel = new AgregarUsuarioViewModel(usuario);
            var window = new AgregarUsuarioWindow(viewModel);
            return window.ShowDialog();
        }
    }
}
