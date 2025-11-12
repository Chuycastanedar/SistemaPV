using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPV.Model
{
    // Representa un registro de la tabla PRODUCTO
    public class Producto
    {
        public int ID_PRODUCTO { get; set; }
        public string NOMBRE_PRODUCTO { get; set; }
        public decimal PRECIO { get; set; }
        public int CANTIDAD_STOCK { get; set; }
        public string DESCRIPCION { get; set; }
    }
}
