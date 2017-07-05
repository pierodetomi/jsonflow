using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Sample.Tasks
{
    public class AskKindOfFoodTask : WorkflowBaseTask
    {
        public override void Execute(params List<WorkflowTaskData>[] inputs)
        {
            Console.WriteLine("Do you want pizza or meat?");
            string food = Console.ReadLine();

            Outputs.Add(new Models.WorkflowTaskData
            {
                Name = "Food",
                Value = food
            });
        }
    }
}