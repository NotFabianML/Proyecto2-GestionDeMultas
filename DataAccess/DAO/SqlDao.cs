using System.Data.SqlClient;
using System.Data;

namespace DataAccess.DAO
{
    public class SqlDao
    {
        private static SqlDao _instance;
        private string _connectionString = "Server=localhost;Database=Proyecto2;Trusted_Connection=True";

        private SqlDao()
        {
        }

        public static SqlDao GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SqlDao();
            }
            return _instance;
        }

        public void ExecuteStoreProcedure(SqlOperation operation)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = operation.ProcedureName;
            command.CommandType = CommandType.StoredProcedure;

            // AGREGAR LOS PARAMETROS
            foreach (var parameter in operation.parameters)
            {
                command.Parameters.Add(parameter);
            }

            // Abrimos la conexión a la BD
            command.Connection.Open();
            // Ejecutamos el comando
            command.ExecuteNonQuery();
        }

        public Dictionary<string, object> ExecuteStoredProcedureWithUniqueResult(SqlOperation operation)
        {
            var result = new Dictionary<string, object>();
            SqlConnection connection = new SqlConnection(_connectionString);

            SqlCommand command = new SqlCommand
            {
                Connection = connection,
                CommandText = operation.ProcedureName,
                CommandType = CommandType.StoredProcedure
            };

            // Agregar los parametros
            foreach (var parameter in operation.parameters)
            {
                command.Parameters.Add(parameter);
            }

            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            // Recorre las filas del resultado
            if (reader.HasRows)
            {
                // Mientras tengamos algo que leer
                while (reader.Read())
                {
                    // Recorre las columnas de cada fila
                    for (int fieldCount = 0; fieldCount < reader.FieldCount; fieldCount++)
                    {
                        result.Add(reader.GetName(fieldCount), reader.GetValue(fieldCount));
                    }
                }
            }

            return result;
        }

        public List<Dictionary<string, object>> ExecuteProcedureWithQuery(SqlOperation operation)
        {
            var result = new List<Dictionary<string, object>>();
            SqlConnection connection = new SqlConnection(_connectionString);

            SqlCommand command = new SqlCommand
            {
                Connection = connection,
                CommandText = operation.ProcedureName,
                CommandType = CommandType.StoredProcedure
            };

            // Agregar los parametros
            foreach (var parameter in operation.parameters)
            {
                command.Parameters.Add(parameter);
            }

            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            // Revisamos si tenemos filas
            if (reader.HasRows)
            {
                // Itera sobre las filas
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    // Recorre las columnas de cada fila
                    for (int fieldCount = 0; fieldCount < reader.FieldCount; fieldCount++)
                    {
                        row.Add(reader.GetName(fieldCount), reader.GetValue(fieldCount));
                    }

                    result.Add(row);
                }
            }

            return result;
        }
    }
}
