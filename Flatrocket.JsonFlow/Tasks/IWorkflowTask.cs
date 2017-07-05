using Flatrocket.JsonFlow.Models;
using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Tasks
{
    public interface IWorkflowTask
    {
        List<WorkflowTaskData> Parameters { get; set; }

        List<WorkflowTaskData> Outputs { get; set; }

        void Execute(params List<WorkflowTaskData>[] inputs);
    }
}