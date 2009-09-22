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
            this.Symbol = ParseNodeKind.Token;
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
            this.Symbol = kind;
            this.Token = TexToken.Null;
            this.Children = new ParseNodeCollection();
        }

        private ParseNode()
        {
            this.IsArgument = false;
        }

        public int SourcePosition
        {
            get { return this.Token.SourcePosition; }
        }

        public string SourceText
        {
            get { return this.Token.SourceText; }
        }

        public ParseNodeKind Symbol
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

        public ParseNodeCollection Children
        {
            get;
            private set;
        }

        public override string ToString()
        {
            if (this.Symbol == ParseNodeKind.Token)
            {
                return this.Token.ToString();
            }
            else
            {
                return this.Symbol.ToString();
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
