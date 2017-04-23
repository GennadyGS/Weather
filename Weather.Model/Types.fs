namespace Weather.Model

open System.Net

type ObservationTime = 
    { Date: System.DateTime
      Hour: byte } 
    with
        member this.ToDateTime() = 
            this.Date.AddHours(float this.Hour)

type StationNumber = 
    | StationNumber of int

type ObservationHeader = 
    { StationNumber: StationNumber
      ObservationTime: ObservationTime }

type Observation = 
    { Header : ObservationHeader
      Temperature: decimal }

type Failure =
    | DatabaseError of string
    | HttpError of (WebExceptionStatus * string)
    | InvalidHeaderFormat of string
    | InvalidObservationFormat of (ObservationHeader * string)
