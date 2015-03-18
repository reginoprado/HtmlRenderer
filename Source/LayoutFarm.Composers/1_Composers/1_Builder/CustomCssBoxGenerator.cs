﻿// 2015,2014 ,BSD, WinterDev 
using System.Collections.Generic;
using System.Globalization;
using PixelFarm.Drawing;
using LayoutFarm.WebDom;
using LayoutFarm.Css;
using LayoutFarm.HtmlBoxes;

namespace LayoutFarm.Composers
{

    public abstract class CustomCssBoxGenerator
    {
        protected abstract HtmlHost MyHost { get; }
        public abstract CssBox CreateCssBox(object tag, CssBox parentBox, BoxSpec spec, LayoutFarm.RootGraphic rootgfx);

        public CssBox CreateWrapper(object owner, RenderElement renderElement, BoxSpec spec, bool isInline)
        {
            if (isInline)
            { 
                return new WrapperInlineCssBox(owner, spec, renderElement.Root, renderElement); 
            }
            else
            {
                return new WrapperBlockCssBox(owner, spec, renderElement);
            }
        }
    }

    public interface IBoxElement
    {
        void ChangeElementSize(int w, int h);
        int MinHeight { get; }
    }

}