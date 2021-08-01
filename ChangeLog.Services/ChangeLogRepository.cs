using ChangeLog.Services.Queries;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ChangeLog.Services
{
    public class ChangeLogRepository
    {
        public async Task<IEnumerable<RecentObjectsResult>> GetRecentObjectsAsync(IDbConnection connection) =>
            await new RecentObjects().ExecuteAsync(connection);
        
    }
}
