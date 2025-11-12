using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPV.Model
{
    public class VentaDetalleItem
    {
        public int Numero { get; set; } // El # en el carrito
        public int ProductoId { get; set; } // ID del producto
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; } // Precio unitario
        public decimal Subtotal => Cantidad * Precio;
    }
}