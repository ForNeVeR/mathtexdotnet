using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class ParseNode : ITexErrorSourceInfo
    {
        public ParseNode(TexToken token, IEnumerable<ParseNode> children)
            : this(token)
        {
            foreach (var childNode in children)
                this.Children.Add(childNode);
        }

        public ParseNode(TexToken token)
            : this()
        {
            this.Kind = ParseNodeKind.Token;
            this.Token = token;
            this.Children = new ParseNodeCollection(this);
        }

        public ParseNode(ParseNodeKind kind, IEnumerable<ParseNode> children)
            : this(kind)
        {
            foreach (var childNode in children)
                this.Children.Add(childNode);
        }

        public ParseNode(ParseNodeKind kind)
            : this()
        {
            this.Kind = kind;
            this.Token = TexToken.Null;
            this.Children = new ParseNodeCollection(this);
        }

        private ParseNode()
        {
            this.IsArgument = false;
            this.IsSubExpression = false;
        }

        public int SourcePosition
        {
            get { return this.Token.SourcePosition; }
        }

        public string SourceText
        {
            get { return this.Token.SourceText; }
        }

        public ParseNodeKind Kind
        {
            get;
            set;
        }

        public TexToken Token
        {
            get;
            set;
        }

        public bool IsArgument
        {
            get;
            set;
        }

        public bool IsSubExpression
        {
            get;
            set;
        }

        public ParseNodeCollection Children
        {
            get;
            private set;
        }

        public ParseNode Parent
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return (this.Kind == ParseNodeKind.Token ? this.Token.ToString() : this.Kind.ToString()) +
                string.Format(" [{0} children]", this.Children.Count);
        }
    }

    public enum ParseNodeKind
    {
        InfixOperator,
        PrefixOperator,
        PostfixOperator,
        Indices,
        Token,
    }
}
