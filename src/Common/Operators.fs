namespace CSharpLanguageServer.Common

open FSharpPlus

[<AutoOpen>]
module Operators =
    // There is no such operator (with type Monad<'a> -> Monad<'b> -> Monad<'b>) in FSharpPlus like `>>` in
    // Haskell, so just write one.
    // In F#, `>>` is function composition, that is why we use another operator.
    let inline (>->) (ma: '``Monad<'a>``) (mb: '``Monad<'b>``): '``Monad<'b>`` = ma >>= konst mb
