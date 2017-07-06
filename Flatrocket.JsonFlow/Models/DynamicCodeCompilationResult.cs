using System.Reflection;

namespace Flatrocket.JsonFlow.Models
{
    public class DynamicCodeCompilationResult
    {
        public MethodInfo Method { get; set; }

        public object Instance { get; set; }
    }
}