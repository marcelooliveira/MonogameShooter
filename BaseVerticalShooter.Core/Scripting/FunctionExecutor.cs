using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core
{
    public class FunctionExecutor : IFunctionExecutor
    {
        public FunctionExecutor()
        {
        }

        public object ExecFunction(object target, string methodName, object[] args)
        {
            if (target == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }

            var methodInfoQuery = target.GetType().GetRuntimeMethods().Where(m => m.Name == methodName);

            if (!methodInfoQuery.Any())
                throw new FunctionNameNotFoundException();

            if (methodInfoQuery.Count() > 1)
                throw new OverloadedMethodNotSupportedException();

            var methodInfo = methodInfoQuery.Single();
            var methodParameters = methodInfo.GetParameters();

            if (methodParameters.Count() != args.Count())
                throw new InvalidArgumentCountException();

            var types = new List<Type>();
            var parameterIndex = 0;
            foreach (var arg in args)
            {
                types.Add(arg.GetType());
                if (methodParameters[parameterIndex].ParameterType != arg.GetType())
                    throw new TypeMismatchException(arg);
                parameterIndex++;
            }

            return target.GetType().GetRuntimeMethod(methodName, types.ToArray()).Invoke(target, args);
        }

        public object Target { get; set; }
    }
    public class FunctionNameNotFoundException : Exception { public FunctionNameNotFoundException() { } }
    public class OverloadedMethodNotSupportedException : Exception { public OverloadedMethodNotSupportedException() { } }
    public class MissingArgumentException : Exception { public MissingArgumentException() { } }
    public class ArgumentNotFoundException : Exception
    {
        public object Argument { get; set; }
        public ArgumentNotFoundException(object argument) { Argument = argument; }
    }
    public class InvalidArgumentCountException : Exception { public InvalidArgumentCountException() { } }
    public class TypeMismatchException : Exception
    {
        public object Argument { get; set; }
        public TypeMismatchException(object argument) { Argument = argument; }
    }
}
