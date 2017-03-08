namespace Weather.Model

type ObservationTime = {
    Date: System.DateTime
    Hour: byte
} with
    member this.ToDateTime() = 
        this.Date.AddHours(float this.Hour)

type ObservationHeader = 
    { StationNumber: int
      ObservationTime: ObservationTime }

type Observation = 
    { Header : ObservationHeader
      Temperature: decimal }

type ParseObservationFailure =
    | InvalidHeaderFormat of string
    | InvalidObservationFormat of (ObservationHeader * string)

type ParseObservationsResults = 
    { Success: Observation list
      WithInvalidObservationFormat : (ObservationHeader * string) list 
      WithInvalidHeaderFormat : string list }