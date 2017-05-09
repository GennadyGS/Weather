module Weather.Utils.Tuple

let mapFirst f (a, b) =
    (f a, b)

let mapSecond f (a, b) =
    (a, f b)

let mapFirstOption f (a, b) =
    match f a with
    | Some(c) -> Some (c, b)
    | None -> None

let mapSecondOption f (a, b) =
    match f b with
    | Some(c) -> Some (a, c)
    | None -> None
