using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flatrocket.JsonFlow.Sample.Tasks
{
    public class WhereToEatTask : WorkflowBaseTask
    {
        public override void Execute(params List<WorkflowTaskData>[] inputs)
        {
            string food = inputs[0].Single(o => o.Name == "Food").Value;

            Console.WriteLine($"Where do you want to eat {food}? (TS/UD)");
            string where = Console.ReadLine();

            Outputs.Add(new Models.WorkflowTaskData
            {
                Name = "Where",
                Value = where
            });
        }
    }
}