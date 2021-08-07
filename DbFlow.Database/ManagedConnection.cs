using DbFlow.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace DbFlow.Database
{
    public class ManagedConnection : BaseTable
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string ConnectionString { get; set; }
    }
}
