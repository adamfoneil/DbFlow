using ChangeLog.Services.Queries;

namespace ChangeLog.Web.ViewModels
{
    public class ObjectVersion
    {
        public EventViewResult EventView { get; set; }
        public DiffPlex.DiffBuilder.Model.DiffPaneModel PaneModel { get; set; }
    }
}
