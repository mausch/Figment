namespace SampleApp

open System
open System.Collections.Specialized
open System.Web
open System.Web.Mvc
open System.Web.UI
open Figment
open Figment.Routing
open Figment.RoutingConstraints
open Figment.Result
open Figment.Actions
open Figment.Binding
open Figment.Filters
open WingBeats.Xhtml
open WingBeats.Xml

type PersonalInfo = {
    firstName: string
    lastName: string
    age: int
}

type MvcApplication() =
    inherit HttpApplication()
    member this.Application_Start() = 
        // it's not necessary to register Wing Beats as view engine
        //ViewEngines.Engines.Clear();
        //ViewEngines.Engines.Add(WingBeats.Mvc.WingBeatsTemplateEngine(System.Reflection.Assembly.GetExecutingAssembly()));

        // hello world
        get "hi" (content "<h1>Hello World!</h1>")

        // redirect root to "hi"
        get "" (redirect "hi")

        // applying cache, showing a regular ASP.NET MVC view
        let cache300 = cache (OutputCacheParameters(Duration = 300, Location = OutputCacheLocation.Any))
        get "showform" (cache300 <| view "sampleform" { firstName = "Cacho"; lastName = "Castaña"; age = 68})

        // handle post to "action6"
        // first, a regular function
        let greet name = sprintf "Hello %s" name
        // binding to request
        let greet' (ctx: ControllerContext) = 
            let boundGreet = greet >> sprintf "<h1>%s</h1>" >> Result.content
            boundGreet ctx.HttpContext.Request.["somefield"]
        post "action6" greet'

        // handle get to "action6"
        // first, a regular function
        let greet firstName lastName age = 
            sprintf "Hello %s %s, you're %d years old" firstName lastName age
        // binding to request
        let greet' (ctx: ControllerContext) =
            let req = ctx.HttpContext.Request
            greet req.["firstname"] req.["lastname"] (int req.["age"])
            |> sprintf "<p>%s</p>" |> Result.content
        get "action6" greet'

        let greet' (p: NameValueCollection) = 
            greet p.["firstname"] p.["lastname"] (int p.["age"])
        get "greetme2" (bindQuerystring greet' >> Result.view "someview")

        // strongly-typed route+binding
        let nameAndAge firstname lastname age = 
            sprintf "Hello %s %s, %d years old" firstname lastname age
            |> Result.content
        getS "route/{firstname:%s}/{lastname:%s}/{age:%d}" nameAndAge

        // wing beats integration
        let e = XhtmlElement()
        let wbpage title = 
            e.Html [
                e.Head [
                    e.Title [ &title ]
                ]
                e.Body [
                    e.H1 [ &title ]
                ]
            ]
        let wbpageview = wbpage >> wbview
        get "wingbeats" (wbpageview "Hello World from Wing Beats")

        // routing dsl
        let ifGetDsl = ifUrlMatches "^/dsl" &&. ifMethodIsGet

        action 
            (ifGetDsl &&. !. (ifUserAgentMatches "MSIE"))
            (wbpageview "You're NOT using Internet Explorer")

        action ifGetDsl (wbpageview "You're using Internet Explorer")

        action unconstrained (status 404 => content "<h1>Not found!</h1>")

        ()
