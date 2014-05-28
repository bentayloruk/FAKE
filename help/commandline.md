# FAKE build command line usage

You can run FAKE from the command line in two ways.

The first usage is as follows:

    fake.exe [<buildScriptPath>] [<targetName>] [details] [<varialble1>[=<value1>] ... <variableN>[=<valueN>]] [-d:<string>]

The second usage is intended for builders that want full control of the arguments FAKE.exe provides to FSI when running the build script.

    fake.exe [<targetName>] [details] [<varialble1>[=<value1>] ... <variableN>[=<valueN>]] [--fsiScriptArgs <arg1> [... <argN>]]

## buildScriptPath

Path to the `fsx` build script file that FAKE will run in an `fsi` session.

Optional.  If not specified, FAKE will either use the first `.fsx` file it finds in the working directory.

**Note:  If you are using `--fsiargs`, you must not provide this argument.** 

## &lt;targetName>

Specify the name of the single Target you wish to run.

Optional.  If you do not specify a Target, the build script will control which Targets are run.

## details

TBC

## &lt;variableN>[=&lt;valueN>]

Specify key value pairs (or just keys) that will be set on the `ProcessStartInfo.EnvironmentVariables` dictionary.  If you specify the variable key only, the value will be set to `true`.

Optional.  

## -d:&lt;string>

Specify a value for the FSI `-d:<string>` option.

## --fsiargs &lt;arg1> .. &lt;argn>

Specify the FSI arguments that FAKE should use to run FSI.  FSI command line usage is as follows:

    fsi.exe [options] [script-file [arguments]]

When using this switch, you must do the following:

* Include your build script path as one of the arguments.
* Make sure you only have FSI option switches preceding the script path argument (they all start with - or --).

For example:

    fake.exe --fsiargs --warnaserror+ buildstuff.fsx argForBuildScript


# FAKE version command line usage

Check the FAKE version with the following usage:

`fake.exe version`

If you use `version` any other arguments you provide will be ignored.


# Running FAKE targets from the command line

For this short sample we assume you have the latest version of FAKE in *./tools/*. Now consider the following small FAKE script:

	#r "FAKE/tools/FakeLib.dll"
	open Fake 
 
	Target "Clean" (fun () ->  trace " --- Cleaning stuff --- ")
 
	Target "Build" (fun () ->  trace " --- Building the app --- ")
 
	Target "Deploy" (fun () -> trace " --- Deploying app --- ")
 
 
	"Clean"
	  ==> "Build"
	  ==> "Deploy"
 
	RunTargetOrDefault "Deploy"

If you are on windows then create this small redirect script:

	[lang=batchfile]
	@echo off
	"tools\Fake.exe" "%1"
	exit /b %errorlevel%

On mono you can use:

	[lang=batchfile]
	#!/bin/bash
    mono ./tools/FAKE.exe "$@"

Now you can run FAKE targets easily from the command line:

![alt text](pics/commandline/cmd.png "Running FAKE from cmd")