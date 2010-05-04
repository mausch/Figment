module FSharpMvc.Routing

open System
open System.Reflection
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Binding
open Helpers
open Microsoft.FSharp.Metadata


let mutable registeredActions = List.empty<MvcAction * RouteBase>

type RouteCollection with
    member this.MapAction(routeConstraint: RoutingConstraints.RouteConstraint, action: MvcAction) = 
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
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
        registeredActions <- (action, route)::registeredActions

    member this.MapWithMethod(url, httpMethod, action: MvcAction) =
        let handler = FSharpMvcRouteHandler(action)
        let defaults = RouteValueDictionary(dict [("controller", "Views" :> obj)])
        let httpMethodConstraint = HttpMethodConstraint([| httpMethod |])
        let constraints = RouteValueDictionary(dict [("httpMethod", httpMethodConstraint :> obj)])
        let route = Route(url, defaults, constraints, handler)
        this.Add(route)
        registeredActions <- (action, route :> RouteBase)::registeredActions

    member this.MapGet(url, action: MvcAction) =
        this.MapWithMethod(url, "GET", action)

    member this.MapPost(url, action: MvcAction) =  
        this.MapWithMethod(url, "POST", action)

let action (routeConstraint: RoutingConstraints.RouteConstraint) (action: MvcAction) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let get url (action: MvcAction) =
    RouteTable.Routes.MapGet(url, action)

let post url (action: MvcAction) =
    RouteTable.Routes.MapPost(url, action)

/// doesn't work yet
let inThisAssembly(): MvcAction seq =
    let entities = 
        let topLevelEntities = 
            (FSharpAssembly.FromAssembly (Assembly.GetCallingAssembly())).Entities
            |> Seq.toList
        let rec getEntities (entities: FSharpEntity list) = 
            let nestedEntities = entities |> Seq.collect (fun e -> e.NestedEntities) |> Seq.toList
            match nestedEntities with
            | [] -> entities
            | _ -> entities @ getEntities nestedEntities
        getEntities topLevelEntities
    let values = entities |> Seq.collect (fun e -> e.MembersOrValues)
    let functions = values |> Seq.filter (fun v -> v.Type.IsFunction)
    let names = functions |> Seq.map (fun f -> f.CompiledName) |> Seq.toList
    [Actions.empty] :> seq<MvcAction>

/// not implemented
let registerAllWithAttribute (attr: #Attribute) (actions: unit -> MvcAction seq) = 
    let t = actions()
    raise <| NotImplementedException()
    ()