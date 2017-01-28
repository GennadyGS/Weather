namespace Weather.Utils

open System

type Result<'a, 'b> = 
    | Success of 'a
    | Failure of 'b

type DateTimeInterval = {
    From : DateTime;
    To: DateTime
}
