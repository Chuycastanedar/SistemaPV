using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPV.Model
{
    public class VentaHistorial
    {
        public int ID_VENTA { get; set; }
        public DateTime FECHA_VENTA { get; set; }
        public decimal MONTO_TOTAL { get; set; }
        public string CAJERO { get; set; }
        public string METODO_PAGO { get; set; }
        public string ESTADO_VENTA { get; set; }
        public int ID_ESTADO_VENTA { get; set; }
        public int CANTIDAD_PRODUCTOS { get; set; }
    }
}
