/*
 * THIS FILE WAS GENERATED BY PLOTLY.BLAZOR.GENERATOR
*/

using System.Text.Json.Serialization;
using System.Runtime.Serialization;
#pragma warning disable 1591

namespace Plotly.Blazor.Traces.ScatterMapLib
{
    /// <summary>
    ///     Determines whether or not this trace is visible. If <c>legendonly</c>, the
    ///     trace is not drawn, but can appear as a legend item (provided that the legend
    ///     itself is visible).
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("Plotly.Blazor.Generator", null)]
    [JsonConverter(typeof(EnumConverter))]
    public enum VisibleEnum
    {
        [EnumMember(Value=@"true")]
        True = 0,
        [EnumMember(Value=@"False")]
        False,
        [EnumMember(Value=@"legendonly")]
        LegendOnly
    }
}