namespace FSharpMvc

open System.Web
open System.Web.Mvc

type Action = ControllerContext -> ActionResult

type Helper() =
    static member BuildControllerFromAction (action: Action) =
        { new Controller() with
            override this.ExecuteCore() = 
                let result = action this.ControllerContext
                result.ExecuteResult this.ControllerContext }