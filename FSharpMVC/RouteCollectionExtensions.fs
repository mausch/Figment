module FSharpMvc.RouteCollectionExtensions

open System.Web.Mvc
open System.Web.Routing
open Combinators

type RouteCollection with
    member this.MapGet(url, action: ControllerContext -> ActionResult) =
        this.MapWithMethod(url, "GET", action)

    member this.MapGet(url, action: unit -> string) =
        let c = contentAction action
        this.MapGet(url, c)

    member this.MapWithMethod(url, httpMethod, action: ControllerContext -> ActionResult) =
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let httpMethodConstraint = HttpMethodConstraint([| httpMethod |])
        let constraints = RouteValueDictionary(dict [("httpMethod", httpMethodConstraint :> obj)])
        let dataTokens = RouteValueDictionary()
        this.Add(Route(url, defaults, constraints, dataTokens, handler))

    member this.MapPost(url, action: ControllerContext -> ActionResult) =  
        this.MapWithMethod(url, "POST", action)

let get url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapGet(url, action)

let post url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapPost(url, action)
