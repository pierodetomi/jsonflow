using Autofac;
using Flatrocket.JsonFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flatrocket.JsonFlow
{
    public class WorkflowValidator
    {
        public WorkflowValidationResult Validate(Workflow workflow)
        {
            WorkflowValidationResult result = new WorkflowValidationResult { };

            // Check that each graph item has a corresponding task definition
            List<WorkflowValidationError> consistencyErrors = CheckTaskDefinitionsConsistency(workflow);
            result.Errors.AddRange(consistencyErrors);

            // Check that multiple references (in graph) to the same task definition
            // have conditions
            List<WorkflowValidationError> referencesErrors = CheckMultipleTaskReferences(workflow);
            result.Errors.AddRange(referencesErrors);

            // TODO: Check for Duplicate task definitions

            // TODO: Check for graph items with multiple parent items with same task id

            return result;
        }

        private List<WorkflowValidationError> CheckTaskDefinitionsConsistency(Workflow workflow)
        {
            List<WorkflowValidationError> errors = new List<WorkflowValidationError> { };
            List<int> checkedIds = new List<int> { };

            workflow.Graph.ForEach(gi =>
            {
                if (checkedIds.Contains(gi.TaskId))
                    return;

                bool hasTaskDefinition = workflow.Tasks.Any(t => t.Id == gi.TaskId);

                if (!hasTaskDefinition)
                    errors.Add(new WorkflowValidationError($"Task with id {gi.TaskId} is not defined. Please check your configuration file."));

                checkedIds.Add(gi.TaskId);
            });

            return errors;
        }

        private List<WorkflowValidationError> CheckMultipleTaskReferences(Workflow workflow)
        {
            List<WorkflowValidationError> errors = new List<WorkflowValidationError> { };

            // Get graph items that reference N times the same task definitions
            List<int> checkedIds = new List<int> { };

            workflow.Graph.ForEach(graphItem =>
            {
                graphItem.Parents.ForEach(parent =>
                {
                    if (checkedIds.Contains(parent.Id))
                        return;

                    List<WorkflowGraphItem> matches = workflow.Graph.Where(g => g.Parents.Any(p => p.Id == parent.Id)).ToList();

                    if(matches.Count > 1)
                        foreach (WorkflowGraphItem match in matches)
                        {
                            WorkflowGraphItemReference parentReference = match.Parents.Single(p => p.Id == parent.Id);

                            if (String.IsNullOrEmpty(parentReference.Condition))
                            {
                                errors.Add(new WorkflowValidationError($"The task with id {parent.Id} is referenced as parent of multiple task, but one or more of them doesn't specify the \"Condition\" property. Please check your configuration file."));

                                // Include this error in list only one time
                                break;
                            }

                        }

                    checkedIds.Add(parent.Id);
                });
            });

            return errors;
        }
    }
}