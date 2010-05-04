module FSharpMvc.Routing

open System.Web
open System.Web.Mvc
open System.Web.Routing
open Combinators

type RouteCollection with
    member this.MapAction(routeConstraint: RoutingConstraints.RouteConstraint, action: Action) = 
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

    member this.MapWithMethod(url, httpMethod, action: Action) =
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let httpMethodConstraint = HttpMethodConstraint([| httpMethod |])
        let constraints = RouteValueDictionary(dict [("httpMethod", httpMethodConstraint :> obj)])
        this.Add(Route(url, defaults, constraints, handler))

    member this.MapGet(url, action: Action) =
        this.MapWithMethod(url, "GET", action)

    member this.MapPost(url, action: Action) =  
        this.MapWithMethod(url, "POST", action)

let action (routeConstraint: RoutingConstraints.RouteConstraint) (action: Action) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let get url (action: Action) =
    RouteTable.Routes.MapGet(url, action)

let post url (action: Action) =
    RouteTable.Routes.MapPost(url, action)

