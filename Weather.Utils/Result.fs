module Weather.Utils.Result

let filterSuccess results
    = results 
    |> List.choose (
        function
        | Success observation -> Some observation
        | _ -> None)

let map f xResult = 
    match xResult with
    | Success success -> Success (f success)
    | Failure failure -> Failure failure

let bind f xResult = 
    match xResult with
    | Success success -> f success
    | Failure failure -> Failure failure

type ResultBuilder() =
    member this.Return x = Success x
    member this.ReturnFrom x = x
    member this.Zero() = Success ()
    member this.Bind(xResult, f) = bind f xResult
    
let result = ResultBuilder()
