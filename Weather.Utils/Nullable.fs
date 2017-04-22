module Weather.Utils.Nullable
    
let toOption item = 
    if (isNull (box item)) then None else Some(item)


