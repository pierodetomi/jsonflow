namespace Flatrocket.JsonFlow.Models
{
    public class WorkflowValidationError
    {
        public string Text { get; set; }

        public WorkflowValidationError(string text)
        {
            Text = text;
        }
    }
}