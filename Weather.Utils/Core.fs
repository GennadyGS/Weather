module Weather.Utils.Core

let tee sideEffect =
    fun x ->
        do sideEffect x
        x


