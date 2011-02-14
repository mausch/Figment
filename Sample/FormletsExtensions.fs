namespace Figment

[<AutoOpen>]
module FormletsExtensions =
    open Figment.Routing
    open Formlets

    let formAction url formlet page successHandler =
        get url (fun _ -> Result.wbview (page url (renderToXml formlet)))
        post url
            (fun ctx -> 
                let env = EnvDict.fromFormAndFiles ctx.HttpContext.Request
                match run formlet env with
                | Success v -> successHandler ctx v
                | Failure(errorForm, _) -> Result.wbview (page url errorForm))

