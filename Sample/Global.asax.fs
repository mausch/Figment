namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing
open FSharpMvc.RouteCollectionExtensions
open Actions

type MvcApplication() =
    inherit HttpApplication()

    member this.RegisterRoutes(routes: RouteCollection) = 
        routes.IgnoreRoute "asd"
        get "" (content action3)
        get "something" action2
        get "action5" action5
        get "action6" action6

    member this.Application_Start() =
        this.RegisterRoutes RouteTable.Routes
    

