using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public struct Token
    {
        public static readonly Token Null = new Token(SymbolKind.Null);

        public static Token FromNumber(double value)
        {
            return new Token(SymbolKind.Number, value);
        }

        public static Token FromKind(SymbolKind kind)
        {
            return new Token(kind);
        }

        public static Token FromValue(SymbolKind kind, object value)
        {
            return new Token(kind, value);
        }

        public readonly SymbolKind Symbol;
        public readonly object Value;

        private Token(SymbolKind kind)
            : this(kind, null)
        {
        }

        private Token(SymbolKind kind, object value)
        {
            this.Symbol = kind;
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}
