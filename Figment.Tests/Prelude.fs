namespace Figment.Tests

[<AutoOpen>]
module Assertions =
    open System
    open Fuchu

    type Assert with
        static member inline Cast v : 'b =
            try
                unbox v
            with _ -> failtestf "Expected type: %A\nActual type: %A" typeof<'b> (v.GetType())
            
    