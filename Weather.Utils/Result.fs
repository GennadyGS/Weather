namespace Weather.Utils

module Result = 
    type Result<'a, 'b> = 
        | Success of 'a
        | Failure of 'b

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
        member this.Zero() = Success ()
        member this.Bind(xResult, f) = bind f xResult
    
    let result = ResultBuilder()
