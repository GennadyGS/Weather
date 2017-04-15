module Weather.Utils.Result

let map f xResult = 
    match xResult with
    | Success success -> Success (f success)
    | Failure failure -> Failure failure

let bind f xResult = 
    match xResult with
    | Success success -> f success
    | Failure failure -> Failure failure

let mapToList func result = 
    match result with
        | Success success -> func success |> List.map Success
        | Failure failure -> [Failure failure]

let bindToList func result = 
    match result with
        | Success success -> func success
        | Failure failure -> [Failure failure]

let mapToOption func result = 
    match result with
        | Success success -> func success |> Option.map Success
        | Failure failure -> Some (Failure failure)

let bindToOption func result = 
    match result with
        | Success success -> func success
        | Failure failure -> Some (Failure failure)

type ResultBuilder() =
    member this.Return x = Success x
    member this.ReturnFrom x = x
    member this.Zero() = Success ()
    member this.Bind(xResult, f) = bind f xResult
    
let result = ResultBuilder()
