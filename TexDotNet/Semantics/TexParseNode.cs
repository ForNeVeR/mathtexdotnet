using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
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
            this.Children = new ParseNodeCollection();
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
            this.Children = new ParseNodeCollection();
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

        public override string ToString()
        {
            if (this.Kind == ParseNodeKind.Token)
            {
                return this.Token.ToString();
            }
            else
            {
                return this.Kind.ToString();
            }
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
