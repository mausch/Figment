namespace SampleApp

open System
open System.Collections.Specialized
open System.Diagnostics
open System.Globalization
open System.Net
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
open Formlets
open WingBeats.Formlets
open Figment.Extensions

type PersonalInfo = {
    FirstName: string
    LastName: string
    Email: string
    DateOfBirth: DateTime
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
        get "showform" (cache300 <| view "sampleform" { FirstName = "Cacho"; LastName = "Castaña"; Email = ""; DateOfBirth = DateTime.MinValue })

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
        getf "route/{firstname:%s}/{lastname:%s}/{age:%d}" nameAndAge

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

        // async
        let google (ctx: ControllerContext) = async {
            Debug.WriteLine "Start async action"
            let query = ctx.HttpContext.Request.Url.Segments.[2]
            let query = HttpUtility.UrlEncode query
            use web = new WebClient()
            let! content = web.AsyncDownloadString(Uri("http://www.google.com/search?q=" + query))
            Debug.WriteLine "got google response"
            return Result.content content
        }
        asyncAction (ifMethodIsGet &&. ifUrlMatches "^/google/") google

        // formlets
        let s = e.Shortcut
        let f = e.Formlets
        let registrationFormlet : PersonalInfo Formlet =
            let dateFormlet : DateTime Formlet =
                let baseFormlet = 
                    yields t3
                    <*> f.LabeledTextBox("Month: ", "", ["size","2"; "maxlength","2"])
                    <*> f.LabeledTextBox("Day: ", "", ["size","2"; "maxlength","2"])
                    <*> f.LabeledTextBox("Year: ", "", ["size","4"; "maxlength","4"])
                let isDate (month,day,year) = 
                    DateTime.TryParseExact(sprintf "%s%s%s" year month day, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None) |> fst
                let dateValidator = err isDate (fun _ -> "Invalid date")
                baseFormlet 
                |> satisfies dateValidator
                |> map (fun (month,day,year) -> DateTime(int year,int month,int day))

            yields (fun f l e d -> 
                        { FirstName = f; LastName = l; Email = e; DateOfBirth = d })
            <*> (f.LabeledTextBox("First name: ", "", []) |> Validate.notEmpty)
            <+ e.Br()
            <*> (f.LabeledTextBox("Last name: ", "", []) |> Validate.notEmpty)
            <+ e.Br()
            <*> (f.LabeledTextBox("Email: ", "", []) |> Validate.isEmail)
            <+ e.Br()
            <*> dateFormlet
            <+ e.Br()
            <+ e.Text "Please read very carefully these terms and conditions before registering for this online program, blah blah blah"
            <+ e.Br()
            <* (f.LabeledCheckBox("I agree to the terms and conditions above", false, []) |> satisfies (err ((=) true) (fun _ -> "Please accept the terms and conditions")))
        let registrationPage url form =
            e.Html [
                e.Head [
                    e.Title [ &"Registration" ]
                    e.Style [ &".error {color:red}" ]
                ]
                e.Body [
                    e.H1 [ &"Registration" ]
                    s.FormPost url [
                        e.Fieldset [
                            yield e.Legend [ &"Please fill the fields below" ]
                            yield!!+form
                            yield e.Br()
                            yield s.Submit "Register!"
                        ]
                    ]
                ]
            ]

        get "thankyou" (fun ctx -> Result.contentf "Thank you for registering, %s %s" ctx.QueryString.["f"] ctx.QueryString.["l"])
            
        formAction "register" registrationFormlet registrationPage 
            (fun _ v -> Result.redirectf "thankyou?f=%s&l=%s" v.FirstName v.LastName)

        action any (status 404 => content "<h1>Not found!</h1>")
        
        ()
