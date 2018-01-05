namespace DownloadHelper.Sample
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ts = new System.Windows.Forms.ToolStrip();
            this.tsb_add = new System.Windows.Forms.ToolStripButton();
            this.tsb_remove = new System.Windows.Forms.ToolStripButton();
            this.lv = new System.Windows.Forms.ListView();
            this.vlch_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.vlch_added = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.vlch_status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.vlch_completed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_pause = new System.Windows.Forms.ToolStripButton();
            this.tsb_start = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_options = new System.Windows.Forms.ToolStripButton();
            this.ts.SuspendLayout();
            this.SuspendLayout();
            // 
            // ts
            // 
            this.ts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_add,
            this.tsb_remove,
            this.toolStripSeparator1,
            this.tsb_start,
            this.tsb_pause,
            this.tsb_options,
            this.toolStripSeparator2});
            this.ts.Location = new System.Drawing.Point(0, 0);
            this.ts.Name = "ts";
            this.ts.Size = new System.Drawing.Size(658, 25);
            this.ts.TabIndex = 0;
            this.ts.Text = "toolStrip1";
            // 
            // tsb_add
            // 
            this.tsb_add.Image = ((System.Drawing.Image)(resources.GetObject("tsb_add.Image")));
            this.tsb_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_add.Name = "tsb_add";
            this.tsb_add.Size = new System.Drawing.Size(49, 22);
            this.tsb_add.Text = "Add";
            // 
            // tsb_remove
            // 
            this.tsb_remove.Image = ((System.Drawing.Image)(resources.GetObject("tsb_remove.Image")));
            this.tsb_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_remove.Name = "tsb_remove";
            this.tsb_remove.Size = new System.Drawing.Size(70, 22);
            this.tsb_remove.Text = "Remove";
            // 
            // lv
            // 
            this.lv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.vlch_name,
            this.vlch_added,
            this.vlch_status,
            this.vlch_completed});
            this.lv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv.Location = new System.Drawing.Point(0, 25);
            this.lv.Name = "lv";
            this.lv.Size = new System.Drawing.Size(658, 299);
            this.lv.TabIndex = 1;
            this.lv.UseCompatibleStateImageBehavior = false;
            this.lv.View = System.Windows.Forms.View.Details;
            // 
            // vlch_name
            // 
            this.vlch_name.Text = "Name";
            this.vlch_name.Width = 300;
            // 
            // vlch_added
            // 
            this.vlch_added.Text = "Added";
            this.vlch_added.Width = 100;
            // 
            // vlch_status
            // 
            this.vlch_status.Text = "Status";
            this.vlch_status.Width = 100;
            // 
            // vlch_completed
            // 
            this.vlch_completed.Text = "Completed";
            this.vlch_completed.Width = 100;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_pause
            // 
            this.tsb_pause.Image = ((System.Drawing.Image)(resources.GetObject("tsb_pause.Image")));
            this.tsb_pause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_pause.Name = "tsb_pause";
            this.tsb_pause.Size = new System.Drawing.Size(58, 22);
            this.tsb_pause.Text = "Pause";
            // 
            // tsb_start
            // 
            this.tsb_start.Image = ((System.Drawing.Image)(resources.GetObject("tsb_start.Image")));
            this.tsb_start.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_start.Name = "tsb_start";
            this.tsb_start.Size = new System.Drawing.Size(51, 22);
            this.tsb_start.Text = "Start";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_options
            // 
            this.tsb_options.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsb_options.Image = ((System.Drawing.Image)(resources.GetObject("tsb_options.Image")));
            this.tsb_options.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_options.Name = "tsb_options";
            this.tsb_options.Size = new System.Drawing.Size(69, 22);
            this.tsb_options.Text = "Options";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 324);
            this.Controls.Add(this.lv);
            this.Controls.Add(this.ts);
            this.Name = "MainForm";
            this.Text = "Downloads";
            this.ts.ResumeLayout(false);
            this.ts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip ts;
        private System.Windows.Forms.ToolStripButton tsb_add;
        private System.Windows.Forms.ToolStripButton tsb_remove;
        private System.Windows.Forms.ListView lv;
        private System.Windows.Forms.ColumnHeader vlch_name;
        private System.Windows.Forms.ColumnHeader vlch_added;
        private System.Windows.Forms.ColumnHeader vlch_status;
        private System.Windows.Forms.ColumnHeader vlch_completed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsb_start;
        private System.Windows.Forms.ToolStripButton tsb_pause;
        private System.Windows.Forms.ToolStripButton tsb_options;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

