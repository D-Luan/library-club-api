using System.Data;

namespace LibraryClub.Api.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
