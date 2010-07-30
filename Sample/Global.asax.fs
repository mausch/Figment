namespace SampleApp

open System
open System.Web
open System.Web.Mvc
open System.Web.UI
open Figment
open Figment.Routing
open Figment.RoutingConstraints
open Figment.Actions
open Figment.Binding
open Figment.Filters
open WingBeats.Xhtml
open WingBeats.Xml

type MvcApplication() =
    inherit HttpApplication()

    let wbview (n: Node) (ctx: ControllerContext) = 
        Result.content <| Renderer.RenderToString n

    member this.Application_Start() = 
        //ViewEngines.Engines.Clear();
        //ViewEngines.Engines.Add(WingBeats.Mvc.WingBeatsTemplateEngine(System.Reflection.Assembly.GetExecutingAssembly()));

        let routes = [
                        ("hi", content "<h1>Hello World!</h1>")
                     ]
        let routes = routes |> Filters.apply (cache (OutputCacheParameters()))
        get "hi" (content "<h1>Hello World!</h1>")
        get "showform" (view "action2viewname" ())
        let nameAndAge (firstname: string) (lastname: string) (age: int) = 
            Result.content (sprintf "Hello %s %s, %d years old" firstname lastname age)
        getS "route/{firstname:%s}/{lastname:%s}/{age:%d}" nameAndAge

        let e = XhtmlElement()
        let r = e.Html [
                    e.Body [
                        e.H1 [ &"Hello World from Wing Beats" ]
                    ]
                ]
        get "wingbeats" (wbview r)

        ()
