module Figment.Routing

open System
open System.Reflection
open System.Text.RegularExpressions
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Binding
open Helpers
open Extensions
open Microsoft.FSharp.Reflection
open RoutingConstraints

type HttpMethod = GET | POST | HEAD | DELETE | PUT

type ActionRegistration = {
    routeName: string
    action: FAction
    route: RouteBase
} with
    static member make(routeName, action, route) = 
        {routeName = routeName; action = action; route = route}

type RouteCollection with
    member private this.MapAction(routeConstraint: RouteConstraint, handler: IRouteHandler) = 
        let defaults = RouteValueDictionary(dict ["controller", "Views" :> obj])
        let route = {new RouteBase() with
                        override this.GetRouteData ctx = 
                            let data = RouteData(routeHandler = handler, route = this)
                            for d in defaults do
                                data.Values.Add(d.Key, d.Value)
                            if routeConstraint (ctx, data)
                                then data
                                else null
                        override this.GetVirtualPath(ctx, values) = null}
        this.Add(route)
        route

    member this.MapAction(routeConstraint: RouteConstraint, action: FAction) = 
        let handler = FigmentRouteHandler(action)
        let route = this.MapAction(routeConstraint, handler)
        ()

    member this.MapAction(routeConstraint: RouteConstraint, action: FAsyncAction) = 
        let handler = FigmentAsyncRouteHandler(action)
        let route = this.MapAction(routeConstraint, handler)
        ()

    member private this.MapWithMethod(url, routeName, httpMethod, handler) = 
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let httpMethodConstraint = HttpMethodConstraint([| httpMethod |])
        let constraints = RouteValueDictionary(dict [("httpMethod", httpMethodConstraint :> obj)])
        let route = Route(url, defaults, constraints, handler)
        this.Add(routeName, route)
        route

    member this.MapWithMethod(url, routeName, httpMethod, action: FAction) =
        let handler = FigmentRouteHandler(action)
        let route = this.MapWithMethod(url, routeName, httpMethod, handler)
        ()

    member this.MapWithMethod(url, routeName, httpMethod, action: FAsyncAction) =
        let handler = FigmentAsyncRouteHandler(action)
        let route = this.MapWithMethod(url, routeName, httpMethod, handler)
        ()

    member this.MapGet(url, routeName, action: FAction) =
        this.MapWithMethod(url, routeName, "GET", action)

    member this.MapGet(url, routeName, action: FAsyncAction) =
        this.MapWithMethod(url, routeName, "GET", action)

    member this.MapPost(url, routeName, action: FAction) =
        this.MapWithMethod(url, routeName, "POST", action)

    member this.MapPost(url, routeName, action: FAsyncAction) =
        this.MapWithMethod(url, routeName, "POST", action)

let action (routeConstraint: RouteConstraint) (action: FAction) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let asyncAction (routeConstraint: RouteConstraint) (action: FAsyncAction) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let get url (action: FAction) =
    RouteTable.Routes.MapGet(url, null, action)

let getn url routeName (action: FAction) =
    RouteTable.Routes.MapGet(url, routeName, action)

let stripFormatting s =
    let parameters = ref []
    let eval (rxMatch: Match) = 
        let name = rxMatch.Groups.Groups.[1].Value
        if rxMatch.Groups.Groups.[2].Captures.Count > 0
            then parameters := name::!parameters
        sprintf "{%s}" name
    let replace = Regex.Replace(s, "{([^:}]+)(:%[^}]+)?}", eval)
    let parameters = List.rev !parameters
    (replace, parameters)

let rec bindAll (fTypes: Type list) (parameters: string list) (ctx: ControllerContext) =
    match fTypes with
    | [] -> failwith "no function types!"
    | hd::[] -> []
    | hd::tl -> 
        let v = bindSingleParameterNG hd (List.head parameters) ctx.Controller.ValueProvider ctx
        v::bindAll tl (List.tail parameters) ctx

let getnf (fmt: PrintfFormat<'a -> 'b, unit, unit, ActionResult>) routeName (action: 'a -> 'b) =
    let url, parameters = stripFormatting fmt.Value
    let args = FSharpType.GetFlattenedFunctionElements(action.GetType())
    let realAction ctx = 
        let values = bindAll args parameters ctx
        FSharpValue.InvokeFunction action values :?> ActionResult
    getn url routeName realAction

let getf (fmt: PrintfFormat<'a -> 'b, unit, unit, ActionResult>) (action: 'a -> 'b) = 
    getnf fmt null action

let post url (action: FAction) =
    RouteTable.Routes.MapPost(url, null, action)

let register (httpMethod: HttpMethod) url action =
    match httpMethod with
    | GET -> get url action
    | POST -> post url action
    | _ -> failwith "Not supported"

let clear () =
    RouteTable.Routes.Clear()
