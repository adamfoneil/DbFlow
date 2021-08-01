using ChangeLog.Services.Queries;
using Dapper.CX.SqlServer;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ChangeLog.Services
{
    public class ChangeLogRepository
    {
        private readonly TableDefRenderer _renderer;

        public ChangeLogRepository()
        {
            _renderer = new TableDefRenderer();
        }

        public async Task<IEnumerable<RecentObjectsResult>> GetRecentObjectsAsync(IDbConnection connection) =>
            await new RecentObjects().ExecuteAsync(connection);

        public async Task<IEnumerable<EventsComparedResult>> GetEventComparisonAsync(IDbConnection connection, int objectId)
        {
            var results = await new EventsCompared() { ObjectId = objectId }.ExecuteAsync(connection);

            foreach (var item in results)
            {
                if (string.IsNullOrEmpty(item.PriorText))
                {
                    item.PriorText = await RenderTableTextAsync(connection, item.PriorEventId, item.PriorXml);
                }

                if (string.IsNullOrEmpty(item.CurrentText))
                {
                    item.CurrentText = await RenderTableTextAsync(connection, item.CurrentEventId, item.CurrentXml);
                }
            }

            return results;
        }

        public async Task<EventViewResult> GetEventViewAsync(IDbConnection connection, int id)
        {
            var result = await new EventView() { EventId = id }.ExecuteSingleOrDefaultAsync(connection);

            if (result.IsTable && string.IsNullOrEmpty(result.Content))
            {
                result.Content = await RenderTableTextAsync(connection, id, result.Xml);
            }

            return result;
        }

        private async Task<string> RenderTableTextAsync(IDbConnection connection, int eventId, string xml)
        {
            var text = _renderer.AsText(xml);

            await new SqlServerCmd("changelog.Table", "EventId")
            {
                ["Text"] = text
            }.UpdateAsync(connection, eventId);

            return text;
        }
    }
}
