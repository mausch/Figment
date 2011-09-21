namespace Figment

open System
open System.Diagnostics
open System.Web
open System.Web.Routing
open FSharpx

module Testing =

    let buildRequest verb path =
        { new HttpContextBase() with
            override x.Request = 
                { new HttpRequestBase() with
                    override y.ValidateInput() = ()
                    override y.HttpMethod = verb
                    override y.RawUrl = path
                    override y.PathInfo = path
                    override y.AppRelativeCurrentExecutionFilePath = "~/"
                    override y.Path = "/" + path
                    override y.Url = Uri("http://localhost/" + path) }}

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

    let buildResponse route resp =
        let ctx =
            { new HttpContextBase() with
                override x.Session = 
                    { new HttpSessionStateBase() with
                        override y.Item 
                            with get (k:string) = box null 
                            and set (k: string) (v:obj) = () }
                override x.Request =
                    { new HttpRequestBase() with
                        override y.ValidateInput() = ()
                        override y.Path = "" }
                override x.Response = resp }
        RequestContext(ctx, route)
    

