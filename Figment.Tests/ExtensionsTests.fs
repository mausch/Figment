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
            assertNotNull r
            let r : int = assertType r
            assertEqual 6 r

        testCase "GetFlattenedFunctionElements throws on non-function" <| fun _ ->
            let a = 2
            assertRaise typeof<ArgumentException>
                (fun () -> FSharpType.GetFlattenedFunctionElements (a.GetType()) |> ignore)

        testCase "GetFlattenedFunctionElements on (unit -> int)" <| fun _ ->
            let f() = 2
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            assertEqual [typeof<unit>; typeof<int>] t

        testCase "GetFlattenedFunctionElements on (int -> float -> string)" <| fun _ ->
            let f (i: int) (j: float) = "bla"
            let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
            assertEqual [typeof<int>; typeof<float>; typeof<string>] t
    ]
