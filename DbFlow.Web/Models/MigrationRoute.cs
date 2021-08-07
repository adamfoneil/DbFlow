namespace DbFlow.Web.Models
{
    public class MigrationRoute
    {
        public string Name { get; set; }

        /// <summary>
        /// connection name to migrate from
        /// </summary>
        public string SourceConnection { get; set; }

        /// <summary>
        /// connection names to migrate to
        /// </summary>
        public string[] TargetConnections { get; set; }
    }
}
