module Figment.Helpers

open System
open System.Reflection
open System.Web
open System.Web.Mvc

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


type FSharpController(action: FAction, filters: ControllerFilters) =
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

type FSharpAsyncController(action: FAsyncAction, filters: ControllerFilters) = 
    inherit AsyncController() with
        let abegin, aend, acancel = Async.AsBeginEnd action
        override this.OnActionExecuted ctx = filters.actionExecutedFilter ctx
        override this.OnActionExecuting ctx = filters.actionExecutingFilter ctx
        override this.OnAuthorization ctx = filters.authorizationFilter ctx
        override this.OnException ctx = filters.exceptionFilter ctx
        override this.OnResultExecuted ctx = filters.resultExecutedFilter ctx
        override this.OnResultExecuting ctx = filters.resultExecutingFilter ctx
        override this.BeginExecuteCore(cb, state) = 
            let endExec r = 
                let result = aend r
                result.ExecuteResult this.ControllerContext
            abegin(this.ControllerContext, AsyncCallback(endExec), null)


type Helper() =
    static member BuildControllerFromAction (action: FAction) =
        new FSharpController(action, DefaultControllerFilters)

    static member BuildControllerFromAsyncAction (action: FAsyncAction) =
        new FSharpAsyncController(action, DefaultControllerFilters)


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