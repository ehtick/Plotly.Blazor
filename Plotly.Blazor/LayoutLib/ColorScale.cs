/*
 * THIS FILE WAS GENERATED BY PLOTLY.BLAZOR.GENERATOR
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Serialization;

namespace Plotly.Blazor.LayoutLib
{
    /// <summary>
    ///     The ColorScale class.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("Plotly.Blazor.Generator", "1.0.0.0")]
    [Serializable]
    public class ColorScale : IEquatable<ColorScale>
    {
        /// <summary>
        ///     Sets the default sequential colorscale for positive values. Note that <c>autocolorscale</c>
        ///     must be true for this attribute to work.
        /// </summary>
        [JsonPropertyName(@"sequential")]
        public object Sequential { get; set;} 

        /// <summary>
        ///     Sets the default sequential colorscale for negative values. Note that <c>autocolorscale</c>
        ///     must be true for this attribute to work.
        /// </summary>
        [JsonPropertyName(@"sequentialminus")]
        public object SequentialMinus { get; set;} 

        /// <summary>
        ///     Sets the default diverging colorscale. Note that <c>autocolorscale</c> must
        ///     be true for this attribute to work.
        /// </summary>
        [JsonPropertyName(@"diverging")]
        public object Diverging { get; set;} 

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is ColorScale other)) return false;

            return ReferenceEquals(this, obj) || Equals(other);
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] ColorScale other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Sequential == other.Sequential ||
                    Sequential != null &&
                    Sequential.Equals(other.Sequential)
                ) && 
                (
                    SequentialMinus == other.SequentialMinus ||
                    SequentialMinus != null &&
                    SequentialMinus.Equals(other.SequentialMinus)
                ) && 
                (
                    Diverging == other.Diverging ||
                    Diverging != null &&
                    Diverging.Equals(other.Diverging)
                );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                if (Sequential != null) hashCode = hashCode * 59 + Sequential.GetHashCode();
                if (SequentialMinus != null) hashCode = hashCode * 59 + SequentialMinus.GetHashCode();
                if (Diverging != null) hashCode = hashCode * 59 + Diverging.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality of the left ColorScale and the right ColorScale.
        /// </summary>
        /// <param name="left">Left ColorScale.</param>
        /// <param name="right">Right ColorScale.</param>
        /// <returns>Boolean</returns>
        public static bool operator == (ColorScale left, ColorScale right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Checks for inequality of the left ColorScale and the right ColorScale.
        /// </summary>
        /// <param name="left">Left ColorScale.</param>
        /// <param name="right">Right ColorScale.</param>
        /// <returns>Boolean</returns>
        public static bool operator != (ColorScale left, ColorScale right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///     Gets a deep copy of this instance.
        /// </summary>
        /// <returns>ColorScale</returns>
        public ColorScale DeepClone()
        {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            return (ColorScale) formatter.Deserialize(ms);
        }
    }
}