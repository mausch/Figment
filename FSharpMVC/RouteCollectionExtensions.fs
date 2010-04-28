module FSharpMvc.RouteCollectionExtensions

open System.Web.Mvc
open System.Web.Routing

let content (action: unit -> string) =
    fun (ctx: ControllerContext) -> action() |> Result.content

type RouteCollection with
    member x.MapGet(url, action: ControllerContext -> ActionResult) =
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let constraints = RouteValueDictionary()
        let dataTokens = RouteValueDictionary()
        x.Add(Route(url, defaults, constraints, dataTokens, handler))

    member x.MapGet(url, action: unit -> string) =
        let c = content action
        x.MapGet(url, c)

let get url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapGet(url, action)
