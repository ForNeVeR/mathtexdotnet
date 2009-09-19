using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public struct Token
    {
        public static readonly Token Null = new Token(SymbolKind.Null, -1);

        public static Token FromNumber(double value, int position)
        {
            return new Token(SymbolKind.Number, value, position);
        }

        public static Token FromKind(SymbolKind kind, int position)
        {
            return new Token(kind, position);
        }

        public static Token FromValue(SymbolKind kind, object value, int position)
        {
            return new Token(kind, value, position);
        }

        // Meta-information
        public int Position;

        public readonly SymbolKind Symbol;
        public readonly object Value;

        private Token(SymbolKind kind, int position)
            : this(kind, null, position)
        {
        }

        private Token(SymbolKind kind, object value, int position)
        {
            this.Symbol = kind;
            this.Value = value;

            this.Position = position;
        }

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}
