namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open System.Web.UI
open FSharpMvc
open FSharpMvc.Routing
open FSharpMvc.RoutingConstraints
open FSharpMvc.Actions
open FSharpMvc.Binding

type MvcApplication() =
    inherit HttpApplication()

    member this.Application_Start() = 
        let bindInt (parameter: string) (f: int -> 'b) (ctx: ControllerContext) =
            let r = bindSingleParameter parameter ctx.Controller.ValueProvider (bindErrorDefault 0) ctx
            f r
        get "hi" (content "<h1>Hello World!</h1>")
        get "showform" (view "action2viewname" ())
        let action6Get (firstname: string) (lastname: string) (age: int) = 
            Result.content (sprintf "Hello %s %s, %d years old" firstname lastname age)
        let b1 = bind "firstname" action6Get
        let b1 b c a = b1 a b c
        let b1 = bind "lastname" b1
        let b1 b c a = b1 a b c
        let b1 = bindInt "age" b1
        let b1 a = b1 a a a
        let cachedB1 = b1 |> Filters.cache (OutputCacheParameters(Duration = 60))
        get "action6" b1
        get "route/{firstname}/{lastname}/{age}" cachedB1
        getS "route/{firstname:%s}/{lastname:%s}/{age:%d}" action6Get
        ()
