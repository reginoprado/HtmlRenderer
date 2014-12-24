﻿//2014 Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;

using LayoutFarm.UI;

namespace LayoutFarm
{

    public class SampleViewport
    {
        LayoutFarm.UI.WinForms.UISurfaceViewportControl vw;
        public SampleViewport(LayoutFarm.UI.WinForms.UISurfaceViewportControl vw)
        {
            this.vw = vw;
            
        }
        public void AddContent(UIElement ui)
        {
             
            this.vw.AddContent(ui.GetPrimaryRenderElement(vw.WinTopRootGfx));
        }
    }
    public abstract class DemoBase
    {
        public void StartDemo(SampleViewport viewport)
        {
            OnStartDemo(viewport);
        }
        protected virtual void OnStartDemo(SampleViewport viewport)
        {
        }
        public virtual string Desciption
        {
            get { return ""; }
        }
    }
    public class DemoNoteAttribute : Attribute
    {
        public DemoNoteAttribute(string msg)
        {
            this.Message = msg;
        }
        public string Message { get; set; }
    }
    class DemoInfo
    {
        public readonly Type DemoType;
        public readonly string DemoNote;
        public DemoInfo(Type demoType, string demoNote)
        {
            this.DemoType = demoType;
            this.DemoNote = demoNote;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(DemoNote))
            {
                return this.DemoType.Name;
            }
            else
            {
                return this.DemoNote + " : " + this.DemoType.Name;
            }
        }
    }

}