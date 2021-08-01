﻿using Dapper.QX;
using System;

namespace ChangeLog.Services.Queries
{
    public class EventsComparedResult
    {
        public string Type { get; set; }
        public DateTime PriorTimestamp { get; set; }
        public string PriorEventType { get; set; }
        public string PriorScript { get; set; }
        public string PriorXml { get; set; }
        public string PriorText { get; set; }
        public int PriorVersion { get; set; }
        public DateTime CurrentTimestamp { get; set; }
        public string CurrentEventType { get; set; }
        public string CurrentScript { get; set; }
        public string CurrentXml { get; set; }
        public string CurrentText { get; set; }
        public int CurrentVersion { get; set; }
        public int PriorEventId { get; set; }
        public int CurrentEventId { get; set; }

        // tables need to get their content from the Table.Text column, not script
        public string PriorContent => (Type.Equals("TABLE")) ? PriorText : PriorScript;
        public string CurrentContent => (Type.Equals("TABLE")) ? CurrentText : CurrentScript;
    }

    public class EventsCompared : Query<EventsComparedResult>
    {
        public EventsCompared() : base(
            @"WITH [current] AS (
                SELECT TOP (20)
                    [obj].[Type], [ev].[Timestamp], [ev].[EventType], [ev].[Schema], [ev].[ObjectName], [ev].[Script], [t].[Xml], [t].[Text], [ev].[Version], [ev].[Id]
                FROM
                    [changelog].[Event] [ev]
                    LEFT JOIN [changelog].[Table] [t] ON [ev].[Id]=[t].[EventId]
                    INNER JOIN [changelog].[Object] [obj] ON [ev].[Schema]=[obj].[Schema] AND [ev].[ObjectName]=[obj].[Name]
                WHERE
                    [obj].[Id]=@objectId
                ORDER BY 
                    [ev].[Timestamp] DESC
            ) SELECT
                [current].[Type], [prior].[Timestamp] AS [PriorTimestamp], [prior].[EventType] AS [PriorEventType], [prior].[Script] AS [PriorScript], 
                [t].[Xml] AS [PriorXml], [t].[Text] AS [PriorText], [prior].[Version] AS [PriorVersion],
                [current].[Timestamp] AS [CurrentTimestamp], [current].[EventType] AS [CurrentEventType], [current].[Script] AS [CurrentScript], 
                [t].[Xml] AS [CurrentXml], [t].[Text] AS [CurrentText], [current].[Version] AS [CurrentVersion], 
                [prior].[Id] AS [PriorEventId], [current].[Id] AS [CurrentEventId]
            FROM
                [changelog].[Event] [prior]
                LEFT JOIN [changelog].[Table] [t] ON [prior].[Id]=[t].[EventId]
                INNER JOIN [changelog].[Object] [obj] ON [prior].[Schema]=[obj].[Schema] AND [prior].[ObjectName]=[obj].[Name]
                INNER JOIN [current] ON [prior].[Version] + 1 = [current].[Version]
            WHERE
                [obj].[Id]=@objectId")
        {
        }

        public int ObjectId { get; set; }
    }
}
