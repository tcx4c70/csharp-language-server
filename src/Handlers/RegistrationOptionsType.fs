namespace CSharpLanguageServer.Handlers

open Ionide.LanguageServerProtocol.Types


type TextDocumentRegistrationOptions = { DocumentSelector: DocumentSelector option }

type TextDocumentChangeRegistrationOptions = { DocumentSelector: DocumentSelector option; SyncKind: TextDocumentSyncKind }

type TextDocumentSaveRegistrationOptions = { DocumentSelector: DocumentSelector option; IncludeText: bool option }
