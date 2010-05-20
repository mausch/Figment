module FSharpMvc.Routing

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
open Microsoft.FSharp.Metadata
open RoutingConstraints

let mutable registeredActions = List.empty<MvcAction * RouteBase>

type RouteCollection with
    member this.MapAction(routeConstraint: RouteConstraint, action: MvcAction) = 
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

let action (routeConstraint: RouteConstraint) (action: MvcAction) = 
    RouteTable.Routes.MapAction(routeConstraint, action)

let get url (action: MvcAction) =
    RouteTable.Routes.MapGet(url, action)

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

let functionInvoke f v (domain: Type) (range: Type) = 
    let fsFunc = typedefof<FSharpFunc<_,_>>.MakeGenericType [| domain; range |]
    let invokeMethod: MethodInfo = fsFunc.GetMethod("Invoke", [| domain |])
    invokeMethod.Invoke(f, [| v |])

let (-!>) (domain: Type) (range: Type) = 
    FSharpType.MakeFunctionType(domain, range)

let (-->) (functionType: Type) (impl: obj -> obj) =
    FSharpValue.MakeFunction(functionType, impl)

// action is 'a -> 'b -> ... -> ActionResult
// return is ControllerContext -> ActionResult
let rec bindAll (action: obj) (parameters: string list) (values: obj list) (ctx: ControllerContext) =
    let domain, range = FSharpType.GetFunctionElements(action.GetType())
    let v = bindSingleParameterNG domain (List.head parameters) ctx.Controller.ValueProvider ctx
    let restAction = 
        (typeof<ControllerContext> -!> range) --> 
        fun ctx -> box ()
    bindAll restAction (List.tail parameters) (v::values) ctx

let getS (fmt: PrintfFormat<'a, unit, unit, ActionResult>) (action: 'a) = 
    let url, parameters = stripFormatting fmt.Value
    get url (bindAll action parameters [])

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