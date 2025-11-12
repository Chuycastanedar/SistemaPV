using SistemaPV.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SistemaPV.Repositories
{
    public class RolRepository : RepositoryBase 
    {
        public List<Rol> GetRoles()
        {
            var roles = new List<Rol>();
            string query = "SELECT ID_ROL, NOMBRE_ROL FROM ROL";

            using (var connection = GetConnection()) 
            using (var adapter = new SqlDataAdapter(query, connection))
            {
                var dtRoles = new DataTable();
                adapter.Fill(dtRoles);

                foreach (DataRow row in dtRoles.Rows)
                {
                    roles.Add(new Rol
                    {
                        ID_ROL = (int)row["ID_ROL"],
                        NOMBRE_ROL = row["NOMBRE_ROL"].ToString()
                    });
                }
            }
            return roles;
        }
    }
}