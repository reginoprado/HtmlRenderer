//BSD 2014, WinterDev
//ArthurHub

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;

namespace HtmlRenderer.Dom
{
    //--------------------------
    class PartialBoxStrip
    {
        internal readonly CssBox owner;
        float _x;
        float _y;
        float _width;
        float _height;

        public PartialBoxStrip(CssBox owner, float x, float y, float w, float h)
        {
            this.owner = owner;
            this._x = x;
            this._y = y;
            this._width = w;
            this._height = h;
        }

        public float Left
        {
            get { return this._x; }
        }
        public float Top
        {
            get { return this._y; }
        }
        public float Width
        {
            get { return this._width; }
        }
        public float Right
        {
            get { return this._x + this._width; }
        }
        public float Height
        {
            get { return this._height; }
        }

        public float Bottom
        {
            get { return this._y + _height; }
        }

        public RectangleF Bound
        {
            get { return new RectangleF(this._x, this._y, this.Width, this.Height); }
        }
        public void Offset(float xdiff, float ydiff)
        {
            this._x += xdiff;
            this._y += ydiff;
        }
        public void MergeBound(float left, float top, float right, float bottom)
        {

            float sR = this.Right;
            float sB = this.Bottom;

            if (left < this._x)
            {
                this._x = left;
            }
            if (top < this._y)
            {
                this._y = top;
            }
            if (right > sR)
            {
                sR = right;
            }
            if (bottom > sB)
            {
                sB = bottom;
            }

            this._width = sR - this._x;
            this._height = sB - this._y;
        }
        public void SetTop(float y)
        {
            this._y = y;
        }
        public void SetLeft(float x)
        {
            this._x = x;
        }

#if DEBUG
        public override string ToString()
        {
            return this.owner.__aa_dbugId + " left:" + this.Left + ",width:" + this.Width + " " + this.owner.ToString();
        }
#endif
    }



    /// <summary>
    /// Represents a line of text.
    /// </summary>
    /// <remarks>
    /// To learn more about line-boxes see CSS spec:
    /// http://www.w3.org/TR/CSS21/visuren.html
    /// </remarks>
    sealed class CssLineBox
    {
        readonly CssBox _ownerBox;

        //a run may come from another CssBox (not from _ownerBox)
        readonly List<CssRun> _runs = new List<CssRun>();

        //linebox and PartialBoxStrip is 1:1 relation 
        //a CssBox (Inline-splittable) may be splitted into many CssLineBoxes

        /// <summary>
        /// handle part of cssBox in this line, handle task about bg/border/bounday of cssBox owner of strip        
        /// </summary>
        readonly List<PartialBoxStrip> _bottomUpBoxStrips = new List<PartialBoxStrip>();
        internal LinkedListNode<CssLineBox> linkedNode;

        float _cacheContentWidth;
#if DEBUG
        bool dbugIsClosed;
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif



        /// <summary>
        /// Creates a new LineBox
        /// </summary>
        public CssLineBox(CssBox ownerBox)
        {
            _ownerBox = ownerBox;
        }
        internal CssLineBox NextLine
        {
            get
            {
                var nn = this.linkedNode.Next;
                if (nn == null)
                {
                    return null;
                }
                else
                {
                    return nn.Value;
                }
            }
        }
        internal float CachedLineBottom
        {
            get { return this.CachedLineTop + this.CacheLineHeight; }
        }
        internal float CacheLineHeight
        {
            get;
            private set;
        }
        internal float CachedLineTop
        {
            get;
            set;
        }
        internal float OwnerLeft
        {
            get { return 0; }
        }
        internal float CachedLineContentWidth
        {
            get { return this._cacheContentWidth; }
            set
            {
                if (value < 0)
                {

                }
                this._cacheContentWidth = value;
            }

        }
        internal void CloseLine()
        {
#if DEBUG
            this.dbugIsClosed = true;

#endif

            //=============================================================
            //part 1: MakeStrips()
            //=============================================================
            //***
            var myruns = this._runs;
            CssBox lineOwner = this._ownerBox;
            int j = myruns.Count;

            List<PartialBoxStrip> totalStrips = this._bottomUpBoxStrips;
            //---------------------------------------------------------------------------
            //first level
            Dictionary<CssBox, PartialBoxStrip> dicStrips = new Dictionary<CssBox, PartialBoxStrip>();
            //location of run and strip related to its containng block
            float maxRight = 0;
            float maxBottom = 0;
            for (int i = 0; i < j; ++i)
            {
                var run = myruns[i];
                maxRight = run.Right > maxRight ? run.Right : maxRight;
                maxBottom = run.Bottom > maxBottom ? run.Bottom : maxBottom;
                //strip size include whitespace 
                if (run.IsSpaces)
                {
                    continue;
                }
                //-------------
                //first level data
                RegisterStripPart(run.OwnerBox, run.Left, run.Top, run.Right, run.Bottom, totalStrips, dicStrips);
            }
            //---------------------------------------------------------------------------
            //other step to upper layer, until no new strip     
            int newStripIndex = 0;
            for (int numNewStripCreate = totalStrips.Count; numNewStripCreate > 0; newStripIndex += numNewStripCreate)
            {
                numNewStripCreate = StepUpRegisterStrips(dicStrips, lineOwner, totalStrips, newStripIndex);
            }
            //=============================================================
            //part 2: CalculateCacheData()
            //=============================================================

            this.CacheLineHeight = maxBottom;
            this.CachedLineContentWidth = maxRight;


            if (lineOwner.SizeWidth < CachedLineContentWidth)
            {
                this.CachedLineContentWidth = this.OwnerBox.SizeWidth;
            }
        }


        internal void OffsetTop(float ydiff)
        {

            this.CachedLineTop += ydiff;

            if (this.OwnerBox.SizeWidth < CachedLineContentWidth)
            {
                this.CachedLineContentWidth = this.OwnerBox.SizeWidth;
            }
        }
        public bool HitTest(float x, float y)
        {
            if (y >= this.CachedLineTop && y <= this.CachedLineBottom)
            {
                return true;
            }
            return false;
        }

        public float CalculateTotalBoxBaseLine()
        {
            float baseline = Single.MinValue;
            for (int i = _bottomUpBoxStrips.Count - 1; i >= 0; --i)
            {
                baseline = Math.Max(baseline, _bottomUpBoxStrips[i].Top);//?top 
            }

            return baseline;
        }
        public void ApplyBaseline(float baseline)
        {
            //Important notes on http://www.w3.org/TR/CSS21/tables.html#height-layout
            //iterate from rectstrip
            //In a single LineBox ,  CssBox:RectStrip => 1:1 relation             
            for (int i = _bottomUpBoxStrips.Count - 1; i >= 0; --i)
            {
                var rstrip = _bottomUpBoxStrips[i];
                var rstripOwnerBox = rstrip.owner;
                switch (rstripOwnerBox.VerticalAlign)
                {
                    case CssVerticalAlign.Sub:
                        {
                            this.SetBaseLine(rstripOwnerBox, baseline + rstrip.Height * .2f);
                        } break;
                    case CssVerticalAlign.Super:
                        {
                            this.SetBaseLine(rstripOwnerBox, baseline - rstrip.Height * .2f);
                        } break;
                    case CssVerticalAlign.TextTop:
                    case CssVerticalAlign.TextBottom:
                    case CssVerticalAlign.Top:
                    case CssVerticalAlign.Bottom:
                    case CssVerticalAlign.Middle:
                        break;
                    default:
                        //case: baseline
                        this.SetBaseLine(rstripOwnerBox, baseline);
                        break;
                }
            }
        }
        public IEnumerable<float> GetAreaStripTopPosIter()
        {
            for (int i = _bottomUpBoxStrips.Count - 1; i >= 0; --i)
            {
                yield return _bottomUpBoxStrips[i].Top;
            }
        }
        internal int WordCount
        {
            get
            {
                return this._runs.Count;
            }
        }
        internal CssRun GetRun(int index)
        {
            return this._runs[index];
        }
        internal CssRun GetFirstRun()
        {
            return this._runs[0];
        }
        internal CssRun GetLastRun()
        {
            return this._runs[this._runs.Count - 1];
        }


        /// <summary>
        /// Gets the owner box
        /// </summary>
        public CssBox OwnerBox
        {
            get { return _ownerBox; }
        }
        /// <summary>
        /// Lets the linebox add the word an its box to their lists if necessary.
        /// </summary>
        /// <param name="run"></param>
        internal void AddRun(CssRun run)
        {
#if DEBUG
            if (this.dbugIsClosed)
            {
                throw new NotSupportedException();
            }
#endif
            //if (run is CssTextRun)
            //{ 
            //    CssTextRun textRun = (CssTextRun)run;
            //    if (textRun.Text.Contains("Cell"))
            //    {

            //    }
            //}
            this._runs.Add(run);//each word has only one owner linebox! 
            CssRun.SetHostLine(run, this);
        }
        internal IEnumerable<CssRun> GetRunIter(CssBox box)
        {
            List<CssRun> tmpRuns = this._runs;
            int j = tmpRuns.Count;
            CssRun run = null;
            for (int i = 0; i < j; ++i)
            {
                if ((run = tmpRuns[i]).OwnerBox == box)
                {
                    yield return run;
                }
            }
        }
        internal IEnumerable<CssRun> GetRunIter()
        {
            List<CssRun> tmpRuns = this._runs;
            int j = tmpRuns.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return tmpRuns[i];
            }
        }



        internal void PaintRuns(IGraphics g, PaintVisitor p)
        {
            //iterate from each words


            CssBox latestOwner = null;
            Font font = null;

            Color color = Color.Empty;
            var tmpRuns = this._runs;
            int j = tmpRuns.Count;

            for (int i = 0; i < j; ++i)
            {
                //-----------------
                dbugCounter.dbugRunPaintCount++;
                //-----------------

                CssRun w = tmpRuns[i];
                switch (w.Kind)
                {
                    case CssRunKind.Image:
                        {
                            CssBoxImage owner = (CssBoxImage)w.OwnerBox;
                            owner.PaintImage(g, new RectangleF(w.Left, w.Top, w.Width, w.Height), p);

                        } break;
                    case CssRunKind.BlockRun:
                        {
                            //Console.WriteLine("blockrun");

                            CssBlockRun blockRun = (CssBlockRun)w;
                            float ox = g.CanvasOriginX;
                            float oy = g.CanvasOriginY;

                            g.SetCanvasOrigin(ox + blockRun.Left, oy + blockRun.Top);

                            blockRun.BlockBox.Paint(g, p);

                            g.SetCanvasOrigin(ox, oy);

                        } break;
                    case CssRunKind.Text:
                        {
                            if (latestOwner != w.OwnerBox)
                            {
                                latestOwner = w.OwnerBox;
                                font = latestOwner.ActualFont;
                                color = latestOwner.ActualColor;
                            }
                            CssTextRun textRun = (CssTextRun)w;

                            var wordPoint = new PointF(w.Left, w.Top);

                            char[] ownerBuffer = CssBox.UnsafeGetTextBuffer(w.OwnerBox);

                            g.DrawString2(ownerBuffer,
                               textRun.TextStartIndex,
                               textRun.TextLength, font,
                               color, wordPoint,
                               new SizeF(w.Width, w.Height));

                        } break;
                    default:
                        {
#if DEBUG
                            // w.OwnerBox.dbugPaintTextWordArea(g, offset, w);
#endif
                        } break;
                }
            }
        }

#if DEBUG

        internal void dbugPaintRuns(IGraphics g, PaintVisitor p)
        {


            return;
            //linebox  
            float x1 = 0;
            float y1 = 0;
            float x2 = x1 + this.CachedLineContentWidth;
            float y2 = y1 + this.CacheLineHeight;
            //draw diagonal  
            p.dbugDrawDiagonalBox(Pens.Blue, x1, y1, x2, y2);
            //g.DrawRectangle(Pens.Blue,
            //    this.OwnerBox.LocationX,
            //    this.CachedLineTop,
            //    this.CachedLineContentWidth,
            //    this.CacheLineHeight);
            //foreach (var strip in this._boxStrips.Values)
            //{
            //    var bound = strip.Bound;
            //    bound.Offset(offset);
            //    dbugDrawDiagnalBox(g, Pens.Green, bound.X, bound.Y, bound.Right, bound.Bottom);
            //}

            //return;
            foreach (CssRun w in this._runs)
            {
                g.DrawRectangle(Pens.DeepPink, w.Left, w.Top, w.Width, w.Height);
            }

            g.FillRectangle(Brushes.Red, 0, 0, 5, 5);

        }

#endif

        internal int LineSelectionStart
        {
            get;
            set;
        }
        internal int LineSelectionWidth
        {
            get;
            set;
        }
        internal void PaintSelection(PaintVisitor p)
        {
            var gfx = p.Gfx;
            gfx.FillRectangle(Brushes.LightGray, this.LineSelectionStart, 0, this.LineSelectionWidth, this.CacheLineHeight);
        }


        internal void PaintBackgroundAndBorder(PaintVisitor p)
        {
            //iterate each strip

            for (int i = _bottomUpBoxStrips.Count - 1; i >= 0; --i)
            {
                var strip = _bottomUpBoxStrips[i];
                var stripOwner = strip.owner;    
                if (!stripOwner.HasVisibleBgColor)
                {
                    continue;
                }

                //-----------------------------------------------------------------
                var stripArea = strip.Bound;
                bool isFirstLine, isLastLine;
                CssBox.GetSplitInfo(stripOwner, this, out isFirstLine, out isLastLine);
                stripOwner.PaintBackground(p, stripArea, isFirstLine, isLastLine);
                
                if (stripOwner.CssDisplay != CssDisplay.TableCell
                    && stripOwner.HasSomeVisibleBorder)
                {
                    p.PaintBorders(stripOwner, stripArea, isFirstLine, isLastLine);
                }

            }
        }

        internal void PaintDecoration(IGraphics g, PaintVisitor p)
        {

            for (int i = _bottomUpBoxStrips.Count - 1; i >= 0; --i)
            {
                var strip = _bottomUpBoxStrips[i];
                CssBox ownerBox = strip.owner;
                bool isFirstLine, isLastLine;
                CssBox.GetSplitInfo(ownerBox, this, out isFirstLine, out isLastLine);
                ownerBox.PaintDecoration(g, strip.Bound, isFirstLine, isLastLine);
            }
        }

        /// <summary>
        /// Gets the baseline Height of the rectangle
        /// </summary>
        /// <param name="b"> </param>
        /// <param name="g"></param>
        /// <returns></returns>

        void SetBaseLine(CssBox stripOwnerBox, float baseline)
        {
            float newtop = baseline;
            foreach (var word in this.GetRunIter(stripOwnerBox))
            {
                if (!word.IsImage)
                {
                    word.Top = newtop;
                }
            }
        }

        /// <summary>
        /// Returns the words of the linebox
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            int j = _runs.Count;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < j; i++)
            {
                sb.Append(_runs[i].Text);
            }
            return sb.ToString();
        }

        static int StepUpRegisterStrips(Dictionary<CssBox, PartialBoxStrip> dicStrips,
            CssBox lineOwnerBox,
            List<PartialBoxStrip> inputList, int startInputAt)
        {

            int j = inputList.Count;
            for (int i = startInputAt; i < j; ++i)
            {
                //step up
                var strip = inputList[i];
                var upperBox = strip.owner.ParentBox;
                if (upperBox != null && upperBox != lineOwnerBox && upperBox.IsInline)
                {
                    RegisterStripPart(upperBox, strip.Left, strip.Top, strip.Right, strip.Bottom, inputList, dicStrips);
                }
            }
            return inputList.Count - j;

        }
        static void RegisterStripPart(CssBox runOwner,
            float left, float top, float right, float bottom,
            List<PartialBoxStrip> newStrips, Dictionary<CssBox, PartialBoxStrip> dic)
        {
            PartialBoxStrip strip;
            if (!dic.TryGetValue(runOwner, out strip))
            {
                strip = new PartialBoxStrip(runOwner, left, top, right - left, bottom - top);
                dic.Add(runOwner, strip);
                newStrips.Add(strip);
            }
            else
            {
                strip.MergeBound(left, top, right, bottom);
            }
        }
        //---------------------------------
        internal bool IsFirstLine
        {
            get { return this.linkedNode.Previous == null; }
        }
        internal bool IsLastLine
        {
            get { return this.linkedNode.Next == null; }
        }
        //---------------------------------
    }
}
