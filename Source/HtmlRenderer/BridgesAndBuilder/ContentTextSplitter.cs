﻿//BSD 2014,WinterDev 

using System;
using System.Collections.Generic;
using HtmlRenderer.Parse;
using HtmlRenderer.Utils;
using System.Text;

namespace HtmlRenderer.Dom
{

    class ContentTextSplitter
    {
        public static readonly ContentTextSplitter DefaultSplitter = new ContentTextSplitter();
        public ContentTextSplitter()
        {

        }
        enum WordParsingState
        {
            Init,
            Whitespace,
            CharacterCollecting
        }

        public List<CssRun> ParseWordContent(char[] textBuffer,
            CssWhiteSpace whitespace, bool notBreakAll, bool keepPreWhiteSpace)
        {

#if DEBUG
            //string dbugStr = new string(textBuffer); 
#endif
            List<CssRun> runList = null;
            switch (whitespace)
            {
                case CssWhiteSpace.Pre:
                case CssWhiteSpace.PreWrap:
                    runList = ParseWordContentPreserveWhitespace(textBuffer, notBreakAll);
                    break;
                case CssWhiteSpace.PreLine:
                    runList = ParseWordContentRespectNewLine(textBuffer, notBreakAll);
                    break;
                default:
                    runList = ParseWordContentDefault(textBuffer, notBreakAll, keepPreWhiteSpace);
                    break;
            }
            return runList;
        }

        List<CssRun> ParseWordContentPreserveWhitespace(char[] textBuffer, bool boxIsNotBreakAll)
        {


            //#if DEBUG
            //            string dbugStr = new string(textBuffer);
            //#endif
            List<CssRun> boxRuns = new List<CssRun>();// CssBox.UnsafeGetRunListOrCreateIfNotExists(box);

            int startIndex = 0;
            int buffLength = textBuffer.Length;

            //whitespace and respect newline  
            WordParsingState parsingState = WordParsingState.Init;
            int appendLength = 0;
            for (int i = 0; i < buffLength; ++i)
            {
                char c0 = textBuffer[i];
                if (c0 == '\n')
                {
                    //flush exising 
                    if (appendLength > 0)
                    {
                        if (parsingState == WordParsingState.CharacterCollecting)
                        {
                            boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));
                        }
                        else
                        {
                            boxRuns.Add(CssTextRun.CreateWhitespace(appendLength));
                        }
                        appendLength = 0; //clear append length 
                    }
                    //append with new line 
                    boxRuns.Add(CssTextRun.CreateLineBreak());
                    startIndex = i;
                    continue;
                }
                else if (c0 == '\r')
                {
                    //skip 
                    continue;
                }
                //----------------------------------------
                //switch by state  
                switch (parsingState)
                {
                    case WordParsingState.Init:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                //other whitespace
                                parsingState = WordParsingState.Whitespace;
                                startIndex = i;
                                appendLength = 1;//start collect whitespace
                            }
                            else
                            {
                                //character 
                                parsingState = WordParsingState.CharacterCollecting;
                                startIndex = i;
                                appendLength = 1;//start collect whitespace 
                            }

                        } break;
                    case WordParsingState.Whitespace:
                        {
                            if (!char.IsWhiteSpace(c0))
                            {
                                //switch to character mode
                                boxRuns.Add(CssTextRun.CreateWhitespace(appendLength));
                                parsingState = WordParsingState.CharacterCollecting;
                                startIndex = i;//start collect
                                appendLength = 1;//start append length
                            }
                            else
                            {
                                if (appendLength == 0)
                                {
                                    startIndex = i;
                                }
                                appendLength++;
                            }
                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                //flush collecting token
                                boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));

                                parsingState = WordParsingState.Whitespace;
                                startIndex = i;//start collect
                                appendLength = 1; //collect whitespace
                            }
                            else
                            {
                                if (appendLength == 0)
                                {
                                    startIndex = i;
                                }
                                appendLength++;
                            }
                        } break;
                }
            }

            //--------------------
            if (appendLength > 0)
            {
                switch (parsingState)
                {
                    case WordParsingState.Whitespace:
                        {
                            boxRuns.Add(CssTextRun.CreateWhitespace(appendLength));


                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));


                        } break;
                }
            }
            return boxRuns;
        }
        List<CssRun> ParseWordContentRespectNewLine(char[] textBuffer, bool boxIsNotBreakAll)
        {
            //not preserve whitespace but respect newline

            List<CssRun> boxRuns = new List<CssRun>();// CssBox.UnsafeGetRunListOrCreateIfNotExists(box);

            int startIndex = 0;
            int buffLength = textBuffer.Length;

            //whitespace and respect newline  
            WordParsingState parsingState = WordParsingState.Init;

            int appendLength = 0;

            for (int i = 0; i < buffLength; ++i)
            {
                char c0 = textBuffer[i];
                //----------------------------------------
                if (c0 == '\n')
                {
                    //flush exising 
                    if (appendLength > 0)
                    {
                        switch (parsingState)
                        {
                            case WordParsingState.Whitespace:
                                {
                                    if (boxRuns.Count > 0)
                                    {
                                        boxRuns.Add(CssTextRun.CreateSingleWhitespace());
                                    }
                                } break;
                            case WordParsingState.CharacterCollecting:
                                {
                                    boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));

                                } break;
                        }
                        appendLength = 0; //clear append length 
                    }
                    //append with new line
                    startIndex = i;
                    boxRuns.Add(CssTextRun.CreateLineBreak());

                    continue;
                }
                else if (c0 == '\r')
                {
                    //skip 
                    continue;
                }
                //=====================================================================
                //switch by state -- like normal
                switch (parsingState)
                {
                    case WordParsingState.Init:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                //not switch
                                parsingState = WordParsingState.Whitespace;
                            }
                            else
                            {
                                parsingState = WordParsingState.CharacterCollecting;
                                startIndex = i;
                                appendLength = 1;//start append length
                            }
                        } break;
                    case WordParsingState.Whitespace:
                        {
                            if (!char.IsWhiteSpace(c0))
                            {
                                //switch to character mode

                                if (boxRuns.Count > 0)
                                {
                                    boxRuns.Add(CssTextRun.CreateSingleWhitespace());
                                }

                                parsingState = WordParsingState.CharacterCollecting;


                                startIndex = i;
                                appendLength = 1;//start append length
                            }
                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                //flush new token  
                                boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));

                                parsingState = WordParsingState.Whitespace;
                                appendLength = 0; //clear append length 
                            }
                            else
                            {
                                appendLength++;
                            }
                        } break;
                }
            }
            //--------------------
            //last one
            if (appendLength > 0)
            {
                switch (parsingState)
                {
                    case WordParsingState.Whitespace:
                        {
                            if (boxRuns.Count > 0)
                            {
                                boxRuns.Add(CssTextRun.CreateSingleWhitespace());
                            }
                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));

                            appendLength = 0; //clear append length 
                        } break;
                }
            }
            return boxRuns;
        }


        List<CssRun> ParseWordContentDefault(char[] textBuffer, bool boxIsNotBreakAll, bool keepPreWhitespace)
        {

            List<CssRun> boxRuns = new List<CssRun>();
            int startIndex = 0;
            int buffLength = textBuffer.Length;

            //not preserve whitespace
            WordParsingState parsingState = WordParsingState.Init;
            int appendLength = 0;


            for (int i = 0; i < buffLength; ++i)
            {
                char c0 = textBuffer[i];
                //switch by state
                switch (parsingState)
                {
                    case WordParsingState.Init:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                parsingState = WordParsingState.Whitespace;
                                appendLength = 1;
                            }
                            else
                            {
                                parsingState = WordParsingState.CharacterCollecting;
                                startIndex = i;
                                appendLength = 1;//start append length
                            }
                        } break;
                    case WordParsingState.Whitespace:
                        {
                            if (!char.IsWhiteSpace(c0))
                            {
                                //flush whitespace
                                //switch to character mode    
                                if (keepPreWhitespace || boxRuns.Count > 0)
                                {
                                    boxRuns.Add(CssTextRun.CreateSingleWhitespace());
                                }
                                parsingState = WordParsingState.CharacterCollecting;
                                startIndex = i;
                                appendLength = 1;//start append length
                            }
                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            if (char.IsWhiteSpace(c0))
                            {
                                //flush new token   
                                boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));

                                parsingState = WordParsingState.Whitespace;
                                appendLength = 1; //clear append length 
                            }
                            else
                            {
                                appendLength++;
                            }
                        } break;
                }
            }
            //--------------------
            if (appendLength > 0)
            {
                switch (parsingState)
                {
                    case WordParsingState.Whitespace:
                        {
                            if (keepPreWhitespace || boxRuns.Count > 0)
                            {
                                boxRuns.Add(CssTextRun.CreateSingleWhitespace());
                            }

                        } break;
                    case WordParsingState.CharacterCollecting:
                        {
                            boxRuns.Add(CssTextRun.CreateTextRun(startIndex, appendLength));
                            appendLength = 0; //clear append length 
                        } break;
                }
            }
            //--------------------  
            return boxRuns;
        }
    }
}