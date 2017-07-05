using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Models
{
    public class WorkflowGraphItem
    {
        public int TaskId { get; set; }

        public List<int> Inputs { get; set; }

        public List<WorkflowGraphItemReference> Parents { get; set; }

        public bool IsEntryPoint { get; set; }
    }
}