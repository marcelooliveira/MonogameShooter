using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core
{
    public interface IScriptProcessor
    {
        object ExecScript(object targetObject, string script);
    }
}
