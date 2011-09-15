module ExtensionsTests

open System
open Xunit
open Microsoft.FSharp.Reflection
open Figment.Extensions

let assertThrows<'e when 'e :> exn> f = 
    Assert.Throws<'e>(Assert.ThrowsDelegate(f)) |> ignore

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
    Assert.Equal(2, t.Length)
    Assert.Equal(typeof<unit>, t.[0])
    Assert.Equal(typeof<int>, t.[1])

[<Fact>]
let GetFlattenedFunctionElements_int_float_string() =
    let f (i: int) (j: float) = "bla"
    let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
    Assert.Equal(3, t.Length)
    Assert.Equal(typeof<int>, t.[0])
    Assert.Equal(typeof<float>, t.[1])
    Assert.Equal(typeof<string>, t.[2])

open System.Collections.Specialized
open System.Linq
    
