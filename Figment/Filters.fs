module Figment.Filters

open System.Linq
open System.Security.Principal
open System.Web.Routing
open System.Web.Mvc
open System.Web.UI
open Figment.Result
open Figment.Helpers

type Filter = MvcAction -> MvcAction

let hasAuthorization (allowedUsers: string list) (allowedRoles: string list) (user: IPrincipal) =
    if not user.Identity.IsAuthenticated
        then false
        else
            let userMatch = allowedUsers.Length = 0 || Enumerable.Any(allowedUsers, fun u -> u = user.Identity.Name)
            let roleMatch = allowedRoles.Length = 0 || Enumerable.Any(allowedRoles, fun r -> user.IsInRole r)
            userMatch && roleMatch

let authorize (allowedUsers: string list) (allowedRoles: string list) (action: MvcAction) (ctx: ControllerContext) = 
    let user = ctx.HttpContext.User
    let authorized = user |> hasAuthorization allowedUsers allowedRoles
    if authorized 
        then action ctx
        else unauthorized

let cache (settings: OutputCacheParameters) (action: MvcAction) (ctx: ControllerContext) : ActionResult = 
    let cachePolicy = ctx.HttpContext.Response.Cache
    cachePolicy.SetExpires(ctx.HttpContext.Timestamp.AddSeconds(float settings.Duration))
    // TODO set the other cache parameters
    action ctx

let requireHttps (action: MvcAction) (ctx: ControllerContext) = 
    if ctx.HttpContext.Request.IsSecureConnection 
        then action ctx
        else
            let request = ctx.HttpContext.Request
            if request.HttpMethod <>. "GET"
                then failwithf "HTTPS required for %s" request.RawUrl
            redirect (sprintf "https://%s%s" request.Url.Host request.RawUrl)

let apply (filter: Filter) (actions: seq<string * MvcAction>) =
    actions |> Seq.map (fun (k,v) -> (k, filter v))

