using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

public class SqlScript : MonoBehaviour
{
    static NpgsqlConnection conn;
    static string server_db = "Server=127.0.0.1;User Id=postgres;Password=admin;Database=RoadMasters;";

    public static short getVersion ()
    {
        short result = 0;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"Version\" FROM \"GameVersion\"";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt16(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static short SignUser(string username, string pass)
    {
        short result = 0;
        pass = MD5Hash(pass);
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"registerUser\" (\'" + username + "\', \'" + pass + "\')";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt16(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static short LogUser(string username, string pass)
    {
        short result = 0;
        pass = MD5Hash(pass);
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"logUser\" (\'" + username + "\', \'" + pass + "\')";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt16(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static string MD5Hash(string input)
    {
        StringBuilder hash = new StringBuilder();
        MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
        byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

        for (int i = 0; i < bytes.Length; i++)
        {
            hash.Append(bytes[i].ToString("x2"));
        }
        return hash.ToString();
    }

    public static string[] getMyMaps (string user)
    {
        List<string> names = new List<string>();
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"Map\".\"Index\", \"Map\".\"Name\"" +
                           "FROM \"Map\" WHERE \"Creator\" = \'" + user + "\' ORDER BY \"Map\".\"Index\" DESC";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                names.Add(reader.GetString(1) + '#' + reader.GetInt32(0).ToString());
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); names.Add("0"); conn = null; }
        return names.ToArray();
    }

    public static string[] getMaps()
    {
        List<string> names = new List<string>();
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"Map\".\"Index\", \"Map\".\"Name\"" +
                           "FROM \"Map\" ORDER BY \"Map\".\"Index\" DESC";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                names.Add(reader.GetString(1) + '#' + reader.GetInt32(0).ToString());
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); names.Add("0"); conn = null; }
        return names.ToArray();
    }

    public static string[] getTimes()
    {
        List<string> times = new List<string>();
        int i = MenuManager.MapIndex;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"BestTimes\".\"User\", \"BestTimes\".\"Time\"" +
                           "FROM \"BestTimes\" WHERE \"MapIndex\" = \'" + i.ToString() + "\' ORDER BY \"BestTimes\".\"Time\" LIMIT 200";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                times.Add(reader.GetString(0));
                times.Add(reader.GetDouble(1).ToString());
            }
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); times.Add("0"); conn = null; }
        return times.ToArray();
    }

    public static short publishMap(string name, Vector3[] points, float[] settingVals, int[] SelectState, float[] speedState, Vector3[] obsPos)
    {
        short result = 0;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            int index = 999999;
            //Create Map and Get its Index
            string query = "INSERT INTO \"Map\" (\"Date\", \"Name\", \"Creator\") VALUES" +
                    "(CURRENT_TIMESTAMP, \'" + name + "\', \'" + MenuManager.UserName + "\');" +
                    "SELECT \"Index\" FROM \"Map\" ORDER BY \"Index\" DESC LIMIT 1";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                index = reader.GetInt32(0);
            reader.Close();

            query = "";
            //RigidBody
            if (settingVals[0] != 1403.0f)
                query += "INSERT INTO \"RigidBody\" (\"MapIndex\", \"Mass\") VALUES" +
                "(" + index.ToString() + ", " + settingVals[0] + ");";
            //Steering
            if (settingVals[1] != 45.0f)
                query += "INSERT INTO \"Steering\" (\"MapIndex\", \"MaxSteerAngle\") VALUES" +
                "(" + index.ToString() + ", " + settingVals[1] + ");";
            //Brake
            if (settingVals[2] != 2800.0f || settingVals[3] != 1500.0f)
                query += "INSERT INTO \"Brake\" (\"MapIndex\", \"MaxBrakeTorque\", \"HandBrakeTorque\") VALUES" +
                "(" + index.ToString() + ", " + settingVals[2] + ", " + settingVals[3] + ");";
            //Engine
            if (settingVals[4] != 210.0f || settingVals[5] != 9000.0f)
                query += "INSERT INTO \"Engine\" (\"MapIndex\", \"MaxTorque\", \"MaxRpm\") VALUES" +
                "(" + index.ToString() + ", " + settingVals[4] + ", " + settingVals[5] + ");";

            for (int i = 0; i < 20; i++)
            {
                //Point
                if (points[i].x == 0.0f)
                    points[i].x = 0.0001f;
                query += "INSERT INTO \"Point\" (\"MapIndex\", \"Pos\", \"Index\", \"Key\") VALUES" +
                    "(" + index.ToString() + ", \'{" +
                    points[i].x.ToString() + ',' + points[i].y.ToString() + ',' + points[i].z.ToString() + "}\', "
                    + i.ToString() + ", \'" + index.ToString() + ':' + i.ToString() + "\');";

                if (SelectState[i] > 0)
                {
                    if (obsPos[i].x == 0.0f)
                        obsPos[i].x = 0.0001f;
                    //Static Obstacle
                    query += "INSERT INTO \"GameObject\" (\"PointKey\", \"Pos\") VALUES" +
                        "(\'" + index.ToString() + ':' + i.ToString() + "\', " + "\'{" +
                        obsPos[i].x.ToString() + ',' + obsPos[i].y.ToString() + ',' + obsPos[i].z.ToString() + "}\');";
                    if (SelectState[i] == 2)
                    {
                        //Moving Obstacle
                        query += "INSERT INTO \"MovingObject\" (\"PointKey\", \"Speed\") VALUES" +
                            "(\'" + index.ToString() + ':' + i.ToString() + "\', " + speedState[i] + ");";
                    }
                }
            }
            command.CommandText = query;
            command.ExecuteNonQuery();
            conn.Close();
            result = 1;
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static short getMapData()
    {
        short result = 0;
        int index = MenuManager.MapIndex;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"Key\", \"Index\", \"Pos\" " +
                           "FROM \"Point\" WHERE \"MapIndex\" = \'" + index.ToString() + "\' ORDER BY \"Index\" ASC";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                //Add point
                string pos = reader.GetValue(2).ToString();
                pos = pos.Substring(1, pos.Length - 2);
                MenuManager.points.Add(new Vector3(float.Parse(pos.Split(',')[0]), float.Parse(pos.Split(',')[1]), float.Parse(pos.Split(',')[2])));
            }
            reader.Close();

            for (int i = 0; i < 20; i++)
            {
                query = "SELECT \"getObject\" (\'" + index.ToString() + ':' + i.ToString() + "\')";
                command.CommandText = query;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string resset = reader.GetString(0);
                    MenuManager.selstate.Add(int.Parse(resset.Split('@')[0]));
                    MenuManager.speedstate.Add(float.Parse(resset.Split('@')[2]));
                    string pos = resset.Split('@')[1];
                    pos = pos.Substring(1, pos.Length - 2);
                    MenuManager.obsPos.Add(new Vector3(float.Parse(pos.Split(',')[0]), float.Parse(pos.Split(',')[1]), float.Parse(pos.Split(',')[2])));
                }
                reader.Close();
            }

            query = "SELECT \"Mass\" " +
               "FROM \"RigidBody\" WHERE \"MapIndex\" = \'" + index.ToString() + "\'";
            command.CommandText = query;
            reader = command.ExecuteReader();
            while (reader.Read())
                MenuManager.settingVals.Add(reader.GetFloat(0));
            if (MenuManager.settingVals.Count == 0)
                MenuManager.settingVals.Add(0.0f);
            reader.Close();

            query = "SELECT \"MaxSteerAngle\" " +
                "FROM \"Steering\" WHERE \"MapIndex\" = \'" + index.ToString() + "\'";
            command.CommandText = query;
            reader = command.ExecuteReader();
            while (reader.Read())
                MenuManager.settingVals.Add(reader.GetFloat(0));
            if (MenuManager.settingVals.Count == 1)
                MenuManager.settingVals.Add(0.0f);
            reader.Close();

            query = "SELECT \"MaxBrakeTorque\", \"HandBrakeTorque\" " +
                "FROM \"Brake\" WHERE \"MapIndex\" = \'" + index.ToString() + "\'";
            command.CommandText = query;
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                MenuManager.settingVals.Add(reader.GetFloat(0));
                MenuManager.settingVals.Add(reader.GetFloat(1));
            }
            if (MenuManager.settingVals.Count == 2)
            {
                MenuManager.settingVals.Add(0.0f);
                MenuManager.settingVals.Add(0.0f);
            }
            reader.Close();

            query = "SELECT \"MaxTorque\", \"MaxRpm\" " +
                "FROM \"Engine\" WHERE \"MapIndex\" = \'" + index.ToString() + "\'";
            command.CommandText = query;
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                MenuManager.settingVals.Add(reader.GetFloat(0));
                MenuManager.settingVals.Add(reader.GetFloat(1));
            }
            if (MenuManager.settingVals.Count == 4)
            {
                MenuManager.settingVals.Add(0.0f);
                MenuManager.settingVals.Add(0.0f);
            }
            reader.Close();

            query = "SELECT \"Time\" " +
                "FROM \"BestTimes\" WHERE \"MapIndex\" = \'" + index.ToString() + "\' AND \"User\" = \'" + MenuManager.UserName + "\'";
            command.CommandText = query;
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                MenuManager.PlayerBestTime = reader.GetDouble(0);
            }

            conn.Close();
            result = 1;
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static double getBestTime(double time)
    {
        double result = 0.0;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"setBestTime\" (" + time + ", \'" + MenuManager.UserName + "\', " + MenuManager.MapIndex + ")";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetDouble(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static short delMap()
    {
        short result = 0;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"deleteMap\" (" + MenuManager.MapIndex + ")";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt16(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }

    public static string[] getCreators()
    {
        List<string> creators = new List<string>();
        int i = MenuManager.MapIndex;
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT * FROM \"BestCreators\" ORDER BY \"MapCount\" DESC LIMIT 100";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                creators.Add(reader.GetString(0));
                creators.Add(reader.GetDouble(1).ToString());
            }
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); creators.Add("0"); conn = null; }
        return creators.ToArray();
    }

    public static short ChangeUser(string username, string pass)
    {
        short result = 0;
        pass = MD5Hash(pass);
        conn = new NpgsqlConnection(server_db);
        try
        {
            conn.Open();
            NpgsqlCommand command = conn.CreateCommand();
            string query = "SELECT \"changeUserName\" (\'" + username + "\', \'" + pass + "\', \'" + MenuManager.UserName + "\')";
            command.CommandText = query;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result = reader.GetInt16(0);
            conn.Close();
        }
        catch (System.Exception e) { Debug.LogError(e); conn = null; }
        return result;
    }
}
