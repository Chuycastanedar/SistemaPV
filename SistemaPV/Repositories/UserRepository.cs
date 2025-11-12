using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SistemaPV.Model;

namespace SistemaPV.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public int GetIdByUsername(string username)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand("SELECT ID_USUARIO FROM USUARIO WHERE NOMBRE_USUARIO = @username", connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@username", username);
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return (int)result;
                }
            }
            return -1; // O lanza una excepción, pero -1 es más seguro.
        }

        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select * from USUARIO where NOMBRE_USUARIO=@username and PASSWORD_HASH=@password";
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;
                command.Parameters.Add("@password", SqlDbType.NVarChar).Value = credential.Password;
                validUser = command.ExecuteScalar() == null ? false : true;
            }
            return validUser;
        }

        public string GetRoleByUsername(string username)
        {
            string role = null;
            using (var connection = GetConnection()) 
            {
                connection.Open();
                string query = @"SELECT R.NOMBRE_ROL 
                         FROM USUARIO U
                         JOIN ROL R ON U.ID_ROL = R.ID_ROL
                         WHERE U.NOMBRE_USUARIO = @username";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@username", System.Data.SqlDbType.NVarChar).Value = username;

                    var result = command.ExecuteScalar(); 
                    if (result != null)
                    {
                        role = result.ToString();
                    }
                }
            }
            return role; // Devuelve "Administrador", "Cajero", o null
        }

        public void Edit(UserModel userModel)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<UserModel> GetByAll()
        {
            throw new NotImplementedException();
        }
        public UserModel GetById(int id)
        {
            throw new NotImplementedException();
        }
        public UserModel GetByUsername(string username)
        {
            UserModel user = null;
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "select *from USUARIO where NOMBRE_USUARIO=@username";
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader[0].ToString(),
                            Username = reader[1].ToString(),
                            Password = string.Empty,
                            Name = reader[3].ToString(),
                            LastName = reader[4].ToString(),
                            Email = reader[5].ToString(),
                        };
                    }
                }
            }
            return user;
        }
        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
