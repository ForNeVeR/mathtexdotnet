open System;
open System.Collections.Generic;
open TexDotNet;

type TokenStream = IEnumerator<TexToken>

let createText tree =
    TexHelper.CreateText (tree)

let createTokenStreamFromString expression =
    TexHelper.CreateTokenStream (expression : string)

let createTokenStreamFromTree tree =
    TexHelper.CreateTokenStream (tree : TexExpressionNode)

let createExpressionTreeFromString expression =
    TexHelper.CreateExpressionTree (expression : string)

let createExpressionTreeFromStream tokenStream =
    TexHelper.CreateExpressionTree (tokenStream : TokenStream)

let createParseTreeFromString expression =
    TexHelper.CreateParseTree (expression : string)

let createParseTreeFromStream tokenStream =
    TexHelper.CreateParseTree (tokenStream : TokenStream)
