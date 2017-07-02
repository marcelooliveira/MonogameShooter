using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core
{
    public class LineProcessor : ILineProcessor
    {
        object target;
        IFunctionExecutor functionExecutor;

        public LineProcessor(IFunctionExecutor functionExecutor)
        {
            if (functionExecutor == null)
                throw new ArgumentNullException();

            this.target = functionExecutor.Target;
            this.functionExecutor = functionExecutor;
        }

        public object ExecLine(object targetObject, string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new EmptyCommandLineException();
            }

            string functionName = string.Empty;
            string args = string.Empty;
            var words = line.Trim().Split(' ').ToList();
            var pattern = @"(\w*)\(([\w|\,|\s]*)\)";
            Match match = Regex.Match(line, pattern);
            if (match.Success)
            {
                int captureCtr = 0;
                for (int ctr = 1; ctr <= match.Groups.Count - 1; ctr++)
                {
                    foreach (Capture capture in match.Groups[ctr].Captures)
                    {
                        if (captureCtr == 0)
                        {
                            functionName = capture.Value;
                        }
                        else
                        {
                            if (capture.Value.Length > 0)
                                args = capture.Value;
                        }
                        captureCtr += 1;
                    }
                }
            }
            else
            {
                throw new LineSyntaxErrorException(line);
            }
            
            return functionExecutor.ExecFunction(targetObject, functionName, args.Split(',').ToList().Where(a => a.Length > 0).ToArray());
        }

        public object TargetObject
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }
    }

    public class EmptyCommandLineException : Exception { public EmptyCommandLineException() { } }
    public class LineSyntaxErrorException : Exception 
    {
        public string Line { get; private set; }
        public LineSyntaxErrorException(string line) 
        {
            Line = line;
        } 
    }
}
