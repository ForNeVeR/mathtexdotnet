using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TexDotNet
{
    using TokenStream = IEnumerator<TexToken>;

    public class TexComposer
    {
        public TexComposer()
        {
        }

        public TokenStream Write(TexExpressionNode tree)
        {
            using(var tokenStream = new ComposedTokenStream())
            {
                Write(tokenStream, tree);
                return tokenStream;
            }
        }

        private void Write(ComposedTokenStream tokenStream, TexExpressionNode node)
        {
            // TODO: Write text for given node, and repeat for children recursively.
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
                this.tokenList = null;
                this.curToken = null;
            }

            private void Write(TexToken token)
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
                return this.curToken != null;
            }

            public void Reset()
            {
                this.curToken = this.tokenList.First;
            }
        }
    }
}
