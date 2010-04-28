namespace FSharpMvc

open System.Web
open System.Web.Mvc
open System.Web.Routing

type FSharpMvcHandler(context: RequestContext, action: ControllerContext -> ActionResult) =
    member this.ProcessRequest(ctx: HttpContextBase) = 
        let controller = { new Controller() with
                            override this.ExecuteCore() = 
                                let result = action this.ControllerContext
                                result.ExecuteResult this.ControllerContext }
        (controller :> IController).Execute context

    interface IHttpHandler with
        member this.IsReusable with get() = false
        member this.ProcessRequest ctx =
            this.ProcessRequest(HttpContextWrapper(ctx))
            
type FSharpMvcRouteHandler(action: ControllerContext -> ActionResult) =
    interface IRouteHandler with
        member this.GetHttpHandler ctx = upcast FSharpMvcHandler(ctx, action)
        

(*type FSharpMvcRoute(action: ControllerContext -> #ActionResult) =
    inherit RouteBase()

    override this.GetRouteData ctx = 
        RouteData(this, FSharpMvcRouteHandler(action))
    override this.GetVirtualPath (ctx, routeValues) = null

*)