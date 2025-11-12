using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPV.Model
{
    public class Usuario
    {
        // Campos de la tabla USUARIO
        public int ID_USUARIO { get; set; }
        public string APATERNO { get; set; }
        public string AMATERNO { get; set; }
        public string NOMBRE { get; set; }
        public string NOMBRE_USUARIO { get; set; }
        public int ID_ROL { get; set; }

        // Campo de la tabla ROL (via JOIN)
        public string NOMBRE_ROL { get; set; }
    }
}
