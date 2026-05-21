using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryClub.Api.Data;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
