using System;
namespace BaseVerticalShooter.Core
{
    public interface IFunctionExecutor
    {
        object ExecFunction(object target, string methodName, object[] args);
        object Target { get; set; }
    }
}
