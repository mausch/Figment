module FSharpMvc.Filters

open System.Linq
open System.Security.Principal
open System.Web.Mvc
open FSharpMvc.Result
open FSharpMvc.Helpers

let hasAuthorization (allowedUsers: string list) (allowedRoles: string list) (user: IPrincipal) =
    if not user.Identity.IsAuthenticated
        then false
        else
            let userMatch = allowedUsers.Length = 0 || Enumerable.Any(allowedUsers, fun u -> u = user.Identity.Name)
            let roleMatch = allowedRoles.Length = 0 || Enumerable.Any(allowedRoles, fun r -> user.IsInRole r)
            userMatch && roleMatch

let authorize (allowedUsers: string list) (allowedRoles: string list) (action: Action) (ctx: ControllerContext) = 
    let user = ctx.HttpContext.User
    let authorized = hasAuthorization allowedUsers allowedRoles user
    if authorized 
        then action ctx
        else unauthorized

let requireHttps (action: Action) (ctx: ControllerContext) = 
    if ctx.HttpContext.Request.IsSecureConnection 
        then action ctx
        else
            let request = ctx.HttpContext.Request
            if request.HttpMethod <>. "GET"
                then failwithf "HTTPS required for %s" request.RawUrl
            redirect (sprintf "https://%s%s" request.Url.Host request.RawUrl)