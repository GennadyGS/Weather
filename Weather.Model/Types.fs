namespace Weather.Model

type ObservationTime = {
    Date: System.DateTime
    Hour: byte
} with
    member this.ToDateTime() = 
        this.Date.AddHours(float this.Hour)

type ParseObservationFailure =
    | InvalidHeaderFormat of string
    | InvalidObservationFormat of string

type ObservationHeader = {
    Time: ObservationTime
    StationNumber: int
}

type Observation = {
    Header : ObservationHeader
    Temperature: decimal
}

