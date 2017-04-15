module Weather.Utils.Result

let map func = function
    | Success success -> Success (func success)
    | Failure failure -> Failure failure

let mapBoth mapSuccess mapFailure = function
    | Success success -> mapSuccess success
    | Failure failure -> mapFailure failure

let bind func = function
    | Success success -> func success
    | Failure failure -> Failure failure

let mapToList func = function
    | Success success -> func success |> List.map Success
    | Failure failure -> [Failure failure]

let bindToList func = function
    | Success success -> func success
    | Failure failure -> [Failure failure]

let mapToOption func = function
    | Success success -> func success |> Option.map Success
    | Failure failure -> Some (Failure failure)

let mapFailureToOption func = function
    | Success success -> Some (Success success)
    | Failure failure -> func failure |> Option.map Failure

let bindToOption func = function
    | Success success -> func success
    | Failure failure -> Some (Failure failure)

type ResultBuilder() =
    member this.Return x = Success x
    member this.ReturnFrom x = x
    member this.Zero() = Success ()
    member this.Bind(xResult, f) = bind f xResult
    
let result = ResultBuilder()
