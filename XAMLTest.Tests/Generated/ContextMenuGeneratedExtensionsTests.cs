﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace XamlTest.Tests.Generated
{
    partial class ContextMenuGeneratedExtensionsTests
    {
        static partial void OnClassInitialize()
        {
            GetWindowContent = x =>
            {
                return @$"
    <Window.ContextMenu>
        {x}
    </Window.ContextMenu>";
            };
        }
    }
}
