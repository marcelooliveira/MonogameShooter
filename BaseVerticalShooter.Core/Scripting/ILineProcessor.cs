using System;
namespace BaseVerticalShooter.Core
{
    public interface ILineProcessor
    {
        object TargetObject { get; set; }
        object ExecLine(object targetObject, string line);
        //void ExecIf(object targetObject, string scriptIf, string scriptElse);
    }
}
