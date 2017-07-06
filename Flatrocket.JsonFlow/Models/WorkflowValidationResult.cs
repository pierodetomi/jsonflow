using System;
using System.Collections.Generic;

namespace Flatrocket.JsonFlow.Models
{
    public class WorkflowValidationResult
    {
        public bool IsValid
        {
            get { return Errors?.Count == 0; }
        }

        public List<WorkflowValidationError> Errors { get; set; }

        public WorkflowValidationResult()
        {
            Errors = new List<WorkflowValidationError> { };
        }

        public override string ToString()
        {
            string list = string.Empty;

            foreach (WorkflowValidationError error in Errors)
                list += $"- {error.Text}{Environment.NewLine}";

            return list;
        }
    }
}