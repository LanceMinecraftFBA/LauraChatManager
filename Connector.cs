using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace Connector
{
    public class Connector
    {
        private static string host { get; set; } = "host";
        private static string user { get; set; } = "user";
        private static string passwrd { get; set; } = "password";
        private static string database { get; set; } = "database";

        public static string message;
        public static int maxWarns;
        public static int UserWarns;
        public static List<string> reasons = new List<string>();
        public static List<DateTime> time_stamp = new List<DateTime>();
        public static int countMess;
        public static int maxMess;
        public static string as_active;
        public static string rule;
        public static long user_rating;
        public static DateTime soc_control;
        public static int count_control;
        public static string capcha_status;
        public static int minutes;

        public static DateTime capcha_end;
        public static int attemps;

        public static DateTime nightmode;
        public static DateTime statemode;

        public static string isActive;

        public static string punishment;
        public static DateTime deadline;

        public static string conn { get; set; } = $"server={host};user={user};database={database};password={passwrd}";

        public static void DeleteUserBot(long chat_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM userbot WHERE chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static bool GetAdmin(long userId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT * FROM botadmins WHERE user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                bool status = false;
                if(myReader.Read() == false)
                {
                    status = false;
                }
                else
                {
                    status = true;
                }
                return status;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return false;
            }
        }
        public static string GetUsers()
        {
            try
                {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT COUNT(*) FROM botusers";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                myReader.Read();
                return myReader[0].ToString();
                }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return null;
            }
        }

        public static void RegisterChatWarns(long chat_id, int max_value, string punishment)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();
                
                var query_ins_warn = $"INSERT INTO max_warns (id, chat_id, max_warns, punishment) VALUES (0, '{chat_id}', '{max_value}', '{punishment}')";
                myCmd = new MySqlCommand(query_ins_warn, mySql);
                myCmd.ExecuteNonQuery();
                
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CheckChatWarns(long chat_id)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                MySqlDataReader myReader;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();
                
                var query_ins_warn = $"SELECT max_warns FROM max_warns WHERE chat_id = '{chat_id}'";
                myCmd = new MySqlCommand(query_ins_warn, mySql);
                myReader = myCmd.ExecuteReader();
                
                if (myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "has max warns";
                    maxWarns = Convert.ToInt32(myReader[0].ToString());
                }
                
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetPunishment(long chat_id)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql = new MySqlConnection(conn);
                mySql.Open();
                var query = $"SELECT punishment FROM max_warns WHERE chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                myReader.Read();
                punishment = myReader[0].ToString();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                punishment = "Connection was aborted!";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CheckUserWarns(long user_id, long chat_id)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                MySqlDataReader myReader;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();
                
                var query_check_warn = $"SELECT warns FROM warn_users WHERE chat_id = '{chat_id}' AND user_id = '{user_id}'";
                myCmd = new MySqlCommand(query_check_warn, mySql);
                myReader = myCmd.ExecuteReader();
                
                if (myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "has warn data";
                    UserWarns = Convert.ToInt32(myReader[0].ToString());
                }
                
                mySql.Close();
                
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void RegisterUserWarns(long user_id, long chat_id, int value, DateTime dead_line)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();
                
                var query_ins_warn = $"INSERT INTO warn_users (id, user_id, chat_id, warns, dead_line) VALUES (0, '{user_id}', '{chat_id}', '{value}', '{dead_line}')";
                myCmd = new MySqlCommand(query_ins_warn, mySql);
                myCmd.ExecuteNonQuery();
                
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetDeadLine(long user_id, long chat_id)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var getDeadLine = $"SELECT dead_line FROM warn_users WHERE user_id = '{user_id}' AND chat_id = '{chat_id}'";
                myCmd = new MySqlCommand(getDeadLine, mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false){
                    message = "not found";
                }
                else
                {
                    deadline = DateTime.Parse(myReader[0].ToString());
                    message = "has deadline";
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }

        }
        public static void UpdateMaxWarns(long chat_id, int value, string punishment)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();
                
                var query_upd_warn = $"UPDATE max_warns SET max_warns = '{value}', punishment = '{punishment}' WHERE chat_id = '{chat_id}'";
                myCmd = new MySqlCommand(query_upd_warn, mySql);
                myCmd.ExecuteNonQuery();
                
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteWarnUser(long user_id, long chat_id)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd;

                mySql = new MySqlConnection(conn);
                
                mySql.Open();
                var query = $"DELETE FROM warn_users WHERE user_id = {user_id} AND chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
            
        }
        public static void UpdateUserWarns(long user_id, long chat_id, int value, DateTime dateNow, DateTime dead_line)
        {
            try
            {
                MySqlConnection mySql;
                MySqlCommand myCmd_sel, myCmd_upd;
                MySqlDataReader myReader;
                
                mySql = new MySqlConnection(conn);

                mySql.Open();

                var query_get_warn = $"SELECT * FROM warn_users WHERE user_id = '{user_id}' AND chat_id = '{chat_id}'";
                myCmd_sel = new MySqlCommand(query_get_warn, mySql);
                myReader = myCmd_sel.ExecuteReader();

                if (myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    deadline = DateTime.Parse(myReader[4].ToString());
                    if (deadline <= dateNow)
                    {
                        value = 1;
                        DeleteWarnUser(user_id, chat_id);
                        message = "deadline lost";
                    }
                    else if (deadline != dead_line)
                    {
                        deadline = dead_line;
                        message = "deadline updated";
                    }
                    else
                    {
                        message = "deadline update error";
                    }
                }

                mySql.Close();
                
                mySql.Open();
                
                var query_upd_warn = $"UPDATE warn_users SET warns = '{value}', dead_line = '{deadline}' WHERE user_id = '{user_id}' AND chat_id = '{chat_id}'";
                myCmd_upd = new MySqlCommand(query_upd_warn, mySql);
                myCmd_upd.ExecuteNonQuery();
                
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetReasonOfWarns(long user_id, long chat_id)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd, myCmd_quant;
                MySqlDataReader myReader, myReader_quant;

                mySql.Open();
                int quant_reasons = 0;
                var get_quant_reasons = $"SELECT COUNT(*) FROM reasons_users_warns WHERE user_id = '{user_id}' AND chat_id = '{chat_id}'";
                myCmd_quant = new MySqlCommand(get_quant_reasons, mySql);
                myReader_quant = myCmd_quant.ExecuteReader();
                if(myReader_quant.Read() != false)
                {
                    quant_reasons = Convert.ToInt32(myReader_quant[0].ToString()) - 1;
                    mySql.Close();

                    mySql.Open();
                    var get_reasons = $"SELECT reason, date_stamp FROM reasons_users_warns WHERE user_id = '{user_id}' AND chat_id = '{chat_id}'";
                    myCmd = new MySqlCommand(get_reasons, mySql);
                    myReader = myCmd.ExecuteReader();
                    if(myReader.Read() == false)
                    {
                        message = "not found";
                    }
                    else
                    {
                        mySql.Close();

                        mySql.Open();
                        myCmd = new MySqlCommand(get_reasons, mySql);
                        myReader = myCmd.ExecuteReader();
                        reasons.Clear();
                        time_stamp.Clear();
                        while (myReader.Read())
                        {
                            reasons.Add(myReader[0].ToString());
                            time_stamp.Add(Convert.ToDateTime(myReader[1].ToString()));
                            message = "has reasons";
                        }
                        mySql.Close();

                    }
                }
                else
                {
                    message = "not found";
                    return;
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateReasonOfWarns(long user_id, long chat_id, string reason, DateTime stamp)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var create = $"INSERT INTO reasons_users_warns(id, chat_id, user_id, reason, date_stamp) VALUES(0, {chat_id}, {user_id}, '{reason}', '{stamp}')";
                myCmd = new MySqlCommand(create, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteReasonsOfWarns(long user_id, long chat_id)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var delReasons = $"DELETE FROM reasons_users_warns WHERE chat_id = '{chat_id}' AND user_id = '{user_id}'";
                myCmd = new MySqlCommand(delReasons, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetNoBadLyrics(long chat_id)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var get_reasons = $"SELECT * FROM no_badlyrics WHERE chat_id = '{chat_id}'";
                myCmd = new MySqlCommand(get_reasons, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "has settings";
                    isActive = myReader[2].ToString();
                }
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void RegisterNoBadLyrics(long chat_id, string isActive)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO no_badlyrics(id, chat_id, is_active) VALUES(0, '{chat_id}', '{isActive}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void SetNewParametrBL(long chat_id, string isActive)
        {
            try
            {
                MySqlConnection mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE no_badlyrics set is_active = '{isActive}' WHERE chat_id = '{chat_id}'";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateAntiSpam(long user_id, long chat_id, int count)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                mySql.Open();
                var query = $"INSERT INTO antispam(id, chat_id, user_id, count) VALUES(0, {chat_id}, {user_id}, {count})";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetAntiSpam(long user_id, long chat_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT * FROM antispam WHERE user_id = {user_id} AND chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "count found";
                    countMess = Convert.ToInt32(myReader[3].ToString());
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateAntiSpam(long chat_id, long user_id, int count)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                GetAntiSpam(user_id, chat_id);
                var newCount = count + countMess;
                var query = $"UPDATE antispam SET count = {newCount} WHERE user_id = {user_id} AND chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
            
        }
        public static void DeleteAntiSpam(long chat_id, long user_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM antispam WHERE user_id = {user_id} AND chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateSpamConfig(long chat_id, int max_mess, string is_active)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO antispam_conf(id, chat_id, max_messages, is_active) VALUES(0, '{chat_id}', '{max_mess}', '{is_active}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetSpamConfig(long chat_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT * FROM antispam_conf WHERE chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "as found";
                    maxMess = Convert.ToInt32(myReader[2].ToString());
                    as_active = myReader[3].ToString();
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateSpamConfig(long chat_id, int newMax, string asActive)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                mySql.Open();
                var query = $"UPDATE antispam_conf SET max_messages = '{newMax}', is_active = '{asActive}' WHERE chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetChatRules(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT rule FROM chat_rules WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false)
                {
                    message = "not setted";
                }
                else
                {
                    message = "setted rules";
                    rule = myReader[0].ToString();
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not setted";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateChatRules(long chatId, string inpRule)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO chat_rules(id, chat_id, rule) VALUES(0, '{chatId}', '{inpRule}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateChatRules(long chatId, string inpRule)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE chat_rules SET rule = '{inpRule}' WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteChatRules(long chatId)
        {
            try
            {
            var mySql = new MySqlConnection(conn);
            MySqlCommand myCmd;

            mySql.Open();
            var query = $"DELETE FROM chat_rules WHERE chat_id = {chatId}";
            myCmd = new MySqlCommand(query, mySql);
            myCmd.ExecuteNonQuery();
            mySql.Close();
            return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetSubData(long userId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT next_response FROM weather_subs WHERE user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "not sub";
                }
                else
                {
                    message = "is sub";
                }
                mySql.Close();
            }
            catch (MySqlException exc)
            {
                message = "is sub";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateSubData(long userId, string city, DateTime next)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO weather_subs(id, user_id, city, next_response) VALUES(0, {userId}, '{city}', '{next}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateSubData(long userId, string city, DateTime next)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE weather_subs SET city = '{city}', next_response = '{next}' WHERE user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteSubData(long userId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM weather_subs WHERE user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetSocialData(long userId)
        {
            try
            {
            var mySql = new MySqlConnection(conn);
            MySqlCommand myCmd;
            MySqlDataReader myReader;

            mySql.Open();
            var query = $"SELECT rating FROM social_rating WHERE user_id = {userId}";
            myCmd = new MySqlCommand(query, mySql);
            myReader = myCmd.ExecuteReader();
            if(myReader.Read() == false)
            {
                message = "not rated";
            }
            else
            {
                message = "rated";
                user_rating = Convert.ToInt64(myReader[0].ToString());
            }
            mySql.Close();
            return;
            }
            catch (MySqlException exc)
            {
                message = "not rated";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateSocialData(long user_id, long rating)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO social_rating(id, user_id, rating) VALUES(0, {user_id}, '{rating}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateSocialData(long userId, long rating)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE social_rating SET rating = '{rating}' WHERE user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void GetControlSocData(long userId, long target_user)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT * FROM social_control WHERE user_id = '{userId}' AND target_id = '{target_user}'";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "don't rate";
                }
                else
                {
                    message = "is rate";
                    soc_control = DateTime.Parse(myReader[4].ToString());
                    count_control = Convert.ToInt32(myReader[3]);
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "don't rate";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateControlSocData(long user_id, long target_id, int counter, DateTime deadLine)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO social_control (id, user_id, target_id, count, dl) VALUES(0, {user_id}, {target_id}, {counter}, '{deadLine}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateControlSocData(long user_id, long target_id, int counter, DateTime deadLine)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE social_control SET count = {counter}, dl = '{deadLine}' WHERE user_id = {user_id} AND target_id = {target_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteControlSocData(long user_id, long target_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM social_control WHERE user_id = {user_id} AND target_id = {target_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }

        }
        public static void GetNightMode(long chat_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT nightmode, statemode FROM nightmode WHERE chat_id = {chat_id}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() == false)
                {
                    message = "not found";
                }
                else
                {
                    message = "have nightmode";
                    nightmode = DateTime.Parse(myReader[0].ToString());
                    statemode = DateTime.Parse(myReader[1].ToString());
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateNightMode(long chatId, string nightmode, string statemode)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO nightmode (id, chat_id, nightmode, statemode, status) VALUES(0, {chatId}, '{nightmode}', '{statemode}', 'statemode')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateNightMode(long chatId, string nightmode, string statemode)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE nightmode SET nightmode = '{nightmode}', statemode = '{statemode}' WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateStatusNight(long chatId, string status)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE nightmode SET status = '{status}' WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteNightMode(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM nightmode WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteOneWarn(long userId, long chat_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                CheckUserWarns(userId, chat_id);
                if (message != "not found")
                {
                    mySql.Open();
                    var query = $"SELECT COUNT(*) FROM reasons_users_warns WHERE user_id = {userId} AND chat_id = {chat_id}";
                    myCmd = new MySqlCommand(query, mySql);
                    myReader = myCmd.ExecuteReader();
                    myReader.Read();
                    var count = Convert.ToInt32(myReader[0].ToString());
                    if (count == 1)
                    {
                        DeleteReasonsOfWarns(userId, chat_id);
                        DeleteWarnUser(userId, chat_id);
                        message = "user warns is 1";
                        mySql.Close();
                    }
                    else
                    {
                        var reason = "";
                        var timestamp = "";

                        mySql.Close();

                        mySql.Open();
                        var query1 = $"SELECT reason, date_stamp FROM reasons_users_warns WHERE user_id = {userId} AND chat_id = {chat_id}";
                        myCmd = new MySqlCommand(query1, mySql);
                        myReader = myCmd.ExecuteReader();
                        if(myReader.Read() != false)
                        {
                            reason = myReader[0].ToString();
                            timestamp = myReader[1].ToString();
                            mySql.Close();

                            GetDeadLine(userId, chat_id);

                            DateTime dl = deadline;

                            mySql.Open();
                            var del = $"DELETE FROM reasons_users_warns WHERE chat_id = '{chat_id}' AND user_id = '{userId}' AND date_stamp = '{timestamp}'";
                            myCmd = new MySqlCommand(del, mySql);
                            myCmd.ExecuteNonQuery();
                            mySql.Close();
                            count = count - 1;
                            UpdateUserWarns(userId, chat_id, count, DateTime.Now, deadline);
                            message = "one warn deleted";
                        }
                    }
                }
                else
                {
                    message = "not found";
                }
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }


        public static void GetCapchaSetting(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT active, minutes FROM capcha_chats WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if(myReader.Read() != false)
                {
                    message = "have capcha";
                    capcha_status = myReader[0].ToString();
                    minutes = Convert.ToInt32(myReader[1].ToString());
                }
                else
                {
                    message = "not found";
                }
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                message = "not found";
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void CreateCapchaSetting(long chatId, string active, int minutes)
        {
            try
            {
            var mySql = new MySqlConnection(conn);
            MySqlCommand myCmd;

            mySql.Open();
            var query = $"INSERT INTO capcha_chats (id, chat_id, active, minutes) VALUES(0, {chatId}, '{active}', {minutes})";
            myCmd = new MySqlCommand(query, mySql);
            myCmd.ExecuteNonQuery();
            mySql.Close();
            return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateCapchaSetting(long chatId, string active, int minute)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE capcha_chats SET active = '{active}', minutes = {minute} WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteCapchaSetting(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM capcha_chats WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }

        public static object[] GetCapchaByUser(long userId, long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;
                

                mySql.Open();
                var query = $"SELECT * FROM capcha_temp WHERE user_id = {userId} AND chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                if (myReader.Read() == false)
                {
                    object[] massive = null;
                    return massive;
                }
                else
                {
                    object[] massive = { myReader[1].ToString(), myReader[2].ToString(), myReader[3].ToString(), myReader[4].ToString(), myReader[5].ToString() };
                    return massive;
                }
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                object[] massive = { null, null };
                return massive;
            }

        }
        public static void TempingCapchaUser(long userId, long chatId, int messageId, DateTime end_in, int attemps)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO capcha_temp (id, chat_id, user_id, messageId, end_in, attemps) VALUES (0, '{chatId}', '{userId}', '{messageId}', '{end_in}', '{attemps}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
            
        }
        public static void UpdateAttempsUser(long userId, long chatId, int attemps)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE capcha_temp SET attemps = {attemps} WHERE chat_id = {chatId} AND user_id = {userId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void DeleteCapchaUser(long chat_id, long user_id)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM capcha_temp WHERE chat_id = {chat_id} AND user_id = {user_id}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
            
        }


        public static bool GetFBANews(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT is_get FROM userbot WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                bool status = false;
                if(myReader.Read() == false)
                {
                    status = false;
                }
                else
                {
                    status = true;
                }
                mySql.Close();
                return status;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return false;
            }
        }
        public static void CreateFBANews(long chatId, string is_get)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO userbot (id, chat_id, is_get) VALUES(0, {chatId}, '{is_get}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }
        public static void UpdateFBANews(long chatId, string is_get)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE userbot SET is_get = '{is_get}' WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
            }
        }

        public static string GetNotifComment(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;
                MySqlDataReader myReader;

                mySql.Open();
                var query = $"SELECT message FROM notif_comment WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myReader = myCmd.ExecuteReader();
                string data = null;
                if (myReader.Read() == false)
                {
                    data = null;
                }
                else
                {
                    data = myReader[0].ToString();
                }
                mySql.Close();
                return data;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return null;
            }
        }
        public static void CreateNotifComment(long chatId, string message)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"INSERT INTO notif_comment(id, chat_id, message) VALUES(0, {chatId}, '{message}')";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return;
            }
        }
        public static void UpdateNotifComment(long chatId, string message)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"UPDATE notif_comment SET message = '{message}' WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return;
            }
        }
        public static void DeleteNotifComment(long chatId)
        {
            try
            {
                var mySql = new MySqlConnection(conn);
                MySqlCommand myCmd;

                mySql.Open();
                var query = $"DELETE FROM notif_comment WHERE chat_id = {chatId}";
                myCmd = new MySqlCommand(query, mySql);
                myCmd.ExecuteNonQuery();
                mySql.Close();
                return;
            }
            catch (MySqlException exc)
            {
                Console.WriteLine($"Connection was aborted: {exc.Message}");
                return;
            }
        }
    }
}
