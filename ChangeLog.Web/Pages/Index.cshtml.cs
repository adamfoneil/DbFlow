using ChangeLog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace ChangeLog.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TableDefRenderer _renderer;
        private readonly ConnectionProvider _connector;     

        public IndexModel(ILogger<IndexModel> logger, TableDefRenderer renderer, ConnectionProvider connectionProvider)
        {
            _logger = logger;
            _renderer = renderer;
            _connector = connectionProvider;
        }

        [BindProperty(SupportsGet = true)]
        public string ConnectionName { get; set; }

        public SelectList ConnectionSelect { get; set; }

        public void OnGet()
        {
            ConnectionSelect = new SelectList(_connector.ConnectionNames, ConnectionName);
        }
    }
}
