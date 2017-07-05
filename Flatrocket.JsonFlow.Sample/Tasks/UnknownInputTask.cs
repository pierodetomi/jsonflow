using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Sample.Tasks
{
    public class UnknownInputTask : WorkflowBaseTask
    {
        public override void Execute(params List<WorkflowTaskData>[] inputs)
        {
            Console.WriteLine("Sorry, I didn't understand what you wrote...");
        }
    }
}