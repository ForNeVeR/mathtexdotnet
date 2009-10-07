using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexComposer
    {
        private const string errorMessageUnexpectedNumberOfChildren =
            "An parse node of kind {0} cannot have {1} children.";
        private const string errorMessageInvalidOperator =
            "Invalid operator symbol";

        public TexComposer()
        {
            this.PadPlusAndMinusSigns = true;
        }

        public bool PadPlusAndMinusSigns
        {
            get;
            set;
        }

        public TokenStream Write(TexExpressionNode node)
        {
            using (var tokenStream = new ComposedTokenStream())
            {
                WriteNode(tokenStream, node, new ComposerState());
                return tokenStream;
            }
        }

        private void WriteNode(ComposedTokenStream tokenStream, TexExpressionNode node, ComposerState state)
        {
            var openBracketSymbol = TexSymbolKind.Null;
            var closeBracketSymbol = TexSymbolKind.Null;
            if ((node.Symbol.IsBinaryOperator() || node.Symbol.IsRelationOperator()) && 
                !node.Symbol.IsRaiseOrLowerOperator())
            {
                // Check if current operator needs brackets because it has lower precedence than parent operator.
                if (node.Parent != null && !node.Parent.Symbol.IsRaiseOrLowerOperator())
                {
                    var nodePrecedence = GetOperatorPrecedence(node);
                    var parentNodePrecedence = GetOperatorPrecedence(node.Parent);
                    if (nodePrecedence != -1 && parentNodePrecedence != -1 &&
                        nodePrecedence < parentNodePrecedence ||
                        (nodePrecedence == parentNodePrecedence && 
                        (node.Parent.Children.IndexOf(node) == 0 ^ node.Parent.Symbol.IsLeftAssociativeOperator())))
                    {
                        openBracketSymbol = TexSymbolKind.RoundBracketOpen;
                        closeBracketSymbol = TexSymbolKind.RoundBracketClose;
                    }
                }
            }
            else if (node.Symbol.IsFunctionOperator() || node.Symbol.IsBigOperator())
            {
                openBracketSymbol = TexSymbolKind.GroupOpen;
                closeBracketSymbol = TexSymbolKind.GroupClose;
            }

            if (openBracketSymbol == TexSymbolKind.Null)
            {
                if (node.Parent != null)
                {
                    // If any descendant node has more than one child, then group is needed.
                    bool needGroup = false;
                    var curNode = node;
                    while (curNode.Children.Count >= 1)
                    {
                        if (curNode.Children.Count > 1)
                        {
                            needGroup = true;
                            break;
                        }
                        curNode = curNode.Children[0];
                    }

                    if (needGroup)
                    {
                        switch (node.Parent.Symbol)
                        {
                            case TexSymbolKind.LowerToIndex:
                            case TexSymbolKind.RaiseToIndex:
                                openBracketSymbol = TexSymbolKind.GroupOpen;
                                closeBracketSymbol = TexSymbolKind.GroupClose;
                                break;
                            case TexSymbolKind.Factorial:
                                openBracketSymbol = TexSymbolKind.GroupOpen;
                                closeBracketSymbol = TexSymbolKind.GroupClose;
                                break;
                        }
                    }
                }
            }

            if (IsRightMostNode(node))
            {
                // No need to write brackets, since current operator will be written last.
                state.IsParentNodeGroupOpen = false;
                openBracketSymbol = TexSymbolKind.Null;
                closeBracketSymbol = TexSymbolKind.Null;
            }
            else if (!state.IsParentNodeGroupOpen && openBracketSymbol == TexSymbolKind.GroupOpen)
            {
                // Do not directly nest group brackets.
                state.IsParentNodeGroupOpen = true;
            }

            if (openBracketSymbol != TexSymbolKind.Null)
                tokenStream.Write(TexToken.FromSymbol(openBracketSymbol));

            if (node.Symbol.IsBinaryOperator() || node.Symbol.IsRelationOperator())
            {
                WriteInfixOperatorNode(tokenStream, node, state);
            }
            else if (node.Symbol.IsBracketedFunction())
            {
                WriteBracketedFunction(tokenStream, node, state);
            }
            else if (node.Symbol.IsFunctionOperator() || node.Symbol.IsBigOperator())
            {
                WritePrefixOperatorNode(tokenStream, node, state);
            }
            else if (node.Symbol.IsPostfixOperator())
            {
                WritePostfixOperatorNode(tokenStream, node, state);
            }
            else if (node.Symbol.IsValue())
            {
                WriteValueNode(tokenStream, node, state);
            }
            else
            {
                throw new TexComposerException(node,
                    "Unrecognised node symbol.");
            }

            if (closeBracketSymbol != TexSymbolKind.Null)
                tokenStream.Write(TexToken.FromSymbol(closeBracketSymbol));
        }

        private void WriteBracketedFunction(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                foreach (var argNode in node.Arguments)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketOpen));
                    state.IsParentNodeGroupOpen = true;
                    WriteNode(tokenStream, argNode, state);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.SquareBracketClose));
                }
                foreach (var childNode in node.Children)
                {
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupOpen));
                    state.IsParentNodeGroupOpen = true;
                    WriteNode(tokenStream, childNode, state);
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.GroupClose));
                }
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errorMessageUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteInfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count == 1)
            {
                // Node is actually term prefix.
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                WriteNode(tokenStream, node.Children[0], state);
            }
            else if (node.Children.Count == 2)
            {
                WriteNode(tokenStream, node.Children[0], state);

                bool writeOpSymbol = true;
                var padSymbol = (this.PadPlusAndMinusSigns && node.Symbol.IsPlusOrMinusOperator()) ||
                    node.Symbol.IsLongOperator();
                if (node.Symbol == TexSymbolKind.Dot)
                {
                    var checkNode = node.Children[1];
                    while (checkNode.Symbol == TexSymbolKind.Dot || checkNode.Symbol == TexSymbolKind.RaiseToIndex ||
                        checkNode.Symbol == TexSymbolKind.LowerToIndex)
                        checkNode = checkNode.Children[0];

                    // If terms can be multipled implicitly, do not write operator token.
                    switch (checkNode.Symbol)
                    {
                        case TexSymbolKind.Number:
                        case TexSymbolKind.Text:
                            break;
                        default:
                            if (checkNode.Children.Count <= 1)
                            {
                                writeOpSymbol = false;
                                padSymbol = false;
                            }
                            break;
                    }
                }

                if (padSymbol)
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));
                if (writeOpSymbol)
                    tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                if (padSymbol)
                    tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));

                WriteNode(tokenStream, node.Children[1], state);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errorMessageUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePrefixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
                foreach (var argNode in node.Arguments)
                {
                    if (argNode.Children.Count != 1)
                        throw new TexComposerException(argNode, string.Format(
                            errorMessageUnexpectedNumberOfChildren, argNode.Symbol, argNode.Children.Count));
                    tokenStream.Write(TexToken.FromSymbol(argNode.Symbol));
                    WriteNode(tokenStream, argNode.Children[0], state);
                }
                tokenStream.Write(TexToken.FromSymbol(TexSymbolKind.Space));
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode, state);
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errorMessageUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WritePostfixOperatorNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            if (node.Children.Count >= 1)
            {
                foreach (var childNode in node.Children)
                    WriteNode(tokenStream, childNode, state);
                tokenStream.Write(TexToken.FromSymbol(node.Symbol));
            }
            else
            {
                throw new TexComposerException(node, string.Format(
                    errorMessageUnexpectedNumberOfChildren, node.Symbol, node.Children.Count));
            }
        }

        private void WriteValueNode(ComposedTokenStream tokenStream, TexExpressionNode node,
            ComposerState state)
        {
            tokenStream.Write(TexToken.FromValue(node.Symbol, node.Value));
        }

        private int GetOperatorPrecedence(TexExpressionNode node)
        {
            // Higher value means higher precedence.
            switch (node.Symbol)
            {
                case TexSymbolKind.Plus:
                case TexSymbolKind.Minus:
                case TexSymbolKind.PlusMinus:
                case TexSymbolKind.MinusPlus:
                    if (node.Children.Count == 1)
                        return 3;
                    return 2;
                case TexSymbolKind.Star:
                case TexSymbolKind.Dot:
                case TexSymbolKind.Cross:
                case TexSymbolKind.Divide:
                case TexSymbolKind.Fraction:
                case TexSymbolKind.InlineModulo:
                    return 4;
                case TexSymbolKind.RaiseToIndex:
                case TexSymbolKind.LowerToIndex:
                    return 5;
                default:
                    if (node.Symbol.IsRelationOperator())
                        return 1;
                    return -1;
            }
        }

        private bool IsRightMostNode(TexExpressionNode node)
        {
            if (node.Parent == null)
                return true;
            return node.Parent.Children.IndexOf(node) == node.Parent.Children.Count &&
                IsRightMostNode(node.Parent);
        }

        private class ComposedTokenStream : TokenStream
        {
            private LinkedList<TexToken> tokenList;
            private LinkedListNode<TexToken> curToken;

            public ComposedTokenStream()
            {
                this.tokenList = new LinkedList<TexToken>();
                this.curToken = null;
            }

            internal LinkedList<TexToken> TokenList
            {
                get { return this.tokenList; }
            }

            public TexToken Current
            {
                get { return this.curToken.Value; }
            }

            object IEnumerator.Current
            {
                get { return this.curToken.Value; }
            }

            public void Dispose()
            {
            }

            public void Write(TexToken token)
            {
                this.tokenList.AddLast(token);
            }

            public bool MoveNext()
            {
                LinkedListNode<TexToken> nextTokenNode;
                if (this.curToken == null)
                    nextTokenNode = this.tokenList.First;
                else
                    nextTokenNode = this.curToken.Next;
                if (nextTokenNode != null)
                    this.curToken = nextTokenNode;
                return nextTokenNode != null;
            }

            public void Reset()
            {
                this.curToken = this.tokenList.First;
            }
        }

        private struct ComposerState
        {
            public bool IsParentNodeGroupOpen;
        }
    }
}
