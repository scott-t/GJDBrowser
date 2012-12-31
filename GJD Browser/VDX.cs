using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using SdlDotNet.Audio;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GJD_Browser
{

    /* The VDX File Format - http://wiki.multimedia.cx/index.php?title=VDX
     *
     *   Chunk based, with all data in little-endian (intel) format
     *
     *
     *   Data formats
     *       Byte    Unsigned 8 bit
     *       Word    Unsigned 16 bit
     *       DWord   Unsigned 32 bit
     *       rgb     3 bytes representing R/G/B
     *
     *
     *   A short header is present in each file (see Structure VDXHeader)
     *       UInt16 ID - usually 0x6792
     *       UInt8  Unknown x6
     *        (byte)
     *
     *   The General Chunk Type
     *       byte    Chunk Type      Determines the chunk of data
     *       byte    Unknown         Possibly replay or sync commands?
     *       dword   Data size       Size excluding header info
     *       byte    Length Mask     Not zero if compressed
     *       byte    Length bits     Not zero if compressed
     *       byte[]  Data            Raw data - length determined by data size
     *
     *
     *       The data can be compressed using LZSS - compressed iff mask and bits are both non-zero.
     *       Decompression circular history buffer and sliding window.
     *
     *           buffersize = 1 << (16 - len bits)
     *           windowsize = 1 << len bits
     *           threshold  = 3
     *
     *       All references are relative to the current write position. Initially, writing starts at
     *       bufferSize - windowSize. The lengthMask value of the header can be used to isolate the
     *       length portion of a buffer reference, though this information seems a bit redundant since
     *       the number of bits used (lengthBits) is also known.
     *
     *
     *   0x00 Chunk Type - Replay Info ???
     *       Data size == 0
     *       Second header byte <> 0
     *           Usually 0x67 or 0x77.
     *           Sync video and audio by repeating last video frame upon 0x67
     *
     *
     *   0x20 Chunk Type - Image
     *       word    numXtiles       Horizontal tile count
     *       word    numYtiles       Vertical tile count
     *       word    colour depth    Colour depth in bits.  Always 8.
     *       rgb     palette[]       As many values as implied by depth.  Always 256
     *       byte    image[]         See Below
     *
     *       Image is split into tiles of 4x4 pixels each.  The image data consists of a sequence
     *       of numX * numY 4-byte structures.  The structures describe the image from topleft
     *       to bottomright.
     *
     *       Image structure
     *           byte    colour1     Palette index of colour 1
     *           byte    colour0     Palette index of colour 0
     *           word    colourMap   2 byte sequence indicating pixel colour.  If the bit == 1, colour 1 used
     *                               else, colour 0 used.
     *
     *                                    +----+----+----+----+
     *                                    | 15 | 14 | 13 | 12 |
     *       LSB (0)        MSB (15)      +----+----+----+----+
     *       |              |             | 11 | 10 |  9 |  8 |
     *       XXXXXXXXXXXXXXXX      -->    +----+----+----+----+
     *                                    |  7 |  6 |  5 |  4 |
     *                                    +----+----+----+----+
     *                                    |  3 |  2 |  1 |  0 |
     *                                    +----+----+----+----+
     *
     *   0x25 Chunk Type - Delta Frame
     *       word    localPalSize        number of palette entries to change.  if 0, header ends here
     *       word    palBitField[16]     field of 256 indicating entries to change      
     *       rgb     localColours[]      number of bits set in palbitfield of rgb values to change palette to
     *       byte    image[]             sequence of opcodes to describe delta info
     *
     *       Palette changes are done on the palette of the last frame.  
     *
     *
     *          LSB ... MSB "i":    0    15  0    15       0    15
     *                              |     |  |     |       |     |
     *                              X ... X  X ... X       X ... X
     *                                 |        |             |
     *          palBitFieldIdx:        0        1     ...    15
     *
     *
     *                                             |
     *                                             |
     *                                             V
     *
     *
     *          palette index:        0      1         15    16         31   ...   255
     *                                |      |          |     |          |          |
     *          (palBitFieldIdx,i): (0,15) (0,14) ... (0,0) (1,15) ... (1,0)     (15,0) 
     *
     *       Opcodes are divided into 7 classes depending on function.
     *           0x00 to 0x5F followed by two parameters, colour1 and colour0.
     *               Current tiles will be altered in the same method as 0x20 chunk.
     *               Colour map extracted from predefined array of 96 words.  opcode == index.
     *               Array map:
     *                  0x00, 0xc8, 0x80, 0xec, 0xc8, 0xfe, 0xec, 0xff, 0xfe, 0xff, 0x00, 0x31, 0x10, 0x73, 0x31, 0xf7,
     *                  0x73, 0xff, 0xf7, 0xff, 0x80, 0x6c, 0xc8, 0x36, 0x6c, 0x13, 0x10, 0x63, 0x31, 0xc6, 0x63, 0x8c,
     *                  0x00, 0xf0, 0x00, 0xff, 0xf0, 0xff, 0x11, 0x11, 0x33, 0x33, 0x77, 0x77, 0x66, 0x66, 0xcc, 0xcc,
     *                  0xf0, 0x0f, 0xff, 0x00, 0xcc, 0xff, 0x76, 0x00, 0x33, 0xff, 0xe6, 0x0e, 0xff, 0xcc, 0x70, 0x67,
     *                  0xff, 0x33, 0xe0, 0x6e, 0x00, 0x48, 0x80, 0x24, 0x48, 0x12, 0x24, 0x00, 0x12, 0x00, 0x00, 0x21,
     *                  0x10, 0x42, 0x21, 0x84, 0x42, 0x00, 0x84, 0x00, 0x88, 0xf8, 0x44, 0x00, 0x32, 0x00, 0x1f, 0x11,
     *                  0xe0, 0x22, 0x00, 0x4c, 0x8f, 0x88, 0x70, 0x44, 0x00, 0x23, 0x11, 0xf1, 0x22, 0x0e, 0xc4, 0x00,
     *                  0x3f, 0xf3, 0xcf, 0xfc, 0x99, 0xff, 0xff, 0x99, 0x44, 0x44, 0x22, 0x22, 0xee, 0xcc, 0x33, 0x77,
     *                  0xf8, 0x00, 0xf1, 0x00, 0xbb, 0x00, 0xdd, 0x0c, 0x0f, 0x0f, 0x88, 0x0f, 0xf1, 0x13, 0xb3, 0x19,
     *                  0x80, 0x1f, 0x6f, 0x22, 0xec, 0x27, 0x77, 0x30, 0x67, 0x32, 0xe4, 0x37, 0xe3, 0x38, 0x90, 0x3f,
     *                  0xcf, 0x44, 0xd9, 0x4c, 0x99, 0x4c, 0x55, 0x55, 0x3f, 0x60, 0x77, 0x60, 0x37, 0x62, 0xc9, 0x64,
     *                  0xcd, 0x64, 0xd9, 0x6c, 0xef, 0x70, 0x00, 0x0f, 0xf0, 0x00, 0x00, 0x00, 0x44, 0x44, 0x22, 0x22
     *
     *           0x60    16 parameters representing individual palette entries.  16 pixels of current tiles to be filled
     *                   by corresponding colours.
     *
     *           0x61    no parameters.  skip the "line" - move a tile along the left border.
     *
     *           0x62-0x6b.  no parameters.  skip (opcode - 0x6b) tiles along the current line.
     *
     *           0x6c-0x75   one parameter defining palette entry.  next (opcode - 0x6b) tiles filled with colour.
     *
     *           0x76-0x7f   (opcode - 0x75) parameters.  each defines a palette entry to fill a tile in the current line
     *
     *           0x80-0xFF   3 parameters. opcode itself and first byte represent colour map.  next two are colour1 and 0.
     *                       tile coloured as normal.
     *
     *
     *   0x80 Chunk Type - Sound
     *       Raw 8 bit mono, 22,050 Hz wave file.
     *
     */

    #region ChunkStructure

    enum VDXBlockType
    {
        Zero = 0,
        Image = 0x20,
        Video = 0x25,
        Sound = 0x80
    }

    enum VDXChunkPlayCmd
    {
        RepeatPrevFrame = 0x67,
        Sync2 = 0x77
    }

    struct VDXHeader
    {
        public UInt16 ID;       // should be 0x6792
        public byte Unknown1, Unknown2, Unknown3, Unknown4;
        public ushort FPS;
        public const byte size = 8;

    }

    enum VDXStatus
    {
        Loading = 0x01,
        Playing = 0x02,
        Stopped = 0x03
    }

    struct VDXChunkHeader
    {
        public VDXBlockType BlockType;
        public VDXChunkPlayCmd PlayCmd;
        public int DataSize;
        public byte LengthMask, LengthBits;

        public void writeToQueue(ref Queue<byte> q)
        {
            q.Enqueue((byte)BlockType);
            q.Enqueue((byte)PlayCmd);
            q.Enqueue((byte)(DataSize & 0xFF));
            q.Enqueue((byte)((DataSize >> 8) & 0xFF));
            q.Enqueue((byte)((DataSize >> 16) & 0xFF0000));
            q.Enqueue((byte)((DataSize >> 24) & 0xFF000000));
            q.Enqueue(LengthMask);
            q.Enqueue(LengthBits);
        }

        public const byte size = 8;
    }
    #endregion

    class VDX
    {
        #region WAVE Format
        private readonly byte[] OpcodeMap = {0x0, 0xC8, 0x80, 0xEC, 0xC8, 0xFE, 0xEC, 0xFF, 0xFE, 0xFF, 0x0, 0x31, 0x10, 0x73, 0x31, 0xF7, 
                      0x73, 0xFF, 0xF7, 0xFF, 0x80, 0x6C, 0xC8, 0x36, 0x6C, 0x13, 0x10, 0x63, 0x31, 0xC6, 0x63, 0x8C, 
                      0x0, 0xF0, 0x0, 0xFF, 0xF0, 0xFF, 0x11, 0x11, 0x33, 0x33, 0x77, 0x77, 0x66, 0x66, 0xCC, 0xCC, 
                      0xF0, 0xF, 0xFF, 0x0, 0xCC, 0xFF, 0x76, 0x0, 0x33, 0xFF, 0xE6, 0xE, 0xFF, 0xCC, 0x70, 0x67, 
                      0xFF, 0x33, 0xE0, 0x6E, 0x0, 0x48, 0x80, 0x24, 0x48, 0x12, 0x24, 0x0, 0x12, 0x0, 0x0, 0x21, 
                      0x10, 0x42, 0x21, 0x84, 0x42, 0x0, 0x84, 0x0, 0x88, 0xF8, 0x44, 0x0, 0x32, 0x0, 0x1F, 0x11, 
                      0xE0, 0x22, 0x0, 0x4C, 0x8F, 0x88, 0x70, 0x44, 0x0, 0x23, 0x11, 0xF1, 0x22, 0xE, 0xC4, 0x0, 
                      0x3F, 0xF3, 0xCF, 0xFC, 0x99, 0xFF, 0xFF, 0x99, 0x44, 0x44, 0x22, 0x22, 0xEE, 0xCC, 0x33, 0x77, 
                      0xF8, 0x0, 0xF1, 0x0, 0xBB, 0x0, 0xDD, 0xC, 0xF, 0xF, 0x88, 0xF, 0xF1, 0x13, 0xB3, 0x19, 
                      0x80, 0x1F, 0x6F, 0x22, 0xEC, 0x27, 0x77, 0x30, 0x67, 0x32, 0xE4, 0x37, 0xE3, 0x38, 0x90, 0x3F, 
                      0xCF, 0x44, 0xD9, 0x4C, 0x99, 0x4C, 0x55, 0x55, 0x3F, 0x60, 0x77, 0x60, 0x37, 0x62, 0xC9, 0x64,
                      0xCD, 0x64, 0xD9, 0x6C, 0xEF, 0x70, 0x0, 0xF, 0xF0, 0x0, 0x0, 0x0, 0x44, 0x44, 0x22, 0x22};

      /* WAVE header format
     * 
     * RIFF chunk - 12 bytes
     *   0   0   0-3     "RIFF" 0x 52, 49, 46, 46
     *   4   4   4-7     length of package to follow
     *   8   8   8-11    "WAVE" 0x 57, 41, 56, 45
     *   
     * FORMAT chunk - 32 bytes
     *   C   12  0-3     "fmt " 0x 66, 6D, 74, 20
     *   10  16  4-7     length of format chunk.  always 0x10
     *   14  20  8-9     0x01
     *   16  22  10-11   Number of channels.  0x01 = mono, 0x02 = stereo
     *   18  24  12-15   sample rate in hz
     *   1C  28  16-19   bytes per second
     *   20  32  20-21   bytes per sample. 1 = 8bitmono, 2 = 8bitstereo or 16bitmono, 4 = 16bitstereo
     *   22  34  22-23   bits per sample
     *
     * DATA chunk
     *   24  36  0-3     "data" 0x 64, 61, 74, 61
     *   28  40  4-7     length of data to follow
     *   2C  44  8-      data/samples
     *
     */

        private readonly byte[] WavHeader = {(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'A', (byte)'V', (byte)'E', 
                            (byte)'f', (byte)'m', (byte)'t', (byte)' ', 0x10, 0, 0, 0, 1, 0, 1, 0, 
                            0x22, 0x56, 0, 0, 0x22, 0x56, 0, 0, 1, 0, 8, 0, 
                            (byte)'d', (byte)'a', (byte)'t', (byte)'a', 0, 0, 0, 0};

        #endregion

        /// <summary>
        /// VDX graphics palette
        /// </summary>
        private uint[] palette = new uint[256];
        /*private Color transColor = Color.Black;
        public Color transparentColor
        {
            get
            {
                return transColor;
            }
        }*/
        //public string filename;

        /// <summary>
        /// Target drawing surface
        /// </summary>
        public Surface surface;

        unsafe uint* surfacePtr;

        public byte fps = 15;

        /// <summary>
        /// Position to place VDX
        /// </summary>
        private Point position;

        /// <summary>
        /// Current status
        /// </summary>
        private VDXStatus status = VDXStatus.Loading;

        public bool playing
        {
            get
            {
                return (status == VDXStatus.Playing);
            }
        }

        /// <summary>
        /// Chunk location map
        /// </summary>
        private Queue<long> audioOffsets = new Queue<long>();
        private Queue<long> videoOffsets;
        private Queue<long> videoMaster = new Queue<long>();
        private long imageOffset;

        private int framecount = 0;
        public int FrameNumber
        {
            get { return framecount; }
        }
        private int videoFrames = 1;
        public int VideoFrames
        {
            get
            {
                return videoFrames;
            }
        }
        public bool audioExists
        {
            get
            {
                return audio != null;
            }
        }


        /// <summary>
        /// Audio stream from the VDX
        /// </summary>
        private SdlDotNet.Audio.Sound audio;
        private byte[] audioBuffer;
        //private int audio;
        private decimal audioLength = 0;

        public byte[] GetAudioStream()
        {
            return audioBuffer;
        }

        /// <summary>
        /// File stream for the VDX
        /// </summary>
        private BinaryReader file;

        /// <summary>
        /// Path to T7G file directory containing files
        /// </summary>
        private string path = Properties.Settings.Default.T7GPath;

        #region LZSS
        /* The LZSS algorithm - http://wiki.xentax.com/index.php/LZSS
     *
     *   Compression
     *       Set up parameters
     *           N           Circular buffer size
     *           F           Length window size
     *           Threshold   Minimum compression size (minimum length for a match)
     *           BufPos      Initial buffer position.  Generally N - F
     *           BufVoid     Initial fill byte.
     *
     *       To compress
     *           Try to find longest sequence that is also in buffer
     *               If length >= Threshold, save offset and length in output file.  record literal to buffer
     *               If no match, copy literal byte to output and repeat
     *
     *           To record if compression or literal copy made, set special flag byte,
     *
     *
     *   Decompression
     *       Done in reverse order.  Parameters need to be known.
     *       Pseudo:
     *           
     *           while InputPos < InputSize do
     *           begin
     *
     *               FlagByte := Input[InputPos++]
     *               for i := 1 to 8 do
     *               begin
     *
     *                  if (FlagByte and 1) = 0 then
     *                  begin
     *
     *                       OfsLen := Input[InputPos] + Input[InputPos + 1] shl 8
     *                       InputPos += 2
     *                       if OfsLen = 0 then
     *                           Exit;
     *
     *                       Length := OfsLen and LengthMask + Threshold
     *                       Offset := (BufPos - (OfsLen shr LengthBits)) and (N - 1)
     *                       copy Length bytes from Buffer[Offset] onwards
     *                       while increasing BufPos 
     *
     *                   end
     *                   else
     *                   begin
     *
     *                       copy 1 byte from Input[InputPos]
     *                       InputPos += 1
     *
     *                  end
     *                  FlagByte := FlagByte shr 1
     *
     *               end
     *           end    
     *
     *       Length bits descibes number of bits to find F.  Above assumes length of buffer reference
     *           in lower bit of the word.  lenght mask = (1 << lenbits) - 1
     *
     *       also assumed buffer offset relative to current write position.  if absolute, BufPos subtraction unneeded
     *
     *       Buffer is circular.  wrap offset.
     *
     *       mirror output writes to history buffer
     *
     *       in most cases, data from history buffer will be byte-wise.  
     */

        private void decompress(System.IO.BinaryReader inStream, ref VDXChunkHeader header, out System.IO.MemoryStream stream)
        {
            stream = new System.IO.MemoryStream();

            uint i, j, N, F, initwrite;
            byte flagbyte, tempa;
            uint offsetlen, length, offset, bufpos;
            byte[] hisbuff;

            N = (uint)(1 << (16 - header.LengthBits));  // History buffer size
            hisbuff = new byte[N];              // History buffer
            F = (uint)(1 << header.LengthBits);         // Window size
            initwrite = N - F;                  // Initial history write pos
            bufpos = 0;

            for (i = 0; i < N; i++)
                hisbuff[i] = 0;                 // Init history buffer

            long final = header.DataSize + inStream.BaseStream.Position;
            while (inStream.BaseStream.Position < final)
            {
                flagbyte = inStream.ReadByte();
                for (i = 1; i <= 8; i++)
                {
                    if (inStream.BaseStream.Position < final)
                    {
                        if ((flagbyte & 1) == 0)
                        {
                            offsetlen = inStream.ReadUInt16();
                            if (offsetlen == 0)
                                break;

                            length = (offsetlen & header.LengthMask) + 3;
                            offset = (bufpos - (offsetlen >> header.LengthBits)) & (N - 1);
                            for (j = 0; j < length; j++)
                            {
                                tempa = hisbuff[(offset + j) & (N - 1)];
                                stream.WriteByte(tempa);
                                hisbuff[bufpos] = tempa;
                                bufpos = (bufpos + 1) & (N - 1);
                            }
                        }
                        else
                        {
                            tempa = inStream.ReadByte();
                            stream.WriteByte(tempa);
                            hisbuff[bufpos] = tempa;
                            bufpos = (bufpos + 1) & (N - 1);
                        }
                        flagbyte = (byte)(flagbyte >> 1);
                    }
                }
            }
        }
        #endregion

        #region Constructors
        
        /// <summary>
        /// Open a vdx based on filestream and rl data
        /// </summary>
        /// <param name="stream">Stream pointing to the VDX location</param>
        /// <param name="rl">RL information including offset and length of the VDX</param>
        /// <param name="flags">VDX flags relating to playback</param>
        /// <param name="baseSurface">Base surface to build from</param>
        public VDX(BinaryReader stream, GJD.RLData rl, Surface baseSurface)
        {
          
            this.file = stream;
            status = VDXStatus.Loading;
            surface = baseSurface;
            parseVDX(rl);
            parseAudio();
            parseImage();
        }

        ~VDX()
        {
            status = VDXStatus.Stopped;
        }

        #endregion

        /// <summary>
        /// Initialise VDX playback
        /// </summary>
        public void play()
        {
            if (status != VDXStatus.Playing)
            {
                parseImage();
                videoOffsets = new Queue<long>(videoMaster);
                status = VDXStatus.Playing;
                if (audio != null)
                    audio.Play();
            }
        }

        public void playAudio()
        {
            if (audio != null)
                audio.Play();
        }

        public void showStill(int frame)
        {
            if (frame == 0)
            {
                surface.Fill(Color.FromArgb((int)palette[0xff]));
                parseImage();
            }
            else
            {
                //parseImage();
                if (frame > videoMaster.Count)
                    frame = videoMaster.Count;

                long[] offsets = videoMaster.ToArray();
                // find pallete
                parseImage();
                surface.Fill(Color.FromArgb((int)palette[0x0]));

                parseVideo(offsets[frame - 1]);
            }
        }

        public void playVideo()
        {
            if (status != VDXStatus.Playing)
            {
                parseImage();
                videoOffsets = new Queue<long>(videoMaster);
            }

            status = VDXStatus.Playing;
        }
        
        public void stop()
        {
            status = VDXStatus.Stopped;
            fps = 15;
        }

        /// <summary>
        /// Fills a tile at the specified location with the two colors dithered as per colorMap
        /// </summary>
        /// <param name="x">x coordinate of tile</param>
        /// <param name="y">y coordinate of tile</param>
        /// <param name="col1">First color to dither with</param>
        /// <param name="col0">Second color to dither with</param>
        /// <param name="colorMap">Map specifying how dithering should occur</param>
        
        unsafe private void fillTile(ushort x, ushort y, byte col1, byte col0, UInt16 map)
        {
            for (ushort j = 0; j < 4; j++)
            {
                uint* p = (uint*)((byte*)surfacePtr + (uint)((y + j) * (surface.Width * 4)) + 4 * x);
                for (ushort k = 0; k < 4; k++)
                {
                    if ((map & 32768) == 32768)
                        *p = palette[col1];
                    else
                        *p = palette[col0];

                    p++;
                    map = (ushort)(map << 1);
                }
            }
        }


        /// <summary>
        /// Called to advance to the next frame
        /// </summary>
        public void doTick()
        {
            
            if (status == VDXStatus.Playing)
            {
                    // Advance frame
                    parseVideo(null);
            }
        }

        #region File parsing

        /// <summary>
        /// Parse the VDX to locate individual chunks
        /// </summary>
        private void parseVDX(GJD.RLData? rlData)
        {
            long stop;
            // Final stream position
            if (rlData.HasValue)
            {
                stop = rlData.Value.offset + rlData.Value.length;
            }
            else
            {
                stop = file.BaseStream.Length;
            }
            VDXHeader header = new VDXHeader();
            VDXChunkHeader chunk = new VDXChunkHeader();

            // Initialise queue's
            audioOffsets = new Queue<long>();
            videoOffsets = new Queue<long>();

            // Read VDX header
            header.ID = file.ReadUInt16();
            if (header.ID != 0x9267)
                throw new Exception("Invalid VDX header");

            header.Unknown1 = file.ReadByte();
            header.Unknown2 = file.ReadByte();
            header.Unknown3 = file.ReadByte();
            header.Unknown4 = file.ReadByte();
            header.FPS = file.ReadUInt16();

            //fps = (byte)header.FPS;
            try
            {
                while (file.BaseStream.Position < stop)
                {
                    // Read VDX chunk header
                    chunk = readChunkHeader();

                    // Add offsets where appropriate
                    switch (chunk.BlockType)
                    {
                        case VDXBlockType.Video:
                            videoFrames++;
                            goto case VDXBlockType.Zero;
                        case VDXBlockType.Zero:
                            videoMaster.Enqueue(file.BaseStream.Position - VDXChunkHeader.size);
                            break;
                        case VDXBlockType.Sound:
                            audioOffsets.Enqueue(file.BaseStream.Position - VDXChunkHeader.size);
                            break;
                        case VDXBlockType.Image:
                            imageOffset = file.BaseStream.Position - VDXChunkHeader.size;
                            break;
                    }

                    // Seek to next chunk
                    file.BaseStream.Seek(chunk.DataSize, System.IO.SeekOrigin.Current);
                }
            } catch {
                audio = null;
                videoFrames = 1;

            }
        }

        /// <summary>
        /// Separate out audio stream for playback
        /// </summary>
        private void parseAudio()
        {
            // Check for null audio
            if (audioOffsets.Count == 0)
                return;

            // Set up buffer
            MemoryStream buffer = new MemoryStream();
            buffer.Write(WavHeader, 0, WavHeader.Length);

            while (audioOffsets.Count > 0)
            {
                // Find next audio chunk
                file.BaseStream.Seek(audioOffsets.Dequeue(), System.IO.SeekOrigin.Begin);

                // Read header
                VDXChunkHeader chunk = readChunkHeader();

                if (chunk.BlockType != VDXBlockType.Sound)
                    throw new Exception("Invalid block type");

                // Check for compression
                if (chunk.LengthBits == 0 || chunk.LengthMask == 0)
                {
                    // No compression - direct copy
                    byte[] t;// = new byte[chunk.DataSize];
                    t = file.ReadBytes(chunk.DataSize);
                    buffer.Write(t, 0, t.Length);
                }
                else
                {
                    // Decompress and copy
                    System.IO.MemoryStream t;
                    decompress(file, ref chunk, out t);
                    t.Position = 0;
                    t.WriteTo(buffer);
                }
            }

            // Little messy but only runs on VDX load at this stage
            byte[] audiob = buffer.ToArray();
            int g = audiob.Length - 8;

            audiob[7] = (byte)((g & 0xFF000000) >> 24);
            audiob[6] = (byte)((g & 0xFF0000) >> 16);
            audiob[5] = (byte)((g & 0xFF00) >> 8);
            audiob[4] = (byte)(g & 0xFF);

            g = audiob.Length - 44;

            audiob[43] = (byte)((g & 0xFF000000) >> 24);
            audiob[42] = (byte)((g & 0xFF0000) >> 16);
            audiob[41] = (byte)((g & 0xFF00) >> 8);
            audiob[40] = (byte)(g & 0xFF);

            audioLength = Math.Ceiling(g / (decimal)22);
            audioBuffer = audiob;
            audio = new SdlDotNet.Audio.Sound(audiob);
        }

        /// <summary>
        /// Parse the still image and save to surface
        /// </summary>
        private void parseImage()
        {

            // Find start of chunk
            file.BaseStream.Seek(imageOffset, System.IO.SeekOrigin.Begin);

            // Read in chunk header
            VDXChunkHeader chunk = readChunkHeader();

            if (chunk.BlockType != VDXBlockType.Image)
                throw new Exception("Invalid chunk type");

            System.IO.BinaryReader stream;
            if (chunk.LengthBits == 0 || chunk.LengthMask == 0)
            {
                // Non-compressed
                stream = file;
            }
            else
            {
                // Compressed
                System.IO.MemoryStream tmp;
                decompress(file, ref chunk, out tmp);
                stream = new System.IO.BinaryReader(tmp);
                stream.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            }

            // Start frame read-in
            ushort height, width, depth;

            // Image details
            width = (ushort)(stream.ReadUInt16() * 4);
            height = (ushort)(stream.ReadUInt16() * 4);
            depth = stream.ReadUInt16();
            /*
            // Create surface
            if (s == null)
            {
                // If no surface to continue on from
                surface = new Surface(width, height, 32, 0xFF << 16, 0xFF << 8, 0xFF, 0xFF << 24);
            }
            else
            {
                // "Continue" from previous frame
                surface = new Surface(s);
            }
            */
            //Position VDX's appopriately (??)
            if (width == 640)
                position = new Point(0, 80);
            else
                position = new Point(0, 0);

            // Read in palette
            for (short k = 0; k < 256; k++)
            {
                palette[k] = (uint)((0xff000000) + (stream.ReadByte() * 65536) + (stream.ReadByte() * 256) + stream.ReadByte());
            }
            // Get pixel data
            unsafe
            {
                surfacePtr = (uint*)((byte*)surface.Pixels);
            }

                // Begin drawing
                byte col1, col0;
                ushort colorMap;
                for (ushort j = 0; j < height; j += 4)
                {
                    for (ushort i = 0; i < width; i += 4)
                    {
                        // Read in dithering colors
                        col1 = stream.ReadByte();
                        col0 = stream.ReadByte();
                        colorMap = stream.ReadUInt16();
                        fillTile(i, j, col1, col0, colorMap);
                    }
                }
            stream = null;

        }

        /// <summary>
        /// Advance an animation frame
        /// </summary>
        private void parseVideo(long? off)
        {
            try
            {
                if (/*(flags & (Groovie.bitFlags.JustAudio)) != 0 || */!off.HasValue && videoOffsets.Count == 0)
                {
                    status = VDXStatus.Stopped;
                    fps = 15;
                    return;
                }
                long offset;
                VDXChunkHeader chunk;
                if (!off.HasValue)
                {
                    do
                    {
                        offset = videoOffsets.Dequeue();
                        file.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);
                        chunk = readChunkHeader();

                        if (videoOffsets.Count == 0)
                        {
                            status = VDXStatus.Stopped;
                            fps = 15;
                            if (chunk.BlockType != VDXBlockType.Video)
                            {
                                return;
                            }
                        }

                        // Repeat prev frame when needed
                        if (chunk.BlockType == VDXBlockType.Zero)// && chunk.PlayCmd == VDXChunkPlayCmd.RepeatPrevFrame)
                            return;

                        if (chunk.PlayCmd != VDXChunkPlayCmd.Sync2)
                        {
                            //Console.WriteLine("non-Sync2 found!");
                            //return;
                        }

                    } while (chunk.BlockType != VDXBlockType.Video);
                }
                else
                {
                    offset = off.Value;
                    file.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);
                    chunk = readChunkHeader();
                }
                System.IO.BinaryReader stream;
                if (chunk.LengthBits == 0 || chunk.LengthMask == 0)
                {
                    // Non-compressed
                    stream = file;
                }
                else
                {
                    // Compressed
                    System.IO.MemoryStream tmp;
                    decompress(file, ref chunk, out tmp);
                    stream = new System.IO.BinaryReader(tmp);
                    tmp = null;
                    stream.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                }

                // Is there a palette mapping?
                ushort localPalette = stream.ReadUInt16();

                #region Alter palette
                if (localPalette > 0)
                {
                    // Yes - update palette
                    ushort[] pattern = new ushort[16];

                    // Read updated indicies
                    for (byte i = 0; i < 16; i++)
                    {
                        pattern[i] = stream.ReadUInt16();
                    }

                    // Update indicies
                    for (byte i = 0; i < 16; i++)
                    {
                        // No update for this ushort
                        if (pattern[i] == 0)
                        {
                            continue;
                        }

                        long offs = i * 16;
                        for (byte j = 0; j < 16; j++)
                        {
                            // Update index
                            if ((32768 & pattern[i]) > 0)
                            {
                                palette[offs] = (uint)((0xff000000) + (stream.ReadByte() * 65536) + (stream.ReadByte() * 256) + stream.ReadByte());
                            }
                            offs++;
                            pattern[i] = (ushort)(pattern[i] * 2);
                        }
                    }
                    pattern = null;
                }
                #endregion

                #region Video opcodes

                ushort x = 0, y = 0;
                byte col1 = 0, col0 = 0, opcode;
                long limit = (chunk.LengthBits == 0 ? offset + chunk.DataSize : stream.BaseStream.Length);
                unsafe
                {
                    uint* p;
                    while (stream.BaseStream.Position < limit)
                    {
                        opcode = stream.ReadByte();
                        if (opcode <= 0x5F)
                        {
                            col1 = stream.ReadByte();
                            col0 = stream.ReadByte();
                            fillTile(x, y, col1, col0, (ushort)(OpcodeMap[(opcode << 1)] + (OpcodeMap[(opcode << 1) + 1] << 8)));
                            x += 4;
                        }
                        else if (opcode == 0x60)
                        {
                            // Fill the 16 pixels with the 16 param colours
                            for (ushort j = 0; j < 4; j++)
                            {
                                p = (uint*)((byte*)surfacePtr + ((y + j) * surface.Width * 4) + 4 * x);
                                *p = palette[stream.ReadByte()];
                                p++;
                                *p = palette[stream.ReadByte()];
                                p++;
                                *p = palette[stream.ReadByte()];
                                p++;
                                *p = palette[stream.ReadByte()];
                            }
                            x += 4;
                        }
                        else if (opcode == 0x61)
                        {
                            // Newline
                            x = 0;
                            y += 4;
                        }
                        else if (opcode <= 0x6B)
                        {
                            // Skip forward opcode-0x62 blocks
                            x += (ushort)((opcode - 0x62) * 4);
                        }
                        else if (opcode <= 0x75)
                        {
                            // Fill opcode-0x6B blocks with param colours
                            uint col = palette[stream.ReadByte()];
                            ushort w = (ushort)((opcode - 0x6B) * 4);
                            for (ushort j = 0; j < 4; j++)
                            {
                                p = (uint*)((byte*)surfacePtr + ((y + j) * surface.Width * 4) + 4 * x);
                                for (ushort i = 0; i < w; i++)
                                {
                                    *p = col;
                                    p++;
                                }
                            }
                            x += (ushort)((opcode - 0x6B) * 4);
                        }
                        else if (opcode <= 0x7F)
                        {
                            // Fill opcode - 0x75 blocks
                            for (byte i = 0; i < (byte)(opcode - 0x75); i++)
                            {
                                uint col = palette[stream.ReadByte()];
                                for (ushort j = 0; j < 4; j++)
                                {
                                    p = (uint*)((byte*)surfacePtr + ((y + j) * surface.Width * 4) + 4 * x);
                                    for (ushort k = 0; k < 4; k++)
                                    {
                                        *p = col;
                                        p++;
                                    }
                                }
                                x += 4;
                            }
                        }
                        else // if (opcode > 0x7F)
                        {
                            UInt16 map = (ushort)(opcode + (stream.ReadByte() << 8));
                            fillTile(x, y, stream.ReadByte(), stream.ReadByte(), map);
                            x += 4;
                        }

                    }
                }


                #endregion

            }
            catch
            {
                stop();
            }
        }

        #endregion

        /// <summary>
        /// Read in the header for the current chunk
        /// </summary>
        /// <returns>Chunk header structure</returns>
        private VDXChunkHeader readChunkHeader()
        {
            VDXChunkHeader chunk = new VDXChunkHeader();

            chunk.BlockType = (VDXBlockType)file.ReadByte();
            chunk.PlayCmd = (VDXChunkPlayCmd)file.ReadByte();
            chunk.DataSize = file.ReadInt32();
            chunk.LengthMask = file.ReadByte();
            chunk.LengthBits = file.ReadByte();

            return chunk;
        }
    }
}
