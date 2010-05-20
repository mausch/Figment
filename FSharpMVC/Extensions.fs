module Extensions

open System
open System.Reflection
open Microsoft.FSharp.Reflection
open System.Text.RegularExpressions

type CaptureCollection with
    member this.Captures 
        with get() =
            this |> Seq.cast<Capture> |> Seq.toArray

type GroupCollection with
    member this.Groups
        with get() =
            this |> Seq.cast<Group> |> Seq.toArray

type FSharpType with
    static member GetFlattenedFunctionElements (functionType: Type) =
        let domain, range = FSharpType.GetFunctionElements functionType
        if not (FSharpType.IsFunction range)
            then domain::[range]
            else domain::FSharpType.GetFlattenedFunctionElements(range)



type FSharpValue with
    static member InvokeFunction (f: obj) (args: obj list): obj =
        let ft = f.GetType()
        if not (FSharpType.IsFunction ft)
            then failwith "Not a function!"
        let domain, range = FSharpType.GetFunctionElements ft
        let fsft = typedefof<FSharpFunc<_,_>>.MakeGenericType [| domain; range |]
        let bindingFlags = BindingFlags.InvokeMethod ||| BindingFlags.Instance ||| BindingFlags.Public
        let r = fsft.InvokeMember("Invoke", bindingFlags, null, f, [| List.head args |])
        if not (FSharpType.IsFunction(r.GetType()))
            then box r
            else FSharpValue.InvokeFunction r (List.tail args)
