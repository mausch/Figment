module RoutingTests

open Xunit
open FSharpMvc.Routing

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