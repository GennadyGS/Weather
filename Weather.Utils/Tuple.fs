module Weather.Utils.Tuple

let mapSecondOption f (a, b) =
    match f b with
    | Some(c) -> Some (a, c)
    | None -> None
