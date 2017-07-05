using Flatrocket.JsonFlow.Tasks;

namespace Flatrocket.JsonFlow.Models
{
    public class WorkflowTaskInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IWorkflowTask Instance { get; set; }
    }
}