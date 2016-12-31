namespace Weather.Model

type Observation = {
    Date: System.DateTime;
    Hour: byte;
    StationNumber: int;
    Temperature: decimal option
}
