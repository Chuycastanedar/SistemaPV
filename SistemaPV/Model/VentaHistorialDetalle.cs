using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPV.Model
{
    public class VentaHistorialDetalle
    {
        public string NOMBRE_PRODUCTO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }
        public decimal SUBTOTAL => CANTIDAD * PRECIO_UNITARIO;
    }
}