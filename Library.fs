namespace SamplePredictor

open System
open System.Collections.Generic
open System.Management.Automation
open System.Management.Automation.Subsystem
open System.Management.Automation.Subsystem.Prediction
open System.Threading

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
                SuggestionPackage(List<PredictiveSuggestion>([]))
            else
                SuggestionPackage(List<PredictiveSuggestion>([ PredictiveSuggestion(sprintf "%s HELLO WORLD" input) ]))

        member __.CanAcceptFeedback(client: PredictionClient, feedback: PredictorFeedbackKind) : bool = false

        member __.OnSuggestionDisplayed(client: PredictionClient, session: uint32, countOrIndex: int) : unit = ()

        member __.OnSuggestionAccepted(client: PredictionClient, session: uint32, acceptedSuggestion: string) : unit =
            ()

        member __.OnCommandLineAccepted(client: PredictionClient, history: IReadOnlyList<string>) : unit = ()

        member __.OnCommandLineExecuted(client: PredictionClient, commandLine: string, success: bool) : unit = ()

type Init() =
    let identifier = "c7f63bae-8f2a-4ded-b768-5897e4c63076"

    interface IModuleAssemblyInitializer with
        member __.OnImport() =
            let predictor = SamplePredictor(identifier)
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor)

    interface IModuleAssemblyCleanup with
        member __.OnRemove(psModuleInfo: PSModuleInfo) =
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, Guid(identifier))
