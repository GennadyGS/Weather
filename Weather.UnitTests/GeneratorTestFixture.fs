namespace Weather.TestUtils

open FsCheck

type GeneratorTestFixture() =
    do Arb.register<Generators>() |> ignore
