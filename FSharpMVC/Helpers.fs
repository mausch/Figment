namespace FSharpMvc

open System
open System.Reflection
open System.Web
open System.Web.Mvc

type MvcAction = ControllerContext -> ActionResult

type Helper() =
    static member BuildControllerFromAction (action: MvcAction) =
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

    let uncheckedClass = Type.GetType "Microsoft.FSharp.Core.Operators+Unchecked, FSharp.Core, Version=2.0.0.0"
    let defaultOfMethod = uncheckedClass.GetMethod "DefaultOf"

    let defaultValueOf (t: Type) =
        let genericMethod = defaultOfMethod.MakeGenericMethod [| t |]
        genericMethod.Invoke(null, null)