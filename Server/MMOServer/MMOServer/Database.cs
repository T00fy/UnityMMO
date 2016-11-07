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
          //      Console.WriteLine(e.ToString());
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
                command.CommandText = "SELECT `username` FROM `account` WHERE `username`=@user";
                command.Parameters.AddWithValue("@user", userName);
                MySqlDataReader rdr = command.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        list.Add(rdr.GetString(0));
                    }
                }
                rdr.Close();
                command.CommandText = "SELECT `username`, `password` FROM `account` WHERE `username`=@user AND `password`=@password";
                command.Parameters.AddWithValue("@password", password);
                rdr = command.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        list.Add(rdr.GetString(0));
                        list.Add(rdr.GetString(1));
                    }
                }
                if (list.Count > 1)
                {
                    if (list.ElementAt(0) == list.ElementAt(1))
                    {
                        list.RemoveAt(1);
                    }
                    else
                    {
                        list.RemoveAt(1);
                        list.RemoveAt(2);
                    }
                }
                rdr.Close();
                return list;

            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.ToString());
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
        /// <summary>
        /// Adds a new character to the database
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="cp"></param>
        /// <returns>Returns an int corresponding to ErrorCodes. If no error found will return -1</returns>
        public int AddCharacterToDb(string accountName, CharacterPacket cp)
        {
            /*            INSERT INTO  `chars` (  `AccountID`,`Name`,`Strength`,`Agility`,`Intellect`,`Vitality`,`Dexterity`  ) 
                        SELECT id, 'test', 1,1,1,1,1
                        FROM account
                        WHERE username = 'toofy'*/
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "INSERT INTO  `chars` (`AccountID`,`Name`,`Strength`,`Agility`,`Intellect`,`Vitality`,`Dexterity`)"
                    + "SELECT id, @charactername, @str,@agi,@int,@vit,@dex FROM account WHERE username = @user";
                command.Parameters.AddWithValue("@user", accountName);
                command.Parameters.AddWithValue("@charactername", cp.characterName);
                command.Parameters.AddWithValue("@str", cp.str);
                command.Parameters.AddWithValue("@agi", cp.agi);
                command.Parameters.AddWithValue("@int", cp.inte);
                command.Parameters.AddWithValue("@vit", cp.vit);
                command.Parameters.AddWithValue("@dex", cp.dex);

                MySqlDataReader rdr = command.ExecuteReader();
                rdr.Close();
                conn.Close();
                return -1;
            }
            catch (MySqlException e)
            {
                conn.Close();
                switch (e.Number)
                {
                    case (1062):
                        Console.WriteLine("Duplicate character name attempted to be created");
                        return (int)ErrorCodes.DuplicateCharacter;
                    default:
                        Console.WriteLine("Got a MySQL error and not sure how to handle it");
                        Console.WriteLine("Error code is " + e.Number);
                        return (int)ErrorCodes.UnknownDatabaseError;
                }

            }
        }
    }
}
