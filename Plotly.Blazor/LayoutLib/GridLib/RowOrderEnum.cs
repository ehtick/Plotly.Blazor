/*
 * THIS FILE WAS GENERATED BY PLOTLY.BLAZOR.GENERATOR
*/

using System.Text.Json.Serialization;
using System.Runtime.Serialization;
#pragma warning disable 1591

namespace Plotly.Blazor.LayoutLib.GridLib
{
    /// <summary>
    ///     Is the first row the top or the bottom? Note that columns are always enumerated
    ///     from left to right.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("Plotly.Blazor.Generator", null)]
    [JsonConverter(typeof(EnumConverter))]
    public enum RowOrderEnum
    {
        [EnumMember(Value=@"top to bottom")]
        TopToBottom = 0,
        [EnumMember(Value=@"bottom to top")]
        BottomToTop
    }
}