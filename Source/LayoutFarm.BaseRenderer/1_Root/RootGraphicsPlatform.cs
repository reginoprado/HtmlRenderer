﻿//Apache2, 2014-2016, WinterDev

 
using PixelFarm.Drawing; 
namespace LayoutFarm
{
    public static class RootGfxPlatform
    {
        static GraphicsPlatform s_selectedGfxPlatform;
        static object initLock = new object();

        static bool SetCurrentPlatform(GraphicsPlatform actualImpl)
        {
            //must init once
            lock (initLock)
            {
                if (s_selectedGfxPlatform == null)
                {
                    s_selectedGfxPlatform = actualImpl;
                    return true;
                }
            }
            return false;
        } 
    }
   

}