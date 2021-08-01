using ChangeLog.Services;
using ChangeLog.Services.Queries;
using ChangeLog.Web.Services;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
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
        private readonly ConnectionProvider _connector;
        private readonly ChangeLogRepository _repository;
        private readonly ISideBySideDiffBuilder _diffBuilder;

        public IndexModel(
            ILogger<IndexModel> logger,
            ConnectionProvider connector, ChangeLogRepository repository, ISideBySideDiffBuilder diffBuilder)
        {
            _logger = logger;
            _connector = connector;
            _repository = repository;
            _diffBuilder = diffBuilder;
        }

        [BindProperty(SupportsGet = true)]
        public string Connection { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ObjectId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PriorEventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentEventId { get; set; }

        public SelectList ConnectionSelect { get; set; }

        public IEnumerable<RecentObjectsResult> RecentObjects { get; set; }

        public IEnumerable<RecentChangesResult> RecentChanges { get; set; }

        public SideBySideDiffModel DiffModel { get; set; }

        public async Task OnGetAsync()
        {
            ConnectionSelect = new SelectList(_connector.ConnectionNames, Connection);

            if (!string.IsNullOrEmpty(Connection))
            {
                using (var cn = _connector.GetConnection(Connection))
                {
                    RecentObjects = await _repository.GetRecentObjectsAsync(cn);

                    if (ObjectId != default)
                    {
                        RecentChanges = await _repository.GetEventComparisonAsync(cn, ObjectId);
                    }

                    if (PriorEventId != default && CurrentEventId != default)
                    {
                        var priorVersion = await _repository.GetEventViewAsync(cn, PriorEventId);
                        var currentVersion = await _repository.GetEventViewAsync(cn, CurrentEventId);                        
                        DiffModel = _diffBuilder.BuildDiffModel(priorVersion.Content, currentVersion.Content);
                    }
                }
            }
        }
    }
}
