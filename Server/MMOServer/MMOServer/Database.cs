using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOServer
{
    class Database
    {
        private static MySqlConnection conn;
        private static string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();



        public bool AddUserToDb(string userName, string password)
        {
            var conn = new MySqlConnection(connString);
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "INSERT INTO account(username, password) VALUES(@user, @pass)";
                command.Parameters.AddWithValue("@user", userName);
                command.Parameters.AddWithValue("@pass", password);
                command.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Duplicate username attempted to be registered");
                return false;
            }
            catch (Exception e)
            {
                return false;

            }
            finally
            {
                conn.Dispose();
            }

        }

        public List<string> CheckUserInDb(string userName, string password)
        {
            var conn = new MySqlConnection(connString);
            List<string> list = new List<string>();
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT `username`, `password` FROM `account` WHERE `username`=@user";
                command.Parameters.AddWithValue("@user", userName);
                MySqlDataReader rdr = command.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        list.Add(rdr.GetString(0));
                        list.Add(rdr.GetString(1));
                    }
                }
                rdr.Close();
                return list;

            }
            catch (MySqlException)
            {
                Console.WriteLine("MySQL error");
                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return list;

            }

            finally
            {
                conn.Dispose();
            }
        }


        public string CheckDbConnection()
        {
            conn = new MySqlConnection(connString);
            string status;
            try
            {
                //        Console.WriteLine("Connecting to MYSQL server...");
                conn.Open();
                status = "OK";
                //        Console.WriteLine("Connected to DB");
            }
            catch (MySqlException e)
            {
                status = e.Message.ToString();
            }
            finally
            {
                conn.Dispose();
            }
            return status;

        }
    }
}
