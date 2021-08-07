using Dapper.QX;

namespace DbFlow.Services.Queries
{
    public class EventViewResult
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public bool IsTable { get; set; }
        public string Content { get; set; }
        public string Xml { get; set; }
    }

    public class EventView : Query<EventViewResult>
    {
        public EventView() : base(
            @"SELECT 
                [ev].[Id], [ev].[Version],
                CASE
                    WHEN [t].[EventId] IS NOT NULL THEN [t].[Text]
                    ELSE [ev].[Script]
                END AS [Content],
                CASE
                    WHEN [obj].[Type] = 'TABLE' THEN 1
                    ELSE 0
                END AS [IsTable],
                [t].[Xml]
            FROM 
                [changelog].[Event] [ev]
                LEFT JOIN [changelog].[Table] [t] ON [ev].[Id]=[t].[EventId]
                INNER JOIN [changelog].[Object] [obj] ON 
                    [ev].[Schema]=[obj].[Schema] AND
                    [ev].[ObjectName]=[obj].[Name]
            WHERE
                [ev].[Id]=@eventId")
        {
        }

        public int EventId { get; set; }
    }
}
