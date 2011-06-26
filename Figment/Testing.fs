namespace Figment

open System
open System.Diagnostics
open System.Web
open System.Web.Routing

module Testing =

    let buildRequest verb path =
        { new HttpContextBase() with
            override x.Request = 
                { new HttpRequestBase() with
                    override y.HttpMethod = verb
                    override y.RawUrl = path
                    override y.PathInfo = path
                    override y.AppRelativeCurrentExecutionFilePath = "~/"
                    override y.Path = "/" + path
                    override y.Url = Uri("http://localhost/" + path) }}

    let getController verb path =
        let ctx = buildRequest verb path
        let route = RouteTable.Routes.GetRouteData ctx
        Debug.Assert(route <> null)
        let rctx = RequestContext(ctx, route)
        let handler : Figment.IControllerProvider = unbox <| route.RouteHandler.GetHttpHandler(rctx)
        Debug.Assert(box handler <> null)
        route, handler.CreateController()

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
    

