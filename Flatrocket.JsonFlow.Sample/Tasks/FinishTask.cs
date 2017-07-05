using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flatrocket.JsonFlow.Sample.Tasks
{
    public class FinishTask : WorkflowBaseTask
    {
        public override void Execute(params List<WorkflowTaskData>[] inputs)
        {
            Console.WriteLine("Do you want to restart again? (S/N)");
            string restart = Console.ReadLine();

            Outputs.Add(new Models.WorkflowTaskData
            {
                Name = "Restart",
                Value = (restart == "S")
            });
        }
    }
}