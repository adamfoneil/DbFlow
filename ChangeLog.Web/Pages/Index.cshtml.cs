using ChangeLog.Services;
using ChangeLog.Services.Queries;
using ChangeLog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChangeLog.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TableDefRenderer _renderer;
        private readonly ConnectionProvider _connector;
        private readonly ChangeLogRepository _repository;

        public IndexModel(
            ILogger<IndexModel> logger, 
            TableDefRenderer renderer, ConnectionProvider connector, ChangeLogRepository repository)
        {
            _logger = logger;
            _renderer = renderer;
            _connector = connector;
            _repository = repository;
        }

        [BindProperty(SupportsGet = true)]
        public string Connection { get; set; }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public SelectList ConnectionSelect { get; set; }

        public IEnumerable<RecentObjectsResult> RecentObjects { get; set; }

        public async Task OnGetAsync()
        {
            ConnectionSelect = new SelectList(_connector.ConnectionNames, Connection);

            if (!string.IsNullOrEmpty(Connection))
            {
                using (var cn = _connector.GetConnection(Connection))
                {
                    RecentObjects = await _repository.GetRecentObjectsAsync(cn);
                }                
            }
        }
    }
}
