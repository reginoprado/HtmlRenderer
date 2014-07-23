﻿//MS-PL, Apache2 
//2014, WinterDev

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using HtmlRenderer.Css;

namespace HtmlRenderer.SvgDom
{

    public abstract class SvgNode
    {

    }


    public abstract class SvgElement : SvgNode
    {
        LinkedListNode<SvgElement> linkedNode = null;
        LinkedList<SvgElement> children;
        SvgElement parent;
        public SvgElement()
        {

        }
        public SvgElement Parent
        {
            get
            {
                return parent;
            }
        }
        public void AddChild(SvgElement child)
        {
            if (this.children == null)
            {
                this.children = new LinkedList<SvgElement>();
            }
            child.linkedNode = this.children.AddLast(child);
            child.parent = this;
        }
        public int Count
        {
            get
            {
                if (this.children == null)
                {
                    return 0;
                }
                else
                {
                    return this.children.Count;
                }
            }
        }

        internal LinkedListNode<SvgElement> GetFirstNode()
        {
            return this.children.First;
        }
        public virtual void ReEvaluateComputeValue(float containerW, float containerH, float emHeight)
        {
        }

        /// <summary>
        /// get length in pixel
        /// </summary>
        /// <param name="length"></param>
        /// <param name="hundredPercent"></param>
        /// <param name="box"></param>
        /// <returns></returns>
        public static float ConvertToPx(CssLength length, float hundredPercent, float emHeight)
        {
            //Return zero if no length specified, zero specified      
            switch (length.UnitOrNames)
            {
                case CssUnitOrNames.EmptyValue:
                    return 0;
                case CssUnitOrNames.Percent:
                    return (length.Number / 100f) * hundredPercent;
                case CssUnitOrNames.Ems:
                    return length.Number * emHeight;
                case CssUnitOrNames.Ex:
                    return length.Number * (emHeight / 2);
                case CssUnitOrNames.Pixels:
                    //atodo: check support for hi dpi
                    return length.Number;
                case CssUnitOrNames.Milimeters:
                    return length.Number * 3.779527559f; //3 pixels per millimeter      
                case CssUnitOrNames.Centimeters:
                    return length.Number * 37.795275591f; //37 pixels per centimeter 
                case CssUnitOrNames.Inches:
                    return length.Number * 96f; //96 pixels per inch 
                case CssUnitOrNames.Points:
                    return length.Number * (96f / 72f); // 1 point = 1/72 of inch   
                case CssUnitOrNames.Picas:
                    return length.Number * 16f; // 1 pica = 12 points 
                default:
                    return 0;
            }
        }


        public virtual void Paint(HtmlRenderer.Drawing.IGraphics g)
        {

        }
    }

    public class SvgRect : SvgElement
    {
        Color strokeColor = Color.Transparent;
        Color fillColor = Color.Black;
        //test path
        GraphicsPath _path;

        public SvgRect()
        {
        }
        public CssLength X
        {
            get;
            set;
        }
        public CssLength Y
        {
            get;
            set;
        }
        public CssLength Width
        {
            get;
            set;
        }
        public CssLength Height
        {
            get;
            set;
        }

        public CssLength CornerRadiusX
        {
            get;
            set;
        }
        public CssLength CornerRadiusY
        {
            get;
            set;
        }

        public Color ActualColor
        {
            get { return this.fillColor; }
            set { this.fillColor = value; }
        }
        public Color StrokeColor
        {
            get { return this.strokeColor; }
            set { this.strokeColor = value; }
        }
        public CssLength StrokeWidth
        {
            get;
            set;
        }
        //----------------------------
        public float ActualX
        {
            get;
            set;
        }
        public float ActualY
        {
            get;
            set;
        }
        public float ActualWidth
        {
            get;
            set;
        }
        public float ActualHeight
        {
            get;
            set;
        }
        public float ActualStrokeWidth
        {
            get;
            set;
        }
        public float ActualCornerRx
        {
            get;
            set;
        }
        public float ActualCornerRy
        {
            get;
            set;
        }
        //----------------------------
        public override void ReEvaluateComputeValue(float containerW, float containerH, float emHeight)
        {
            this.ActualX = ConvertToPx(this.X, containerW, emHeight);
            this.ActualY = ConvertToPx(this.Y, containerW, emHeight);
            this.ActualWidth = ConvertToPx(this.Width, containerW, emHeight);
            this.ActualHeight = ConvertToPx(this.Height, containerW, emHeight);
            this.ActualStrokeWidth = ConvertToPx(this.StrokeWidth, containerW, emHeight);

            this.ActualCornerRx = ConvertToPx(this.CornerRadiusX, containerW, emHeight);
            this.ActualCornerRy = ConvertToPx(this.CornerRadiusY, containerW, emHeight);

            //update graphic path
            if (this.ActualCornerRx == 0 && this.ActualCornerRy == 0)
            {
                this._path = CreateRectGraphicPath(
                    this.ActualX,
                    this.ActualY,
                    this.ActualWidth,
                    this.ActualHeight);
            }
            else
            {
                this._path = CreateRoundRectGraphicPath(
                    this.ActualX,
                    this.ActualY,
                    this.ActualWidth,
                    this.ActualHeight,
                    this.ActualCornerRx,
                    this.ActualCornerRy);
            }

        }
        static GraphicsPath CreateRectGraphicPath(float x, float y, float w, float h)
        {
            var _path = new GraphicsPath();
            _path.StartFigure();
            _path.AddRectangle(new RectangleF(x, y, w, h));
            _path.CloseFigure();
            return _path;
        }
        static GraphicsPath CreateRoundRectGraphicPath(float x, float y, float w, float h, float c_rx, float c_ry)
        {
            var _path = new GraphicsPath();
            var arcBounds = new RectangleF();
            var lineStart = new PointF();
            var lineEnd = new PointF();
            var width = w;
            var height = h;
            var rx = c_rx * 2;
            var ry = c_ry * 2;

            // Start
            _path.StartFigure();

            // Add first arc
            arcBounds.Location = new PointF(x, y);
            arcBounds.Width = rx;
            arcBounds.Height = ry;
            _path.AddArc(arcBounds, 180, 90);

            // Add first line
            lineStart.X = Math.Min(x + rx, x + width * 0.5f);
            lineStart.Y = y;
            lineEnd.X = Math.Max(x + width - rx, x + width * 0.5f);
            lineEnd.Y = lineStart.Y;
            _path.AddLine(lineStart, lineEnd);

            // Add second arc
            arcBounds.Location = new PointF(x + width - rx, y);
            _path.AddArc(arcBounds, 270, 90);

            // Add second line
            lineStart.X = x + width;
            lineStart.Y = Math.Min(y + ry, y + height * 0.5f);
            lineEnd.X = lineStart.X;
            lineEnd.Y = Math.Max(y + height - ry, y + height * 0.5f);
            _path.AddLine(lineStart, lineEnd);

            // Add third arc
            arcBounds.Location = new PointF(x + width - rx, y + height - ry);
            _path.AddArc(arcBounds, 0, 90);

            // Add third line
            lineStart.X = Math.Max(x + width - rx, x + width * 0.5f);
            lineStart.Y = y + height;
            lineEnd.X = Math.Min(x + rx, x + width * 0.5f);
            lineEnd.Y = lineStart.Y;
            _path.AddLine(lineStart, lineEnd);

            // Add third arc
            arcBounds.Location = new PointF(x, y + height - ry);
            _path.AddArc(arcBounds, 90, 90);

            // Add fourth line
            lineStart.X = x;
            lineStart.Y = Math.Max(y + height - ry, y + height * 0.5f);
            lineEnd.X = lineStart.X;
            lineEnd.Y = Math.Min(y + ry, y + height * 0.5f);
            _path.AddLine(lineStart, lineEnd);

            // Close
            _path.CloseFigure();

            return _path;
        }

        public override void Paint(Drawing.IGraphics g)
        {

            using (SolidBrush sb = new SolidBrush(this.ActualColor))
            {
                g.FillPath(sb, this._path);

            }
            if (this.strokeColor != Color.Transparent
                && this.ActualStrokeWidth > 0)
            {
                using (SolidBrush sb = new SolidBrush(this.strokeColor))
                using (Pen pen = new Pen(sb))
                {
                    pen.Width = this.ActualStrokeWidth;
                    g.DrawPath(pen, this._path);
                }
            }

        }
    }

    public class SvgFragment : SvgElement
    {

    }



}