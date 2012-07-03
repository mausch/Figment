namespace Figment.Tests

[<AutoOpen>]
module Assertions =
    open System
    open Fuchu

    let inline assertEqual expected actual =
        if expected <> actual
            then failtestf "Expected: %A\nActual: %A" expected actual

    let inline assertNotNull x =
        if x = null 
            then failtest "Should not have been null"

    let inline assertRaise (ex: Type) f =
        try
            f()
            failtestf "Expected exception '%s' but no exception was raised" ex.FullName
        with e ->
            if e.GetType() <> ex
                then failtestf "Expected exception '%s' but raised:\n%A" ex.FullName e
                
    let inline assertType v : 'b = 
        try
            unbox v
        with _ -> failtestf "Expected type: %A\nActual type: %A" typeof<'b> (v.GetType())
    