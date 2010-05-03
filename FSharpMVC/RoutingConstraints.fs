module FSharpMvc.RoutingConstraints

open System.Linq
open System.Text.RegularExpressions
open System.Web
open System.Web.Routing

type RouteConstraint = HttpContextBase * RouteData -> bool

(* operators *)
let allOf (constraints: RouteConstraint list) (ctx: HttpContextBase, route: RouteData) = 
    Enumerable.All(constraints, (fun c -> c(ctx,route)))

let anyOf (constraints: RouteConstraint list) (ctx: HttpContextBase, route: RouteData) = 
    Enumerable.Any(constraints, (fun c -> c(ctx,route)))

let (||.) (x: RouteConstraint) (y: RouteConstraint) = anyOf [x;y]

let (&&.) (x: RouteConstraint) (y: RouteConstraint) = allOf [x;y]

(* constraints *)
let unconstrained (ctx: HttpContextBase, route: RouteData) = true

let urlMatches (rx: string) (ctx: HttpContextBase, route: RouteData) =
    if rx = null
        then invalidArg "rx" "regex null"
    let rxx = Regex(rx)
    rxx.IsMatch ctx.Request.Url.AbsolutePath

let methodIs httpMethod (ctx: HttpContextBase, route: RouteData) = 
    if httpMethod = null
        then invalidArg "httpMethod" "httpMethod null"
    ctx.Request.HttpMethod = httpMethod

let methodIsGet x = methodIs "GET" x

let methodIsPost x = methodIs "POST" x