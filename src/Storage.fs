/// Persistence: browser localStorage, plus an optional cloud-sync module.
/// Fable-only module (everything above it is platform-neutral).
module NightCourt.Storage

open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom
open Browser.WebStorage
open NightCourt.Domain
open NightCourt.Codec

let [<Literal>] private DataKey = "nightcourt-data-v1"

let saveData (data: AppData) =
    try localStorage.setItem (DataKey, serialize data)
    with _ -> ()

let loadData () : AppData option =
    try
        match localStorage.getItem DataKey with
        | null | "" -> None
        | json ->
            match deserialize json with
            | Ok data -> Some data
            | Error _ -> None // schema mismatch → fall back to seed
    with _ -> None

/// Optional cloud sync. Point SyncEndpoint at any URL that accepts
/// PUT (save) and GET (load) of the JSON blob — a tiny worker, a
/// key-value store, or a home-server route all work. Everything still
/// functions fully offline; sync is additive.
module Sync =

    let private fetchJs (url: string) (opts: obj) : JS.Promise<obj> =
        window?fetch (url, opts)

    /// Fire-and-forget push of the current data to the endpoint.
    let push (data: AppData) =
        match data.SyncEndpoint with
        | Some url when url.Trim() <> "" ->
            try
                fetchJs (url.Trim())
                    (createObj
                        [ "method" ==> "PUT"
                          "headers" ==> createObj [ "Content-Type" ==> "application/json" ]
                          "body" ==> serialize data ])
                |> ignore
            with _ -> ()
        | _ -> ()

    /// Pull data from the endpoint; calls back with the parse result.
    let pull (url: string) (onResult: Result<AppData, string> -> unit) =
        try
            fetchJs (url.Trim()) (createObj [ "method" ==> "GET" ])
            |> fun p ->
                p?``then`` (fun (resp: obj) -> resp?text ())
                |> fun (p2: JS.Promise<obj>) ->
                    p2?``then`` (fun (text: obj) -> onResult (deserialize (string text)))
                    |> fun (p3: JS.Promise<obj>) ->
                        p3?catch (fun _ -> onResult (Error "The cloud did not answer.")) |> ignore
        with _ ->
            onResult (Error "The cloud did not answer.")
