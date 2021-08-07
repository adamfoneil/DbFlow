using AO.Models;
using DbFlow.Models.Conventions;

namespace DbFlow.Database
{
    /// <summary>
    /// a typical migration flow (for example from QA -> Production)
    /// </summary>
    public class MigrationRoute : BaseTable
    {
        /// <summary>
        /// migrate from this connection
        /// </summary>
        [References(typeof(ManagedConnection))]
        public int SourceConnectionId { get; set; }

        /// <summary>
        /// to these target connections
        /// </summary>
        [References(typeof(ConnectionGroup))]
        public int DestinationGroupId { get; set; }
    }
}
