namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing
open System.Collections.Specialized
open FSharpMvc.Routing
open FSharpMvc.Combinators
open FSharpMvc.Result
open Actions

type MvcApplication() =
    inherit HttpApplication()

    member this.RegisterRoutes(routes: RouteCollection) = 
        routes.IgnoreRoute "asd"
        get "" action2
        get "something" (action3 |> contentAction |> ignoreContext)
        get "action5" action5
        //get "action6" action6
        //post "action6" (formAction postAction6Easy)
        post "action6" (bindForm postAction6Easier <@ postAction6Easier @>)
        get "inline" (fun _ -> content "<h1>Hello world!</h1>")

    member this.Application_Start() =
        this.RegisterRoutes RouteTable.Routes
    

