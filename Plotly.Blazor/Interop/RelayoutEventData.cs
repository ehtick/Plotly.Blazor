﻿using System.Collections.Generic;
using System.Text.Json;

namespace Plotly.Blazor.Interop
{
    /// <summary>
    ///     This class is used to parse the event data from the relayout jsinterop action.
    /// </summary>
    public class RelayoutEventData
    {
        /// <summary>
        ///     The x-axis of the layout. [x0, x1]
        /// </summary>
        /// <remarks>
        ///     In some cases this may be not be set.
        /// </remarks>
        public object[] XRange { get; set; }

        /// <summary>
        ///     The y-axis of the layout. [y0, y1].
        /// </summary>
        /// <remarks>
        ///      In some cases this may be not be set.
        /// </remarks>
        public object[] YRange { get; set; }

        /// <summary>
        ///     The z-axis of the layout. [z0, z1].
        /// </summary>
        /// <remarks>
        ///      In some cases this may be not be set.
        /// </remarks>
        public object[] ZRange { get; set; }

        /// <summary>
        ///     Returns a dictionary of all updated properties including their new value.
        /// </summary>
        public IDictionary<string, JsonElement> RawData { get; set; }
    }
}