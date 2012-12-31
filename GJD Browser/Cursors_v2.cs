using System;
using System.Collections.Generic;
using System.Text;

using SdlDotNet.Graphics;
using System.Drawing;

namespace GJD_Browser
{
    class V2_Cursor
    {
        public UInt16 frames;
        public UInt16 width, height;
        public UInt16 hotx, hoty;
        public UInt16 morphTypes;
        public UInt16[] morphTo;
        public UInt16[] morphFrames;

        public byte[][] cursorData;
        public byte[][] frameData;
        public byte[][] morphData;
    }

    class Cursors_v2
    {
        private int _numCursors;
        public int numCursors
        {
            get { return _numCursors; }
        }

        public V2_Cursor[] cursors;

        public Cursors_v2(string path)
        {
            System.IO.BinaryReader stream = new System.IO.BinaryReader(new System.IO.FileStream(path + "system\\icons.ph", System.IO.FileMode.Open, System.IO.FileAccess.Read));

            if (stream.ReadUInt32() != 0x6e6f6369 || stream.ReadUInt16() != 1)
                throw new Exception("Invalid icon file");

            _numCursors = stream.ReadUInt16();
            cursors = new V2_Cursor[_numCursors];

            for (int i = 0; i < _numCursors; i++)
            {
                readCursor(i, stream);
            }
        }

        private void readCursor(int cursorIndex, System.IO.BinaryReader stream)
        {
            V2_Cursor cursor = new V2_Cursor();

            cursor.frames = stream.ReadUInt16();
            cursor.width = stream.ReadUInt16();
            cursor.height = stream.ReadUInt16();
            cursor.hotx = stream.ReadUInt16();
            cursor.hoty = stream.ReadUInt16();
            cursor.morphTypes = stream.ReadUInt16();

            if (cursor.morphTypes > 0)
            {
                cursor.morphTo = new UInt16[cursor.morphTypes];
                cursor.morphFrames = new UInt16[cursor.morphTypes];

                for (int i = 0; i < cursor.morphTypes; i++)
                {
                    cursor.morphTo[i] = stream.ReadUInt16();
                    cursor.morphFrames[i] = stream.ReadUInt16();
                }
            }

            cursor.cursorData = new byte[3][]{new byte[0x20], new byte[0x20], new byte[0x20]};
            stream.Read(cursor.cursorData[0], 0, 0x20);
            stream.Read(cursor.cursorData[1], 0, 0x20);
            stream.Read(cursor.cursorData[2], 0, 0x20);

            cursor.frameData = new byte[cursor.frames][];
            for (int i = 0; i < cursor.frames; i++)
            {
                UInt32 len = stream.ReadUInt32();
                cursor.frameData[i] = new byte[len];
                stream.Read(cursor.frameData[i], 0, (int)len);
            }

            cursors[cursorIndex] = cursor;
            
        }

        public static void decodeCursor(byte index, byte frame, ref Cursors_v2 cursorMan, ref Surface s)
        {
            // Icons!
            bool isCursor4 = index == 4;
            if (isCursor4)
                index--;
            
            V2_Cursor newCursor = cursorMan.cursors[index];

            byte[] backBuffer = new byte[/*cursor.width * cursor.height*/ 88 * 88 * 4 /** 2*/], cursorBuffer = new byte[backBuffer.Length];

            int backCtr = 0, cursorCtr = 0;

            int var64 = 0, var65 = 0;
            int ptr = 0;
            byte alpha = 0, palIdx = 0;

            byte r = 0, g = 0, b = 0;

            for (int y = 0; y < newCursor.height; y++)
            {
                for (int x = 0; x < newCursor.width; x++)
                {

                    /*if (px < cursor.width && py < cursor.height)
                    {*/
                    if (var64 == 0 && var65 == 0)
                    {
                        alpha = newCursor.frameData[frame][ptr];
                        ptr++;
                        if ((alpha & 0x80) == 0x80)
                        {
                            var64 = (alpha & 0x7f) + 1;
                        }
                        else
                        {
                            var65 = alpha + 1;
                            alpha = newCursor.frameData[frame][ptr];
                            palIdx = (byte)(alpha & 0x1F);
                            ptr++;
                            alpha &= 0xE0;
                        }
                    }

                    if (var64 > 0)
                    {
                        var64--;
                        palIdx = (byte)(newCursor.frameData[frame][ptr] & 0x1F);

                        r = newCursor.cursorData[0][palIdx];
                        g = newCursor.cursorData[1][palIdx];
                        b = newCursor.cursorData[2][palIdx];

                        alpha = (byte)(newCursor.frameData[frame][ptr] & 0xE0);

                        ptr++;
                    }
                    else
                    {
                        var65--;
                        r = newCursor.cursorData[0][palIdx];
                        g = newCursor.cursorData[1][palIdx];
                        b = newCursor.cursorData[2][palIdx];
                        //flag = 0palIdx
                    }
                    /*}
                    else { b = 0; }*/

                    switch (index)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 5:
                        case 7:
                        case 8:
                        case 0xD:
                        case 0xE:
                            //b = (byte)((uint)b * 5 / 4);
                            //b = (byte)((b >> 3) + (b >> 1));
                            //b = (byte)((uint)b * 5 / 8);
                            break;
                        default:
                            break;
                    }

                    if (alpha > 0 || 0 == 1) //var46 goes here
                    {
                        byte v71, v72, v73;
                        if (/*lots of stuff*/ false)
                        {
                            // direct byte copies
                        }
                        else
                        {

                            // pixel colour underneath goes here
                            v71 = 0;// 0xc8;
                            v72 = 0;// 0xfc;
                            v73 = 0;// 0xf8;
                        }
                        backBuffer[backCtr + 1] = v71;
                        backBuffer[backCtr + 2] = v72;
                        backBuffer[backCtr + 3] = v73;
                        backBuffer[backCtr] = 1;

                        if (isCursor4)
                        {
                            byte t = v71;
                            v71 = v73;
                            v73 = t;
                        }


                        if (alpha > 0)
                        {
                            if (alpha != 0xE0)
                            {
                                alpha = (byte)(((uint)alpha << 8) / 224);
                                r = (byte)((alpha * r + (256 - alpha) * v71) >> 8);
                                g = (byte)((alpha * g + (256 - alpha) * v72) >> 8);
                                b = (byte)((alpha * b + (256 - alpha) * v73) >> 8);

                            }
                        }
                        cursorBuffer[cursorCtr] = 1;
                        cursorBuffer[cursorCtr + 1] = r;
                        cursorBuffer[cursorCtr + 2] = g;
                        cursorBuffer[cursorCtr + 3] = b;
                    }
                    cursorCtr += 4;
                    backCtr += 4;
                }

            }


            /*
            for (int i = 0; i < cursor.height * cursor.width; i++, x++)
            {
                if (x >= cursor.width){
                    x = 0;
                    y++;
                }
                pix[x, y] = Color.FromArgb(cursor.frameData[0][i]);
            }
             * */


            // perform interlace magic
            Surface cSurf = new Surface(newCursor.width, newCursor.height);
            Color[,] pix = new Color[cSurf.Width, cSurf.Height];
            cursorCtr = 0;
            Color last = Color.Black;
            for (int y = 0; y < newCursor.height; y++)
            {
                for (int x = 0; x < newCursor.width; x++)
                {
                    if (cursorBuffer[cursorCtr] > 0)
                    {
                        //sub_406720(v9 + v13, v6 + v11, *(_BYTE*)(memBlockC + 1), *(_BYTE*)(memBlockC + 2), *(_BYTE*)(memBlockC + 3));
                        /*(unsigned __int8)(*(_BYTE *)v3 >> (3))
    + ((unsigned __int8)(*(_BYTE *)(v3 + 1) >> (2)) << 5)
    + ((unsigned __int8)(*(_BYTE *)(v3 + 2) >> (3)) << 0xb) */

                        /*
                         * inline static void YUV2RGB(byte y, byte u, byte v, byte &r, byte &g, byte &b) {
r = CLIP<int>(y + ((1357 * (v - 128)) >> 10), 0, 255);
g = CLIP<int>(y - (( 691 * (v - 128)) >> 10) - ((333 * (u - 128)) >> 10), 0, 255);
b = CLIP<int>(y + ((1715 * (u - 128)) >> 10), 0, 255);*/
                        /*byte yy = memB[bctr + 1];
                        byte u = memB[bctr + 2];
                        byte v = memB[bctr + 3];
                        uint bb = (uint)(y + ((1357 * (v - 128)) >> 10));
                        uint g = (uint)(y - ((691 * (v - 128)) >> 10) - ((333 * (u - 128)) >> 10));
                        uint r = (uint)(y + ((1715 * (u - 128)) >> 10));
                                 
                         Color.FromArgb((byte)(r > 255 ? 255 : 0), (byte)(g > 255 ? 255 : g), (byte)(bb > 255 ? 255 : bb));*/

                        pix[x, y] = Color.FromArgb(cursorBuffer[cursorCtr + 1], cursorBuffer[cursorCtr + 2], cursorBuffer[cursorCtr + 3]);
                        last = pix[x, y];
                        cursorBuffer[cursorCtr] = 0;
                    }

                    cursorCtr += 4;
                }
            }
            cSurf.SetPixels(new Point(0, 0), pix);
            //cSurf.SaveBmp("h:\\test.bmp");
            s.Fill(Color.Black);
            s.Blit(cSurf, new Point(20, 20));
        }
    }
}
