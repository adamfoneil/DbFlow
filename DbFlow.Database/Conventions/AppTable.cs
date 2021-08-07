using AO.Models;

namespace DbFlow.Models.Conventions
{
    [Schema("app")]
    public abstract class AppTable
    {
        public long Id { get; set; }
    }
}
