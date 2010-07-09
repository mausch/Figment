module Figment.RoutingConstraints

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

let (!.) (x: RouteConstraint) (ctx: HttpContextBase, route: RouteData) = 
    not (x(ctx, route))

(* constraints *)
let unconstrained (ctx: HttpContextBase, route: RouteData) = true

let ifUrlMatches (rx: string) (ctx: HttpContextBase, route: RouteData) =
    if rx = null
        then invalidArg "rx" "regex null"
    let rxx = Regex(rx)
    rxx.IsMatch ctx.Request.Url.AbsolutePath

let ifMethodIs httpMethod (ctx: HttpContextBase, route: RouteData) = 
    if httpMethod = null
        then invalidArg "httpMethod" "httpMethod null"
    ctx.Request.HttpMethod = httpMethod

let ifMethodIsGet x = ifMethodIs "GET" x

let ifMethodIsPost x = ifMethodIs "POST" x

let ifUserAgentMatches (rx: string) (ctx: HttpContextBase, route: RouteData) =
    if rx = null
        then invalidArg "rx" "regex null"
    let rxx = Regex(rx)
    rxx.IsMatch ctx.Request.UserAgent