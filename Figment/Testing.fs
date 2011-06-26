namespace Figment

open System
open System.Diagnostics
open System.Web
open System.Web.Routing

module Option =
    type MaybeBuilder() =
        member x.Bind(a,f) = Option.bind f a
        member x.Return v = Some v
        member x.Zero() = None
    let builder = MaybeBuilder()
    let inline getOrElse v =
        function
        | Some v -> v
        | _ -> v            

module Testing =

    type MaybeBuilder() =
        member x.Bind(a,f) = Option.bind f a
        member x.Return v = Some v
        member x.Zero() = None

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

    let tryGetController verb path =
        let ctx = buildRequest verb path
        Option.builder {
            let! route = RouteTable.Routes.tryGetRouteData ctx
            let rctx = RequestContext(ctx, route)
            let handler : Figment.IControllerProvider = unbox <| route.RouteHandler.GetHttpHandler(rctx)
            return route, handler.CreateController()
        }

    let getController verb path =
        tryGetController verb path
        |> Option.getOrElse (failwithf "No controller found for %s %s" verb path)

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
    

