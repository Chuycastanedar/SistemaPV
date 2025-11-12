using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaPV.Model;

namespace SistemaPV.Services
{
    public interface IDialogService
    {
        // Devuelve 'true' si el diálogo se cerró con "Guardar"
        bool? ShowAgregarProducto();
        bool? ShowEditarProducto(Producto producto);
        bool? ShowAgregarUsuario();
        bool? ShowEditarUsuario(Usuario usuario);
    }
}