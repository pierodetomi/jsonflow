using Autofac;
using Flatrocket.JsonFlow.Models;
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

        public bool EvaluateCondition(string condition, Dictionary<string, IWorkflowTask> inputs)
        {
            List<string> references = new List<string>
            {
                "System.dll",
                "System.Core.dll",
                "System.Data.dll",
                "System.Linq.dll",
                "Flatrocket.JsonFlow.dll",
                "Microsoft.CSharp.dll"
            };

            string code = dynamicExpressionCode.ToString();
            code = dynamicExpressionCode.Replace("/*CONDITION_PLACEHOLDER*/", condition);

            DynamicCodeCompilationResult compilationResult = CompileExpression(
                references,
                code,
                fqnClassName: "Flatrocket.JsonFlow.WorkflowDynamicExpression",
                methodName: "EvaluateCondition");

            object executionResult = compilationResult.Method.Invoke(compilationResult.Instance, new object[] { inputs });
            return Convert.ToBoolean(executionResult);
        }

        public DynamicCodeCompilationResult CompileExpression(List<string> references, string code, string fqnClassName, string methodName)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters compilerParameters = new CompilerParameters();

            foreach(string reference in references)
                compilerParameters.ReferencedAssemblies.Add(reference);

            // Save the assembly as a physical file.
            compilerParameters.GenerateInMemory = true;

            // Set whether to treat all warnings as errors.
            compilerParameters.TreatWarningsAsErrors = false;
            
            // Invoke compilation of the source file.
            CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParameters, code);

            if (compilerResults.Errors.Count > 0)
            {
                string errors = String.Empty;

                foreach (CompilerError ce in compilerResults.Errors)
                    errors += $"- {ce}{Environment.NewLine}";

                // Display compilation errors.
                throw new Exception($"There are errors with the condition in one of your tasks in the workflow graph.{Environment.NewLine}{Environment.NewLine}Error list:{Environment.NewLine}{Environment.NewLine}{errors}");
            }
            else
            {
                // Display a successful compilation message.
                Assembly assembly = compilerResults.CompiledAssembly;
                object instance = assembly.CreateInstance(fqnClassName);
                MethodInfo methodInfo = instance.GetType().GetMethod(methodName);

                return new DynamicCodeCompilationResult
                {
                    Method = methodInfo,
                    Instance = instance
                };
            }
        }
    }
}