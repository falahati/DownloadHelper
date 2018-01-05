namespace DownloadHelper.Sample
{
    partial class DownloadForm
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
            this.gb_details = new System.Windows.Forms.GroupBox();
            this.btn_action = new System.Windows.Forms.Button();
            this.lbl_speed = new System.Windows.Forms.Label();
            this.lbl_status = new System.Windows.Forms.Label();
            this.lbl_progress = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_file = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_url = new System.Windows.Forms.Label();
            this.lv_connections = new System.Windows.Forms.ListView();
            this.lvch_number = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvch_status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvch_speed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvch_progress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvch_downloaded = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.gb_connections = new System.Windows.Forms.GroupBox();
            this.gb_details.SuspendLayout();
            this.gb_connections.SuspendLayout();
            this.SuspendLayout();
            // 
            // gb_details
            // 
            this.gb_details.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_details.Controls.Add(this.btn_action);
            this.gb_details.Controls.Add(this.lbl_speed);
            this.gb_details.Controls.Add(this.lbl_status);
            this.gb_details.Controls.Add(this.lbl_progress);
            this.gb_details.Controls.Add(this.label5);
            this.gb_details.Controls.Add(this.label4);
            this.gb_details.Controls.Add(this.label3);
            this.gb_details.Controls.Add(this.lbl_file);
            this.gb_details.Controls.Add(this.label2);
            this.gb_details.Controls.Add(this.label1);
            this.gb_details.Controls.Add(this.lbl_url);
            this.gb_details.Location = new System.Drawing.Point(12, 12);
            this.gb_details.Name = "gb_details";
            this.gb_details.Size = new System.Drawing.Size(458, 107);
            this.gb_details.TabIndex = 0;
            this.gb_details.TabStop = false;
            this.gb_details.Text = "Details";
            // 
            // btn_action
            // 
            this.btn_action.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_action.Location = new System.Drawing.Point(377, 79);
            this.btn_action.Name = "btn_action";
            this.btn_action.Size = new System.Drawing.Size(75, 23);
            this.btn_action.TabIndex = 10;
            this.btn_action.Text = "{ACTION}";
            this.btn_action.UseVisualStyleBackColor = true;
            this.btn_action.Click += new System.EventHandler(this.btn_action_Click);
            // 
            // lbl_speed
            // 
            this.lbl_speed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_speed.Location = new System.Drawing.Point(74, 84);
            this.lbl_speed.Name = "lbl_speed";
            this.lbl_speed.Size = new System.Drawing.Size(297, 13);
            this.lbl_speed.TabIndex = 9;
            this.lbl_speed.Text = "{AVERAGESPEED} ({SPEED}) [{LIMITEDSPEED}]";
            // 
            // lbl_status
            // 
            this.lbl_status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_status.Location = new System.Drawing.Point(74, 67);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(297, 13);
            this.lbl_status.TabIndex = 8;
            this.lbl_status.Text = "{DOWNLOADSTATUS}";
            // 
            // lbl_progress
            // 
            this.lbl_progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_progress.Location = new System.Drawing.Point(74, 50);
            this.lbl_progress.Name = "lbl_progress";
            this.lbl_progress.Size = new System.Drawing.Size(378, 13);
            this.lbl_progress.TabIndex = 7;
            this.lbl_progress.Text = "{WRRITEN} / {SIZE} {PERCENT}%";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Speed:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Status:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Progress:";
            // 
            // lbl_file
            // 
            this.lbl_file.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_file.Location = new System.Drawing.Point(74, 33);
            this.lbl_file.Name = "lbl_file";
            this.lbl_file.Size = new System.Drawing.Size(378, 13);
            this.lbl_file.TabIndex = 3;
            this.lbl_file.Text = "{FULLFILENAME}";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File Address:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Url:";
            // 
            // lbl_url
            // 
            this.lbl_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_url.Location = new System.Drawing.Point(74, 16);
            this.lbl_url.Name = "lbl_url";
            this.lbl_url.Size = new System.Drawing.Size(378, 13);
            this.lbl_url.TabIndex = 0;
            this.lbl_url.Text = "{DOWNLOADURL}";
            // 
            // lv_connections
            // 
            this.lv_connections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvch_number,
            this.lvch_status,
            this.lvch_speed,
            this.lvch_progress,
            this.lvch_downloaded});
            this.lv_connections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_connections.Location = new System.Drawing.Point(3, 16);
            this.lv_connections.Name = "lv_connections";
            this.lv_connections.Size = new System.Drawing.Size(452, 155);
            this.lv_connections.TabIndex = 1;
            this.lv_connections.UseCompatibleStateImageBehavior = false;
            this.lv_connections.View = System.Windows.Forms.View.Details;
            // 
            // lvch_number
            // 
            this.lvch_number.Text = "#";
            this.lvch_number.Width = 30;
            // 
            // lvch_status
            // 
            this.lvch_status.Text = "Status";
            this.lvch_status.Width = 100;
            // 
            // lvch_speed
            // 
            this.lvch_speed.Text = "Speed";
            this.lvch_speed.Width = 100;
            // 
            // lvch_progress
            // 
            this.lvch_progress.Text = "Progress";
            this.lvch_progress.Width = 100;
            // 
            // lvch_downloaded
            // 
            this.lvch_downloaded.Text = "Downloaded";
            this.lvch_downloaded.Width = 118;
            // 
            // gb_connections
            // 
            this.gb_connections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_connections.Controls.Add(this.lv_connections);
            this.gb_connections.Location = new System.Drawing.Point(12, 125);
            this.gb_connections.Name = "gb_connections";
            this.gb_connections.Size = new System.Drawing.Size(458, 174);
            this.gb_connections.TabIndex = 1;
            this.gb_connections.TabStop = false;
            this.gb_connections.Text = "Connections";
            // 
            // DownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 311);
            this.Controls.Add(this.gb_connections);
            this.Controls.Add(this.gb_details);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(768, 800);
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "DownloadForm";
            this.ShowIcon = false;
            this.Text = "Download {FILENAME}";
            this.gb_details.ResumeLayout(false);
            this.gb_details.PerformLayout();
            this.gb_connections.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gb_details;
        private System.Windows.Forms.ListView lv_connections;
        private System.Windows.Forms.GroupBox gb_connections;
        private System.Windows.Forms.ColumnHeader lvch_number;
        private System.Windows.Forms.ColumnHeader lvch_status;
        private System.Windows.Forms.Button btn_action;
        private System.Windows.Forms.Label lbl_speed;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Label lbl_progress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_file;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_url;
        private System.Windows.Forms.ColumnHeader lvch_speed;
        private System.Windows.Forms.ColumnHeader lvch_progress;
        private System.Windows.Forms.ColumnHeader lvch_downloaded;
    }
}