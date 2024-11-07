namespace SamplePredictor

open System
open System.Collections.Generic
open System.Management.Automation
open System.Management.Automation.Subsystem
open System.Management.Automation.Subsystem.Prediction
open System.Threading

[<AutoOpen>]
module Debug =
    open System.IO
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices

    let lockObj = new obj ()
    let logPath = "./SamplePredictor-debug.log"

    [<AbstractClass; Sealed>]
    type Logger =
        static member LogFile
            (
                res,
                [<Optional; DefaultParameterValue(""); CallerMemberName>] caller: string,
                [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
                [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
            ) =

            // NOTE: lock to avoid another process error when dotnet test.
            lock lockObj (fun () ->
                use sw = new StreamWriter(logPath, true)

                res
                |> List.iter (
                    fprintfn
                        sw
                        "[%s] %s at %d %s <%A>"
                        (DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"))
                        path
                        line
                        caller
                ))

type SamplePredictor(guid: string) =
    let id = Guid.Parse(guid)
    let name = "SamplePredictor"
    let description = "A sample predictor"

    interface ICommandPredictor with
        member __.Id = id
        member __.Name = name
        member __.Description: string = description
        member __.FunctionsToDefine: Dictionary<string, string> = Dictionary<string, string>()

        member __.GetSuggestion
            (
                client: PredictionClient,
                context: PredictionContext,
                cancellationToken: CancellationToken
            ) : SuggestionPackage =
            let input = context.InputAst.Extent.Text

            if String.IsNullOrWhiteSpace(input) then
                // NOTE: cannot pass null.
                Logger.LogFile([ $"suggestion with empty input. client: '{client.Name}' '{client.Name}' " ])
                SuggestionPackage(List<PredictiveSuggestion>([]))
            else
                Logger.LogFile([ $"suggestion with input '{input}'. client: '{client.Name}' '{client.Name}" ])
                SuggestionPackage(List<PredictiveSuggestion>([ PredictiveSuggestion(sprintf "%s HELLO WORLD" input) ]))


        member __.CanAcceptFeedback(client: PredictionClient, feedback: PredictorFeedbackKind) : bool =
            Logger.LogFile([ "cannot accept feedback" ])
            false

        member __.OnSuggestionDisplayed(client: PredictionClient, session: uint32, countOrIndex: int) : unit =
            Logger.LogFile([ "suggest displayed" ])

        member __.OnSuggestionAccepted(client: PredictionClient, session: uint32, acceptedSuggestion: string) : unit =
            Logger.LogFile([ "suggest accepted" ])

        member __.OnCommandLineAccepted(client: PredictionClient, history: IReadOnlyList<string>) : unit =
            Logger.LogFile([ "command accepted" ])

        member __.OnCommandLineExecuted(client: PredictionClient, commandLine: string, success: bool) : unit =
            Logger.LogFile([ "command executed" ])

type Init() =
    let identifier = "c7f63bae-8f2a-4ded-b768-5897e4c63076"

    interface IModuleAssemblyInitializer with
        member __.OnImport() =
            Logger.LogFile([ "import" ])
            let predictor = SamplePredictor(identifier)
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor)

    interface IModuleAssemblyCleanup with
        member __.OnRemove(psModuleInfo: PSModuleInfo) =
            Logger.LogFile([ "remove" ])
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, Guid(identifier))
