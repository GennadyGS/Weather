module Weather.Utils.Result

let map func = function
    | Success success -> Success <| func success
    | Failure failure -> Failure failure

let mapFailure func = function
    | Success success -> Success success
    | Failure failure -> Failure <| func failure

let mapBoth successFunc failureFunc = function
    | Success success -> Success <| successFunc success
    | Failure failure -> Failure <| failureFunc failure

let bind func = function
    | Success success -> func success
    | Failure failure -> Failure failure

let bindBoth successFunc failureFunc = function
    | Success success -> successFunc success
    | Failure failure -> failureFunc failure

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

let ignore (result : Result<unit, unit>) = ()

let ignoreAll (result : Result<unit, unit> seq) = ()

type ResultBuilder() =
    member this.Return x = Success x
    member this.ReturnFrom x = x
    member this.Zero() = Success ()
    member this.Bind(xResult, f) = bind f xResult
    
let result = ResultBuilder()
