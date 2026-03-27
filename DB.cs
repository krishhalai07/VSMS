using Microsoft.Data.SqlClient;

namespace VSMS
{
    public class DB
    {
        private readonly string _conn;
        public DB(IConfiguration config) => _conn = config.GetConnectionString("DefaultConnection")!;
        public SqlConnection Open() { var c = new SqlConnection(_conn); c.Open(); return c; }
    }
}
