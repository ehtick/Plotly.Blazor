using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.JSInterop;
using NUnit.Framework;

namespace Plotly.Blazor.Tests;

public class DisposalTests
{
    [Test]
    public void PlotlyJsInterop_Dispose_DisposesDotNetReference()
    {
        var chart = new PlotlyChart();

        var interop = new PlotlyJsInterop(new TestJsRuntime(), chart, false);
        var dotNetObjectReference = GetDotNetObjectReference(interop);

        GetDisposed(dotNetObjectReference).Should().BeFalse();

        interop.Dispose();

        GetDisposed(dotNetObjectReference).Should().BeTrue();
    }

    [Test]
    public void PlotlyChart_Dispose_DelegatesToInterop()
    {
        var chart = new PlotlyChart();
        var jsRuntime = new TestJsRuntime();

        SetPrivateProperty(chart, "JsRuntime", jsRuntime);
        InvokeNonPublic(chart, "OnInitialized");

        var interop = GetPrivateProperty<PlotlyJsInterop>(chart, "Interop");
        var dotNetObjectReference = GetDotNetObjectReference(interop);

        interop.React(CancellationToken.None).GetAwaiter().GetResult();
        GetDisposed(dotNetObjectReference).Should().BeFalse();

        chart.Dispose();

        GetDisposed(dotNetObjectReference).Should().BeTrue();
        jsRuntime.Module.DisposeCalled.Should().BeTrue();
    }

    [Test]
    public async Task PlotlyChart_React_IsNoOp_AfterDispose()
    {
        var chart = new PlotlyChart();
        var jsRuntime = new TestJsRuntime();

        SetPrivateProperty(chart, "JsRuntime", jsRuntime);
        InvokeNonPublic(chart, "OnInitialized");

        await chart.DisposeAsync();
        var importCallCount = jsRuntime.ImportCallCount;
        var invocationCallCount = jsRuntime.Module.InvocationCallCount;
        await Assert.ThatAsync(() => chart.React(), Throws.Nothing);

        jsRuntime.ImportCallCount.Should().Be(importCallCount);
        jsRuntime.Module.InvocationCallCount.Should().Be(invocationCallCount);
    }

    [Test]
    public async Task PlotlyJsInterop_Operations_AreNoOps_AfterDispose()
    {
        var jsRuntime = new TestJsRuntime();
        var interop = new PlotlyJsInterop(jsRuntime, new PlotlyChart(), false);

        await interop.DisposeAsync();
        var importCallCount = jsRuntime.ImportCallCount;
        var invocationCallCount = jsRuntime.Module.InvocationCallCount;

        var operationMethods = typeof(PlotlyJsInterop)
                                     .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                     .Where(method => typeof(Task).IsAssignableFrom(method.ReturnType));

        foreach (var operationMethod in operationMethods)
        {
            var method = operationMethod.IsGenericMethodDefinition
                ? operationMethod.MakeGenericMethod(typeof(object))
                : operationMethod;
            var arguments = method.GetParameters()
                                  .Select(parameter => parameter.ParameterType.IsValueType
                                      ? Activator.CreateInstance(parameter.ParameterType)
                                      : null)
                                  .ToArray();

            await (Task)method.Invoke(interop, arguments)!;
        }

        jsRuntime.ImportCallCount.Should().Be(importCallCount);
        jsRuntime.Module.InvocationCallCount.Should().Be(invocationCallCount);
    }

    [Test]
    public async Task PlotlyChart_Hidden_RefreshesChart_WhenShownAgain()
    {
        var chart = new PlotlyChart();
        var jsRuntime = new TestJsRuntime();

        SetPrivateProperty(chart, "Hidden", true);
        SetPrivateProperty(chart, "JsRuntime", jsRuntime);
        InvokeNonPublic(chart, "OnInitialized");

        await InvokeNonPublicAsync(chart, "OnAfterRenderAsync", true);
        SetPrivateProperty(chart, "Hidden", false);
        SetPrivateField(chart, "hiddenChanged", true);
        await InvokeNonPublicAsync(chart, "OnAfterRenderAsync", false);

        jsRuntime.Module.NewPlotCallCount.Should().Be(1);
        jsRuntime.Module.ReactCallCount.Should().Be(1);
        GetDisposed(GetDotNetObjectReference(GetPrivateProperty<PlotlyJsInterop>(chart, "Interop"))).Should().BeFalse();
    }

    [Test]
    public async Task PlotlyJsInterop_DisposeAsync_IsFailSafe_WhenModuleInitializationFailed()
    {
        var chart = new PlotlyChart();

        var interop = new PlotlyJsInterop(new TestJsRuntime
        {
            ImportException = new InvalidOperationException("import failed")
        }, chart, false);

        var dotNetObjectReference = GetDotNetObjectReference(interop);

        await Assert.ThatAsync(() => interop.React(CancellationToken.None), Throws.TypeOf<InvalidOperationException>());
        await Assert.ThatAsync(() => interop.DisposeAsync().AsTask(), Throws.Nothing);
        GetDisposed(dotNetObjectReference).Should().BeTrue();
    }

    [Test]
    public async Task PlotlyJsInterop_DisposeAsync_IsFailSafe_WhenJsCleanupThrows()
    {
        var chart = new PlotlyChart();

        var jsRuntime = new TestJsRuntime
        {
            Module = new()
            {
                ThrowOnPurge = true
            }
        };

        var interop = new PlotlyJsInterop(jsRuntime, chart, false);
        var dotNetObjectReference = GetDotNetObjectReference(interop);

        await interop.React(CancellationToken.None);
        await Assert.ThatAsync(() => interop.DisposeAsync().AsTask(), Throws.Nothing);
        GetDisposed(dotNetObjectReference).Should().BeTrue();
        jsRuntime.Module.DisposeCalled.Should().BeTrue();
    }

    private static object GetDotNetObjectReference(PlotlyJsInterop interop)
    {
        return typeof(PlotlyJsInterop)
                   .GetField("dotNetObj", BindingFlags.Instance | BindingFlags.NonPublic)
                   ?.GetValue(interop)
               ?? throw new InvalidOperationException("dotNetObj field not found.");
    }

    private static bool GetDisposed(object dotNetObjectReference)
    {
        return (bool)(dotNetObjectReference.GetType()
                          .GetProperty("Disposed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          ?.GetValue(dotNetObjectReference)
                      ?? throw new InvalidOperationException("Disposed property not found."));
    }

    private static T GetPrivateProperty<T>(object instance, string propertyName)
    {
        return (T)(instance.GetType()
                       .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic)
                       ?.GetValue(instance)
                   ?? throw new InvalidOperationException($"Property '{propertyName}' not found."));
    }

    private static void SetPrivateProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName,
                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                       ?? throw new InvalidOperationException($"Property '{propertyName}' not found.");

        property.SetValue(instance, value);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Field '{fieldName}' not found.");

        field.SetValue(instance, value);
    }

    private static void InvokeNonPublic(object instance, string methodName)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException($"Method '{methodName}' not found.");

        method.Invoke(instance, null);
    }

    private static async Task InvokeNonPublicAsync(object instance, string methodName, params object[] parameters)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException($"Method '{methodName}' not found.");

        await (Task)method.Invoke(instance, parameters)!;
    }

    private sealed class TestJsRuntime : IJSRuntime
    {
        public int ImportCallCount { get; private set; }

        public Exception ImportException { get; init; }

        public TestJsObjectReference Module { get; init; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
            object[] args)
        {
            ImportCallCount++;

            if (identifier != "import")
            {
                throw new InvalidOperationException($"Unexpected identifier '{identifier}'.");
            }

            return ImportException is not null
                ? throw ImportException
                : new((TValue)(object)Module);
        }
    }

    private sealed class TestJsObjectReference : IJSObjectReference
    {
        public bool ThrowOnPurge { get; init; }

        public bool ThrowOnDispose { get; init; }

        public bool DisposeCalled { get; private set; }

        public int InvocationCallCount { get; private set; }

        public int NewPlotCallCount { get; private set; }

        public int ReactCallCount { get; private set; }

        public ValueTask DisposeAsync()
        {
            DisposeCalled = true;

            return ThrowOnDispose
                ? throw new InvalidOperationException("dispose failed")
                : ValueTask.CompletedTask;
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
            object[] args)
        {
            InvocationCallCount++;

            if (identifier == "purge" && ThrowOnPurge)
            {
                throw new InvalidOperationException("purge failed");
            }

            if (identifier == "newPlot")
            {
                NewPlotCallCount++;
            }

            if (identifier == "react")
            {
                ReactCallCount++;
            }

            return identifier is "newPlot" or "react" or "importScript" or "purge"
                ? new(default(TValue))
                : throw new InvalidOperationException($"Unexpected identifier '{identifier}'.");
        }
    }
}