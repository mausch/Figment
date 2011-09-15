namespace Figment

open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Microsoft.FSharp.Reflection
open System.Text.RegularExpressions
open System.Web.Caching
open System.Linq

[<AutoOpen>]
module Extensions = 
    let internal bindingFlags = BindingFlags.NonPublic ||| BindingFlags.Instance
    let internal underlyingRequest = typeof<HttpRequestWrapper>.GetField("_httpRequest", bindingFlags)
    let internal httpRequestFlags = typeof<HttpRequest>.GetField("_flags", bindingFlags)
    let internal simpleBitVectorClear = httpRequestFlags.FieldType.GetMethod("Clear", bindingFlags)
    let internal simpleBitVectorIntValueSet = httpRequestFlags.FieldType.GetProperty("IntegerValue", bindingFlags)
    let internal formValidation = 2

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

    type RouteCollection with
        /// <summary>
        /// Shallow clone of routes.
        /// Does not copy route names.
        /// </summary>
        member x.Clone() =
            let r = RouteCollection()
            use lok = x.GetReadLock()
            Seq.iter r.Add x
            r

        member x.tryGetRouteData ctx =
            match x.GetRouteData ctx with
            | null -> None
            | a -> Some a

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
        member x.DisableValidation() =
            let httpRequest = underlyingRequest.GetValue(x)
            let flags = httpRequestFlags.GetValue(httpRequest)
            simpleBitVectorIntValueSet.SetValue(flags, 0, null)
            httpRequestFlags.SetValue(httpRequest, flags)

    type HttpResponseBase with
        member x.Writef fmt = Printf.ksprintf x.Write fmt
        member x.Writefn fmt = 
            let printn (s: string) = 
                x.Write s
                x.Write Environment.NewLine
            Printf.ksprintf printn fmt

    type HttpContextBase with
        member x.GetService() : 'a =
            unbox (x.GetService typeof<'a>)
        member x.UnderlyingHttpContext = 
            x.GetService<HttpApplication>().Context

    type HttpSessionStateBase with
        member x.Get (key: string) = unbox x.[key]
        member x.Pop (key: string) =
            let v = x.[key]
            x.Remove key
            unbox v
        member x.Set (key: string) v = x.[key] <- v
        member x.AsDictionary() =
            let notimpl() = raise <| NotImplementedException()
            let getEnumerator() =
                let sessionEnum = x.GetEnumerator()
                let wrapElem (o: obj) = 
                    let key = o :?> string
                    let value = x.[key]
                    KeyValuePair(key,value)
                { new IEnumerator<KeyValuePair<string,obj>> with
                    member e.Current = wrapElem sessionEnum.Current
                    member e.MoveNext() = sessionEnum.MoveNext()
                    member e.Reset() = sessionEnum.Reset()
                    member e.Dispose() = ()
                    member e.Current = box (wrapElem sessionEnum.Current) }
            { new IDictionary<string,obj> with
                member d.Count = x.Count
                member d.IsReadOnly = false 
                member d.Item 
                    with get k = 
                        let v = x.[k]
                        if v = null
                            then raise <| KeyNotFoundException(sprintf "Key '%s' not found" k)
                            else v
                    and set k v = x.Add(k,v)
                member d.Keys = upcast ResizeArray<string>(x.Keys |> Seq.cast)
                member d.Values = 
                    let values = ResizeArray<obj>()
                    for i in 0..x.Count-1 do
                        values.Add x.[i]
                    upcast values
                member d.Add v = d.Add(v.Key, v.Value)
                member d.Add(key,value) = 
                    if key = null
                        then raise <| ArgumentNullException("key")
                    if d.ContainsKey key
                        then raise <| ArgumentException(sprintf "Duplicate key '%s'" key, "key")
                    x.Add(key,value)
                member d.Clear() = x.Clear()
                member d.Contains item = x.[item.Key] = item.Value
                member d.ContainsKey key = x.[key] <> null
                member d.CopyTo(array,arrayIndex) = notimpl()
                member d.GetEnumerator() = getEnumerator()
                member d.GetEnumerator() = getEnumerator() :> IEnumerator
                member d.Remove (item: KeyValuePair<string,obj>) = 
                    if d.Contains item then
                        x.Remove item.Key
                        true
                    else
                        false
                member d.Remove (key: string) = 
                    let exists = d.ContainsKey key
                    x.Remove key
                    exists
                member d.TryGetValue(key: string, value: byref<obj>) = 
                    if d.ContainsKey key then
                        value <- x.[key]
                        true
                    else
                        false
                }

    type ControllerContext with
        member x.UrlHelper = UrlHelper(x.RequestContext)
        member x.Cache = x.HttpContext.Cache
        member x.Session = x.HttpContext.Session
        member x.SessionDict = x.Session.AsDictionary()
        member x.Request = x.HttpContext.Request
        member x.Response = x.HttpContext.Response
        member x.Url = x.Request.Url
        member x.QueryString = x.Request.QueryString
        member x.Form = x.Request.Form
        member x.IP = x.Request.UserHostAddress
        member x.GetValue n = 
            let r = x.Controller.ValueProvider.GetValue n
            unbox r.RawValue
        member x.Item 
            with get k = x.Request.[k]