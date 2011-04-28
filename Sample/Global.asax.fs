namespace SampleApp

open System
open System.Collections.Specialized
open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Globalization
open System.Text.RegularExpressions
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
    Name: string
    Email: string
    Password: string
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
        get "showform" (cache300 <| view "sampleform" { Name = "Cacho Castaña"; Email = ""; Password = ""; DateOfBirth = DateTime.MinValue })

        // handle post to "action6"
        // first, a regular function
        let greet name = sprintf "Hello %s" name
        // binding to request
        let greet' (ctx: ControllerContext) = 
            let boundGreet = greet >> sprintf "<h1>%s</h1>" >> Result.content
            boundGreet ctx.["somefield"]
        post "action6" greet'

        // handle get to "action6"
        // first, a regular function
        let greet firstName lastName age = 
            sprintf "Hello %s %s, you're %d years old" firstName lastName age
        // binding to request
        let greet' (ctx: ControllerContext) =
            greet ctx.["firstname"] ctx.["lastname"] (int ctx.["age"])
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
        let s = e.Shortcut
        let wbpage title = 
            [e.Html [
                e.Head [
                    e.Title [ &title ]
                ]
                e.Body [
                    e.H1 [ &title ]
                ]
            ]]
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
            let query = ctx.Url.Segments.[2]
            let query = HttpUtility.UrlEncode query
            use web = new WebClient()
            let! content = web.AsyncDownloadString(Uri("http://www.google.com/search?q=" + query))
            Debug.WriteLine "got google response"
            return Result.content content
        }
        asyncAction (ifMethodIsGet &&. ifUrlMatches "^/google/") google

        // formlets

        let layout title body =
            [ 
                e.DocTypeHTML5
                e.Html [
                    e.Head [
                        e.Title [ &title ]
                        e.Style [ 
                            &".error {color:red;}"
                            &"body {font-family:Verdana,Geneva,sans-serif; line-height: 160%;}"
                        ]
                    ]
                    e.Body [
                        yield e.H1 [ &title ]
                        yield! body
                    ]
                ]
            ]

        let f = e.Formlets

        let registrationFormlet =

            let reCaptcha = reCaptcha {PublicKey = "6LfbkMESAAAAAPBL8AK4JhtzHMgcRez3UlQ9FZkz"; PrivateKey = "6LfbkMESAAAAANzdOHD_A6uZwAplnJCoiL2F6hEF"; MockedResult = None}

            let dateFormlet : DateTime Formlet =
                let baseFormlet = 
                    yields t3
                    <*> (f.Text(maxlength = 2, attributes = ["type","number"; "min","1"; "max","12"; "required","required"; "size","3"]) |> f.WithLabel "Month: ")
                    <*> (f.Text(maxlength = 2, attributes = ["type","number"; "min","1"; "max","31"; "required","required"; "size","3"]) |> f.WithLabel "Day: ")
                    <*> (f.Text(maxlength = 4, attributes = ["type","number"; "min","1900"; "required","required"; "size","5"]) |> f.WithLabel "Year: ")
                let isDate (month,day,year) = 
                    let pad n (v: string) = v.PadLeft(n,'0')
                    let ymd = sprintf "%s%s%s" (pad 4 year) (pad 2 month) (pad 2 day)
                    DateTime.TryParseExact(ymd, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None) |> fst
                let dateValidator = err isDate (fun _ -> "Invalid date")
                baseFormlet 
                |> satisfies dateValidator
                |> map (fun (month,day,year) -> DateTime(int year,int month,int day))

            let doublePassword =
                // http://bugsquash.blogspot.com/2011/02/password-strength-entropy-and.html
                let compressedLength (s: string) =
                    use buffer = new MemoryStream()
                    use comp = new DeflateStream(buffer, CompressionMode.Compress)
                    use w = new StreamWriter(comp)
                    w.Write(s)
                    w.Flush()
                    buffer.Length
                let isStrong s = compressedLength s >= 106L
                let f =
                    yields t2
                    <*> (f.Password(required = true) |> f.WithLabel "Password: ")
                    <+ e.Br()
                    <*> (f.Password(required = true) |> f.WithLabel "Repeat password: ")
                let areEqual (a,b) = a = b
                f
                |> satisfies (err areEqual (fun _ -> "Passwords don't match"))
                |> map fst
                |> satisfies (err isStrong (fun _ -> "Password too weak"))

            fun ip ->
                yields (fun n e p d -> 
                            { Name = n; Email = e; Password = p; DateOfBirth = d })
                <*> (f.Text(required = true) |> f.WithLabel "Name: ")
                <+ e.Br()
                <*> (f.Email(required = true) |> f.WithLabel "Email: ")
                <+ e.Br()
                <*> doublePassword
                <+ e.Br()
                <+ &"Date of birth: " <*> dateFormlet
                <+ e.Br()
                <+ &"Please read very carefully these terms and conditions before registering for this online program, blah blah blah"
                <+ e.Br()
                <* (f.Checkbox(false) |> satisfies (err id (fun _ -> "Please accept the terms and conditions")) |> f.WithLabel "I agree to the terms and conditions above")
                <* reCaptcha ip

        let jsValidation = 
            e.Div [
                s.JavascriptFile "http://cdn.jquerytools.org/1.2.5/full/jquery.tools.min.js"
                e.Script [ &"$('form').validator();" ]
            ]

        let registrationPage form =
            layout "Registration" [
                s.FormPost "" [
                    e.Fieldset [
                        yield e.Legend [ &"Please fill the fields below" ]
                        yield!!+form
                        yield e.Br()
                        yield s.Submit "Register!"
                    ]
                ]
                //jsValidation
            ]

        get "thankyou" (fun ctx -> Result.contentf "Thank you for registering, %s" ctx.QueryString.["n"])
            
        formAction "register" {
            Formlet = fun ctx -> registrationFormlet ctx.IP
            Page = fun _ -> registrationPage
            Success = fun _ v -> Result.redirectf "thankyou?n=%s" v.Name
        }

        // http://www.paulgraham.com/arcchallenge.html
        let arcChallenge() =            
            let k,url,url2 = "s","said","showsaid"
            get url (wbview [s.FormPost url [e.Input ["name",k]; s.Submit "Send"]])
            post url (fun ctx -> (k, ctx.Form.[k]) ||> ctx.Session.Set; Result.wbview [s.Link url2 "click here"])
            get url2 (fun ctx -> Result.wbview [&ctx.Session.Get(k)])
        //arcChallenge()

        // http://www.paulgraham.com/arcchallenge.html
        let arcChallenge2() =
            let getpost url formlet action =
                let page _ form = [s.FormPost url [yield!!+ form; yield s.Submit "Send"]]
                formAction url {
                    Formlet = fun _ -> formlet
                    Page = page
                    Success = action
                }
            let k,url = "s","showsaid"
            getpost "said" (f.Text()) (fun ctx v -> ctx.Session.Set k v; Result.wbview [s.Link url "click here"])
            get url (fun ctx -> Result.wbview [&ctx.Session.Get(k)])
        //arcChallenge2()

        let formletSequence() =
            let inputsend = input "" [] <* submit "Send" []
            let ffirstname = text "First name:" *> inputsend
            let flastname = text "Last name:" *> inputsend

            ("name", 0)
            |> actionFormlet nop (fun _ _ _ -> (),ffirstname)
            |> actionFormlet ffirstname (fun _ _ firstname -> firstname,flastname)
            |> actionFormlet flastname (fun _ firstname lastname -> (),textf "Hello %s %s" firstname lastname)
            |> ignore
            ()

        //formletSequence()

        let formletBind() =
            let web = WebBuilder()
            let inputsend = input "" [] <* submit "Send" []
            let ffirstname = text "First name:" *> inputsend
            let flastname = text "Last name:" *> inputsend

            let cc = 
                web {
                    let! firstname = web.ShowFormlet ffirstname
                    let! lastname = web.ShowFormlet flastname
                    let show = textf "Hello %s %s" firstname lastname
                    return! web.ShowFormlet show
                }
            action (ifPathIs "name") (web.ToAction cc)
            ()

        formletBind()

        action any (status 404 => content "<h1>Not found!</h1>")
        
        ()
