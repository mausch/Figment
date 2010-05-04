namespace SampleApp

open System
open System.Web
open FSharpMvc.Routing
open FSharpMvc.Actions

type MvcApplication() =
    inherit HttpApplication()

    member this.Application_Start() = 
        get "hi" (content "<h1>Hello World!</h1>")
        ()
