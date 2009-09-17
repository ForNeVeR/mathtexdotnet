﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    public class ParserException : Exception
    {
        private const string errorMessageInvalidSymbol =
            "Expected one of the following symbol kinds: {0}.";

        public ParserException(Token tokenRead, ICollection<SymbolKind> expectedSymbolKinds)
            : this(tokenRead, string.Format(errorMessageInvalidSymbol,
            string.Join(", ", expectedSymbolKinds.Select(tokenKind => tokenKind.ToString()).ToArray())))
        {
        }

        public ParserException(Token tokenRead, string message)
            : base(message + Environment.NewLine + string.Format("Token read: {0}", tokenRead))
        {
            this.TokenRead = tokenRead;
        }

        public Token TokenRead
        {
            get;
            private set;
        }
    }
}