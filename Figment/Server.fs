namespace Figment

open System
open System.IO
open System.Diagnostics
open System.Reflection

type Server =
    static member private startup(path: string option, port: string option, vpath: string option) =
        // code adapted from FSharp.PowerPack's AspNetTester
        let progfile = 
            let prg = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            if Environment.Is64BitProcess
                then prg + " (x86)"
                else prg
            
        let webserver = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE")
        if not (File.Exists webserver)
            then failwith "No ASP.NET dev web server found."
        printfn "%s" webserver

        let webSitePath = 
            match path with
            | None -> Directory.GetParent(Directory.GetCurrentDirectory()).FullName
            | Some a -> a.Substring 5
        printfn "path: %s" webSitePath

        let port = 
            match port with
            | None -> Random().Next(10000, 65535)
            | Some a -> Convert.ToInt32 (a.Substring 5)
        printfn "port: %d" port

        let vpath =
            match vpath with
            | None -> ""
            | Some a -> a.Substring 6
        let pathArg = sprintf "/path:%s" webSitePath
        let portArg = sprintf "/port:%d" port
    
        let asm = Assembly.LoadFile webserver
        let run (args: string[]) = asm.EntryPoint.Invoke(null, [| args |]) :?> int

        Process.Start (sprintf "http://localhost:%d%s" port vpath) |> ignore

    (*
        AppDomain.CurrentDomain.GetAssemblies()
        |> Seq.map (fun a -> a.FullName)
        |> Seq.iter (printfn "%s")
    *)

        run [| pathArg; portArg |]            

    static member start(?path: string, ?port: string, ?vpath: string) =
        Server.startup(path, port, vpath)

    static member start(args: string[]) =
        let getArg arg = args |> Seq.tryFind (fun a -> a.ToUpperInvariant().StartsWith arg)
        Server.startup(getArg "PATH:", getArg "PORT:", getArg "VPATH:")
