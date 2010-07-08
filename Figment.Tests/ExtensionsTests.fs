module ExtensionsTests

open Xunit
open Microsoft.FSharp.Reflection
open Extensions

[<Fact>]
let InvokeFunctionTests() =
    let f a b c = a + b + c
    let r = FSharpValue.InvokeFunction f [1;2;3]
    Assert.NotNull r
    Assert.IsType<int> r |> ignore
    Assert.Equal(6, r :?> int)
    ()