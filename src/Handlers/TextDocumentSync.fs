namespace CSharpLanguageServer.Handlers

open System
open FSharpPlus
open Ionide.LanguageServerProtocol.Server
open Ionide.LanguageServerProtocol.Types

open CSharpLanguageServer.Common.Types
open CSharpLanguageServer.Common.LspUtil

module TextDocumentSync =
    let private dynamicRegistration (clientCapabilities: ClientCapabilities option) =
        clientCapabilities
        >>= fun x -> x.TextDocument
        >>= fun x -> x.Synchronization
        >>= fun x -> x.DynamicRegistration
        |> Option.defaultValue false

    let provider (clientCapabilities: ClientCapabilities option) : TextDocumentSyncOptions option =
        match dynamicRegistration clientCapabilities with
        | true -> None
        | false ->
            Some
                { TextDocumentSyncOptions.Default with
                    OpenClose = Some true
                    Save = Some { IncludeText = Some true }
                    Change = Some TextDocumentSyncKind.Incremental }

    let didOpenRegistration (clientCapabilities: ClientCapabilities option) : Registration option =
        match dynamicRegistration clientCapabilities with
        | false -> None
        | true ->
            Some
                { Id = Guid.NewGuid().ToString()
                  Method = "textDocument/didOpen"
                  RegisterOptions = { DocumentSelector = Some defaultDocumentSelector } |> serialize |> Some }


    let didChangeRegistration (clientCapabilities: ClientCapabilities option) : Registration option =
        match dynamicRegistration clientCapabilities with
        | false -> None
        | true ->
            Some
                { Id = Guid.NewGuid().ToString()
                  Method = "textDocument/didChange"
                  RegisterOptions =
                    { DocumentSelector = Some defaultDocumentSelector
                      SyncKind = TextDocumentSyncKind.Incremental } |> serialize |> Some }

    let didSaveRegistration (clientCapabilities: ClientCapabilities option) : Registration option =
        match dynamicRegistration clientCapabilities with
        | false -> None
        | true ->
            Some
                { Id = Guid.NewGuid().ToString()
                  Method = "textDocument/didSave"
                  RegisterOptions =
                    { DocumentSelector = Some defaultDocumentSelector
                      IncludeText = Some true } |> serialize |> Some }

    let didCloseRegistration (clientCapabilities: ClientCapabilities option) : Registration option =
        match dynamicRegistration clientCapabilities with
        | false -> None
        | true ->
            Some
                { Id = Guid.NewGuid().ToString()
                  Method = "textDocument/didClose"
                  RegisterOptions = { DocumentSelector = Some defaultDocumentSelector } |> serialize |> Some }

    let willSaveRegistration (clientCapabilities: ClientCapabilities option) : Registration option = None

    let willSaveWaitUntilRegistration (clientCapabilities: ClientCapabilities option) : Registration option = None

    let didOpen (wm: IWorkspaceManager) (p: DidOpenTextDocumentParams): Async<unit> =
        wm.OpenDocument p.TextDocument.Uri p.TextDocument.Version p.TextDocument.Text

    let didChange (wm: IWorkspaceManager) (p: DidChangeTextDocumentParams): Async<unit> =
        wm.ChangeDocument p.TextDocument.Uri p.TextDocument.Version p.ContentChanges

    let didClose (wm: IWorkspaceManager) (p: DidCloseTextDocumentParams): Async<unit> =
        wm.CloseDocument p.TextDocument.Uri

    let willSave (wm: IWorkspaceManager) (p: WillSaveTextDocumentParams): Async<unit> = ignoreNotification

    let willSaveUntil (wm: IWorkspaceManager) (p: WillSaveTextDocumentParams): AsyncLspResult<TextEdit [] option> = notImplemented

    let didSave (wm: IWorkspaceManager) (p: DidSaveTextDocumentParams): Async<unit> =
        wm.SaveDocument p.TextDocument.Uri p.Text
