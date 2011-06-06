module Figment.RoutingConstraints

open System.Text.RegularExpressions
open System.Web
open System.Web.Routing

type RouteConstraint = HttpContextBase * RouteData -> bool

(* operators *)
let allOf (constraints: RouteConstraint list) (ctx: HttpContextBase, route: RouteData) = 
    Seq.forall (fun c -> c(ctx,route)) constraints

let anyOf (constraints: RouteConstraint list) (ctx: HttpContextBase, route: RouteData) = 
    Seq.exists (fun c -> c(ctx,route)) constraints

let (||.) (x: RouteConstraint) (y: RouteConstraint) = anyOf [x;y]

let (&&.) (x: RouteConstraint) (y: RouteConstraint) = allOf [x;y]

let (!.) (x: RouteConstraint) (ctx: HttpContextBase, route: RouteData) = 
    not (x(ctx, route))

(* constraints *)
let any (ctx: HttpContextBase, route: RouteData) = true

let ifPathIs (url: string) =
    fun (ctx: HttpContextBase, route: RouteData) ->
        ctx.Request.Url.AbsolutePath = "/"+url

let ifUrlMatches (rx: string) =
    if rx = null
        then invalidArg "rx" "regex null"
    let rxx = Regex(rx)
    fun (ctx: HttpContextBase, route: RouteData) ->
        rxx.IsMatch ctx.Request.Url.AbsolutePath

let ifMethodIs httpMethod = 
    if httpMethod = null
        then invalidArg "httpMethod" "httpMethod null"
    fun (ctx: HttpContextBase, route: RouteData) -> 
        ctx.Request.HttpMethod = httpMethod

let ifMethodIsGet x = ifMethodIs "GET" x
let ifMethodIsPost x = ifMethodIs "POST" x
let ifMethodIsHead x = ifMethodIs "HEAD" x
let ifMethodIsPut x = ifMethodIs "PUT" x
let ifMethodIsOptions x = ifMethodIs "OPTIONS" x

let ifUserAgentMatches (rx: string) =
    if rx = null
        then invalidArg "rx" "regex null"
    let rxx = Regex(rx)
    fun (ctx: HttpContextBase, route: RouteData) ->
        rxx.IsMatch ctx.Request.UserAgent

open System
open Helpers

let ifIsAjax (ctx: HttpContextBase, route: RouteData) =
    let requestedWith = ctx.Request.Headers.["X-Requested-With"]
    requestedWith <> null && requestedWith =. "xmlhttprequest"