namespace Figment

open System
open System.Reflection
open System.Web
open System.Web.Mvc

type FAction = ControllerContext -> ActionResult

type ControllerFilters = {
    actionExecutedFilter: ActionExecutedContext -> unit
    actionExecutingFilter: ActionExecutingContext -> unit
    authorizationFilter: AuthorizationContext -> unit
    exceptionFilter: ExceptionContext -> unit
    resultExecutedFilter: ResultExecutedContext -> unit
    resultExecutingFilter: ResultExecutingContext -> unit
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

type Helper() =
    static member BuildControllerFromAction (action: FAction) =
        { new Controller() with
            override this.ExecuteCore() = 
                let result = action this.ControllerContext
                result.ExecuteResult this.ControllerContext }

module Helpers =
    let DefaultControllerFilters = {
        actionExecutedFilter = fun c -> () 
        actionExecutingFilter = fun c -> ()
        authorizationFilter = fun c -> ()
        exceptionFilter = fun c -> ()
        resultExecutedFilter = fun c -> ()
        resultExecutingFilter = fun c -> ()
    }

    /// case-insensitive string comparison
    let (==.) (x: string) (y: string) = 
        StringComparer.InvariantCultureIgnoreCase.Compare(x,y) = 0

    /// case-insensitive string comparison
    let (<>.) (x: string) (y: string) =
        StringComparer.InvariantCultureIgnoreCase.Compare(x,y) <> 0

    let uncheckedClass = Type.GetType "Microsoft.FSharp.Core.Operators+Unchecked, FSharp.Core, Version=2.0.0.0"
    let defaultOfMethod = uncheckedClass.GetMethod "DefaultOf"

    let defaultValueOf (t: Type) =
        let genericMethod = defaultOfMethod.MakeGenericMethod [| t |]
        genericMethod.Invoke(null, null)