module ExtensionsTests

open System
open Xunit
open Microsoft.FSharp.Reflection
open Figment.Extensions
open Fuchu

let assertThrows<'e when 'e :> exn> f = 
    Assert.Throws<'e>(Assert.ThrowsDelegate(f)) |> ignore

[<Tests>]
let tests =
    testList "Extensions" [
        testCase "InvokeFunction" <| fun _ ->
            let f a b c = a + b + c
            let r = FSharpValue.InvokeFunction f [1;2;3]
            Assert.NotNull r
            let r = Assert.IsType<int> r
            Assert.Equal(6, r)

        testCase "GetFlattenedFunctionElements throws on non-function" <| fun _ ->
            let a = 2
            assertThrows<ArgumentException>(fun () -> FSharpType.GetFlattenedFunctionElements (a.GetType()) |> ignore)

        testCase "GetFlattenedFunctionElements on (unit -> int)" <| fun _ ->
            let f() = 2
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            Assert.Equal(2, t.Length)
            Assert.Equal(typeof<unit>, t.[0])
            Assert.Equal(typeof<int>, t.[1])

        testCase "GetFlattenedFunctionElements on (int -> float -> string)" <| fun _ ->
            let f (i: int) (j: float) = "bla"
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            Assert.Equal(3, t.Length)
            Assert.Equal(typeof<int>, t.[0])
            Assert.Equal(typeof<float>, t.[1])
            Assert.Equal(typeof<string>, t.[2])
    ]
