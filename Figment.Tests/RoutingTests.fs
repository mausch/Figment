module RoutingTests

open System
open System.Web
open System.Web.Routing
open Xunit
open Figment.Routing
open Figment.RoutingConstraints
open Fuchu

[<Tests>]
let tests =
    testList "Routing tests" [
        testList "stripFormatting" [
            testCase "one int" <| fun _ ->
                let url, parameters = stripFormatting "/question/{id:%d}/{title}"
                Assert.Equal<string>("/question/{id}/{title}", url)
                Assert.Equal(1, parameters.Length)
                Assert.Equal<string>("id", parameters.[0])

            testCase "one int and one string" <| fun _ -> 
                let url, parameters = stripFormatting "/question/{id:%d}/{title:%s}"
                Assert.Equal<string>("/question/{id}/{title}", url)
                Assert.Equal(2, parameters.Length)
                Assert.Equal<string>("id", parameters.[0])
                Assert.Equal<string>("title", parameters.[1])
        ]

        testCase "ifUrlMatches" <| fun _ ->
            let ctx = {new HttpContextBase() with
                        override x.Request = {new HttpRequestBase() with
                            override x.Url = Uri("http://localhost/something")}}
            let route = RouteData()
            let c = ctx, route
            Assert.True(ifUrlMatches "^/some" c)
            Assert.False(ifUrlMatches "^/some$" c)            
    ]