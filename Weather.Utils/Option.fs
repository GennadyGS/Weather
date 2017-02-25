module Weather.Utils.Option

let inline (|??) (a: 'a option) b = 
    if a.IsSome then a.Value else b  

