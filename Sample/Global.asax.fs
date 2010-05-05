namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open FSharpMvc
open FSharpMvc.Routing
open FSharpMvc.RoutingConstraints
open FSharpMvc.Actions
open FSharpMvc.Binding

type MvcApplication() =
    inherit HttpApplication()

    member this.Application_Start() = 
        get "hi" (content "<h1>Hello World!</h1>")
        get "showform" (view "action2viewname" ())
        let action6Get (firstname: string) (lastname: string) = 
            Result.content (sprintf "Hello %s %s" firstname lastname)
        get "action6" (bind2 "firstname" "lastname" action6Get)
        ()
