module FSharpMvc.Filters

open System.Linq
open System.Security.Principal
open System.Web.Mvc
open FSharpMvc.Result

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