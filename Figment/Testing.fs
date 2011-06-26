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
                    override y.PathInfo = ""
                    override y.AppRelativeCurrentExecutionFilePath = "~/"
                    override y.Url = Uri("http://localhost/" + path) }}

    let getController verb path =
        let route = buildRequest verb path |> RouteTable.Routes.GetRouteData
        Debug.Assert(route <> null)
        let handler : Figment.IControllerProvider = unbox <| route.RouteHandler.GetHttpHandler(RequestContext())
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
    

