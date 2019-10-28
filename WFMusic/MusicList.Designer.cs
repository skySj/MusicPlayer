namespace WFMusic
{
    partial class MusicList
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
            this.skinTreeView1 = new CCWin.SkinControl.SkinTreeView();
            this.SuspendLayout();
            // 
            // skinTreeView1
            // 
            this.skinTreeView1.BackColor = System.Drawing.SystemColors.Window;
            this.skinTreeView1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.skinTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinTreeView1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinTreeView1.ForeColor = System.Drawing.Color.Black;
            this.skinTreeView1.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.skinTreeView1.Location = new System.Drawing.Point(0, 0);
            this.skinTreeView1.Margin = new System.Windows.Forms.Padding(0);
            this.skinTreeView1.Name = "skinTreeView1";
            this.skinTreeView1.Size = new System.Drawing.Size(284, 261);
            this.skinTreeView1.TabIndex = 0;
            this.skinTreeView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.skinTreeView1_ItemDrag);
            this.skinTreeView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.skinTreeView1_DragEnter);
            this.skinTreeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.skinTreeView1_MouseDown);
            // 
            // MusicList
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.skinTreeView1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "MusicList";
            this.Text = "歌曲列表";
            this.Load += new System.EventHandler(this.MusicList_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinTreeView skinTreeView1;
    }
}