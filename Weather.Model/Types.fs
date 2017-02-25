namespace Weather.Model

type ObservationTime = {
    Date: System.DateTime;
    Hour: byte;
} with
    member this.ToDateTime() = 
        this.Date.AddHours(float this.Hour)

type Observation = {
    Time: ObservationTime; 
    StationNumber: int;
    Temperature: decimal
}

