[<AutoOpen>]
module CSharpLanguageServer.Common.Extensions

open System
open System.Collections.Concurrent


type ConcurrentDictionary<'key, 'value> with
    member this.TryFind key =
        match this.TryGetValue key with
        | true, value -> Some value
        | _ -> None


module Seq =
    let inline tryMaxBy (projection: 'T -> 'U) (source: 'T seq): 'T option =
        if isNull source || Seq.isEmpty source then
            None
        else
            Seq.maxBy projection source |> Some


module Option =
    let inline ofString (value: string) =
        match String.IsNullOrWhiteSpace(value) with
        | true -> None
        | false -> Some value