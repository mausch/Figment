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
        let b1 = bind "firstname" action6Get
        let bf (f: 'a -> 'b -> 'c) a b = f b a
        let b2 = bind "lastname" (bf b1)
        let dup1 (f: 'a -> 'a -> 'b) (a: 'a) = f a a
        let b3 = dup1 b2
        get "action6" b3
        ()
