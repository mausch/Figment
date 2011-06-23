module Figment.Helpers

open System
open System.Reflection
open System.Web
open System.Web.Routing
open System.Web.Mvc
open System.Web.Mvc.Async

open System.Diagnostics

type FAction = ControllerContext -> unit

type FAsyncAction = ControllerContext -> Async<unit>

let inline buildActionResult r = 
    {new ActionResult() with
        override x.ExecuteResult ctx = 
            if ctx = null
                then raise <| System.ArgumentNullException("ctx")
                else r ctx }

let inline exec ctx (r: ActionResult) =
    r.ExecuteResult ctx

let inline fromActionResult a : FAction =
    fun ctx -> exec ctx a

type ControllerFilters = {
    actionExecutedFilter: ActionExecutedContext -> unit
    actionExecutingFilter: ActionExecutingContext -> unit
    authorizationFilter: AuthorizationContext -> unit
    exceptionFilter: ExceptionContext -> unit
    resultExecutedFilter: ResultExecutedContext -> unit
    resultExecutingFilter: ResultExecutingContext -> unit
}

let DefaultControllerFilters = {
    actionExecutedFilter = fun c -> () 
    actionExecutingFilter = fun c -> ()
    authorizationFilter = fun c -> ()
    exceptionFilter = fun c -> ()
    resultExecutedFilter = fun c -> ()
    resultExecutingFilter = fun c -> ()
}

type FigmentAsyncController(action: FAsyncAction, filters: ControllerFilters) = 
    inherit ControllerBase()
        override this.ExecuteCore() = 
            Debug.WriteLine "ExecuteCore"
    interface IAsyncController with
        member this.BeginExecute(requestContext, cb, state) = 
            Debug.WriteLine "BeginExecute"
            // cb and state are from asp.net
            let controllerContext = ControllerContext(requestContext, this)
            let abegin, aend, acancel = Async.AsBeginEnd action
            let callback r = 
                Debug.WriteLine "BeginExecute callback"
                aend r
                cb.Invoke r

            abegin(controllerContext, AsyncCallback(callback), null)

        member this.EndExecute r = 
            Debug.WriteLine "EndExecute"
        member this.Execute r = 
            Debug.WriteLine "Execute"

let inline buildRouteHandler f =
    { new IRouteHandler with
        member x.GetHttpHandler ctx = f ctx }

let inline buildActionInvoker f =
    { new IActionInvoker with
        member x.InvokeAction(ctx, actionName) = f ctx actionName }

let buildControllerFromAction (action: FAction) =
    { new Controller() with 
        override x.CreateActionInvoker() = 
            upcast { new ControllerActionInvoker() with
                        override y.FindAction(ctx, descriptor, actionName) = 
                            { new ActionDescriptor() with
                                override z.ActionName = actionName
                                override z.ControllerDescriptor = 
                                    { new ControllerDescriptor() with
                                        override a.ControllerType = x.GetType()
                                        override a.FindAction(ctx, actionName) = z
                                        override a.GetCanonicalActions() = [|z|] }
                                override z.Execute(ctx, param) = 
                                    action ctx
                                    buildActionResult (fun _ -> ()) |> box
                                override z.GetParameters() = [||] } } }

let inline buildControllerFromAsyncAction (action: FAsyncAction) =
    new FigmentAsyncController(action, DefaultControllerFilters)

/// case-insensitive string comparison
let inline (=.) (x: string) (y: string) = 
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) = 0

/// case-insensitive string comparison
let inline (<>.) (x: string) (y: string) =
    StringComparer.InvariantCultureIgnoreCase.Compare(x,y) <> 0

let uncheckedClass = Type.GetType "Microsoft.FSharp.Core.Operators+Unchecked, FSharp.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
let defaultOfMethod = uncheckedClass.GetMethod "DefaultOf"

let defaultValueOf (t: Type) =
    let genericMethod = defaultOfMethod.MakeGenericMethod [| t |]
    genericMethod.Invoke(null, null)

let inline asyncf f x = 
    async {
        return f x
    }

let inline htmlencode x = HttpUtility.HtmlEncode x
let inline urlencode (x: string) = HttpUtility.UrlEncode x

type DelegatingHttpRequestBase(request: HttpRequestBase) =
    inherit HttpRequestBase() 
        override x.AcceptTypes = request.AcceptTypes
        override x.AnonymousID = request.AnonymousID
        override x.ApplicationPath = request.ApplicationPath
        override x.AppRelativeCurrentExecutionFilePath = request.AppRelativeCurrentExecutionFilePath
        override x.Browser = request.Browser
        override x.ClientCertificate = request.ClientCertificate
        override x.ContentEncoding = request.ContentEncoding
        override x.ContentLength = request.ContentLength
        override x.ContentType = request.ContentType
        override x.Cookies = request.Cookies
        override x.CurrentExecutionFilePath = request.CurrentExecutionFilePath
        override x.FilePath = request.FilePath
        override x.Files = request.Files
        override x.Filter = request.Filter
        override x.Form = request.Form
        override x.Headers = request.Headers
        override x.HttpMethod = request.HttpMethod
        override x.InputStream = request.InputStream
        override x.IsAuthenticated = request.IsAuthenticated
        override x.IsLocal = request.IsLocal
        override x.IsSecureConnection = request.IsSecureConnection
        override x.LogonUserIdentity = request.LogonUserIdentity
        override x.Params = request.Params
        override x.Path = request.Path
        override x.PathInfo = request.PathInfo
        override x.PhysicalApplicationPath = request.PhysicalApplicationPath
        override x.PhysicalPath = request.PhysicalPath
        override x.QueryString = request.QueryString
        override x.RawUrl = request.RawUrl
        override x.RequestType = request.RequestType
        override x.ServerVariables = request.ServerVariables
        override x.Item with get i = request.[i]
        override x.TotalBytes = request.TotalBytes
        override x.Url = request.Url
        override x.UrlReferrer = request.UrlReferrer
        override x.UserAgent = request.UserAgent
        override x.UserHostAddress = request.UserHostAddress
        override x.UserHostName = request.UserHostName
        override x.UserLanguages = request.UserLanguages
        override x.BinaryRead count = request.BinaryRead count
        override x.Equals o = request.Equals o
        override x.GetHashCode() = request.GetHashCode()
        override x.MapImageCoordinates imageFieldName = request.MapImageCoordinates imageFieldName
        override x.MapPath virtualPath = request.MapPath virtualPath
        override x.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping) = request.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping)
        override x.SaveAs(filename, includeHeaders) = request.SaveAs(filename, includeHeaders)
        override x.ToString() = request.ToString()
        override x.ValidateInput() = request.ValidateInput()


type DelegatingHttpContextBase(ctx: HttpContextBase) =
    inherit HttpContextBase() 
        override x.AllErrors = ctx.AllErrors
        override x.Application = ctx.Application
        override x.ApplicationInstance = ctx.ApplicationInstance
        override x.Cache = ctx.Cache
        override x.CurrentHandler = ctx.CurrentHandler
        override x.CurrentNotification = ctx.CurrentNotification
        override x.Error = ctx.Error
        override x.Handler = ctx.Handler
        override x.IsCustomErrorEnabled = ctx.IsCustomErrorEnabled
        override x.IsDebuggingEnabled = ctx.IsDebuggingEnabled
        override x.IsPostNotification = ctx.IsPostNotification
        override x.Items = ctx.Items
        override x.PreviousHandler = ctx.PreviousHandler
        override x.Profile = ctx.Profile
        override x.Request = ctx.Request
        override x.Response = ctx.Response
        override x.Server = ctx.Server
        override x.Session = ctx.Session
        override x.SkipAuthorization = ctx.SkipAuthorization
        override x.Timestamp = ctx.Timestamp
        override x.Trace = ctx.Trace
        override x.User = ctx.User
        override x.AddError e = ctx.AddError e
        override x.ClearError() = ctx.ClearError()
        override x.Equals o = ctx.Equals o
        override x.GetGlobalResourceObject(classKey, resourceKey) = ctx.GetGlobalResourceObject(classKey, resourceKey)
        override x.GetGlobalResourceObject(classKey, resourceKey, culture) = ctx.GetGlobalResourceObject(classKey, resourceKey, culture)
        override x.GetHashCode() = ctx.GetHashCode()
        override x.GetLocalResourceObject(virtualPath, resourceKey) = ctx.GetLocalResourceObject(virtualPath, resourceKey)
        override x.GetLocalResourceObject(virtualPath, resourceKey, culture) = ctx.GetLocalResourceObject(virtualPath, resourceKey, culture)
        override x.GetSection sectionName = ctx.GetSection sectionName
        override x.GetService serviceType = ctx.GetService serviceType
        override x.RewritePath path = ctx.RewritePath path
        override x.RewritePath(path, rebaseClientPath) = ctx.RewritePath(path, rebaseClientPath)
        override x.RewritePath(filePath, pathInfo, querystring) = ctx.RewritePath(filePath, pathInfo, querystring)
        override x.RewritePath(filePath, pathInfo, querystring, setClientFilePath) = ctx.RewritePath(filePath, pathInfo, querystring, setClientFilePath)
        override x.ToString() = ctx.ToString()

