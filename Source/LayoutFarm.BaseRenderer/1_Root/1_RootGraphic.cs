﻿// 2015,2014 ,Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;

namespace LayoutFarm
{
    public abstract partial class RootGraphic
    {

        public delegate void PaintToOutputDelegate();

        protected PaintToOutputDelegate paintToOutputHandler;
        CanvasPaintToOutputDelegate canvasPaintToOutput;

        int accumRectVer;
        Rectangle accumulateInvalidRect;
        bool hasAccumRect;

        public RootGraphic(int width, int heigth)
        {
            this.Width = width;
            this.Height = heigth;
        }
    
        public abstract GraphicsPlatform P { get; }

        public IFonts SampleIFonts { get { return this.P.SampleIFonts; } }

        public abstract void CaretStartBlink();
        public abstract void CaretStopBlink();
        public abstract void ClearRenderRequests();

        public abstract void AddToLayoutQueue(RenderElement renderElement);


        internal int Width
        {
            get;
            set;
        }
        internal int Height
        {
            get;
            set;
        }
        public bool LayoutQueueClearing
        {
            get;
            set;
        }

        public bool IsInRenderPhase
        {
            get;
            set;
        }
        public abstract void CloseWinRoot();


        public abstract void ForcePaint();


        public void SetPaintToOutputHandler(PaintToOutputDelegate paintToOutputHandler)
        {
            this.paintToOutputHandler = paintToOutputHandler;
        }

        public abstract GraphicsTimerTask SubSccribeGraphicsIntervalTask(
            object uniqueName,
            TaskIntervalPlan planName,
            int intervalMs,
            EventHandler<GraphicsTimerTaskEventArgs> tickhandler);

        public abstract void RemoveIntervalTask(object uniqueName);


#if DEBUG

        bool dbugNeedContentArrangement
        {
            get;
            set;
        }
        bool dbugNeedReCalculateContentSize
        {
            get;
            set;
        }
#endif
        public abstract void PrepareRender();


        public void FlushAccumGraphics()
        {
            if (!this.hasAccumRect)
            {   
            }
            this.canvasPaintToOutput(accumulateInvalidRect);
            this.accumRectVer = 0;
            hasAccumRect = false;
        }
        public void SetCanvasPaintToOutputDel(CanvasPaintToOutputDelegate canvasPaintToOutput)
        {
            this.canvasPaintToOutput = canvasPaintToOutput;
        }

#if DEBUG
        void dbugWriteStopGfxBubbleUp(RenderElement fromElement, ref int dbug_ncount, int nleftOnStack, string state_str)
        {
            RootGraphic dbugMyroot = this;
            if (dbugMyroot.dbugEnableGraphicInvalidateTrace && dbugMyroot.dbugGraphicInvalidateTracer != null)
            {
                if (this.dbugNeedContentArrangement || this.dbugNeedReCalculateContentSize)
                {
                    state_str = "!!" + state_str;
                }
                dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo(state_str, fromElement);
                while (dbug_ncount > nleftOnStack)
                {
                    dbugMyroot.dbugGraphicInvalidateTracer.PopElement();
                    dbug_ncount--;
                }
            }
        }
#endif



        public void AddToInvalidateGraphicQueue(RenderElement fromElement, Rectangle totalBounds)
        {
            //total bounds = total bounds at level

            if (this.IsInRenderPhase) { return; }
            //--------------------------------------            
            //bubble up ,find global offset of 'fromElement' 
            //and then merge to accumulate rect
            int globalX = 0;
            int globalY = 0;
            bool passFirstRound = false;

            // start with parent of fromElement *** 
            // unlike InvalidateGraphicArea()
            if (!fromElement.Visible)
            {
                return;
            }
            fromElement = fromElement.ParentRenderElement;
            //--------------------------------------- 
            if (fromElement == null)
            {
                return;
            }
            //--------------------------------------- 
#if DEBUG
            int dbug_ncount = 0;
            dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, dbug_ncount, ">> :" + totalBounds.ToString());
#endif
            do
            {

                if (!fromElement.Visible)
                {
#if DEBUG
                    dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "EARLY-RET: ");
#endif
                    return;
                }
                else if (fromElement.BlockGraphicUpdateBubble)
                {
#if DEBUG
                    dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "BLOCKED2: ");
#endif
                    return;
                }
                //---------------------------------------------------------------------  

#if DEBUG
                dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, dbug_ncount, ">> ");
#endif


                globalX += fromElement.BubbleUpX;
                globalY += fromElement.BubbleUpY;


                if (fromElement.MayHasViewport && passFirstRound)
                {
                    totalBounds.Offset(globalX, globalY);
                    if (fromElement.HasDoubleScrollableSurface)
                    {
                        //container.VisualScrollableSurface.WindowRootNotifyInvalidArea(elementClientRect);
                    }
                    Rectangle elementRect = fromElement.RectBounds;
                    elementRect.Offset(fromElement.ViewportX, fromElement.ViewportY);
                    totalBounds.Intersect(elementRect);
                    globalX = -fromElement.ViewportX;
                    globalY = -fromElement.ViewportY;
                }

                if (fromElement.IsTopWindow)
                {
                    break;
                }
                else
                {
#if DEBUG
                    if (fromElement.dbugParentVisualElement == null)
                    {
                        dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "BLOCKED3: ");
                    }
#endif

                    fromElement = fromElement.ParentRenderElement;
                    if (fromElement == null)
                    {
                        return;
                    }
                }

                passFirstRound = true;

            } while (true);

#if DEBUG
            var dbugMyroot = this;
            if (dbugMyroot.dbugEnableGraphicInvalidateTrace
             && dbugMyroot.dbugGraphicInvalidateTracer != null)
            {
                while (dbug_ncount > 0)
                {
                    dbugMyroot.dbugGraphicInvalidateTracer.PopElement();
                    dbug_ncount--;
                }
            }
#endif

            //----------------------------------------
            totalBounds.Offset(globalX, globalY);
            Rectangle rootGlobalArea = totalBounds;


            if (totalBounds.Top > this.Height
                || totalBounds.Left > this.Width
                || totalBounds.Bottom < 0
                || totalBounds.Right < 0)
            {
#if DEBUG
                if (dbugMyroot.dbugEnableGraphicInvalidateTrace &&
                    dbugMyroot.dbugGraphicInvalidateTracer != null)
                {
                    dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("ZERO-EEX");
                    dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("\r\n");
                }
#endif
                return;
            }
            //--------------------------------------------------------------------------------------------------
            if (!hasAccumRect)
            {
                accumulateInvalidRect = rootGlobalArea;
                hasAccumRect = true;
            }
            else
            {
                accumulateInvalidRect = Rectangle.Union(accumulateInvalidRect, rootGlobalArea);
            }
            //----------------------
            accumRectVer++;
            //----------------------
#if DEBUG
            if (dbugMyroot.dbugEnableGraphicInvalidateTrace &&
                dbugMyroot.dbugGraphicInvalidateTracer != null)
            {
                string state_str = "ACC: ";
                if (this.dbugNeedContentArrangement || this.dbugNeedReCalculateContentSize)
                {
                    state_str = "!!" + state_str;
                }
                dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("ACC: " + accumulateInvalidRect.ToString());
                dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("\r\n");
            }
#endif


        }
        public void InvalidateGraphicArea(RenderElement fromElement, ref Rectangle elemClientRect)
        {
            //total bounds = total bounds at level

            if (this.IsInRenderPhase) { return; }
            //--------------------------------------            
            //bubble up ,find global rect coord
            //and then merge to accumulate rect
            int globalX = 0;
            int globalY = 0;
            bool isBubbleUp = false;

#if DEBUG
            int dbug_ncount = 0;
            dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, dbug_ncount, ">> :" + elemClientRect.ToString());
#endif
            do
            {

                if (!fromElement.Visible)
                {
#if DEBUG
                    dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "EARLY-RET: ");
#endif
                    return;
                }
                else if (fromElement.BlockGraphicUpdateBubble)
                {
#if DEBUG
                    dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "BLOCKED2: ");
#endif
                    return;
                }
                //--------------------------------------------------------------------- 


#if DEBUG
                dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, dbug_ncount, ">> ");
#endif


                globalX += fromElement.BubbleUpX;
                globalY += fromElement.BubbleUpY;


                if (fromElement.MayHasViewport && isBubbleUp)
                {
                    elemClientRect.Offset(globalX, globalY);
                    if (fromElement.HasDoubleScrollableSurface)
                    {
                        //container.VisualScrollableSurface.WindowRootNotifyInvalidArea(elementClientRect);
                    }
                    Rectangle elementRect = fromElement.RectBounds;
                    elementRect.Offset(fromElement.ViewportX, fromElement.ViewportY);
                    elemClientRect.Intersect(elementRect);
                    globalX = -fromElement.ViewportX;
                    globalY = -fromElement.ViewportY;
                }

                if (fromElement.IsTopWindow)
                {

                    break;
                }
                else
                {

#if DEBUG
                    if (fromElement.dbugParentVisualElement == null)
                    {
                        dbugWriteStopGfxBubbleUp(fromElement, ref dbug_ncount, 0, "BLOCKED3: ");
                    }
#endif

                    fromElement = fromElement.ParentRenderElement;
                    if (fromElement == null)
                    {
                        return;
                    }
                }

                isBubbleUp = true;

            } while (true);

#if DEBUG
            var dbugMyroot = this;
            if (dbugMyroot.dbugEnableGraphicInvalidateTrace
             && dbugMyroot.dbugGraphicInvalidateTracer != null)
            {
                while (dbug_ncount > 0)
                {
                    dbugMyroot.dbugGraphicInvalidateTracer.PopElement();
                    dbug_ncount--;
                }
            }
#endif

            //----------------------------------------
            elemClientRect.Offset(globalX, globalY);



            if (elemClientRect.Top > this.Height
                || elemClientRect.Left > this.Width
                || elemClientRect.Bottom < 0
                || elemClientRect.Right < 0)
            {
#if DEBUG
                if (dbugMyroot.dbugEnableGraphicInvalidateTrace &&
                    dbugMyroot.dbugGraphicInvalidateTracer != null)
                {
                    dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("ZERO-EEX");
                    dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("\r\n");
                }
#endif
                return;
            }
            //--------------------------------------------------------------------------------------------------
            if (!hasAccumRect)
            {
                accumulateInvalidRect = elemClientRect;
                hasAccumRect = true;
            }
            else
            {
                accumulateInvalidRect = Rectangle.Union(accumulateInvalidRect, elemClientRect);
            }
            //----------------------
            accumRectVer++;
            //----------------------
#if DEBUG
            if (dbugMyroot.dbugEnableGraphicInvalidateTrace &&
                dbugMyroot.dbugGraphicInvalidateTracer != null)
            {
                string state_str = "ACC: ";
                if (this.dbugNeedContentArrangement || this.dbugNeedReCalculateContentSize)
                {
                    state_str = "!!" + state_str;
                }
                dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("ACC: " + accumulateInvalidRect.ToString());
                dbugMyroot.dbugGraphicInvalidateTracer.WriteInfo("\r\n");
            }
#endif

        }

        public abstract TopWindowRenderBox TopWindowRenderBox
        {
            get;
            protected set;
        }

    }
}