using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using EncryptedType;

namespace MSSQLServer
{
    public class MSSQLServer : IKeyServer
    {
        private const string _NAME = "EncryptedType.MSSQL";

        public string ID { get { return _NAME; } }

        private string _connStr;

        public MSSQLServer()
        {
            _connStr = System.Configuration.ConfigurationManager.ConnectionStrings[_NAME].ConnectionString;
        }

        public IList<string> Keys
        {
            get
            {
                try
                {
                    var retVal  = new List<string>();
                    var cn = new SqlConnection(_connStr);
                    if(cn.State != System.Data.ConnectionState.Open)
                    {
                        cn.Open();
                    }
                    using (var rdr = new SqlCommand("EXEC [dbo].[ListItems]", cn).ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                retVal.Add(rdr.GetString(0));
                            }
                        }
                    }
                    return retVal;
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }

        public IDictionary<string, string> Map => throw new NotImplementedException();

        public string GetKey(string KeyName)
        {
            if(string.IsNullOrEmpty(KeyName))
            {
                return null;
            }
            SqlConnection conn = new SqlConnection(_connStr);
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                var cmd = new SqlCommand("[dbo].[GetItem]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", KeyName);
                var rdr = cmd.ExecuteReader();
                if (rdr.HasRows && rdr.Read())
                {
                    return rdr.GetString(1);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
    }
}
