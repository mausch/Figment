namespace Figment

open System
open System.Reflection
open System.Web
open System.Web.Mvc
open Microsoft.FSharp.Reflection
open System.Text.RegularExpressions
open System.Web.Caching

module Extensions = 

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


    type ControllerContext with
        member x.UrlHelper with get() = UrlHelper(x.RequestContext)
        member x.Cache with get() = x.HttpContext.Cache

    type Cache with
        member x.GetOrAdd(key: string, valueFactory: string -> 'a, ?dependencies, ?absoluteExpiration, ?slidingExpiration, ?priority, ?onRemoveCallback): 'a = 
            let item = x.Get(key)
            if item <> null then
                unbox item
            else
                let dependencies = defaultArg dependencies null
                let absoluteExpiration = defaultArg absoluteExpiration Cache.NoAbsoluteExpiration
                let slidingExpiration = defaultArg slidingExpiration Cache.NoSlidingExpiration
                let priority = defaultArg priority CacheItemPriority.Default
                let onRemoveCallback = defaultArg onRemoveCallback (fun _ _ _ -> ())
                let onRemoveCallback = CacheItemRemovedCallback(onRemoveCallback)
                let value = valueFactory key
                x.Add(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback) |> ignore
                value
            
    type HttpRequestBase with
        member x.files =
            x.Files.AllKeys
            |> Seq.map (fun k -> k, x.Files.[k])