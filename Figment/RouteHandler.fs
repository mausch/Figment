namespace Figment

open System
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Async
open System.Web.Routing
open Figment.Helpers

open System.Diagnostics
open Extensions

type FigmentHandler(context: RequestContext, action: FAction) =
    member this.ProcessRequest(ctx: HttpContextBase) = 
        let controller = buildControllerFromAction action
        ctx.Request.DisableValidation() |> ignore
        controller.ValidateRequest <- false
        (controller :> IController).Execute context

    interface System.Web.SessionState.IRequiresSessionState
    interface IHttpHandler with
        member this.IsReusable = false
        member this.ProcessRequest ctx =
            this.ProcessRequest(HttpContextWrapper(ctx))

type FigmentAsyncHandler(context: RequestContext, action: FAsyncAction) = 
    member this.ProcessRequest(ctx: HttpContextBase) = 
        let controller = buildControllerFromAsyncAction action
        ctx.Request.DisableValidation() |> ignore
        controller.ValidateRequest <- false
        (controller :> IController).Execute context

    interface System.Web.SessionState.IRequiresSessionState
    interface IHttpAsyncHandler with
        member this.IsReusable = false
        member this.ProcessRequest ctx =
            this.ProcessRequest(HttpContextWrapper(ctx))
        member this.BeginProcessRequest(ctx, cb, state) = 
            Debug.WriteLine "BeginProcessRequest"
            let controller = buildControllerFromAsyncAction action :> IAsyncController
            controller.BeginExecute(context, cb, state)

        member this.EndProcessRequest r =
            Debug.WriteLine "EndProcessRequest"

module RouteHandlerHelpers =
    let inline buildActionRouteHandler (action: FAction) = 
        buildRouteHandler (fun ctx -> upcast FigmentHandler(ctx, action))

    let inline buildAsyncActionRouteHandler (action: FAsyncAction) =
        buildRouteHandler (fun ctx -> upcast FigmentAsyncHandler(ctx, action))
