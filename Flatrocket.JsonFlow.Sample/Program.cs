using Autofac;
using Autofac.Core;
using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Sample.Tasks;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flatrocket.JsonFlow.Sample
{
    class Program
    {
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AskKindOfFoodTask>().Named<IWorkflowTask>("Root");
            builder.RegisterType<UnknownInputTask>().Named<IWorkflowTask>("UnknownInput");
            builder.RegisterType<WhereToEatTask>().Named<IWorkflowTask>("WhereToEat");
            builder.RegisterType<RestaurantListTask>().Named<IWorkflowTask>("RestaurantList");
            builder.RegisterType<FinishTask>().Named<IWorkflowTask>("Finish");

            Container = builder.Build();

            WorkflowManager wfManager = new WorkflowManager(Container);

            while(!wfManager.IsFinished)
                wfManager.Next();

            Console.WriteLine("The workflow is finished.");
            Console.ReadLine();
        }
    }
}