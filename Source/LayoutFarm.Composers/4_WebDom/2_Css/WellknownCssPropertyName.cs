﻿//BSD  2014 ,WinterDev 
using System; 
using System.Collections.Generic;
using System.Text; 

 
namespace HtmlRenderer.WebDom
{
    public enum WellknownCssPropertyName
    {
        Unknown,

        [Map("display")]
        Display,

        [Map("border")]
        Border,
        [Map("border-left")]
        BorderLeft,
        [Map("border-right")]
        BorderRight,
        [Map("border-bottom")]
        BorderBottom,
        [Map("border-top")]
        BorderTop,
        //----------------------------------
        [Map("border-width")]
        BorderWidth,
        [Map("border-bottom-width")]
        BorderBottomWidth,
        [Map("border-left-width")]
        BorderLeftWidth,
        [Map("border-top-width")]
        BorderTopWidth,
        [Map("border-right-width")]
        BorderRightWidth,
        //----------------------------------
        [Map("border-style")]
        BorderStyle,
        [Map("border-bottom-style")]
        BorderBottomStyle,
        [Map("border-left-style")]
        BorderLeftStyle,
        [Map("border-top-style")]
        BorderTopStyle,
        [Map("border-right-style")]
        BorderRightStyle,
        //----------------------------------
        [Map("border-color")]
        BorderColor,
        [Map("border-bottom-color")]
        BorderBottomColor,
        [Map("border-left-color")]
        BorderLeftColor,
        [Map("border-top-color")]
        BorderTopColor,
        [Map("border-right-color")]
        BorderRightColor,
        //----------------------------------
        [Map("border-spacing")]
        BorderSpacing,
        [Map("border-collapse")]
        BorderCollapse,
        //----------------------------------
        [Map("corner-radius")]
        CornerRadius,
        [Map("corner-nw-radius")]
        CornerNWRadius,
        [Map("corner-ne-radius")]
        CornerNERadius,
        [Map("corner-sw-radius")]
        CornerSWRadius,
        [Map("corner-se-radius")]
        CornerSERadius,
        //----------------------------------
        [Map("margin")]
        Margin,
        [Map("margin-bottom")]
        MarginBottom,
        [Map("margin-top")]
        MarginTop,
        [Map("margin-left")]
        MarginLeft,
        [Map("margin-right")]
        MarginRight,
        //----------------------------------
        [Map("padding")]
        Padding,
        [Map("padding-bottom")]
        PaddingBottom,
        [Map("padding-top")]
        PaddingTop,
        [Map("padding-left")]
        PaddingLeft,
        [Map("padding-right")]
        PaddingRight,
        //----------------------------------
        [Map("left")]
        Left,
        [Map("top")]
        Top,
        [Map("width")]
        Width,
        [Map("max-width")]
        MaxWidth,
        [Map("height")]
        Height,
        //----------------------------------
        [Map("background-color")]
        BackgroundColor,
        [Map("background-image")]
        BackgroundImage,
        [Map("background-position")]
        BackgroundPosition,
        [Map("background-gradient")]
        BackgroundGradient,
        [Map("background-gradient-angle")]
        BackgroundGradientAngle,
        [Map("background-angle")]
        BackgroundAngle,
        [Map("background-repeat")]
        BackgroundRepeat,
        //----------------------------------
        [Map("color")]
        Color,
        [Map("direction")]
        Direction,
        [Map("empty-cells")]
        EmptyCells,
        [Map("float")]
        Float,
        [Map("position")]
        Position,
        [Map("line-height")]
        LineHeight,
        [Map("vertical-align")]
        VerticalAlign,
        [Map("text-indent")]
        TextIndent,
        [Map("text-align")]
        TextAlign,
        [Map("text-decoration")]
        TextDecoration,

        [Map("white-space")]
        Whitespace,
        [Map("word-break")]
        WordBreak,
        [Map("word-spacing")]
        WordSpacing,
        [Map("visibility")]
        Visibility,
        [Map("font")]
        Font,
        [Map("font-family")]
        FontFamily,
        [Map("font-size")]
        FontSize,
        [Map("font-style")]
        FontStyle,
        [Map("font-variant")]
        FontVariant,
        [Map("font-weight")]
        FontWeight,

        [Map("list-style")]
        ListStyle,
        [Map("list-style-position")]
        ListStylePosition,
        [Map("list-style-image")]
        ListStyleImage,
        [Map("list-style-type")]
        ListStyleType,

        [Map("overflow")]
        Overflow,
        //----------------------------------
        [Map("content")]
        Content,
        //----------------------------------
        [Map("outline")]
        Outline,
        [Map("unicode-bidi")]
        UnicodeBidi,
        [Map("page-break-before")]
        PageBreakBefore,
        [Map("page-break-after")]
        PageBreakAfter,
        

        //----------------------------------
        [Map("cursor")]
        Cursor
    }

}