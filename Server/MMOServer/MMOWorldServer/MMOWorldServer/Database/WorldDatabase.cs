using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMOWorldServer
{
    public static class WorldDatabase
    {
        private static MySqlConnection conn;
        private static string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();

        public static String SetupConnection()
        {
            string status;
            try
            {
                conn = new MySqlConnection(connString);
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "TRUNCATE TABLE online_players; ALTER TABLE online_players AUTO_INCREMENT = 1;";
                command.ExecuteNonQuery();
                status = "OK";
            }
            catch (MySqlException e)
            {
                status = e.Message.ToString();
            }
            return status;
        }

        public static void RemoveFromOnlinePlayerList(uint sessionId)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "DELETE FROM online_players where sessionId=@sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.ExecuteNonQuery();
        }

        public static uint GetSessionId(int characterId)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "SELECT sessionId from online_players where charId=@characterId";
            command.Parameters.AddWithValue("@characterId", characterId);
            MySqlDataReader rdr = command.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                var sessionId = rdr.GetUInt32(0);
                rdr.Close();

                return sessionId;
            }
            else
            {
                throw new Exception("Could not find session id for character id: " + characterId);
            }
        }

        public static void AddToOnlinePlayerList(int characterId, IPAddress clientAddress)
        {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "INSERT INTO online_players(`charId`, `accountId`, `name`, `ipAddress`) SELECT id,accountId,name,@ipAddress from login.characters where id=@characterId;";
            command.Parameters.AddWithValue("@characterId", characterId);
            command.Parameters.AddWithValue("@ipAddress", clientAddress.ToString());
            command.ExecuteNonQuery();
        }
    }
}