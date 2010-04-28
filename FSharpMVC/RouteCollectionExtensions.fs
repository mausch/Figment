module FSharpMvc.RouteCollectionExtensions

open System.Web.Mvc
open System.Web.Routing

let content (action: unit -> string) =
    fun (ctx: ControllerContext) -> action() |> Result.content

type RouteCollection with
    member x.MapGet(url, action: ControllerContext -> ActionResult) =
        x.Add(Route(url, FSharpMvcRouteHandler(action)))

    member x.MapGet(url, action: unit -> string) =
        let c = content action
        x.MapGet(url, c)

let get url (action: ControllerContext -> ActionResult) =
    RouteTable.Routes.MapGet(url, action)
