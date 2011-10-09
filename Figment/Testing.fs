namespace Figment

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open System.Diagnostics
open System.Web
open System.Web.Mvc
open System.Web.Routing
open FSharpx

module Testing =
    let dummyController = { new ControllerBase() with member x.ExecuteCore() = () }

    let buildCtx ctx = 
        let req = RequestContext(ctx, RouteData())
        ControllerContext(req, dummyController)

    let buildRequest verb path =
        { new HttpContextBase() with
            override x.Request = 
                { new HttpRequestBase() with
                    override y.ValidateInput() = ()
                    override y.HttpMethod = verb
                    override y.RawUrl = path
                    override y.PathInfo = path
                    override y.AppRelativeCurrentExecutionFilePath = "~/"
                    override y.Path = path
                    override y.Url = Uri("http://localhost" + path) }}

    let buildResponse resp = 
        { new HttpContextBase() with
            override x.Response = resp }

    let withRequest request ctx =
        { new DelegatingHttpContextBase(ctx) with
            override x.Request = request } :> HttpContextBase

    let withResponse response ctx = 
        { new DelegatingHttpContextBase(ctx) with
            override x.Response = response } :> HttpContextBase

    let withForm form ctx =
        ctx 
        |> withRequest
            { new DelegatingHttpRequestBase(ctx.Request) with
                override y.Form = form }

    let fileCollection (files: seq<string * HttpPostedFileBase>) = 
        { new HttpFileCollectionBase() with
            override x.AllKeys = files |> Seq.map fst |> Seq.toArray
            override x.Item
                with get (k: string) = 
                    files 
                    |> Seq.tryFind (fst >> (=)k) 
                    |> Option.map snd 
                    |> Option.getOrDefault }

    let withFiles files ctx = 
        ctx
        |> withRequest
            { new DelegatingHttpRequestBase(ctx.Request) with
                override y.Files = fileCollection files }
    
    let withQueryString querystring ctx =
        ctx 
        |> withRequest
            { new DelegatingHttpRequestBase(ctx.Request) with
                override y.QueryString = querystring }
        
    let tryGetController verb path =
        let ctx = buildRequest verb path
        RouteTable.Routes.tryGetRouteData ctx
        |> Option.map (fun route ->
                        let rctx = RequestContext(ctx, route)
                        let handler : Figment.IControllerProvider = unbox <| route.RouteHandler.GetHttpHandler(rctx)
                        route, handler.CreateController())

    let getController verb path =
        tryGetController verb path
        |> Option.getOrElseF (fun () -> failwithf "No controller found for %s %s" verb path)

    let stubSession ctx = 
        let session = OrderedDictionary()
        let timeout = ref 0
        { new DelegatingHttpContextBase(ctx) with
            override x.Session = 
                { new HttpSessionStateBase() with
                    override y.Abandon() = 
                        session.Clear()
                    override y.Clear() = 
                        session.Clear()
                    override y.CopyTo(array, index) = 
                        (session :> ICollection).CopyTo(array, index)
                    override y.GetEnumerator() = 
                        session.Keys.GetEnumerator()
                    override y.Item 
                        with get (k:string) = session.[k]
                        and set (k: string) (v:obj) = session.[k] <- v
                    override y.Item 
                        with get (k: int) = session.[k]
                        and set (k: int) (v:obj) = session.[k] <- v
                    override y.Remove k =
                        session.Remove k
                    override y.RemoveAll() = session.Clear()
                    override y.RemoveAt i = session.RemoveAt i
                    override y.Timeout
                        with get() = !timeout
                        and set v = timeout := v
                 } } :> HttpContextBase    

//    let buildResponse route resp =
//        let ctx =
//            { new HttpContextBase() with
//                override x.Session = 
//                    { new HttpSessionStateBase() with
//                        override y.Item 
//                            with get (k:string) = box null 
//                            and set (k: string) (v:obj) = () }
//                override x.Request =
//                    { new HttpRequestBase() with
//                        override y.ValidateInput() = ()
//                        override y.Path = "" }
//                override x.Response = resp }
//        RequestContext(ctx, route)
//    
//
