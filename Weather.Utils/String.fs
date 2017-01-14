namespace Weather.Utils

module String =
    let splitString (char : char) (string : string) : string[] = 
        string.Split([|char|])
