using DbFlow.Services.Queries;

namespace DbFlow.Web.ViewModels
{
    public class ObjectVersion
    {
        public EventViewResult EventView { get; set; }
        public DiffPlex.DiffBuilder.Model.DiffPaneModel PaneModel { get; set; }
    }
}
