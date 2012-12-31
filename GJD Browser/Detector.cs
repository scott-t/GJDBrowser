using System;
using System.Collections.Generic;
using System.Text;

namespace GJD_Browser
{
    enum GameID
    {
        None = -1,
        T7G = 0,
        T7GMac = 1,
        T11H = 2
    }

    class Detector
    {


        public static GameID detectGame(string Path)
        {
            try
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path);
                System.IO.FileInfo[] files = dir.GetFiles();
                foreach (System.IO.FileInfo file in files)
                {
                    if (file.Name.Equals("DISK.1"))
                    {
                        /*switch ()
                        {
                            case 0x00c750ce:*/
                                return GameID.T11H;
                        /*
                            default:
                                return GameID.None;
                        }*/
                    }
                    else if (file.Name.ToUpper().Equals("SCRIPT.GRV"))
                    {/*
                        switch (file.GetHashCode())
                        {
                            case 0x00953a8d:*/
                                return GameID.T7G;
                        /*
                            default:
                                return GameID.None;
                        }*/
                    }
                }
            }
            catch
            {
                return GameID.None;
            }
            return GameID.None;
        }
    }
}
