namespace PS4_Cheater
{
    partial class PointerFinder
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
            this.components = new System.ComponentModel.Container();
            this.address_box = new System.Windows.Forms.TextBox();
            this.find_btn = new System.Windows.Forms.Button();
            this.level_updown = new System.Windows.Forms.DomainUpDown();
            this.pointer_finder_worker = new System.ComponentModel.BackgroundWorker();
            this.pointer_list_view = new System.Windows.Forms.DataGridView();
            this.pointer_list_menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pointer_list_view_add_to_cheat_list = new System.Windows.Forms.ToolStripMenuItem();
            this.next_btn = new System.Windows.Forms.Button();
            this.next_pointer_finder_worker = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.fast_scan_box = new System.Windows.Forms.CheckBox();
            this.isFilterBox = new System.Windows.Forms.CheckBox();
            this.isFastNextScanBox = new System.Windows.Forms.CheckBox();
            this.filterRuleBtn = new System.Windows.Forms.Button();
            this.status_strip = new System.Windows.Forms.StatusStrip();
            this.progress_bar = new System.Windows.Forms.ToolStripProgressBar();
            this.msg = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pointer_list_view)).BeginInit();
            this.pointer_list_menu.SuspendLayout();
            this.status_strip.SuspendLayout();
            this.SuspendLayout();
            // 
            // address_box
            // 
            this.address_box.Location = new System.Drawing.Point(194, 10);
            this.address_box.Name = "address_box";
            this.address_box.Size = new System.Drawing.Size(133, 22);
            this.address_box.TabIndex = 3;
            // 
            // find_btn
            // 
            this.find_btn.Location = new System.Drawing.Point(333, 10);
            this.find_btn.Name = "find_btn";
            this.find_btn.Size = new System.Drawing.Size(96, 23);
            this.find_btn.TabIndex = 4;
            this.find_btn.Text = "First Scan";
            this.find_btn.UseVisualStyleBackColor = true;
            this.find_btn.Click += new System.EventHandler(this.find_btn_Click);
            // 
            // level_updown
            // 
            this.level_updown.Location = new System.Drawing.Point(17, 9);
            this.level_updown.Name = "level_updown";
            this.level_updown.ReadOnly = true;
            this.level_updown.Size = new System.Drawing.Size(120, 22);
            this.level_updown.TabIndex = 6;
            // 
            // pointer_finder_worker
            // 
            this.pointer_finder_worker.WorkerReportsProgress = true;
            this.pointer_finder_worker.WorkerSupportsCancellation = true;
            this.pointer_finder_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.pointer_finder_worker_DoWork);
            this.pointer_finder_worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.pointer_finder_worker_ProgressChanged);
            this.pointer_finder_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.pointer_finder_worker_RunWorkerCompleted);
            // 
            // pointer_list_view
            // 
            this.pointer_list_view.AllowUserToAddRows = false;
            this.pointer_list_view.AllowUserToDeleteRows = false;
            this.pointer_list_view.AllowUserToResizeRows = false;
            this.pointer_list_view.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pointer_list_view.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pointer_list_view.ContextMenuStrip = this.pointer_list_menu;
            this.pointer_list_view.Location = new System.Drawing.Point(0, 39);
            this.pointer_list_view.Name = "pointer_list_view";
            this.pointer_list_view.ReadOnly = true;
            this.pointer_list_view.RowTemplate.Height = 23;
            this.pointer_list_view.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.pointer_list_view.Size = new System.Drawing.Size(851, 362);
            this.pointer_list_view.TabIndex = 7;
            this.pointer_list_view.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.pointer_list_view_CellDoubleClick);
            // 
            // pointer_list_menu
            // 
            this.pointer_list_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pointer_list_view_add_to_cheat_list});
            this.pointer_list_menu.Name = "pointer_list_menu";
            this.pointer_list_menu.Size = new System.Drawing.Size(171, 26);
            // 
            // pointer_list_view_add_to_cheat_list
            // 
            this.pointer_list_view_add_to_cheat_list.Name = "pointer_list_view_add_to_cheat_list";
            this.pointer_list_view_add_to_cheat_list.Size = new System.Drawing.Size(170, 22);
            this.pointer_list_view_add_to_cheat_list.Text = "Add to Cheat List";
            this.pointer_list_view_add_to_cheat_list.Click += new System.EventHandler(this.pointer_list_view_add_to_cheat_list_Click);
            // 
            // next_btn
            // 
            this.next_btn.Enabled = false;
            this.next_btn.Location = new System.Drawing.Point(435, 10);
            this.next_btn.Name = "next_btn";
            this.next_btn.Size = new System.Drawing.Size(96, 23);
            this.next_btn.TabIndex = 8;
            this.next_btn.Text = "Next Scan";
            this.next_btn.UseVisualStyleBackColor = true;
            this.next_btn.Click += new System.EventHandler(this.next_btn_Click);
            // 
            // next_pointer_finder_worker
            // 
            this.next_pointer_finder_worker.WorkerReportsProgress = true;
            this.next_pointer_finder_worker.WorkerSupportsCancellation = true;
            this.next_pointer_finder_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.next_pointer_finder_worker_DoWork);
            this.next_pointer_finder_worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.next_pointer_finder_worker_ProgressChanged);
            this.next_pointer_finder_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.next_pointer_finder_worker_RunWorkerCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(143, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "Address:";
            // 
            // fast_scan_box
            // 
            this.fast_scan_box.AutoSize = true;
            this.fast_scan_box.Checked = true;
            this.fast_scan_box.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fast_scan_box.Location = new System.Drawing.Point(604, 12);
            this.fast_scan_box.Name = "fast_scan_box";
            this.fast_scan_box.Size = new System.Drawing.Size(64, 16);
            this.fast_scan_box.TabIndex = 10;
            this.fast_scan_box.Text = "FastScan";
            this.fast_scan_box.UseVisualStyleBackColor = true;
            this.fast_scan_box.CheckedChanged += new System.EventHandler(this.fast_scan_box_CheckedChanged);
            // 
            // isFilterBox
            // 
            this.isFilterBox.AutoSize = true;
            this.isFilterBox.Location = new System.Drawing.Point(766, 12);
            this.isFilterBox.Name = "isFilterBox";
            this.isFilterBox.Size = new System.Drawing.Size(48, 16);
            this.isFilterBox.TabIndex = 11;
            this.isFilterBox.Text = "Filter";
            this.isFilterBox.UseVisualStyleBackColor = true;
            // 
            // isFastNextScanBox
            // 
            this.isFastNextScanBox.AutoSize = true;
            this.isFastNextScanBox.Location = new System.Drawing.Point(674, 12);
            this.isFastNextScanBox.Name = "isFastNextScanBox";
            this.isFastNextScanBox.Size = new System.Drawing.Size(86, 16);
            this.isFastNextScanBox.TabIndex = 12;
            this.isFastNextScanBox.Text = "FastNextScan";
            this.isFastNextScanBox.UseVisualStyleBackColor = true;
            // 
            // filterRuleBtn
            // 
            this.filterRuleBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterRuleBtn.Location = new System.Drawing.Point(809, 9);
            this.filterRuleBtn.Name = "filterRuleBtn";
            this.filterRuleBtn.Size = new System.Drawing.Size(35, 20);
            this.filterRuleBtn.TabIndex = 13;
            this.filterRuleBtn.TabStop = false;
            this.filterRuleBtn.Text = "Rule";
            this.filterRuleBtn.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.filterRuleBtn.UseVisualStyleBackColor = true;
            this.filterRuleBtn.Click += new System.EventHandler(this.filterRuleBtn_Click);
            // 
            // status_strip
            // 
            this.status_strip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progress_bar,
            this.msg});
            this.status_strip.Location = new System.Drawing.Point(0, 399);
            this.status_strip.Name = "status_strip";
            this.status_strip.Size = new System.Drawing.Size(851, 22);
            this.status_strip.TabIndex = 2;
            this.status_strip.Text = "statusStrip1";
            // 
            // progress_bar
            // 
            this.progress_bar.Name = "progress_bar";
            this.progress_bar.Size = new System.Drawing.Size(600, 16);
            // 
            // msg
            // 
            this.msg.Name = "msg";
            this.msg.Size = new System.Drawing.Size(234, 17);
            this.msg.Spring = true;
            this.msg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PointerFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 421);
            this.Controls.Add(this.filterRuleBtn);
            this.Controls.Add(this.isFastNextScanBox);
            this.Controls.Add(this.isFilterBox);
            this.Controls.Add(this.fast_scan_box);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.next_btn);
            this.Controls.Add(this.pointer_list_view);
            this.Controls.Add(this.level_updown);
            this.Controls.Add(this.find_btn);
            this.Controls.Add(this.address_box);
            this.Controls.Add(this.status_strip);
            this.Name = "PointerFinder";
            this.Text = "Pointer Finder";
            this.Load += new System.EventHandler(this.PointerFinder_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pointer_list_view)).EndInit();
            this.pointer_list_menu.ResumeLayout(false);
            this.status_strip.ResumeLayout(false);
            this.status_strip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox address_box;
        private System.Windows.Forms.Button find_btn;
        private System.Windows.Forms.DomainUpDown level_updown;
        private System.ComponentModel.BackgroundWorker pointer_finder_worker;
        private System.Windows.Forms.DataGridView pointer_list_view;
        private System.Windows.Forms.Button next_btn;
        private System.ComponentModel.BackgroundWorker next_pointer_finder_worker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox fast_scan_box;
        private System.Windows.Forms.CheckBox isFilterBox;
        private System.Windows.Forms.ContextMenuStrip pointer_list_menu;
        private System.Windows.Forms.ToolStripMenuItem pointer_list_view_add_to_cheat_list;
        private System.Windows.Forms.CheckBox isFastNextScanBox;
        private System.Windows.Forms.Button filterRuleBtn;
        private System.Windows.Forms.StatusStrip status_strip;
        private System.Windows.Forms.ToolStripProgressBar progress_bar;
        private System.Windows.Forms.ToolStripStatusLabel msg;
    }
}