namespace TexDotNet

open System
open System.Collections.Generic
open TexDotNet

type TokenStream = IEnumerator<TexToken>

module TexUtilities =
    type TexExpressionNode with
        static member of_text text =
            TexUtilities.CreateExpressionTree (text : string)
        
        static member of_stream tokenStream =
            TexUtilities.CreateExpressionTree (tokenStream : TokenStream)
        
        member this.to_text () =
            TexUtilities.CreateText (this)
        
        member this.to_stream () =
            TexUtilities.CreateTokenStream (this)
    
    let text_to_tree tree =
        TexExpressionNode.of_text tree
    
    let text_to_stream text =
        TexUtilities.CreateTokenStream (text : string)
    
    let stream_to_text tokenStream =
        TexUtilities.CreateText (tokenStream : TokenStream)
    
    let stream_to_tree tokenStream =
        TexExpressionNode.of_stream tokenStream
    
    let tree_to_text tree =
        (tree : TexExpressionNode).to_text
    
    let tree_to_stream tree =
        (tree : TexExpressionNode).to_stream
