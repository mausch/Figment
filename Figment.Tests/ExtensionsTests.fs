module Figment.Tests.Extensions

open System
open Microsoft.FSharp.Reflection
open Figment.Extensions
open Fuchu

[<Tests>]
let tests =
    testList "Extensions" [
        testCase "InvokeFunction" <| fun _ ->
            let f a b c = a + b + c
            let r = FSharpValue.InvokeFunction f [1;2;3]
            Assert.NotNull("InvokeFunction result", r)
            let r : int = Assert.Cast r
            Assert.Equal("InvokeFunction result", 6, r)

        testCase "GetFlattenedFunctionElements throws on non-function" <| fun _ ->
            let a = 2
            let r () = FSharpType.GetFlattenedFunctionElements (a.GetType())
            Assert.Raise("", typeof<ArgumentException>, r >> ignore)

        testCase "GetFlattenedFunctionElements on (unit -> int)" <| fun _ ->
            let f() = 2
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            Assert.Equal("elements", [typeof<unit>; typeof<int>], t)

        testCase "GetFlattenedFunctionElements on (int -> float -> string)" <| fun _ ->
            let f (i: int) (j: float) = "bla"
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            Assert.Equal("", [typeof<int>; typeof<float>; typeof<string>], t)
    ]
