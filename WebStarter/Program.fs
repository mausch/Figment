open System
open System.IO
open System.Diagnostics
open System.Reflection

type IntFun = int * int -> bool

let (&&.) (fun1: IntFun) (fun2: IntFun) =
    fun (a: int) (b: int) -> fun1(a,b) && fun2(a,b)

let f1 (x,y) = x > y
let f2 (x,y) = x = y
let f3 = f1 &&. f2

[<EntryPoint>]
let main args =
    // code adapted from FSharp.PowerPack's AspNetTester
    let progfile = 
        let prg = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        if Environment.Is64BitProcess
            then prg + " (x86)"
            else prg
            
    let webserver = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE")
    if not (File.Exists webserver)
        then failwith "No ASP.NET dev web server found."

    let getArg arg = args |> Seq.tryFind (fun a -> a.ToUpperInvariant().StartsWith arg)
    let webSitePath = 
        match getArg "PATH:" with
        | None -> Directory.GetParent(Directory.GetCurrentDirectory()).FullName
        | Some a -> a.Substring 5
    let port = 
        match getArg "PORT:" with
        | None -> Random().Next(10000, 65535)
        | Some a -> Convert.ToInt32 (a.Substring 5)
    let vpath =
        match getArg "VPATH:" with
        | None -> ""
        | Some a -> a.Substring 6
    let pathArg = sprintf "/path:%s" webSitePath
    let portArg = sprintf "/port:%d" port
    
    let asm = Assembly.LoadFile webserver
    let run (args: string[]) = asm.EntryPoint.Invoke(null, [| args |]) :?> int

    Process.Start (sprintf "http://localhost:%d%s" port vpath) |> ignore
    run [| pathArg; portArg |]    