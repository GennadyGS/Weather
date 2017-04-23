namespace Weather.Model

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
    | InvalidHeaderFormat of string
    | InvalidObservationFormat of (ObservationHeader * string)
