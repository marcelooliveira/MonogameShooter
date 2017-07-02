using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseVerticalShooter.Core
{
    public class ScriptProcessor : IScriptProcessor
    {
        ILineProcessor lineProcessor;
        object targetObject;
        List<ScriptLine> scriptLines = new List<ScriptLine>();

        public ScriptProcessor(ILineProcessor lineProcessor)
        {
            this.lineProcessor = lineProcessor;
            this.targetObject = lineProcessor.TargetObject;
        }

        public object ExecScript(object targetObject, string script)
        {
            object returnValue = null;
            int lineIndex = 0;
            this.targetObject = targetObject;

            var lines = script.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            ScriptLine lastScriptLine = null;
            while (lineIndex < lines.Count())
            {
                var scriptLine = new ScriptLine(lineIndex, lines[lineIndex]);
                if (lastScriptLine != null)
                {
                    if (scriptLine.Level == lastScriptLine.Level + 1)
                    {
                        scriptLine.ParentIndex = lastScriptLine.LineIndex;
                    }
                    else if (scriptLine.Level == lastScriptLine.Level)
                    {
                        scriptLine.ParentIndex = lastScriptLine.ParentIndex;
                    }
                    else if (scriptLine.Level == lastScriptLine.Level - 1)
                    {
                        var parentLine =
                        scriptLines
                            .Where(l => l.Level == scriptLine.Level - 1)
                            .OrderBy(l => l.LineIndex)
                            .LastOrDefault();

                        if (parentLine != null)
                        {
                            scriptLine.ParentIndex = parentLine.LineIndex;
                        }
                    }
                }
                scriptLines.Add(scriptLine);
                lastScriptLine = scriptLine;
                lineIndex++;
            }

            returnValue = ExecScriptInternal(targetObject, -1);
            return returnValue;
        }

        private object ExecScriptInternal(object targetObject, int parentLineIndex)
        {
            object returnValue = null;
            var ifExpressionResult = false;
            foreach (var scriptLine in scriptLines
                .Where(sl => sl.ParentIndex == parentLineIndex)
                .OrderBy(sl => sl.LineIndex))
            {
                if (scriptLine.Text.StartsWith("if "))
                {
                    ifExpressionResult = false;
                    var ifExpression = scriptLine.Text.Replace("if ", "");
                    ifExpressionResult = ProcessIfBlock(targetObject, scriptLine, ifExpression);
                }
                else if (!ifExpressionResult && scriptLine.Text.StartsWith("else if "))
                {
                    ifExpressionResult = false;
                    var ifExpression = scriptLine.Text.Replace("else if ", "");
                    ifExpressionResult = ProcessIfBlock(targetObject, scriptLine, ifExpression);
                }
                else if (!ifExpressionResult && scriptLine.Text.Equals("else"))
                {
                    ifExpressionResult = ProcessIfBlock(targetObject, scriptLine, string.Empty);
                }
                else if (scriptLine.Text.StartsWith("while "))
                {
                    var ifExpression = scriptLine.Text.Replace("while ", "");
                    while (ProcessIfBlock(targetObject, scriptLine, ifExpression)) { }
                }
                else if (scriptLine.Text.StartsWith("return "))
                {
                    var returnExpression = scriptLine.Text.Replace("return ", "");
                    object obj = null;

                    var intResult = int.MaxValue;
                    var isInt = int.TryParse(returnExpression, out intResult);
                    if (isInt)
                        obj = intResult;
                    else
                    {
                        var boolResult = false;
                        var isBool = bool.TryParse(returnExpression, out boolResult);
                        if (isBool)
                            obj = boolResult;
                        else
                        {
                            var functionPattern = @"(\w*)\(([\w|\,|\s]*)\)";
                            Match match = Regex.Match(returnExpression, functionPattern);
                            if (match.Success)
                            {
                                obj = lineProcessor.ExecLine(targetObject, returnExpression);
                            }
                        }
                    }
                   
                    returnValue = obj;
                }
                else if (scriptLine.Text.Length > 0)
                {
                    lineProcessor.ExecLine(targetObject, scriptLine.Text);
                }
            }
            return returnValue;
        }

        private bool ProcessIfBlock(object targetObject, ScriptLine scriptLine, string ifExpression)
        {
            var result = false;
            if (!scriptLines.Where(sl => sl.ParentIndex == scriptLine.LineIndex).Any())
                throw new IfWithoutBlockException();

            if (string.IsNullOrEmpty(ifExpression))
                result = true;
            else
                result = (bool)lineProcessor.ExecLine(targetObject, ifExpression);

            if (result)
            {
                ExecScriptInternal(targetObject, scriptLine.LineIndex);
            }
            return result;
        }
    }

    public class ScriptLine
    {
        public int LineIndex { get; private set; }
        public string Text { get; private set; }
        public int ParentIndex { get; set; }
        public int Level { get; set; }
        public ScriptLine(int lineIndex, string text)
        {
            for (var l = text.Length; l >= 0; l--)
            {
                if (text.StartsWith(new String('\t', l)))
                {
                    Level = l;
                    break;
                }
            }
            LineIndex = lineIndex;
            Text = text.Replace("\t", "").Trim();
            ParentIndex = -1;
        }
    }

    public class IfWithoutBlockException : Exception { }
}
