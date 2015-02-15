﻿// 2015,2014 ,Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;

using LayoutFarm.UI;

namespace LayoutFarm
{

    public static class SampleViewportExtension
    {

        public static void AddContent(this SampleViewport viewport, UIElement ui)
        {
            viewport.ViewportControl.AddContent(ui.GetPrimaryRenderElement(viewport.ViewportControl.RootGfx));
        }
        public static void AddContent(this SampleViewport viewport, HtmlBoxes.HtmlHost host, HtmlWidgets.WidgetBase ui)
        {
            AddContent(viewport, ui.GetPrimaryUIElement(host));
        }
    }

}