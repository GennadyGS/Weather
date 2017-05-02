namespace Weather.UnitTests

open FsCheck

type GeneratorTests() =
    do Arb.register<Generators>() |> ignore
