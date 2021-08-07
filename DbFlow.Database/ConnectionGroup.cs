using DbFlow.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace DbFlow.Database
{
    /// <summary>
    /// a set of connections you typically migrate to (for example "production")
    /// </summary>
    public class ConnectionGroup : BaseTable
    {
        [Key]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
