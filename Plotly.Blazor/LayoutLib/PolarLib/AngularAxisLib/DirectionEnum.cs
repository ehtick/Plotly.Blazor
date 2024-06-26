/*
 * THIS FILE WAS GENERATED BY PLOTLY.BLAZOR.GENERATOR
*/

using System.Text.Json.Serialization;
using System.Runtime.Serialization;
#pragma warning disable 1591

namespace Plotly.Blazor.LayoutLib.PolarLib.AngularAxisLib
{
    /// <summary>
    ///     Sets the direction corresponding to positive angles.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("Plotly.Blazor.Generator", null)]
    [JsonConverter(typeof(EnumConverter))]
    public enum DirectionEnum
    {
        [EnumMember(Value=@"counterclockwise")]
        Counterclockwise = 0,
        [EnumMember(Value=@"clockwise")]
        Clockwise
    }
}