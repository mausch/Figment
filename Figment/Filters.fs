namespace Figment

open System.Linq
open System.Security.Principal
open System.Web.Routing
open System.Web.Mvc
open System.Web.UI
open Figment.Result
open Figment.Helpers

module private Internals = 

    let internal irequireHttps (action: ControllerContext -> 'a) redirect (ctx: ControllerContext): 'a =
        if ctx.HttpContext.Request.IsSecureConnection 
            then action ctx
            else
                let request = ctx.HttpContext.Request
                if request.HttpMethod <>. "GET"
                    then failwithf "HTTPS required for %s" request.RawUrl
                redirect (sprintf "https://%s%s" request.Url.Host request.RawUrl) ctx

module Filters = 

    open FSharpx
    open FSharpx.Reader

    type Filter = FAction -> FAction

    let hasAuthorization (allowedUsers: string list) (allowedRoles: string list) (user: IPrincipal) =
        if not user.Identity.IsAuthenticated
            then false
            else
                let userMatch = allowedUsers.Length = 0 || (Seq.exists ((=) user.Identity.Name) allowedUsers)
                let roleMatch = allowedRoles.Length = 0 || (Seq.exists user.IsInRole allowedRoles)
                userMatch && roleMatch

    let authorize (allowedUsers: string list) (allowedRoles: string list) (action: FAction) : FAction = 
        fun ctx ->
            let user = ctx.HttpContext.User
            let authorized = user |> hasAuthorization allowedUsers allowedRoles
            if authorized 
                then action ctx
                else unauthorized ctx

    let requireHttps (action: FAction) : FAction = 
        Internals.irequireHttps action redirect

    let internal getCookieValueOrNull name ctx = 
        match getHttpCookie name ctx with
        | None -> null
        | Some c -> c.Value

    let flash (a: FAction): FAction =
        getCookieValueOrNull flashCookieKey
        |> Reader.map base64decode
        >>= setInContext flashContextKey
        >>. removeHttpCookie flashCookieKey
        >>. a

    let apply (filter: Filter) (actions: seq<string * FAction>) =
        actions |> Seq.map (fun (k,v) -> (k, filter v))

module AsyncFilters = 
    let requireHttps (action: FAsyncAction) : FAsyncAction = 
        fun ctx ->
            Internals.irequireHttps action (fun s ctx -> async.Return (redirect s ctx)) <| ctx

    // TODO implement other filters for async actions
