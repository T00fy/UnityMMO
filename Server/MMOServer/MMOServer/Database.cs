using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOServer
{
    public class Database
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
                command.CommandText = "SELECT `username` FROM `account` WHERE BINARY `username`=@user";
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
                command.CommandText = "SELECT `username`, `password` FROM `account` WHERE BINARY `username`=@user AND `password`=@password";
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
                conn.Close();
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

        public string GetAccountIdFromAccountName(string accountName)
        {
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT `id` FROM `account` where `username`=@username";
                command.Parameters.AddWithValue("@username", accountName);
                rdr = command.ExecuteReader();
                string accountId = "";
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        accountId = rdr.GetString(0);
                    }

                }
                else
                {
                    Console.WriteLine("Could not find your username.");
                    return "-1";
                    //set is authenticated to false
                }
                rdr.Close();
                return accountId;
            }
            catch (MySqlException e)
            {
                return "-1";
            }
            finally
            {
                conn.Dispose();
            }
            
        }


        /// <summary>
        /// Returns a list of string arrays corresponding to character information. Each array is a character
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<string[]> GetListOfCharacters(string accountId)
        {
            //get accountid with account name
            MySqlDataReader rdr = null;
            List<string[]> characters = new List<string[]>();
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();

                command = conn.CreateCommand();
                command.CommandText = "SELECT CharID, CharacterSlot, AccountID, Name, Strength, Agility, Intellect, Vitality, Dexterity FROM `chars` left join account on account.id = chars.AccountID WHERE account.id=@accountId";
                command.Parameters.AddWithValue("@accountId", accountId);
                rdr = command.ExecuteReader();
                
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        string[] character = new string[rdr.FieldCount];
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            
                            character[i] = rdr.GetString(i);
                        }
                        characters.Add(character);
                    }
                }
                rdr.Close();
                conn.Close();
            }
            catch (MySqlException e)
            {

                Console.WriteLine(e);
                rdr.Close();
                conn.Close();
            }

            return characters;
        }

        public int GetNumberOfCharactersForAccount(string userName)
        {
            //SELECT  `CharacterSlot` FROM  `chars` LEFT JOIN account ON account.id = chars.AccountID AND account.username = 'toofy'
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT  `CharacterSlot` FROM  `chars` LEFT JOIN account ON account.id = chars.AccountID AND account.username=@username";
                command.Parameters.AddWithValue("@username", userName);

                MySqlDataReader rdr = command.ExecuteReader();

                int amountOfRows=0;
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        amountOfRows++;
                    }
                }
                rdr.Close();
                conn.Close();
                return amountOfRows;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error when trying to check how many characters account has for user: " + userName);
                Console.WriteLine(e);
                return 999;
            }

            
        }

        public bool EnsuredThatCharacterSlotIsUniqueValue(ushort selectedSlot, string userName)
        {
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "SELECT  `CharacterSlot` FROM  `chars` LEFT JOIN account ON account.id = chars.AccountID AND account.username=@username";
                command.Parameters.AddWithValue("@username", userName);

                MySqlDataReader rdr = command.ExecuteReader();
                List<int> temp = new List<int>();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        temp.Add(rdr.GetInt32(0));
                    }
                }
                temp.Add(selectedSlot);
                rdr.Close();
                conn.Close();
                return temp.GroupBy(n => n).Any(c => c.Count() < 2);
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL exception when trying to ensure character slot value is unique");
                Console.WriteLine(e);
                return false;
            }
        }


        public int DeleteCharacterFromDb(int charId)
        {
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "DELETE FROM `login`.`chars` WHERE `chars`.`CharID` =@charId";
                command.Parameters.AddWithValue("@charId", charId);

                MySqlDataReader rdr = command.ExecuteReader();
                rdr.Close();
                conn.Close();
                return -1;
            }
            catch(MySqlException e)
            {
                Console.WriteLine("SQL exception when trying to delete character");
                Console.WriteLine(e);
                return 999;
            }
        }

        /// <summary>
        /// Adds a new character to the database
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="cp"></param>
        /// <returns>Returns an int corresponding to ErrorCodes. If no error found will return -1</returns>
        public int AddCharacterToDb(string accountName, CharacterCreatePacket cp)
        {
            try
            {
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "INSERT INTO  `chars` (`AccountID`,`CharacterSlot`,`Name`,`Strength`,`Agility`,`Intellect`,`Vitality`,`Dexterity`)"
                    + "SELECT id, @selectedSlot, @characterName, @str,@agi,@int,@vit,@dex FROM account WHERE username = @user";
                command.Parameters.AddWithValue("@user", accountName);
                command.Parameters.AddWithValue("@characterName", cp.characterName);
                command.Parameters.AddWithValue("@selectedSlot", cp.selectedSlot);
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
