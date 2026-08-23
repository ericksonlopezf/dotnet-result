// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Sample.Examples;

namespace EricksonLopez.Result.Sample;

sealed class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("======================================================");
        Console.WriteLine(" EricksonLopez.Result - Official Reference Showcase");
        Console.WriteLine("======================================================");

        BasicCreation.Run();
        ErrorsAndBuilders.Run();
        MatchingAndExecuting.Run();
        MappingAndChaining.Run();
        Tapping.Run();
        Combining.Run();
        await AsyncOperations.RunAsync();
        StatePassingAllocFree.Run();
        await CumulativeValidation.RunAsync();
        await MaybeOptionType.RunAsync();
        GenericResult.Run();
        ErrorEqualityAndMutation.Run();
        LinqIntegration.Run();
        SyncExtensions.Run();
        await AdvancedAsyncOperations.RunAsync();
        TestingAssertionsExample.Run();
        RecoverAndErrorTransformation.Run();
        SafeAccessAndDeconstruct.Run();
        CombineAndMergeAdvanced.Run();
        WellKnownErrorsAndTry.Run();
        AdvancedApiCoverage.Run();

        Console.WriteLine("\n[All Core Showcase Examples Finished Successfully]\n");
    }
}
