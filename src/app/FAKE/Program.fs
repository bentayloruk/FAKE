open System
open Fake
open System.IO

let fsiArgsSwitch = "--fsiargs"

let printVersion() =
    traceFAKE "FakePath: %s" fakePath
    traceFAKE "%s" fakeVersionStr

let printEnvironment cmdArgs args =
    printVersion()

    if buildServer = LocalBuild then
        trace localBuildLabel
    else
        tracefn "Build-Version: %s" buildVersion

    if cmdArgs |> Array.length > 1 then
        traceFAKE "FAKE Arguments:"
        args 
          |> Seq.map fst
          |> Seq.iter (tracefn "%A")

    log ""
    traceFAKE "FSI-Path: %s" fsiPath
    traceFAKE "MSBuild-Path: %s" msBuildExe

let containsParam param = Seq.map toLower >> Seq.exists ((=) (toLower param))

let paramIsHelp param = containsParam param ["help"; "?"; "/?"; "-h"; "--help"; "/h"; "/help"]

let buildScripts = !! "*.fsx" |> Seq.toList

try
    try
        AutoCloseXmlWriter <- true
        let cmdArgs = System.Environment.GetCommandLineArgs()
        if containsParam "version" cmdArgs then printVersion()
        else
            if (cmdArgs.Length = 2 && paramIsHelp cmdArgs.[1]) || (cmdArgs.Length = 1 && List.length buildScripts = 0) then CommandlineParams.printAllParams()
            else
                match Boot.ParseCommandLine(cmdArgs) with
                | None ->
                    //We support two models.
                    let oldFsiOptions = cmdArgs |> Array.filter (fun x -> x.StartsWith "-d:") |> Array.toList
                    let fakeArgs, fsiArgs = 
                        let fsiArgsSwitchIndex = cmdArgs |> Array.tryFindIndex (fun arg -> toLower arg = "--fsiargs")
                        match fsiArgsSwitchIndex with
                        | None ->
                            //This is the "fake.exe build.fsx target".
                            let buildScriptArg = if cmdArgs.Length > 1 && cmdArgs.[1].EndsWith ".fsx" then cmdArgs.[1] else Seq.head buildScripts
                            let fakeArgs = cmdArgs |> Array.filter (fun x -> x.StartsWith "-d:" = false || x.Equals(buildScriptArg, StringComparison.InvariantCultureIgnoreCase))
                            fakeArgs, FsiArgs(oldFsiOptions, buildScriptArg, []) 
                        | Some(i) ->
                            //This is the "fake.exe target --fsiargs --define:MONO build.fsx arg1 arg2".
                            let fsiArgs = cmdArgs.[i..]
                            if fsiArgs.Length = 1 then failwith "--fsiargs switch is present, but it is not followed by any arguments."
                            if oldFsiOptions.Length > 0 then
                                failwith "-d:<string> fsi option should not be used when also using --fsiargs.  Use -d:<string> after the --fsiargs switch."
                            match FsiArgs.parse fsiArgs.[1..] with
                            | Choice1Of2(fsiArgs) -> cmdArgs.[..i-1], fsiArgs
                            | Choice2Of2(errMsg) -> failwith errMsg

                    let args = CommandlineParams.parseArgs (fakeArgs |> Seq.filter ((<>) "details"))

                    traceStartBuild()
                    let printDetails = containsParam "details" cmdArgs
                    if printDetails then
                        printEnvironment cmdArgs args
                    if not (runBuildScriptWithFsiArgsAt "" printDetails fsiArgs args) then
                        Environment.ExitCode <- 1
                    else
                        if printDetails then log "Ready."
                | Some handler ->
                    handler.Interact()
    with
    | exn -> 
        if exn.InnerException <> null then
            sprintf "Build failed.\nError:\n%s\nInnerException:\n%s" exn.Message exn.InnerException.Message
            |> traceError
        else
            sprintf "Build failed.\nError:\n%s" exn.Message
            |> traceError

        sendTeamCityError exn.Message
        Environment.ExitCode <- 1

    if buildServer = BuildServer.TeamCity then
        killAllCreatedProcesses()

finally
    traceEndBuild()