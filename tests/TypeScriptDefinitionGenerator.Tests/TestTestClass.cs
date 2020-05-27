// When editing, make sure you have VS extension "TypeScript Definition Generator" installed -->
// https://aploris.sharepoint.com/_layouts/OneNote.aspx?id=%2FOneNote%2FDev%20Infrastructure&wd=target%28Visual%20Studio.one%7C7A318C9B-6DAA-4033-85F0-D913C6FF25D1%2FTypeScript%20Definition%20Generator%7CDE3B0E73-AAC3-48DD-B5EA-8F6CF2F7D29A%2F%29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LibraryServer.Scripts.Ts.Models.Components
{
    /// <summary>
    /// Model for Fabric component FabricCommandActions
    /// </summary>
    public class FabricCommandActionsModel
    {
#pragma warning disable SA1300 // Re-implement TS interface. Allow lower-case property names
        public List<ContextualMenuItem> items { get; set; }

        /// <summary>
        /// Model as JSON object
        /// </summary>
        /// <returns>JSON object string</returns>
        public string AsJsonObject() { return "blubb"; }

        public class ContextualMenuItem
        {
            public string key { get; set; }
            public string className { get; set; }
            public string text { get; set; }
            public bool disabled { get; set; }
            public IconProps iconProps { get; set; }
            public string href { get; internal set; }

            /// <summary>
            /// Special action for this menu item, handled by FabricCommandActions.tsx. If set, <see cref="href"/> is ignored
            /// </summary>
            public ContextualMenuItemAction Action { get; internal set; }

            /// <summary>
            /// Data used by and depending on <see cref="Action"/>
            /// </summary>
            public string ActionDataString1 { get; internal set; }

            /// <summary>
            /// Data used by and depending on <see cref="Action"/>
            /// </summary>
            public string ActionDataString2 { get; internal set; }

            /// <summary>
            /// Data used by and depending on <see cref="Action"/>
            /// </summary>
            public int ActionDataNumber1 { get; internal set; }
        }

        public enum ContextualMenuItemAction
        {
            /// <summary>
            /// Click HTML element identified by selector stored in <see cref="ContextualMenuItem.ActionDataString1"/>
            /// </summary>
            /// <remarks>
            /// <see cref="ContextualMenuItem.ActionDataString1"/>: Selector for element to click
            /// </remarks>
            ClickHtmlElement,

            /// <summary>
            /// Show HTML element identified by selector stored in <see cref="ContextualMenuItem.ActionDataString1"/> in Fabric/FluentUI
            /// panel using <see cref="ContextualMenuItem.ActionDataString2"/> as panel title.
            /// </summary>
            /// <remarks>
            /// <see cref="ContextualMenuItem.ActionDataString1"/>: Selector for panel content
            /// <see cref="ContextualMenuItem.ActionDataString2"/>: Title for panel
            /// </remarks>
            OpenHtmlElementInPanel,
        }

        public class IconProps
        {
            public string iconName { get; set; }
        }
    }
}