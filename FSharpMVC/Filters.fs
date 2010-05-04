module FSharpMvc.Filters

open System.Linq
open System.Security.Principal
open System.Web.Routing
open System.Web.Mvc
open FSharpMvc.Result
open FSharpMvc.Helpers

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

let requireHttps (action: MvcAction) (ctx: ControllerContext) = 
    if ctx.HttpContext.Request.IsSecureConnection 
        then action ctx
        else
            let request = ctx.HttpContext.Request
            if request.HttpMethod <>. "GET"
                then failwithf "HTTPS required for %s" request.RawUrl
            redirect (sprintf "https://%s%s" request.Url.Host request.RawUrl)

let applyFilterToAllRegisteredActions (filter: Filter): unit = 
    let replaceHandler (route: RouteBase) (action: MvcAction) = 
        let handler = FSharpMvcRouteHandler(action)
        {new RouteBase() with
            override this.GetVirtualPath(ctx, values) = route.GetVirtualPath(ctx, values)
            override this.GetRouteData ctx =
                let data = route.GetRouteData ctx
                if data = null
                    then null
                    else 
                        let newData = RouteData(route = this, routeHandler = handler)
                        for d in data.DataTokens do
                            newData.DataTokens.Add(d.Key, d.Value)
                        for d in data.Values do
                            newData.Values.Add(d.Key, d.Value)
                        newData }

    let applyFilter (filter: Filter) (action: MvcAction, route: RouteBase) = 
        let newAction = filter action
        let newRoute = replaceHandler route newAction
        ((action, route), (newAction, newRoute))

    let filteredActions = 
        Routing.registeredActions
        |> Seq.map (applyFilter filter)

    filteredActions
    |> Seq.iter (fun (oldRoute, newRoute) -> 
                        RouteTable.Routes.Remove (snd oldRoute) |> ignore
                        RouteTable.Routes.Add (snd newRoute))
    
    let newRoutes =
        filteredActions
        |> Seq.map snd
        |> Seq.toList

    Routing.registeredActions <- newRoutes