using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public struct TexToken : ITexErrorSourceInfo
    {
        public static readonly TexToken Null = new TexToken(TexSymbolKind.Null, -1, null);

        public static TexToken FromNumber(double value)
        {
            return new TexToken(TexSymbolKind.Number, value);
        }

        public static TexToken FromNumber(double value, int sourcePosition, string sourceText)
        {
            return new TexToken(TexSymbolKind.Number, value, sourcePosition, sourceText);
        }

        public static TexToken FromSymbol(TexSymbolKind symbol)
        {
            return new TexToken(symbol);
        }

        public static TexToken FromSymbol(TexSymbolKind symbol, int sourcePosition, string sourceText)
        {
            return new TexToken(symbol, sourcePosition, sourceText);
        }

        public static TexToken FromValue(TexSymbolKind kind, object value)
        {
            return new TexToken(kind, value);
        }

        public static TexToken FromValue(TexSymbolKind kind, object value, int sourcePosition, string sourceText)
        {
            return new TexToken(kind, value, sourcePosition, sourceText);
        }

        // Information about error source.
        private int sourcePosition;
        private string sourceText;

        public readonly TexSymbolKind Symbol;
        public readonly object Value;

        private TexToken(TexSymbolKind symbol)
            : this(symbol, -1, null)
        {
        }

        private TexToken(TexSymbolKind symbol, object value)
            : this(symbol, value, -1, null)
        {
        }

        private TexToken(TexSymbolKind symbol, int sourcePosition, string sourceText)
            : this(symbol, null, sourcePosition, sourceText)
        {
        }

        private TexToken(TexSymbolKind symbol, object value, int sourcePosition, string sourceText)
        {
            this.Symbol = symbol;
            this.Value = value;

            this.sourcePosition = sourcePosition;
            this.sourceText = sourceText;
        }

        public int SourcePosition
        {
            get { return this.sourcePosition; }
        }

        public string SourceText
        {
            get { return this.sourceText; }
        }

        public override string ToString()
        {
            return this.Symbol + (this.Value == null ? string.Empty :
                "(" + this.Value.ToString() + ")");
        }
    }
}
