using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class StringTemplateNode : AstNode
    {
        internal enum SegmentType
        {
            Text,
            Expression
        }

        internal class TemplateSegment
        {
            public SegmentType Type;
            public string Text;
            public AstNode ExpressionNode;
            public int Position;
            public TemplateSegment(string text, AstNode node, int position)
            {
                Type = node == null ? SegmentType.Text : SegmentType.Expression;
                Text = text;
                ExpressionNode = node;
                Position = position;
            }
        }

        internal class SegmentList : List<TemplateSegment> { }

        private string template;
        private string tokenText;
        private StringTemplateSettings templateSettings;
        private SegmentList segments = new SegmentList();

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            if(!parseNode.FindTokenAndGetText().StartsWith("$"))
            {
                parseNode.AstNode = new LiteralValueNode();
                ((LiteralValueNode)parseNode.AstNode).Init(context, parseNode);
                return;
            }

            base.Init(context, parseNode);
            template = parseNode.Token.ValueString;
            tokenText = parseNode.Token.Text;
            templateSettings = parseNode.Term.AstConfig.Data as StringTemplateSettings;
            ParseSegments(context);
            AsString = "\"" + template + "\" (templated string)";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            string value = BuildString(thread);
            thread.CurrentNode = this.Parent;
            return value;
        }

        private string BuildString(ScriptThread thread)
        {
            var values = new string[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                switch (segment.Type)
                {
                    case SegmentType.Text:
                        values[i] = segment.Text;
                        break;
                    case SegmentType.Expression:
                        values[i] = EvaluateExpression(thread, segment);
                        break;
                }
            }

            return string.Concat(values);
        }

        private string EvaluateExpression(ScriptThread thread, TemplateSegment segment)
        {
            try
            {
                var value = segment.ExpressionNode.Evaluate(thread);
                return value == null ? string.Empty : value.ToString();
            }
            catch
            {
                thread.CurrentNode = this;
                throw;
            }
        }

        private void ParseSegments(Irony.Ast.AstContext context)
        {
            var exprParser = new Parser(context.Language, templateSettings.ExpressionRoot);
            int currentPos = 0;
            int exprPosInTokenText = 0;

            while(true)
            {
                var startTagPos = template.IndexOf(templateSettings.StartTag, currentPos);
                if (startTagPos < 0) startTagPos = template.Length;
                var text = template.Substring(currentPos, startTagPos - currentPos);

                if (!string.IsNullOrEmpty(text))
                    segments.Add(new TemplateSegment(text, null, 0));

                if (startTagPos >= template.Length)
                    break;

                currentPos = startTagPos + templateSettings.StartTag.Length;
                var endTagPos = template.IndexOf(templateSettings.EndTag, currentPos);
                if(endTagPos < 0)
                {
                    context.AddMessage(Irony.ErrorLevel.Error, Location.SourceLocation, "No endtag in embedded expression found");
                    return;
                }

                var exprText = template.Substring(currentPos, endTagPos - currentPos);

                if(!string.IsNullOrEmpty(exprText))
                {
                    var exprTree = exprParser.Parse(exprText);
                    if(exprTree.HasErrors())
                    {
                        var baseLocation = Location.SourceLocation + tokenText.IndexOf(exprText);
                        CopyMessages(exprTree.ParserMessages, context.Messages, baseLocation, "");
                        return;
                    }

                    exprPosInTokenText = tokenText.IndexOf(templateSettings.StartTag, exprPosInTokenText) + templateSettings.StartTag.Length;
                    var segmNode = exprTree.Root.AstNode as AstNode;
                    segments.Add(new TemplateSegment(null, segmNode, exprPosInTokenText));

                    exprPosInTokenText += exprText.Length + templateSettings.EndTag.Length;
                }

                currentPos = endTagPos + templateSettings.EndTag.Length;
            }
        }

        private void CopyMessages(LogMessageList parserMessages, LogMessageList messages, SourceLocation baseLocation, string v)
        {
            foreach (var other in parserMessages)
                messages.Add(new LogMessage(other.Level, baseLocation + other.Location, v + other.Message, other.ParserState));
        }
    }
}
