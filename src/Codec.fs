/// JSON (de)serialization for persistence and sync. Thoth.Json in the browser,
/// Thoth.Json.Net in the .NET test suite — identical auto-coder semantics,
/// which lets the tests verify the exact persistence round-trip.
module NightCourt.Codec

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

open NightCourt.Domain

let serialize (data: AppData) : string =
    Encode.Auto.toString (0, data)

let deserialize (json: string) : Result<AppData, string> =
    Decode.Auto.fromString<AppData> json
