// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Result;
using EricksonLopez.Result.Grpc;
using Grpc.Core;

namespace EricksonLopez.Result.Sample.Examples;

public static class GrpcShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 33. GRPC PROTOCOL BRIDGE & STATUS INTERCEPTOR SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. ErrorType to Grpc StatusCode Mapping
        Console.WriteLine("\n[1] Canonical ErrorType -> StatusCode mapping:");
        Console.WriteLine($"  Validation     -> {ErrorType.Validation.ToStatusCode()}");
        Console.WriteLine($"  NotFound       -> {ErrorType.NotFound.ToStatusCode()}");
        Console.WriteLine($"  Conflict       -> {ErrorType.Conflict.ToStatusCode()}");
        Console.WriteLine($"  Unauthorized   -> {ErrorType.Unauthorized.ToStatusCode()}");
        Console.WriteLine($"  Forbidden      -> {ErrorType.Forbidden.ToStatusCode()}");
        Console.WriteLine($"  Unavailable    -> {ErrorType.Unavailable.ToStatusCode()}");
        Console.WriteLine($"  Infrastructure -> {ErrorType.Infrastructure.ToStatusCode()}");
        Console.WriteLine($"  Unexpected     -> {ErrorType.Unexpected.ToStatusCode()}");

        // 2. Mapping Domain Error to RpcException with Trailers
        Console.WriteLine("\n[2] Converting Error to RpcException with Metadata Trailers:");
        var domainError = Error.Create("Catalog.ItemOutOfStock", "The requested product inventory is depleted.")
            .WithType(ErrorType.Conflict)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Transient)
            .WithMetadata("Sku", "SKU-99482")
            .WithMetadata("WarehouseId", "WH-04")
            .Build();

        RpcException rpcEx = domainError.ToRpcException();

        Console.WriteLine($"  RpcException Status:   {rpcEx.StatusCode}");
        Console.WriteLine($"  RpcException Detail:   {rpcEx.Status.Detail}");
        Console.WriteLine($"  Trailer 'error-code':  {rpcEx.Trailers.GetValue("error-code")}");
        Console.WriteLine($"  Trailer 'error-type':  {rpcEx.Trailers.GetValue("error-type")}");
        Console.WriteLine($"  Trailer 'error-meta':  {rpcEx.Trailers.GetValue("error-meta-sku")}");

        // 3. Round-Trip Reconstitution: RpcException -> Error
        Console.WriteLine("\n[3] Reconstituting Error from RpcException trailers on client side:");
        Error reconstituted = rpcEx.ToError();

        Console.WriteLine($"  Reconstituted Code:        {reconstituted.Code}");
        Console.WriteLine($"  Reconstituted Type:        {reconstituted.Type}");
        Console.WriteLine($"  Reconstituted Description: {reconstituted.Description}");
        Console.WriteLine($"  Reconstituted Sku:         {reconstituted.Metadata?["sku"]}");

        // 4. Server Interceptor & Client Extension Architecture
        Console.WriteLine("\n[4] gRPC Interceptor Architecture:");
        Console.WriteLine($"  • ResultServerInterceptor intercepts UnaryServerHandler calls.");
        Console.WriteLine($"  • If handler returns IResultOutcome with IsFailure == true, it throws RpcException with trailers.");
        Console.WriteLine($"  • Client extension call.ToResultAsync() catches RpcException and reconstitutes Error automatically.");
    }
}
