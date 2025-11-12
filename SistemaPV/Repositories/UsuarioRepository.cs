using SistemaPV.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SistemaPV.Repositories
{
    public class UsuarioRepository : RepositoryBase 
    {
        public List<Usuario> GetUsuarios()
        {
            var usuarios = new List<Usuario>();
            
            string query = @"SELECT 
                                u.ID_USUARIO, u.APATERNO, u.AMATERNO, u.NOMBRE, u.NOMBRE_USUARIO, u.ID_ROL,
                                r.NOMBRE_ROL 
                             FROM USUARIO u 
                             JOIN ROL r ON u.ID_ROL = r.ID_ROL";

            using (var connection = GetConnection()) 
            using (var adapter = new SqlDataAdapter(query, connection))
            {
                var dtUsuarios = new DataTable();
                adapter.Fill(dtUsuarios);

                foreach (DataRow row in dtUsuarios.Rows)
                {
                    usuarios.Add(new Usuario
                    {
                        ID_USUARIO = (int)row["ID_USUARIO"],
                        APATERNO = row["APATERNO"]?.ToString(),
                        AMATERNO = row["AMATERNO"]?.ToString(),
                        NOMBRE = row["NOMBRE"]?.ToString(),
                        NOMBRE_USUARIO = row["NOMBRE_USUARIO"].ToString(),
                        ID_ROL = (int)row["ID_ROL"],
                        NOMBRE_ROL = row["NOMBRE_ROL"].ToString()
                    });
                }
            }
            return usuarios;
        }

        public bool DeleteUsuario(int id)
        {
            string query = "DELETE FROM USUARIO WHERE ID_USUARIO = @id";
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }

        public bool AddUsuario(Usuario usuario, string password)
        {
            string query = @"INSERT INTO USUARIO (APATERNO, AMATERNO, NOMBRE, NOMBRE_USUARIO, PASSWORD_HASH, ID_ROL) 
                             VALUES (@aPaterno, @aMaterno, @nombre, @nombreUsuario, @passwordHash, @idRol)";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@aPaterno", usuario.APATERNO);
                command.Parameters.AddWithValue("@aMaterno", (object)usuario.AMATERNO ?? DBNull.Value);
                command.Parameters.AddWithValue("@nombre", usuario.NOMBRE);
                command.Parameters.AddWithValue("@nombreUsuario", usuario.NOMBRE_USUARIO);
                command.Parameters.AddWithValue("@passwordHash", password); 
                command.Parameters.AddWithValue("@idRol", usuario.ID_ROL);

                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }

        public bool UpdateUsuario(Usuario usuario)
        {
            string query = @"UPDATE USUARIO 
                             SET APATERNO = @aPaterno, 
                                 AMATERNO = @aMaterno, 
                                 NOMBRE = @nombre, 
                                 ID_ROL = @idRol 
                             WHERE ID_USUARIO = @id";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@aPaterno", usuario.APATERNO);
                command.Parameters.AddWithValue("@aMaterno", (object)usuario.AMATERNO ?? DBNull.Value);
                command.Parameters.AddWithValue("@nombre", usuario.NOMBRE);
                command.Parameters.AddWithValue("@idRol", usuario.ID_ROL);
                command.Parameters.AddWithValue("@id", usuario.ID_USUARIO);

                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }
    }
}