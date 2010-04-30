namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing
open System.Collections.Specialized
open FSharpMvc.RouteCollectionExtensions
open FSharpMvc.Combinators
open Actions

type MvcApplication() =
    inherit HttpApplication()

    member this.RegisterRoutes(routes: RouteCollection) = 
        routes.IgnoreRoute "asd"
        get "" action2
        get "something" (contentAction action3)
        get "action5" action5
        //get "action6" action6
        //post "action6" (formAction postAction6Easy)
        post "action6" (bindForm postAction6Easier <@ postAction6Easier @>)

    member this.Application_Start() =
        this.RegisterRoutes RouteTable.Routes
    

