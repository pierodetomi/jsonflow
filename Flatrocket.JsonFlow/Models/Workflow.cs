using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Models
{
    public class Workflow
    {
        public List<WorkflowTaskInfo> Tasks { get; set; }

        public List<WorkflowGraphItem> Graph { get; set; }
    }
}