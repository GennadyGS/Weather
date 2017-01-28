namespace Weather.Utils

module String =
    let splitString (chars : char array) (string : string) : string[] = 
        string.Split(chars)
