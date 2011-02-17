module ExtensionsTests

open System
open Xunit
open Microsoft.FSharp.Reflection
open Figment.Extensions

let assertThrows<'e when 'e :> exn> f = 
    Assert.Throws<'e>(Assert.ThrowsDelegate(f)) |> ignore

let (=.) a b = Assert.Equal(b,a)

[<Fact>]
let InvokeFunction() =
    let f a b c = a + b + c
    let r = FSharpValue.InvokeFunction f [1;2;3]
    Assert.NotNull r
    let r = Assert.IsType<int> r
    Assert.Equal(6, r)

[<Fact>]
let GetFlattenedFunctionElements_non_function() =
    let a = 2
    assertThrows<ArgumentException>(fun () -> FSharpType.GetFlattenedFunctionElements (a.GetType()) |> ignore)

[<Fact>]
let GetFlattenedFunctionElements_unit_int() =
    let f() = 2
    let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
    t.Length =. 2
    t.[0] =. typeof<unit>
    t.[1] =. typeof<int>    

[<Fact>]
let GetFlattenedFunctionElements_int_float_string() =
    let f (i: int) (j: float) = "bla"
    let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
    t.Length =. 3
    t.[0] =. typeof<int>
    t.[1] =. typeof<float>
    t.[2] =. typeof<string>
    