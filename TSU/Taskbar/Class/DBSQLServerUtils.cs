using System.Data.SqlClient;

namespace TSU
{
    class DBSQLServerUtils
    {
        public static SqlConnection
           GetDBConnection(string datasource, string database, string username, string password, string security)
        {
            string connString = @"Data Source=" + datasource + ";Integrated Security=" + security + ";Initial Catalog="
                + database + ";Persist Security Info=True;User ID=" + username + ";Password=" + password;

            SqlConnection conn = new SqlConnection(connString);

            return conn;
        }
    }
}
