module Figment.Helpers

open System
open System.Reflection
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Async

open System.Diagnostics

type FAction = ControllerContext -> ActionResult

type FAsyncAction = ControllerContext -> Async<ActionResult>

type ControllerFilters = {
    actionExecutedFilter: ActionExecutedContext -> unit
    actionExecutingFilter: ActionExecutingContext -> unit
    authorizationFilter: AuthorizationContext -> unit
    exceptionFilter: ExceptionContext -> unit
    resultExecutedFilter: ResultExecutedContext -> unit
    resultExecutingFilter: ResultExecutingContext -> unit
}

let DefaultControllerFilters = {
    actionExecutedFilter = fun c -> () 
    actionExecutingFilter = fun c -> ()
    authorizationFilter = fun c -> ()
    exceptionFilter = fun c -> ()
    resultExecutedFilter = fun c -> ()
    resultExecutingFilter = fun c -> ()
}

type FigmentAsyncController(action: FAsyncAction, filters: ControllerFilters) = 
    inherit ControllerBase()
        override this.ExecuteCore() = 
            Debug.WriteLine "ExecuteCore"
    interface IAsyncController with
        member this.BeginExecute(requestContext, cb, state) = 
            Debug.WriteLine "BeginExecute"
            // cb and state are from asp.net
            let controllerContext = ControllerContext(requestContext, this)
            let abegin, aend, acancel = Async.AsBeginEnd action
            let callback r = 
                Debug.WriteLine "BeginExecute callback"
                let result = aend r
                result.ExecuteResult controllerContext
                cb.Invoke r

            abegin(controllerContext, AsyncCallback(callback), null)

        member this.EndExecute r = 
            Debug.WriteLine "EndExecute"
        member this.Execute r = 
            Debug.WriteLine "Execute"

type Helper() =
    static member BuildActionInvoker (f: ControllerContext -> string -> bool) =
        { new IActionInvoker with
            member x.InvokeAction(ctx, actionName) = f ctx actionName }
    static member BuildControllerFromAction (action: FAction) =
        { new Controller() with 
            override x.CreateActionInvoker() = 
                upcast { new ControllerActionInvoker() with
                            override y.FindAction(ctx, descriptor, actionName) = 
                                { new ActionDescriptor() with
                                    override z.ActionName = actionName
                                    override z.ControllerDescriptor = 
                                        { new ControllerDescriptor() with
                                            override a.ControllerType = x.GetType()
                                            override a.FindAction(ctx, actionName) = z
                                            override a.GetCanonicalActions() = [|z|] }
                                    override z.Execute(ctx, param) = upcast action ctx
                                    override z.GetParameters() = [||] } } }

    static member BuildControllerFromAsyncAction (action: FAsyncAction) =
        new FigmentAsyncController(action, DefaultControllerFilters)

/// case-insensitive string comparison
let (=.) (x: string) (y: string) = 
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) = 0

/// case-insensitive string comparison
let (<>.) (x: string) (y: string) =
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) <> 0

let uncheckedClass = Type.GetType "Microsoft.FSharp.Core.Operators+Unchecked, FSharp.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
let defaultOfMethod = uncheckedClass.GetMethod "DefaultOf"

let defaultValueOf (t: Type) =
    let genericMethod = defaultOfMethod.MakeGenericMethod [| t |]
    genericMethod.Invoke(null, null)

let asyncf f x = 
    async {
        return f x
    }

let inline htmlencode x = HttpUtility.HtmlEncode x
let inline urlencode (x: string) = HttpUtility.UrlEncode x