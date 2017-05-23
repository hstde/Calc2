using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using CalcLang.Interpreter;

namespace CalcLang.Ast
{
    public delegate object EvaluateMethod(ScriptThread thread);
    public delegate void ValueSetterMethod(ScriptThread thread, object value);

    [Flags]
    public enum AstNodeType
    {
        Unkown = 0,
        BindingName = 1 << 1,
        CallTarget = 1 << 2,
        ValueRead = 1 << 3,
        ValueWrite = 1 << 4,
        Parameter = 1 << 5,
        Keyword = 1 << 6,
        SpecialSymbol = 1 << 7,

        ValueReadWrite = ValueRead | ValueWrite
    }

    public enum FlowControl
    {
        Next,
        Continue,
        Break,
        Return
    }

    public class AstNodeList : List<AstNode> { }

    public class AstNode : IAstNodeInit, IBrowsableAstNode
    {
        protected object lockObject = new object();

        public AstNode Parent
        {
            get;
            set;
        }
        public BnfTerm Term
        {
            get;
            set;
        }
        public SourceSpan Span
        {
            get;
            set;
        }
        public int Position => Span.Location.Position;
        public SourceInfo Location
        {
            get;
            set;
        }
        public SourceInfo ErrorAnchor
        {
            get;
            set;
        }
        public string Role
        {
            get;
            set;
        }
        public virtual  string AsString
        {
            get;
            protected set;
        }
        public AstNodeList ChildNodes
        {
            get;
            protected set;
        }
        public EvaluateMethod Evaluate
        {
            get;
            protected set;
        }
        public ValueSetterMethod SetValue
        {
            get;
            protected set;
        }
        public virtual ScopeInfo DependentScopeInfo { get; set; }

        public AstNodeType NodeType
        {
            get;
            set;
        }

        public FlowControl FlowControl = FlowControl.Next;

        protected ExpressionType ExpressionType = (ExpressionType)(-1);

        public AstNode()
        {
            ChildNodes = new AstNodeList();
            Evaluate = DoEvaluate;
            SetValue = DoSetValue;
            NodeType = AstNodeType.Unkown;
        }

        public virtual void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            var acon = context as AstContext;
            Term = parseNode.Term;
            Span = parseNode.Span;
            Location = new SourceInfo(Span.Location, acon.FileName);
            ErrorAnchor = Location;
            parseNode.AstNode = this;
            AsString = (Term == null ? GetType().Name : Term.Name);
        }

        public virtual void Reset()
        {
            Evaluate = DoEvaluate;
            foreach (var child in ChildNodes)
                child.Reset();
        }

        protected virtual object DoEvaluate(ScriptThread thread) => null;

        public virtual void DoSetValue(ScriptThread thread, object value)
        {
        }

        public virtual bool IsConstant() => false;

        public IEnumerable GetChildNodes() => ChildNodes;

        protected AstNode AddChild(string role, ParseTreeNode childParseNode) => AddChild(AstNodeType.Unkown, role, childParseNode);
        protected AstNode AddChild(AstNodeType type, string role, ParseTreeNode childParseNode)
        {
            var child = (AstNode)childParseNode.AstNode ?? new NothingNode(childParseNode.Term);
            child.Role = role;
            child.Parent = this;
            child.NodeType = type;
            ChildNodes.Add(child);
            return child;
        }

        public override string ToString() => String.IsNullOrEmpty(Role) ? AsString : Role + ": " + AsString;
    }
}
