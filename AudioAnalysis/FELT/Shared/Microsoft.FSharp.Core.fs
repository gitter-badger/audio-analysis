﻿namespace Microsoft.FSharp.Core
    open System

    [<AutoOpen>]
    module Operators =
        let fsts tuples = Seq.map (fst) tuples

        let third3 (_,_,c) = c
        let fst3 (a,_,_) = a
        let snd3 (_,b,_) = b

    [<AutoOpen>]
    module LanguagePrimitives =
        let inline GenericN n = NumericLiteralG.FromInt32 n