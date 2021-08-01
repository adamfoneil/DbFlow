using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ChangeLog.Web.Services
{
    public class ConnectionProvider
    {
        private readonly Dictionary<string, string> _connections;

        public ConnectionProvider(IConfiguration config)
        {
            _connections = new Dictionary<string, string>();            
            var section = config.GetSection("ConnectionStrings");
            foreach (var child in section.GetChildren()) _connections.Add(child.Key, child.Value);
        }

        public IEnumerable<string> ConnectionNames => _connections.Select(kp => kp.Key);

        public IDbConnection GetConnection(string name) => new SqlConnection(_connections[name]);
        
    }
}
