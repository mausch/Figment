module Figment.Tests.Routing

open System
open System.Web
open System.Web.Routing
open Figment.Routing
open Figment.RoutingConstraints
open Fuchu

[<Tests>]
let tests =
    testList "Routing tests" [
        testList "stripFormatting" [
            testCase "one int" <| fun _ ->
                let url, parameters = stripFormatting "/question/{id:%d}/{title}"
                assertEqual "/question/{id}/{title}" url
                assertEqual 1 parameters.Length
                assertEqual "id" parameters.[0]

            testCase "one int and one string" <| fun _ -> 
                let url, parameters = stripFormatting "/question/{id:%d}/{title:%s}"
                assertEqual "/question/{id}/{title}" url
                assertEqual 2 parameters.Length
                assertEqual "id" parameters.[0]
                assertEqual "title" parameters.[1]
        ]

        testCase "ifUrlMatches" <| fun _ ->
            let ctx = {new HttpContextBase() with
                        override x.Request = {new HttpRequestBase() with
                            override x.Url = Uri("http://localhost/something")}}
            let route = RouteData()
            let c = ctx, route
            if not (ifUrlMatches "^/some" c) 
                then failtest "Should have matched ^/some"
            if ifUrlMatches "^/some$" c 
                then failtest "Should not have matched ^/some$"
    ]