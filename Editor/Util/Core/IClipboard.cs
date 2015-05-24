﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Sheet.Editor
{
    public interface IClipboard
    {
        void Set(string text);
        string Get();
    }
}
