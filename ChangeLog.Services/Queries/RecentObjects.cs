using Dapper.QX;
using System;

namespace ChangeLog.Services.Queries
{
    public class RecentObjectsResult
    {
        public int Id { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Version { get; set; }
        public DateTime? LastChange { get; set; }
        public int? LastEventId { get; set; }

        public string DisplayName => $"{Schema}.{Name}";
    }

    public class RecentObjects : Query<RecentObjectsResult>
    {
        public RecentObjects() : base(
            @"WITH [recent_events] AS (
                SELECT TOP (100) [Schema], [ObjectName], [Timestamp], [Id]
                FROM [changelog].[Event]
                ORDER BY [Timestamp] DESC
            ), [recent_objects] AS (
                SELECT [Schema], [ObjectName], MAX([Timestamp]) AS [LastChange], MAX([Id]) AS [LastEventId]
                FROM [recent_events]
                GROUP BY [Schema], [ObjectName]
            ) SELECT [obj].*, [ro].[LastChange], [ro].[LastEventId]
            FROM [changelog].[Object] [obj]
            INNER JOIN [recent_objects] [ro] ON 
                [obj].[Schema]=[ro].[Schema] AND 
                [obj].[Name]=[ro].[ObjectName]")
        {
        }
    }
}
