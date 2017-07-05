using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flatrocket.JsonFlow.Sample.Tasks
{
    public class RestaurantListTask : WorkflowBaseTask
    {
        public override void Execute(params List<WorkflowTaskData>[] inputs)
        {
            string food = inputs[0].Single(o => o.Name == "Food").Value;
            string where = inputs[1].Single(o => o.Name == "Where").Value;

            string restaurants = where == "TS" ? "Da Gino" : "Da mario";

            Console.WriteLine($"You can eat {food} in these restaurants in {where}: {restaurants}");
        }
    }
}