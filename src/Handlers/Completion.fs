namespace CSharpLanguageServer.Handlers

open System
open FSharpPlus
open Microsoft.CodeAnalysis.Completion
open Microsoft.CodeAnalysis.Text
open Ionide.LanguageServerProtocol.Server
open Ionide.LanguageServerProtocol.Types
open Ionide.LanguageServerProtocol.Types.LspResult

open CSharpLanguageServer.Common.Extensions
open CSharpLanguageServer.Common.Types
open CSharpLanguageServer.Common.LspUtil

[<RequireQualifiedAccess>]
module Completion =
    let private dynamicRegistration (clientCapabilities: ClientCapabilities option) =
        clientCapabilities
        |> Option.bind (fun x -> x.TextDocument)
        |> Option.bind (fun x -> x.Completion)
        |> Option.bind (fun x -> x.DynamicRegistration)
        |> Option.defaultValue false

    let provider (clientCapabilities: ClientCapabilities option) : CompletionOptions option =
        match dynamicRegistration clientCapabilities with
        | true -> None
        | false ->
            Some { ResolveProvider = None
                   TriggerCharacters = Some ([| '.'; '''; |])
                   AllCommitCharacters = None
                   CompletionItem = None }

    let registration (clientCapabilities: ClientCapabilities option) : Registration option =
        match dynamicRegistration clientCapabilities with
        | false -> None
        | true ->
            Some
                { Id = Guid.NewGuid().ToString()
                  Method = "textDocument/completion"
                  RegisterOptions =
                    { ResolveProvider = None
                      TriggerCharacters = Some ([| '.'; '''; |])
                      AllCommitCharacters = None
                      CompletionItem = None
                      DocumentSelector = Some defaultDocumentSelector }
                    |> serialize
                    |> Some }

    let private roslynTagToLspCompletion tag =
        match tag with
        | "Class" -> CompletionItemKind.Class
        | "Delegate" -> CompletionItemKind.Function
        | "Enum" -> CompletionItemKind.Enum
        | "EnumMember" -> CompletionItemKind.EnumMember
        | "Interface" -> CompletionItemKind.Interface
        | "Struct" -> CompletionItemKind.Struct
        | "Local" -> CompletionItemKind.Variable
        | "Parameter" -> CompletionItemKind.Variable
        | "RangeVariable" -> CompletionItemKind.Variable
        | "Const" -> CompletionItemKind.Constant
        | "Event" -> CompletionItemKind.Event
        | "Field" -> CompletionItemKind.Field
        | "Method" -> CompletionItemKind.Method
        | "Property" -> CompletionItemKind.Property
        | "Label" -> CompletionItemKind.Unit
        | "Keyword" -> CompletionItemKind.Keyword
        | "Namespace" -> CompletionItemKind.Module
        | _ -> CompletionItemKind.Property

    // TODO: Add parameters to label so that we can distinguish override versions?
    // TODO: Change parameters to snippets like clangd
    let private makeLspCompletionItem
        (item: Microsoft.CodeAnalysis.Completion.CompletionItem)
        (description: Microsoft.CodeAnalysis.Completion.CompletionDescription option) =
        { CompletionItem.Create(item.DisplayText) with
            Kind             = item.Tags |> Seq.tryHead |> Option.map roslynTagToLspCompletion
            SortText         = item.SortText |> Option.ofString
            FilterText       = item.FilterText |> Option.ofString
            Detail           = item.InlineDescription |> Option.ofString
            TextEditText     = item.DisplayTextPrefix |> Option.ofObj
            InsertTextFormat = Some InsertTextFormat.PlainText
            // TODO: Change description to MarkupContent instead of plain text?
            Documentation    = description |> Option.map (fun x -> Documentation.String x.Text) }

    let handle (wm: IWorkspaceManager) (p: CompletionParams) : AsyncLspResult<CompletionList option> = async {
        match wm.GetDocument p.TextDocument.Uri with
        | None -> return None |> success
        | Some doc ->
            let! ct = Async.CancellationToken
            let! sourceText = doc.GetTextAsync(ct) |> Async.AwaitTask

            let position =
                sourceText.Lines.GetPosition(LinePosition(p.Position.Line, p.Position.Character))

            let completionService = CompletionService.GetService(doc)
            // TODO: Avoid unnecessary GetCompletionsAsync. For example, for the first time, we will always get
            // `AbandonedMutexException`, `Accessibility`, ..., which are unnecessary and time-consuming.
            let! completions = completionService.GetCompletionsAsync(doc, position, cancellationToken=ct) |> Async.AwaitTask

            match Option.ofObj completions with
            | None -> return None |> success
            | Some completions ->
                // TODO: Move it to resolve? But it needs us to remember/cache the completions.ItemsList.
                let! descriptions =
                    completions.ItemsList
                    |> Seq.map (fun item -> completionService.GetDescriptionAsync(doc, item, ct) |> Async.AwaitTask)
                    |> Async.Parallel
                    |> Async.map (Seq.map Option.ofObj)
                let items =
                    Seq.zip completions.ItemsList descriptions
                    |> Seq.map (uncurry makeLspCompletionItem)
                    |> Array.ofSeq
                let completionList =
                    { IsIncomplete = false
                      Items = items
                      ItemDefaults = None }
                return completionList |> Some |> success
    }

    let resolve (wm: IWorkspaceManager) (p: CompletionItem) : AsyncLspResult<CompletionItem> = notImplemented
