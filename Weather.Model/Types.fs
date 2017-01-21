namespace Weather.Model

type ObservationTime = {
    Date: System.DateTime;
    Hour: byte;
}

type Observation = {
    Time: ObservationTime; 
    StationNumber: string;
    Temperature: decimal
}

