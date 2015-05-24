﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Sheet.Core
{
    public class Dependency
    {
        public XElement Element { get; set; }
        public Action<XElement, XPoint> Update { get; set; }
        public Dependency(XElement element, Action<XElement, XPoint> update)
        {
            Element = element;
            Update = update;
        }
    }
}
