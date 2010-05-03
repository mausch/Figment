namespace FSharpMvc

open System.Web
open System.Web.Mvc
open System.Web.Routing

type FSharpMvcHandler(context: RequestContext, action: Action) =
    member this.ProcessRequest(ctx: HttpContextBase) = 
        let controller = Helper.BuildControllerFromAction action
        (controller :> IController).Execute context

    interface IHttpHandler with
        member this.IsReusable with get() = false
        member this.ProcessRequest ctx =
            this.ProcessRequest(HttpContextWrapper(ctx))
            
type FSharpMvcRouteHandler(action: Action) =
    interface IRouteHandler with
        member this.GetHttpHandler ctx = upcast FSharpMvcHandler(ctx, action)

type RouteConstraintParameters = {
    Context: HttpContextBase
    Route: Route
    ParameterName: string
    Values: RouteValueDictionary
    Direction: RouteDirection
}
    
type FSharpMvcRouteConstraint(f: RouteConstraintParameters -> bool) =
    interface IRouteConstraint with
        member x.Match(ctx, route, parameterName, values, direction) = 
            f {Context = ctx; Route = route; ParameterName = parameterName; Values = values; Direction = direction}

type FSharpMvcSimpleRouteConstraint(f: HttpContextBase -> bool) =
    interface IRouteConstraint with
        member x.Match(ctx, route, parameterName, values, direction) = f ctx

(*
type FSharpMvcRoute(action: ControllerContext -> ActionResult, acceptRoute: HttpContextBase -> bool) =
    inherit RouteBase()

    override this.GetRouteData ctx = 
        if acceptRoute ctx
            then RouteData(this, FSharpMvcRouteHandler(action))
            else null

    override this.GetVirtualPath (ctx, routeValues) = null
*)