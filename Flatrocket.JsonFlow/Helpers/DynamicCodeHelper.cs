using Autofac;
using Flatrocket.JsonFlow.Tasks;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Flatrocket.JsonFlow.Helpers
{
    public class DynamicCodeHelper
    {
        private IContainer container;

        private string dynamicExpressionCode;

        public DynamicCodeHelper(IContainer container)
        {
            this.container = container;

            try { dynamicExpressionCode = File.ReadAllText("Resources/WorkflowDynamicExpression.cs"); }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read configuration file: Resources/WorkflowDynamicExpression.cs{Environment.NewLine}{Environment.NewLine}Detailed error: {ex.Message}");
            }
        }

        public bool CompileAndEvaluateCondition(string condition, Dictionary<string, IWorkflowTask> inputs)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Linq.dll");
            cp.ReferencedAssemblies.Add("Flatrocket.JsonFlow.dll");
            cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

            // Save the assembly as a physical file.
            cp.GenerateInMemory = true;

            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = false;

            string code = dynamicExpressionCode.ToString();
            code = dynamicExpressionCode.Replace("/*CONDITION_PLACEHOLDER*/", condition);

            // Invoke compilation of the source file.
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, code);
            bool isValid = false;

            if (cr.Errors.Count > 0)
            {
                string errors = String.Empty;

                foreach (CompilerError ce in cr.Errors)
                    errors += $"- {ce}{Environment.NewLine}";

                // Display compilation errors.
                throw new Exception($"There are errors with the condition in one of your tasks in the workflow graph.{Environment.NewLine}{Environment.NewLine}Error list:{Environment.NewLine}{Environment.NewLine}{errors}");
            }
            else
            {
                // Display a successful compilation message.
                Assembly assembly = cr.CompiledAssembly;
                object instance = assembly.CreateInstance("Flatrocket.JsonFlow.WorkflowDynamicExpression");
                MethodInfo methodInfo = instance.GetType().GetMethod("EvaluateCondition");

                isValid = Convert.ToBoolean(methodInfo.Invoke(instance, new object[] { inputs }));
            }

            return isValid;
        }
    }
}