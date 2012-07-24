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
                Assert.Equal("url", "/question/{id}/{title}", url)
                Assert.Equal("parameter length", 1, parameters.Length)
                Assert.Equal("first parameter", "id", parameters.[0])

            testCase "one int and one string" <| fun _ -> 
                let url, parameters = stripFormatting "/question/{id:%d}/{title:%s}"
                Assert.Equal("url", "/question/{id}/{title}", url)
                Assert.Equal("parameter length", 2, parameters.Length)
                Assert.Equal("first parameter", "id", parameters.[0])
                Assert.Equal("2nd parameter", "title", parameters.[1])
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