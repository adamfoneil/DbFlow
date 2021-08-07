using AO.Models;
using DbFlow.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace DbFlow.Database
{
    /// <summary>
    /// a specific connection within a group
    /// </summary>
    public class ConnectionGroupItem : BaseTable
    {
        [Key]
        [References(typeof(ConnectionGroup), CascadeDelete = true)]
        public int ConnectionGroupId { get; set; }

        [Key]
        [References(typeof(ManagedConnection))]
        public int ConnectionId { get; set; }
    }
}
