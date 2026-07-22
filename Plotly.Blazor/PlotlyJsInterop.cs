using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Plotly.Blazor;

/// <summary>
///     Allows JsInterop functionality for Plotly.js
/// </summary>
/// <remarks>
///     Creates a new instance of <see cref="PlotlyJsInterop"/>.
/// </remarks>
/// <param name="jsRuntime"></param>
/// <param name="chart"></param>
/// <param name="useBasicVersion"></param>
public class PlotlyJsInterop(IJSRuntime jsRuntime, PlotlyChart chart, bool useBasicVersion) : IAsyncDisposable, IDisposable
{
    private const string InteropPath = "./_content/Plotly.Blazor/plotly-interop-7.2.0.js";
    private const string PlotlyPath = "./_content/Plotly.Blazor/plotly-3.7.0.min.js";
    private const string PlotlyBasicPath = "./_content/Plotly.Blazor/plotly-basic-3.7.0.min.js";

    private readonly DotNetObjectReference<PlotlyChart> dotNetObj = DotNetObjectReference.Create(chart);
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(LoadModulesAsync(jsRuntime, useBasicVersion));
    private readonly SemaphoreSlim lifecycleSemaphore = new(1, 1);
    private int disposeState;

    private static async Task<IJSObjectReference> LoadModulesAsync(IJSRuntime jsRuntime, bool useBasicVersion)
    {
        var jsObject = await jsRuntime.InvokeAsync<IJSObjectReference>("import", InteropPath);

        await jsObject.InvokeVoidAsync("importScript", "plotly-import", useBasicVersion ? PlotlyBasicPath : PlotlyPath);

        return jsObject;
    }

    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new PolymorphicConverter<ITrace>(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter()
        }
    };

    private async Task ExecuteAsync(
        Func<IJSObjectReference, PlotlyChart, ValueTask> action,
        CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref disposeState) != 0)
        {
            return;
        }

        await lifecycleSemaphore.WaitAsync(cancellationToken);

        try
        {
            // A component reference can still point to an old instance after Blazor has
            // removed it. Calls on that disposed instance intentionally become no-ops.
            if (Volatile.Read(ref disposeState) != 0)
            {
                return;
            }

            var runtime = await moduleTask.Value;
            await action(runtime, dotNetObj.Value);
        }
        finally
        {
            lifecycleSemaphore.Release();
        }
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IJSObjectReference, PlotlyChart, ValueTask<TResult>> action,
        CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref disposeState) != 0)
        {
            return default;
        }

        await lifecycleSemaphore.WaitAsync(cancellationToken);

        try
        {
            if (Volatile.Read(ref disposeState) != 0)
            {
                return default;
            }

            var runtime = await moduleTask.Value;
            return await action(runtime, dotNetObj.Value);
        }
        finally
        {
            lifecycleSemaphore.Release();
        }
    }

    /// <summary>
    ///     Can be used to add a new trace.
    /// </summary>
    /// <param name="trace">The trace data.</param>
    /// <param name="index">The optional index, where to add the trace.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task AddTrace(ITrace trace, int? index, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("addTrace",
                cancellationToken,
                currentChart.Id,
                trace?.PrepareJsInterop(SerializerOptions), index), cancellationToken);
    }

    /// <summary>
    ///     Can be used to delete a trace.
    /// </summary>
    /// <param name="index">The index of the trace, which should be removed.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task DeleteTrace(int index, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("deleteTrace",
                cancellationToken,
                currentChart.Id, index), cancellationToken);
    }

    /// <summary>
    ///     Can be used to download the chart as an image.
    /// </summary>
    /// <param name="format">Format of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="width">Width of the image.</param>
    /// <param name="fileName">Name od the image file.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<string> DownloadImage(ImageFormat format, uint height, uint width, string fileName, CancellationToken cancellationToken)
    {
        return await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeAsync<string>("downloadImage", cancellationToken,
                currentChart.Id, format, height, width, fileName), cancellationToken);
    }

    /// <summary>
    ///     Can be used to add data to an existing trace.
    /// </summary>
    /// <param name="x">X-Values.</param>
    /// <param name="y">Y-Values</param>
    /// <param name="indices">Indices.</param>
    /// <param name="max">Max Points.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task ExtendTraces(IEnumerable<IEnumerable<object>> x, IEnumerable<IEnumerable<object>> y, IEnumerable<int> indices, int? max, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("extendTraces",
                cancellationToken,
                currentChart.Id,
                x, y, indices, max), cancellationToken);
    }


    /// <summary>
    ///     Can be used to add data to an existing trace.
    /// </summary>
    /// <param name="x">X-Values.</param>
    /// <param name="y">Y-Values</param>
    /// <param name="z"></param>
    /// <param name="indices">Indices.</param>
    /// <param name="max">Max Points.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task ExtendTraces3D(IEnumerable<IEnumerable<object>> x, IEnumerable<IEnumerable<object>> y, IEnumerable<IEnumerable<object>> z, IEnumerable<int> indices, int? max, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("extendTraces3D",
                cancellationToken,
                currentChart.Id,
                x, y, z, indices, max), cancellationToken);
    }

    /// <summary>
    ///     Draws a new plot in an div element, overwriting any existing plot.
    ///     To update an existing plot in a div, it is much more efficient to use <see cref="React" /> than to overwrite it.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task NewPlot(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("newPlot",
                cancellationToken,
                currentChart.Id,
                currentChart.Data?.Select(trace => trace?.PrepareJsInterop(SerializerOptions)),
                currentChart.Layout?.PrepareJsInterop(SerializerOptions),
                currentChart.Config?.PrepareJsInterop(SerializerOptions),
                currentChart.Frames?.PrepareJsInterop(SerializerOptions)), cancellationToken);
    }

    /// <summary>
    ///     Can be used to prepend data to an existing trace.
    /// </summary>
    /// <param name="x">X-Values.</param>
    /// <param name="y">Y-Values</param>
    /// <param name="indices">Indices.</param>
    /// <param name="max">Max Points.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task PrependTraces(IEnumerable<IEnumerable<object>> x, IEnumerable<IEnumerable<object>> y, IEnumerable<int> indices, int? max, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("prependTraces",
                cancellationToken,
                currentChart.Id,
                x, y, indices, max), cancellationToken);
    }

    /// <summary>
    ///     Can be used to prepend data to an existing 3D trace.
    /// </summary>
    /// <param name="x">X-Values.</param>
    /// <param name="y">Y-Values</param>
    /// <param name="z"></param>
    /// <param name="indices">Indices.</param>
    /// <param name="max">Max Points.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task PrependTraces3D(IEnumerable<IEnumerable<object>> x, IEnumerable<IEnumerable<object>> y, IEnumerable<IEnumerable<object>> z, IEnumerable<int> indices, int? max, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("prependTraces3D",
                cancellationToken,
                currentChart.Id,
                x, y, z, indices, max), cancellationToken);
    }

    /// <summary>
    ///     Can be used to purge a chart.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task Purge(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("purge", cancellationToken, currentChart.Id), cancellationToken);
    }

    /// <summary>
    ///     Can be used in its place to create a plot, but when called again on the same div will update it far more
    ///     efficiently than <see cref="NewPlot" />.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task React(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("react",
                cancellationToken,
                currentChart.Id,
                currentChart.Data?.Select(trace => trace?.PrepareJsInterop(SerializerOptions)),
                currentChart.Layout?.PrepareJsInterop(SerializerOptions),
                currentChart.Config?.PrepareJsInterop(SerializerOptions),
                currentChart.Frames?.PrepareJsInterop(SerializerOptions)), cancellationToken);
    }

    /// <summary>
    ///     This function has comparable performance to <see cref="React"/> and is faster than redrawing the whole plot with <see cref="NewPlot"/> .
    ///     An efficient means of updating both the data array and layout object in an existing plot, basically a combination of <see cref="Restyle"/> and <see cref="Relayout"/>.
    /// </summary>
    /// <param name="dataUpdate">The data update, can be an anonymous type or a new trace object.</param>
    /// <param name="layoutUpdate">The layout update, can be an anonymous type or a new layout object.</param>
    /// <param name="indices">The traces to update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task Update(object dataUpdate = default, object layoutUpdate = default, IEnumerable<int> indices = default, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("update",
                cancellationToken,
                currentChart.Id,
                dataUpdate?.PrepareJsInterop(),
                layoutUpdate?.PrepareJsInterop(),
                indices?.PrepareJsInterop(SerializerOptions)), cancellationToken);
    }

    /// <summary>
    ///     An efficient means of updating the layout object of an existing plot.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task Relayout(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("relayout",
                cancellationToken,
                currentChart.Id,
                currentChart.Layout?.PrepareJsInterop(SerializerOptions)), cancellationToken);
    }

    /// <summary>
    ///     An efficient means of updating the layout object of an existing plot.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task Relayout<T>(T value, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("relayout",
                cancellationToken,
                currentChart.Id,
                value?.PrepareJsInterop(SerializerOptions)), cancellationToken);
    }

    /// <summary>
    ///     Can be used to add data to an existing trace.
    /// </summary>
    /// <param name="trace">The new trace parameter</param>
    /// <param name="indices">Indices.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task Restyle(ITrace trace, IEnumerable<int> indices, CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("restyle", cancellationToken,
                currentChart.Id, trace?.PrepareJsInterop(SerializerOptions), indices), cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe click events for legend.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task SubscribeLegendClickEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeLegendClickEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe click events for points.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task SubscribeClickEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeClickEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe hover events for points.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task SubscribeHoverEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeHoverEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe selected events for points.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task SubscribeSelectedEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeSelectedEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe to relayout events.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task SubscribeRelayoutEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeRelayoutEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to subscribe to restyle events.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task SubscribeRestyleEvent(CancellationToken cancellationToken)
    {
        await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeVoidAsync("subscribeRestyleEvent", cancellationToken, dotNetObj, currentChart.Id),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to export the chart as a static image and returns a binary string of the exported image.
    /// </summary>
    /// <param name="format">Format of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="width">Width of the image.</param>
    /// <returns>Binary string of the exported image.</returns>
    /// <param name="cancellationToken">CancellationToken</param>
    public async Task<string> ToImage(ImageFormat format, uint height, uint width, CancellationToken cancellationToken)
    {
        return await ExecuteAsync((runtime, currentChart) =>
            runtime.InvokeAsync<string>("toImage", cancellationToken, currentChart.Id, format, height, width),
            cancellationToken);
    }

    /// <summary>
    ///     Can be used to export the chart as a static image and returns a binary string of the exported image.
    /// </summary>
    /// <param name="chartDefinition">The chart definition to be exported.</param>
    /// <param name="format">Format of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="width">Width of the image.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Binary string of the exported image.</returns>
    public async Task<string> ToImage(ChartDefinition chartDefinition, ImageFormat format, uint height, uint width, CancellationToken cancellationToken)
    {
        return await ExecuteAsync((runtime, _) =>
            runtime.InvokeAsync<string>("toImageFromChartData", cancellationToken,
                chartDefinition.PrepareJsInterop(SerializerOptions), format, height, width), cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsyncCore().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (Interlocked.CompareExchange(ref disposeState, 1, 0) != 0)
        {
            return;
        }

        await lifecycleSemaphore.WaitAsync();

        try
        {
            var chartId = dotNetObj?.Value.Id;

            if (moduleTask?.IsValueCreated == true)
            {
                IJSObjectReference jsRuntime = null;

                try
                {
                    jsRuntime = await moduleTask.Value;
                }
                catch
                {
                    // ignore
                }

                if (jsRuntime != null)
                {
                    try
                    {
                        await jsRuntime.InvokeVoidAsync("purge", chartId);
                    }
                    catch
                    {
                        // ignore
                    }

                    try
                    {
                        await jsRuntime.DisposeAsync();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            dotNetObj?.Dispose();
        }
        finally
        {
            lifecycleSemaphore.Release();
        }
    }
}