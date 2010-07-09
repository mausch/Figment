module RoutingTests

open System
open System.Web
open System.Web.Routing
open Xunit
open Figment.Routing
open Figment.RoutingConstraints

[<Fact>]
let stripFormattingTest() =
    let url, parameters = stripFormatting "/question/{id:%d}/{title}"
    Assert.Equal("/question/{id}/{title}", url)
    Assert.Equal(1, parameters.Length)
    Assert.Equal("id", parameters.[0])

    let url, parameters = stripFormatting "/question/{id:%d}/{title:%s}"
    Assert.Equal("/question/{id}/{title}", url)
    Assert.Equal(2, parameters.Length)
    Assert.Equal("id", parameters.[0])
    Assert.Equal("title", parameters.[1])

    ()

[<Fact>]
let urlMatchesTest() =
    let ctx = {new HttpContextBase() with
                override x.Request = {new HttpRequestBase() with
                    override x.Url = Uri("http://localhost/something")}}
    let route = RouteData()
    Assert.True(ifUrlMatches "^/some" (ctx, route))
    Assert.False(ifUrlMatches "^/some$" (ctx, route))
    ()
