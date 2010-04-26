open System
open System.IO
open System.Diagnostics
open System.Reflection

[<EntryPoint>]
let main args =
    // code adapted from FSharp.PowerPack's AspNetTester
    let progfile = 
        let prg = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        if Environment.Is64BitOperatingSystem && Environment.Is64BitProcess
            then prg + " (x86)"
            else prg
            
    let webserver = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE")
    if not (File.Exists webserver)
        then failwith "No ASP.NET dev web server found."
    
    let webSitePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName
    let port = Random().Next(10000, 65535)
    let pathArg = sprintf "/path:%s" webSitePath
    let portArg = sprintf "/port:%d" port
    
    let asm = Assembly.LoadFile webserver
    let run (args: string[]) = asm.EntryPoint.Invoke(null, [| args |]) :?> int

    Process.Start (sprintf "http://localhost:%d" port) |> ignore
    run [| pathArg; portArg |]    