namespace Weather.Utils

module String =
    let split (chars : char array) (string : string) : string[] = 
        string.Split(chars)
