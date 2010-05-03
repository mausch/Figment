module FSharpMvc.Routing

open System.Web
open System.Web.Mvc
open System.Web.Routing
open Combinators

type RouteCollection with
    member this.MapGet(url, action: ControllerContext -> ActionResult) =
        this.MapWithMethod(url, "GET", action)

    member this.MapGet(url, action: unit -> string) =
        let c = action |> contentResult |> ignoreContext
        this.MapGet(url, c)

    member this.MapAction(routeConstraint: RoutingConstraints.RouteConstraint, action: ControllerContext -> ActionResult) = 
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        this.Add({new RouteBase() with
                    override this.GetRouteData ctx = 
                        let data = RouteData(RouteHandler = handler, Route = this)
                        for d in defaults do
                            data.Values.Add(d.Key, d.Value)
                        if routeConstraint (ctx, data)
                            then data
                            else null
                    override this.GetVirtualPath(ctx, values) = null})

    member this.MapWithMethod(url, httpMethod, action: ControllerContext -> ActionResult) =
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let httpMethodConstraint = HttpMethodConstraint([| httpMethod |])
        let constraints = RouteValueDictionary(dict [("httpMethod", httpMethodConstraint :> obj)])
        this.Add(Route(url, defaults, constraints, handler))

    member this.MapPost(url, action: ControllerContext -> ActionResult) =  
        this.MapWithMethod(url, "POST", action)

let action (routeConstraint: RoutingConstraints.RouteConstraint) (action: ControllerContext -> ActionResult) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let get url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapGet(url, action)

let post url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapPost(url, action)

