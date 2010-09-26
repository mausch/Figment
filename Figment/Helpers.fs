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


type FigmentController(action: FAction, filters: ControllerFilters) =
    inherit Controller() with
        override this.OnActionExecuted ctx = filters.actionExecutedFilter ctx
        override this.OnActionExecuting ctx = filters.actionExecutingFilter ctx
        override this.OnAuthorization ctx = filters.authorizationFilter ctx
        override this.OnException ctx = filters.exceptionFilter ctx
        override this.OnResultExecuted ctx = filters.resultExecutedFilter ctx
        override this.OnResultExecuting ctx = filters.resultExecutingFilter ctx
        override this.ExecuteCore() = 
            let result = action this.ControllerContext
            result.ExecuteResult this.ControllerContext

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
    static member BuildControllerFromAction (action: FAction) =
        new FigmentController(action, DefaultControllerFilters)

    static member BuildControllerFromAsyncAction (action: FAsyncAction) =
        new FigmentAsyncController(action, DefaultControllerFilters)


/// case-insensitive string comparison
let (==.) (x: string) (y: string) = 
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) = 0

/// case-insensitive string comparison
let (<>.) (x: string) (y: string) =
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) <> 0

let uncheckedClass = Type.GetType "Microsoft.FSharp.Core.Operators+Unchecked, FSharp.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
let defaultOfMethod = uncheckedClass.GetMethod "DefaultOf"

let defaultValueOf (t: Type) =
    let genericMethod = defaultOfMethod.MakeGenericMethod [| t |]
    genericMethod.Invoke(null, null)