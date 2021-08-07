namespace DbFlow.Web.Models
{
    public class MigrationRoute
    {
        /// <summary>
        /// connection name to migrate from
        /// </summary>
        public string SourceConnection { get; set; }

        /// <summary>
        /// connection names to migrate to
        /// </summary>
        public string[] DestinationConnections { get; set; }
    }
}
