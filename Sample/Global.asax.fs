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
        //get "/" Actions.action1
        get "" (content action3)
        get "something" action2

    member this.Application_Start() =
        this.RegisterRoutes RouteTable.Routes
    

