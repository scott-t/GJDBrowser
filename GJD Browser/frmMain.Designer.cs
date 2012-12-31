namespace GJD_Browser
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.gjdChooser = new System.Windows.Forms.ComboBox();
            this.vdxChooser = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblGame = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.surface = new SdlDotNet.Windows.SurfaceControl();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button9 = new System.Windows.Forms.Button();
            this.frameSeek = new System.Windows.Forms.TrackBar();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.surface)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frameSeek)).BeginInit();
            this.SuspendLayout();
            // 
            // gjdChooser
            // 
            this.gjdChooser.FormattingEnabled = true;
            this.gjdChooser.Location = new System.Drawing.Point(6, 19);
            this.gjdChooser.Name = "gjdChooser";
            this.gjdChooser.Size = new System.Drawing.Size(121, 21);
            this.gjdChooser.TabIndex = 0;
            this.gjdChooser.SelectedIndexChanged += new System.EventHandler(this.gjdChooser_SelectedIndexChanged);
            // 
            // vdxChooser
            // 
            this.vdxChooser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.vdxChooser.FormattingEnabled = true;
            this.vdxChooser.Location = new System.Drawing.Point(7, 46);
            this.vdxChooser.Name = "vdxChooser";
            this.vdxChooser.Size = new System.Drawing.Size(120, 446);
            this.vdxChooser.TabIndex = 1;
            this.vdxChooser.SelectedIndexChanged += new System.EventHandler(this.vdxChooser_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(138, 49);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "VDX Index Search (dec)";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(7, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(120, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.vdxChooser);
            this.groupBox2.Controls.Add(this.gjdChooser);
            this.groupBox2.Location = new System.Drawing.Point(12, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(138, 499);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "GJD Browser";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Controls.Add(this.surface);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Location = new System.Drawing.Point(156, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(654, 554);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "VDX Player";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.lblGame);
            this.groupBox5.Controls.Add(this.btnBrowse);
            this.groupBox5.Controls.Add(this.textBox2);
            this.groupBox5.Location = new System.Drawing.Point(7, 474);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(640, 73);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Game Details";
            // 
            // lblGame
            // 
            this.lblGame.AutoSize = true;
            this.lblGame.Location = new System.Drawing.Point(28, 23);
            this.lblGame.Name = "lblGame";
            this.lblGame.Size = new System.Drawing.Size(35, 13);
            this.lblGame.TabIndex = 2;
            this.lblGame.Text = "label1";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(559, 44);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse ... ";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(6, 47);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(547, 20);
            this.textBox2.TabIndex = 0;
            // 
            // surface
            // 
            this.surface.AccessibleDescription = "SdlDotNet SurfaceControl";
            this.surface.AccessibleName = "SurfaceControl";
            this.surface.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.surface.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.surface.Image = ((System.Drawing.Image)(resources.GetObject("surface.Image")));
            this.surface.InitialImage = ((System.Drawing.Image)(resources.GetObject("surface.InitialImage")));
            this.surface.Location = new System.Drawing.Point(7, 20);
            this.surface.Name = "surface";
            this.surface.Size = new System.Drawing.Size(640, 320);
            this.surface.TabIndex = 6;
            this.surface.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.button11);
            this.groupBox4.Controls.Add(this.button10);
            this.groupBox4.Controls.Add(this.button9);
            this.groupBox4.Controls.Add(this.frameSeek);
            this.groupBox4.Controls.Add(this.button5);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.button6);
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.button7);
            this.groupBox4.Controls.Add(this.button1);
            this.groupBox4.Controls.Add(this.button8);
            this.groupBox4.Location = new System.Drawing.Point(7, 354);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(640, 114);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Controls";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(503, 18);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(112, 23);
            this.button9.TabIndex = 5;
            this.button9.Text = "Stop Video";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // frameSeek
            // 
            this.frameSeek.AutoSize = false;
            this.frameSeek.Location = new System.Drawing.Point(31, 76);
            this.frameSeek.Maximum = 0;
            this.frameSeek.Name = "frameSeek";
            this.frameSeek.Size = new System.Drawing.Size(584, 25);
            this.frameSeek.TabIndex = 4;
            this.frameSeek.Scroll += new System.EventHandler(this.frameSeek_Scroll);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(385, 444);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(88, 23);
            this.button5.TabIndex = 3;
            this.button5.Text = "Video + Audio";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(385, 18);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(112, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Play Video + Audio";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(103, 444);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(88, 23);
            this.button6.TabIndex = 2;
            this.button6.Text = "Still";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(31, 18);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Show Still";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(267, 18);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Play Video";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(291, 444);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(88, 23);
            this.button7.TabIndex = 1;
            this.button7.Text = "Video";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(149, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Play Audio";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(197, 444);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(88, 23);
            this.button8.TabIndex = 0;
            this.button8.Text = "Audio";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(31, 47);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(112, 23);
            this.button10.TabIndex = 6;
            this.button10.Text = "Save Still";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(149, 47);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(112, 23);
            this.button11.TabIndex = 7;
            this.button11.Text = "Save Audio";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 578);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "GJD Browser";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.surface)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.frameSeek)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox gjdChooser;
        private System.Windows.Forms.ListBox vdxChooser;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TrackBar frameSeek;
        private SdlDotNet.Windows.SurfaceControl surface;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblGame;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
    }
}

