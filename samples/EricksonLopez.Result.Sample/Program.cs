using System;
using System.Threading.Tasks;
using EricksonLopez.Result.Sample.Examples;

namespace EricksonLopez.Result.Sample;

sealed class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("======================================================");
        Console.WriteLine(" EricksonLopez.Result - Core Framework Sample Guide");
        Console.WriteLine("======================================================");

        BasicCreation.Run();
        ErrorsAndBuilders.Run();
        MatchingAndExecuting.Run();
        MappingAndChaining.Run();
        Tapping.Run();
        Combining.Run();
        await AsyncOperations.RunAsync();
        StatePassingAllocFree.Run();
        
        LinqIntegration.Run();
        SyncExtensions.Run();
        await AdvancedAsyncOperations.RunAsync();

        Console.WriteLine("\n[Core Examples Finished Successfully]\n");
    }
}
