using System.Data.SqlClient;

namespace DataAccess.DAO
{
    public class SqlOperation
    {
        public string ProcedureName { get; set; }
        public List<SqlParameter> parameters;

        public SqlOperation()
        {
            parameters = new List<SqlParameter>();
        }

        public void AddVarcharParameter(string parameterName, string value)
        {
            parameters.Add(new SqlParameter($"@{parameterName}", value));
        }

        public void AddIntParameter(string parameterName, int value)
        {
            parameters.Add(new SqlParameter($"@{parameterName}", value));
        }
    }
}
