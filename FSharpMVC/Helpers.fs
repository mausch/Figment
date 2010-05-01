namespace FSharpMvc

open System.Web
open System.Web.Mvc

type Helper() =
    static member BuildControllerFromAction (action: ControllerContext -> ActionResult) =
        { new Controller() with
            override this.ExecuteCore() = 
                let result = action this.ControllerContext
                result.ExecuteResult this.ControllerContext }