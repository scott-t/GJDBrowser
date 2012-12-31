using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GJD_Browser
{
    class GJD
    {
        /// <summary>
        /// Path to T7G file directory containing files
        /// </summary>
        private string path = Properties.Settings.Default.T7GPath;

        /// <summary>
        /// RL file format structure
        /// </summary>
        public struct RLData
        {
            public string filename;
            public uint offset;
            public uint length;
            public static byte size = 20;
            public uint number;
        }

        /// <summary>
        /// Mapping table for obtaining GJD file names
        /// </summary>
        public string[] filemap = { "at", "b", "ch", "d", "dr", "fh", "ga", "hdisk" /* 07 */, 
                                     "htbd", "intro", "jhek", "k", "la", "li", "mb" /* 0E */, 
                                     "mc", "mu", "n", "p", "xmi", "gamwav" /* 14 */};

        private ushort index;

        public ushort Index
        {
            get
            {
                return index;
            }
        }

        public string Name { get { return filemap[index]; } }

        private Dictionary<ushort, RLData> vdxMap;

        public Dictionary<ushort, RLData> rlMap
        {
            get
            {
                return vdxMap;
            }
        }

        private BinaryReader stream;

        /// <summary>
        /// Constructor object
        /// </summary>
        /// <param name="index">GJD index to open</param>
        public GJD(ushort index)
        {
            this.index = index;
            vdxMap = new Dictionary<ushort, RLData>();
            try
            {
                stream = new BinaryReader(new BufferedStream(new FileStream(path + filemap[index] + ".gjd", FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8);
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                indexGJD();
            }
            catch { }
        }

        public GJD(string file)
        {
            
            ushort index = (ushort)Array.IndexOf<string>(filemap, file.Substring(0,file.Length - 3).ToLower());
            this.index = index;
            vdxMap = new Dictionary<ushort, RLData>();
            try
            {
                stream = new BinaryReader(new BufferedStream(new FileStream(path + filemap[index] + ".gjd", FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8);
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                //if (!file.ToLower().Contains("xmi"))
                    indexGJD();
            }
            catch { }
        }

        /// <summary>
        /// Clean up things
        /// </summary>
        ~GJD()
        {
            if (stream != null)
                stream.Close();
            vdxMap = null;
        }

        /// <summary>
        /// Locate a VDX based on index number within the GJD
        /// </summary>
        /// <param name="index">Index number of the VDX to get</param>
        /// <param name="vdxInfo">RL information about the VDX</param>
        /// <returns>Filestream to the VDX</returns>
        public BinaryReader getVDX(ushort index, out RLData vdxInfo)
        {
            if (!vdxMap.TryGetValue(index, out vdxInfo))
            {
                return null;
            }

            stream.BaseStream.Seek(vdxInfo.offset, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Locate a VDX based on filename stored in the RL
        /// </summary>
        /// <param name="filename">Filename of the VDX to locate</param>
        /// <param name="vdxInfo">RL information about the VDX</param>
        /// <returns>Filestream to the VDX</returns>
        public BinaryReader getVDX(string filename, out RLData vdxInfo)
        {
            vdxInfo.length = 0;
            vdxInfo.filename = "";
            vdxInfo.offset = 0;
            vdxInfo.number = 0;
            foreach (RLData rl in vdxMap.Values)
            {
                if (rl.filename == filename)
                {
                    vdxInfo = rl;
                    break;
                }
            }

            if (vdxInfo.length == 0)
                return null;

            stream.BaseStream.Seek(vdxInfo.offset, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Creates a lookup for the location of each VDX file within the GJD
        /// </summary>
        private void indexGJD()
        {
            BinaryReader rl = new BinaryReader(new FileStream(path + "/" + filemap[index] + ".rl", FileMode.Open, FileAccess.Read, FileShare.Read),Encoding.UTF8);
            RLData d;
            ushort counter = 0;
            while (rl.BaseStream.Position < rl.BaseStream.Length)
            {
                d.filename = new System.Text.ASCIIEncoding().GetString(rl.ReadBytes(12)).Trim('\0');
                d.offset = rl.ReadUInt32();
                d.length = rl.ReadUInt32();
                d.number = counter;
                if (!d.filename.ToLower().Contains(".wav"))
                    vdxMap.Add(counter, d);
                counter++;
            }
        }
    }
}

