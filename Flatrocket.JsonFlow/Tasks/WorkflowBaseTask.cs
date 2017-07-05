using Flatrocket.JsonFlow.Models;
using System.Collections.Generic;
using System;

namespace Flatrocket.JsonFlow.Tasks
{
    public abstract class WorkflowBaseTask : IWorkflowTask
    {
        public List<WorkflowTaskData> Parameters { get; set; }

        public List<WorkflowTaskData> Outputs { get; set; }

        public WorkflowBaseTask()
        {
            Parameters = new List<WorkflowTaskData> { };
            Outputs = new List<WorkflowTaskData> { };
        }

        public abstract void Execute(params List<WorkflowTaskData>[] inputs);
    }
}