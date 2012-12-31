using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace GJD_Browser
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// Path to T7G file directory containing files
        /// </summary>
        private string path = Properties.Settings.Default.T7GPath;

        private GJD gjd;

        private VDX vdx;
        private ROQ roq;

        private SdlDotNet.Graphics.Surface s;

        private GameID game;

        private SdlDotNet.Audio.Music midi;

        private List<GJD.RLData>[] V2_RL;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            // "Fix" T7G path
            if (!Properties.Settings.Default.T7GPath.EndsWith("\\"))
            {
                Properties.Settings.Default.T7GPath = Properties.Settings.Default.T7GPath + "\\";
                Properties.Settings.Default.Save();
            }

            game = Detector.detectGame(path);
            readGameData();
            s = new SdlDotNet.Graphics.Surface(this.surface.Width, this.surface.Height);
            SdlDotNet.Core.Events.Tick += new EventHandler<SdlDotNet.Core.TickEventArgs>(this.Tick);
            SdlDotNet.Core.Events.TargetFps = 15;
            Thread thread = new Thread(new ThreadStart(SdlDotNet.Core.Events.Run));
            thread.IsBackground = true;
            thread.Name = "SDL.NET";
            thread.Priority = ThreadPriority.Normal;
            /*
            GJD.RLData rl;
            rl.length = (uint)(new System.IO.FileInfo("tmp.vdx")).Length;
            rl.filename = "tmp.vdx";
            rl.offset = 0;
            rl.number = 0;
            vdx = new VDX(new System.IO.BinaryReader(System.IO.File.Open("tmp.vdx", System.IO.FileMode.Open)), rl, s);
            */
            thread.Start();
        }

        private void gjdChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (game == GameID.T7G)
            {
                if (vdx != null)
                    vdx.stop();

                vdx = null;

                gjd = new GJD(this.gjdChooser.SelectedItem.ToString());
                System.Collections.Generic.Dictionary<ushort, GJD.RLData> rlMap = gjd.rlMap;
                this.vdxChooser.Items.Clear();
                foreach (GJD.RLData rl in rlMap.Values)
                {
                    this.vdxChooser.Items.Add(rl.filename);
                }
            }
            else
            {
                if (this.gjdChooser.SelectedItem.ToString() == "Icons")
                {
                    if (this.gjdChooser.Tag == null)
                        this.gjdChooser.Tag = new Cursors_v2(path);

                    Cursors_v2 cursor = (Cursors_v2)this.gjdChooser.Tag;

                    this.vdxChooser.Items.Clear();
                    for (int i = 0; i < cursor.numCursors; i++)
                        this.vdxChooser.Items.Add(i);
                }
                else
                {
                    // V2 GJD
                    this.vdxChooser.Items.Clear();
                    foreach (GJD.RLData rl in V2_RL[this.gjdChooser.SelectedIndex])
                    {
                        this.vdxChooser.Items.Add(rl.filename);
                    }
                }
            }
        }
        
        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                // Load VDX
                //System.Console.WriteLine("Load VDX");
                ushort offset;
                try
                {
                    offset = System.Convert.ToUInt16(textBox1.Text);
                }
                catch { textBox1.Text = "";  return; }

                ushort gjdIndex = (ushort)(offset >> 10);
                ushort file = (ushort)(offset & 0x3FF);
                GJD.RLData rl;
                gjd = new GJD(gjdIndex);
                System.IO.BinaryReader reader = gjd.getVDX(file, out rl);
                if (reader != null)
                {
                    gjdChooser.SelectedItem = gjd.Name.ToUpper() + ".RL";
                    gjdChooser_SelectedIndexChanged(null,null);
                    if (!gjd.Name.ToLower().Contains("xmi"))
                    {
                        vdx = new VDX(gjd.getVDX(file, out rl), rl, s);
                        modEnviron();
                        vdxChooser.SelectedItem = rl.filename;
                    }
                    else
                    {
                        gjd.getVDX(file, out rl);
                        vdx = null;
                        modEnviron();
                        vdxChooser.SelectedItem = rl.filename;
                    }

                }
                

            }
            else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
            }
            else if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9 || e.Modifiers != 0)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void Tick(object sender, SdlDotNet.Core.TickEventArgs e)
        {
            if (vdx != null)
            {
                vdx.doTick();
                //frameSeek.Value = vdx.FrameNumber;
            }
            if (roq != null)
                roq.doTick();

            this.surface.Blit(s);
        }

        private void vdxChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (game == GameID.T7G)
            {
                if (vdx != null)
                    vdx.stop();

                string file = vdxChooser.SelectedItem.ToString();

                GJD.RLData rl;
                System.IO.BinaryReader reader = gjd.getVDX(file, out rl);
                if (reader != null)
                {
                    if (midi != null)
                        midi.Close();
                    if (!gjd.Name.Contains("xmi"))
                        vdx = new VDX(reader, rl, s);
                    else
                    {
                        if (!System.IO.File.Exists(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid"))
                        {
                            System.Diagnostics.Process p = new System.Diagnostics.Process();
                            p.StartInfo.FileName = path + "mid\\xmi2mid.exe";
                            p.StartInfo.Arguments = path + "mid\\" + file.Substring(0, file.IndexOf(".") + 4) + " " + path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid";
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.WorkingDirectory = path + "mid\\";
                            System.Threading.Thread.Sleep(500);
                            p.Start();
                            p.WaitForExit();
                            System.Console.WriteLine("Missing midi");
                        }
                        if (System.IO.File.Exists(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid"))
                        {
                            midi = new SdlDotNet.Audio.Music(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid");
                            midi.Play(1);
                        }

                    }

                    modEnviron();

                    textBox1.Text = ((Array.IndexOf(gjd.filemap, gjd.Name) << 10) + rl.number).ToString();
                }
            }
            else
            {
                frameSeek.Enabled = false;
                // 11H
                if (gjdChooser.SelectedItem.ToString() == "Icons")
                {
                    Cursors_v2 cur = ((Cursors_v2)(this.gjdChooser.Tag));
                    frameSeek.Maximum = cur.cursors[vdxChooser.SelectedIndex == 4 ? 3 : vdxChooser.SelectedIndex].frames - 1;
                    frameSeek.Value = 0;
                    Cursors_v2.decodeCursor((byte)vdxChooser.SelectedIndex, 0, ref cur, ref s);
                    frameSeek.Enabled = true;
                }
                else
                {
                    // ROQ parser
                    if (roq != null)
                        roq.stop();
                    roq = null;

                    GJD.RLData rl = new GJD.RLData();
                    
                    foreach (GJD.RLData subrl in V2_RL[gjdChooser.SelectedIndex])
                    {
                        if (subrl.filename == vdxChooser.SelectedItem.ToString())
                        {
                            rl = subrl;
                            break;
                        }
                    }
                    System.IO.BinaryReader r = new System.IO.BinaryReader(new System.IO.FileStream(path + "\\media\\" + gjdChooser.SelectedItem, System.IO.FileMode.Open,  System.IO.FileAccess.Read, System.IO.FileShare.Read));
                    r.BaseStream.Seek(rl.offset, System.IO.SeekOrigin.Begin);

                    System.IO.BinaryReader reader = new System.IO.BinaryReader(new System.IO.MemoryStream(r.ReadBytes((int)rl.length)));
                    if (rl.filename.Contains("xmi"))
                    {
                        roq = null;
                        string file = rl.filename;
                        if (file.Contains("\0"))
                            file = file.Substring(0, file.IndexOf("\0"));

                        if (!System.IO.File.Exists(path + "mid\\" + file))
                        {
                            byte[] buffer = new byte[reader.BaseStream.Length];
                            reader.Read(buffer, 0, buffer.Length);
                            System.IO.File.WriteAllBytes(path + "mid\\" + file, buffer);
                        }

                        if (!System.IO.File.Exists(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid"))
                        {
                            System.Diagnostics.Process p = new System.Diagnostics.Process();
                            p.StartInfo.FileName = path + "mid\\xmi2mid.exe";
                            p.StartInfo.Arguments = path + "mid\\" + file.Substring(0, file.IndexOf(".") + 4) + " " + path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid";
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.WorkingDirectory = path + "mid\\";
                           // System.Threading.Thread.Sleep(500);
                           // p.Start();
                           // p.WaitForExit();
                            System.Console.WriteLine("Missing midi");
                        }
                        if (System.IO.File.Exists(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid"))
                        {
                            midi = new SdlDotNet.Audio.Music(path + "mid\\" + file.Substring(0, file.IndexOf(".")) + ".mid");
                            midi.Play(1);
                        }
                    }
                    else
                    {
                        roq = new ROQ(reader, s);
                    }
                    r.Close();
                    r = null;
                    modEnviron();

                    textBox1.Text = (rl.number).ToString();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (vdx != null)
            {
                vdx.showStill(0);
                frameSeek.Enabled = true;
                frameSeek.Value = 0;
            }

            if (roq != null)
            {
                roq.showStill(0);
                frameSeek.Enabled = true;
                frameSeek.Value = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (vdx != null)
                vdx.playAudio();

            if (roq != null)
                roq.playAudio();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (vdx != null)
                vdx.playVideo();

            frameSeek.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (vdx != null)
                vdx.play();

            frameSeek.Enabled = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (vdx != null)
                vdx.stop();
            if (roq != null)
                roq.stop();

        }
 
        private void modEnviron(){
            button10.Enabled = false;
            switch (game)
            {
                case GameID.T7G:
                    if (vdx == null)
                    {
                        button1.Enabled = false;
                        button2.Enabled = false;
                        button9.Enabled = false;
                        button4.Enabled = false;
                        button3.Enabled = false;
                        button11.Enabled = false;
                        frameSeek.Enabled = false;
                    }
                    else
                    {
                        button1.Enabled = vdx.audioExists;
                        button11.Enabled = vdx.audioExists;
                        frameSeek.Value = 0;
                        frameSeek.Maximum = vdx.VideoFrames;
                        button9.Enabled = (vdx.VideoFrames > 1);
                        button2.Enabled = (vdx.VideoFrames > 1);
                        button4.Enabled = (vdx.VideoFrames > 1) && (button1.Enabled);
                        button3.Enabled = (vdx.VideoFrames > 1);
                        //button10.Enabled = (vdx.VideoFrames > 1);
                        vdx.showStill(0);
                        frameSeek.Enabled = true;
                        frameSeek.Value = 0;
                    }
                    break;
                    
                case GameID.T11H:
                    if (roq == null)
                    {
                        button1.Enabled = false;
                        button2.Enabled = false;
                        button9.Enabled = false;
                        button4.Enabled = false;
                        button3.Enabled = false;
                        button10.Enabled = false;
                        button11.Enabled = false;
                        frameSeek.Enabled = false;
                    }
                    else
                    {
                        button1.Enabled = roq.audioExists;
                        frameSeek.Value = 0;
                        frameSeek.Maximum = roq.FrameCount;
                        /*button9.Enabled = (vdx.VideoFrames > 1);
                        button2.Enabled = (vdx.VideoFrames > 1);
                        button4.Enabled = (vdx.VideoFrames > 1) && (button1.Enabled);
                        button3.Enabled = (vdx.VideoFrames > 1);*/
                        roq.showStill(0);
                        frameSeek.Enabled = true;
                        frameSeek.Value = 0;
                    }
                    break;
            }
        }

        private void frameSeek_Scroll(object sender, EventArgs e)
        {
            if (vdx != null)
                vdx.showStill(frameSeek.Value);
            else if (roq != null)
                roq.showStill(frameSeek.Value);
            else if (gjdChooser.Tag is Cursors_v2)
            {
                Cursors_v2 cur = ((Cursors_v2)(this.gjdChooser.Tag));
                Cursors_v2.decodeCursor((byte)vdxChooser.SelectedIndex, (byte)frameSeek.Value, ref cur, ref s);
            }
                
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.ShowNewFolderButton = false;
            f.Description = "Path to game installation";
            f.SelectedPath = Properties.Settings.Default.T7GPath;
            DialogResult res = f.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                if (!f.SelectedPath.EndsWith("\\"))                
                    f.SelectedPath = f.SelectedPath + "\\";

                Properties.Settings.Default.T7GPath = f.SelectedPath;
                Properties.Settings.Default.Save();
                path = f.SelectedPath;    

                game = Detector.detectGame(path);

                readGameData();
                
            }
                
        }

        private void readGameData()
        {
            textBox2.Text = path;
            if (roq != null)
                roq.stop();
            roq = null;
            if (vdx != null)
                vdx.stop();
            vdx = null;
            switch (game)
            {
                case GameID.T7G:
                    lblGame.Text = "The 7th Guest";

                    System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(path);
                    System.IO.FileInfo[] files = DI.GetFiles("*.rl");
                    this.gjdChooser.Items.Clear();

                    foreach (System.IO.FileInfo rl in files)
                    {
                        this.gjdChooser.Items.Add(rl.Name);
                    }

                    break;
                case GameID.T11H:
                    lblGame.Text = "The 11th Hour";
                    //ROQ roq = new ROQ(new System.IO.BinaryReader(new System.IO.FileStream(path + "\\media\\final_hr.rol", System.IO.FileMode.Open)));

                    this.gjdChooser.Items.Clear();
                    System.IO.BinaryReader idx = new System.IO.BinaryReader(new System.IO.FileStream(path + "\\groovie\\gjd.gjd", System.IO.FileMode.Open));
                    string name = "";
                    while (idx.BaseStream.Position < idx.BaseStream.Length)
                    {
                        if (idx.PeekChar() == 0x0A)
                        {
                            idx.ReadChar();
                            if (name.Length > 0)
                                this.gjdChooser.Items.Add(name.Substring(0, name.IndexOf(" ")));

                            name = "";
                        } 
                        else
                            name += "" + idx.ReadChar();
                    }
                    idx.Close();
                    V2_RL = new List<GJD.RLData>[this.gjdChooser.Items.Count];
                    for (int i = 0; i < V2_RL.Length; i++)
                        V2_RL[i] = new List<GJD.RLData>();

                    this.gjdChooser.Items.Add("Icons");

                    idx = new System.IO.BinaryReader(new System.IO.FileStream(path + "\\groovie\\dir.rl", System.IO.FileMode.Open));
                    uint ctr = 0;
                    while (idx.BaseStream.Position < idx.BaseStream.Length)
                    {
                        // Get RL content
                        GJD.RLData rl = new GJD.RLData();
                        idx.ReadUInt32();
                        rl.offset = idx.ReadUInt32();
                        rl.length = idx.ReadUInt32();
                        rl.number = ctr;
                        ctr++;
                        ushort target = idx.ReadUInt16();
                        byte[] filename;
                        filename = idx.ReadBytes(12);
                        rl.filename = System.Text.Encoding.ASCII.GetString(filename).Trim();
                        idx.ReadBytes(6);
                        V2_RL[target].Add(rl);
                    }

                    break;
                default:
                    lblGame.Text = "None";
                    break;
            }


        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.FileName = vdxChooser.SelectedItem.ToString();
            dlg.OverwritePrompt = true;
            dlg.Title = "Export VDX audio stream";
            dlg.Filter = "WAV (*.wav)|*.wav";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] stream = vdx.GetAudioStream();
                if (stream.Length > 0)
                {
                    System.IO.FileStream fs = System.IO.File.Open(dlg.FileName, System.IO.FileMode.Create);
                    System.IO.BinaryWriter wr = new System.IO.BinaryWriter(fs);
                    wr.Write(stream, 0, stream.Length);
                    wr.Close();
                    fs.Close();
                }
                else
                    MessageBox.Show("Bad audio stream?");
            }

        }

    }
}
