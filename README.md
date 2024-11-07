# A sample command-line predictor written in F\#

This project is based on this article.

[How to create a command-line predictor - PowerShell | Microsoft Learn](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/create-cmdline-predictor?view=powershell-7.4)

## Usage

```powershell
# PredictionSource = HistoryAndPlugin required.
Get-PSReadLineOption | Select-Object PredictionSource
#
# PredictionSource
# ----------------
# HistoryAndPlugin

dotnet build
Import-Module .\bin\Debug\net6.0\SamplePredictor.dll

# Confirm SamplePredictor loaded.
Get-PSSubsystem -Kind CommandPredictor
#
# Kind              SubsystemType      IsRegistered Implementations
# ----              -------------      ------------ ---------------
# CommandPredictor  ICommandPredictor          True {Windows Package Manager - WinGet, SamplePredictor}
```

Then you can see a sample suggestion as follows.

```powershell
> sss
<-/1>                                                                           <SamplePredictor(1)>
> sss HELLO WORLD                                                                  [SamplePredictor]
```

## Known issues

A nested PowerShell session seems unable to find `FSharp.Core.dll`.

```powershell
pwsh # start a new PowerShell session
Import-Module .\bin\Debug\net6.0\SamplePredictor.dll
# Import-Module .\bin\Debug\net6.0\SamplePredictor.dll: Could not load file or assembly 'FSharp.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'. The system cannot find the file specified.
```
