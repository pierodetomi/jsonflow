using Autofac;
using Flatrocket.JsonFlow.Helpers;
using Flatrocket.JsonFlow.Models;
using Flatrocket.JsonFlow.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flatrocket.JsonFlow
{
    public class WorkflowManager
    {
        private IContainer container;

        private DynamicCodeHelper dynamicCode;

        private WorkflowValidator workflowValidator;

        private Workflow workflow;

        private WorkflowGraphItem currentGraphItem;

        public bool IsFinished { get; private set; }

        public WorkflowManager(IContainer container)
        {
            this.container = container;
            this.dynamicCode = new DynamicCodeHelper(container);
            this.workflowValidator = new WorkflowValidator();

            IsFinished = false;

            LoadConfiguration();
            LoadTasks();
        }

        public void Next()
        {
            WorkflowTaskInfo taskInfo = GetNextTaskInfo();

            if (taskInfo == null)
                return;

            IWorkflowTask task = GetTaskInstanceById(taskInfo.Id);

            // Clear outputs (in case this is the N-th time this task is executed)
            task.Outputs.Clear();

            List<List<WorkflowTaskData>> inputs = new List<List<WorkflowTaskData>> { };

            foreach (int taskId in currentGraphItem.Inputs)
            {
                var taskOutputs = GetTaskInstanceById(taskId).Outputs;
                inputs.Add(taskOutputs);
            }

            task.Execute(inputs.ToArray());
        }

        private WorkflowTaskInfo GetNextTaskInfo()
        {
            if (currentGraphItem == null)
            {
                bool hasEntryPoint = workflow.Graph.Any(g => g.IsEntryPoint);

                if (!hasEntryPoint)
                    throw new Exception("No entry point defined for this workflow. Please review the configuration file");
                
                // Beginning of the workflow - get root task
                currentGraphItem = workflow.Graph.SingleOrDefault(g => g.IsEntryPoint);
            }
            else
            {
                IEnumerable<WorkflowGraphItem> graphItems = workflow
                    .Graph
                    .Where(g => g.Parents != null && g.Parents.Any(p => p.Id == currentGraphItem.TaskId));

                bool hasResults = (graphItems != null && graphItems.Any());
                
                if (!hasResults)
                {
                    IsFinished = true;
                    return null;
                }
                else if(graphItems.Count() == 1 && String.IsNullOrEmpty(graphItems.First().Parents.Single(p => p.Id == currentGraphItem.TaskId).Condition))
                {
                    currentGraphItem = graphItems.First();
                }
                else
                {
                    currentGraphItem = GetGraphItemByCondition(graphItems, currentGraphItem.TaskId);
                }
            }

            if(currentGraphItem == null)
            {
                IsFinished = true;
                return null;
            }

            return workflow.Tasks.Single(t => t.Id == currentGraphItem.TaskId);
        }

        private WorkflowGraphItem GetGraphItemByCondition(IEnumerable<WorkflowGraphItem> graphItems, int parentId)
        {
            WorkflowGraphItem matchingGraphItem = null;

            foreach (WorkflowGraphItem graphItem in graphItems)
            {
                WorkflowGraphItemReference parentReference = graphItem.Parents.Single(p => p.Id == parentId);

                if (String.IsNullOrEmpty(parentReference.Condition))
                    throw new Exception($"Task with id {parentReference.Id} has multiple children, but at least one of them does not have a condition");

                MatchCollection matches = Regex.Matches(parentReference.Condition, @"(\$[0-9]+)(\.[a-zA-Z0-9_]+)");

                if (matches != null && matches.Count > 0)
                {
                    List<Match> matchList = new List<Match> { };
                    foreach (Match match in matches)
                        matchList.Add(match);

                    matchList = matchList
                        .OrderByDescending(m => m.Index)
                        .ToList();

                    Dictionary<string, IWorkflowTask> inputs = new Dictionary<string, IWorkflowTask> { };
                    string condition = parentReference.Condition;

                    foreach(Match match in matchList)
                    {
                        Group taskIdGroup = match.Groups[1];
                        string taskIdReference = taskIdGroup.Value;

                        int taskId = Convert.ToInt32(taskIdReference.Substring(1));
                        IWorkflowTask task = GetTaskInstanceById(taskId);

                        inputs[match.Groups[1].Value] = task;

                        Group outputGroup = match.Groups[2];
                        string outputField = outputGroup.Value.Substring(1);
                        
                        condition = condition.Remove(match.Index, match.Length);
                        condition = condition.Insert(match.Index, $"inputs[\"{taskIdReference}\"].Outputs.Single(o => o.Name == \"{outputField}\").Value");
                    }

                    if(dynamicCode.EvaluateCondition(condition, inputs))
                    {
                        matchingGraphItem = graphItem;
                        break;
                    }
                }

                if (matchingGraphItem != null)
                    break;
            }

            return matchingGraphItem;
        }

        private IWorkflowTask GetTaskInstanceById(int id)
        {
            return workflow.Tasks.Single(t => t.Id == id).Instance;
        }

        private void LoadConfiguration()
        {
            string configPath = ConfigurationManager.AppSettings["WFConfigPath"];

            if (String.IsNullOrEmpty(configPath))
                throw new Exception($"Missing required configuration key \"WFConfigPath\" in app.config or web.config");

            try
            {
                string configJson = File.ReadAllText(configPath);
                workflow = JsonConvert.DeserializeObject<Workflow>(configJson);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading workflow configuration file.{Environment.NewLine}{ex.Message}");
            }

            WorkflowValidationResult validationResult = workflowValidator.Validate(workflow);

            if(!validationResult.IsValid)
                throw new Exception($"There are errors in your workflow configuration file.{Environment.NewLine}{validationResult}");
        }

        private void LoadTasks()
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                foreach(WorkflowTaskInfo taskInfo in workflow.Tasks)
                {
                    object task = null;

                    if (!scope.TryResolveKeyed(taskInfo.Name, typeof(IWorkflowTask), out task))
                        throw new Exception($"Unable to load workflow task \"{taskInfo.Name}\" (task not found). Please check your configuration file.");

                    taskInfo.Instance = (task as IWorkflowTask);
                }
            }
        }
    }
}