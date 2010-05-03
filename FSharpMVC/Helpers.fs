namespace FSharpMvc

open System
open System.Web
open System.Web.Mvc

type Action = ControllerContext -> ActionResult

type Helper() =
    static member BuildControllerFromAction (action: Action) =
        { new Controller() with
            override this.ExecuteCore() = 
                let result = action this.ControllerContext
                result.ExecuteResult this.ControllerContext }

module Helpers =
    /// case-insensitive string comparison
    let (==.) (x: string) (y: string) = 
        StringComparer.InvariantCultureIgnoreCase.Compare(x,y) = 0

    /// case-insensitive string comparison
    let (<>.) (x: string) (y: string) =
        StringComparer.InvariantCultureIgnoreCase.Compare(x,y) <> 0