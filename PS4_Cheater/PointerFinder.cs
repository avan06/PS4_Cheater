﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PS4_Cheater
{

    public partial class PointerFinder : Form
    {
        private PointerList pointerList = new PointerList();
        private ProcessManager processManager = null;
        private ulong address = 0;
        private DataGridView cheatList;
        private List<PointerResult> pointerResults  = new List<PointerResult>();
        private main mainForm;
        private MemoryHelper MemoryHelper;
        private delegate void NextFinderHandler(PointerList pointerList, PointerFinderWorkerArgs pointerFinderWorkerArgs);
        private event NextFinderHandler NextFinderEvent;
        private List<int> mappedSections;
        private DateTime timeStart;

        public PointerFinder(main mainForm, ulong address, string dataType, ProcessManager processManager, DataGridView cheat_list_view)
        {
            MemoryHelper = new MemoryHelper(true, 0);
            MemoryHelper.InitMemoryHandler(dataType, CONSTANT.EXACT_VALUE, true);

            this.mainForm = mainForm;
            this.address = address;
            this.cheatList = cheat_list_view;
            this.processManager = processManager;

            InitializeComponent();
        }

        private void PointerFinder_Load(object sender, EventArgs e)
        {
            address_box.Text = address.ToString("X");
            for (int i = 1; i <= 20; ++i)
            {
                level_updown.Items.Add(i);
            }
            level_updown.SelectedIndex = 9;
            pointerList.NewPathGeneratedEvent += PointerList_NewPathGeneratedEvent;
            NextFinderEvent += NextFinderWorkerEvent;
        }

        private void PointerList_NewPathGeneratedEvent(PointerList pointerList, List<long> path_offset, List<Pointer> path_address)
        {
            if (path_address.Count > 0)
            {

                int baseSectionID = processManager.MappedSectionList.GetMappedSectionID(path_address[path_offset.Count - 1].Address);

                if (fast_scan && !processManager.MappedSectionList[baseSectionID].Name.StartsWith("executable"))
                {
                    return;
                }

                PointerFinderWorkerListViewUpdate view_info = new PointerFinderWorkerListViewUpdate(pointerList, path_offset, path_address, baseSectionID);
                pointer_finder_worker.ReportProgress(95, view_info);
            }
        }

        void set_controls(bool enable)
        {
            level_updown.Enabled = enable;
            address_box.Enabled = enable;
            find_btn.Enabled = enable;
        }

        class PointerFinderWorkerArgs
        {
            public ulong Address { get; set; }
            public List<int> Range { get; set; }

            public PointerFinderWorkerArgs(ulong address, List<int> range)
            {
                this.Address = address;
                this.Range = range;
            }
        }

        class PointerFinderWorkerListViewUpdate
        {
            public PointerList PointerList { get; set; }
            public List<long> PathOffset { get; set; }
            public List<Pointer> PathAddress { get; set; }

            public int SectionID { get; set; }

            public PointerFinderWorkerListViewUpdate(PointerList pointerList, List<long> path_offset, List<Pointer> path_address, int SectionID)
            {
                this.PointerList = pointerList;
                this.PathOffset = new List<long>(path_offset);
                this.PathAddress = new List<Pointer>(path_address);
                this.SectionID = SectionID;
            }
        }

        static int result_counter = 0;

        private void find_btn_Click(object sender, EventArgs e)
        {
            if (find_btn.Text == "First Scan")
            {
                if (MessageBox.Show("Perform First Scan?", "First Scan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
                timeStart = DateTime.Now;
                mappedSections = new List<int>();
                ulong address = ulong.Parse(address_box.Text, System.Globalization.NumberStyles.HexNumber);
                int level = level_updown.SelectedIndex + 1;
                pointerResults.Clear();
                pointerList.Clear();
                result_counter = 0;
                pointerList.Stop = false;
                List<int> range = new List<int>();

                int i = 0;

                for (i = 0; i < level; ++i)
                {
                    range.Add(8 * 1024);
                }

                pointer_list_view.Rows.Clear();
                pointer_list_view.Columns.Clear();

                for (i = 0; i < level; ++i)
                {
                    pointer_list_view.Columns.Add("Offset " + (i + 1), "Offset " + (i + 1));

                    pointer_list_view.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                if (level > 0)
                {

                    pointer_list_view.Columns.Add("Base Address", "Base Address");
                    pointer_list_view.Columns.Add("Base Section", "Base Section");

                    pointer_list_view.Columns[level + 0].SortMode = DataGridViewColumnSortMode.NotSortable;
                    pointer_list_view.Columns[level + 1].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                find_btn.Text = "Stop";
                pointer_finder_worker.RunWorkerAsync(new PointerFinderWorkerArgs(address, range));

                //DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs(new PointerFinderWorkerArgs(address, range));
                //pointer_finder_worker_DoWork(null, doWorkEventArgs);
            }
            else
            {
                pointerList.Stop = true;
                pointer_finder_worker.CancelAsync();
                find_btn.Text = "First Scan";
                next_btn.Enabled = false;
            }
        }

        private void next_btn_Click(object sender, EventArgs e)
        {
            if (next_btn.Text == "Next Scan")
            {
                if (MessageBox.Show("Verify if you're connected!", "Next Scan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
                ulong address = ulong.Parse(address_box.Text, System.Globalization.NumberStyles.HexNumber);
                result_counter = 0;
                next_btn.Text = "Stop";
                next_pointer_finder_worker.RunWorkerAsync(new PointerFinderWorkerArgs(address, null));
            } else
            {
                next_pointer_finder_worker.CancelAsync();
                next_btn.Text = "Next Scan";
            }
            //DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs(new PointerFinderWorkerArgs(address, null));
            //next_pointer_finder_worker_DoWork(null, doWorkEventArgs);
        }

        private void pointer_finder_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            pointer_finder_worker.ReportProgress(0);
            for (int section_idx = 0; section_idx < processManager.MappedSectionList.Count; ++section_idx)
            {
                if (pointer_finder_worker.CancellationPending) break;
                MappedSection mappedSection = processManager.MappedSectionList[section_idx];
                if (isFilterBox.Checked && Util.SectionIsFilter(mappedSection.Name)) continue;
                if (!isFilterBox.Checked && mappedSection.Name.StartsWith("libSce")) continue;
                mappedSection.PointerSearchInit(processManager, MemoryHelper, pointerList);
                pointer_finder_worker.ReportProgress((int)(((float)section_idx / processManager.MappedSectionList.Count) * 80),
                    "Init scanning pointer..." + pointerList.Count + " (" + (section_idx + 1) + "/" + processManager.MappedSectionList.Count + "), " + mappedSection.ToString());
            }

            if (pointer_finder_worker.CancellationPending) return;

            pointer_finder_worker.ReportProgress(80);
            pointerList.Init();
            pointer_finder_worker.ReportProgress(90);

            PointerFinderWorkerArgs pointerFinderWorkerArgs = (PointerFinderWorkerArgs)e.Argument;
            pointerList.FindPointerList(pointerFinderWorkerArgs.Address, pointerFinderWorkerArgs.Range);
            pointer_finder_worker.ReportProgress(100);
        }

        /// <param name="e">UserState will be passed by PointerList_NewPathGeneratedEvent, it will be invoke when PointerList.FindPointerList => PointerList.PointerFinder is finished</param>
        private void pointer_finder_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress_bar.Value = e.ProgressPercentage;
            if (e.UserState == null)
            {
                return;
            }
            if (e.UserState is string)
            {
                msg.Text = (string)e.UserState;
            }
            if (e.UserState is PointerFinderWorkerListViewUpdate)
            {
                PointerFinderWorkerListViewUpdate pointerFinderWorkerListViewUpdate = (PointerFinderWorkerListViewUpdate)e.UserState;

                List<long> pathOffset = pointerFinderWorkerListViewUpdate.PathOffset;
                List<Pointer> pathAddress = pointerFinderWorkerListViewUpdate.PathAddress;
                int baseSectionID = pointerFinderWorkerListViewUpdate.SectionID;
                try
                {
                    ulong baseOffset = pathAddress[pathOffset.Count - 1].Address - processManager.MappedSectionList[baseSectionID].Start;

                    PointerResult pointerResult = new PointerResult(baseSectionID, baseOffset, pathOffset);
                    pointerResults.Add(pointerResult);

                    if (result_counter < 2000)
                    {
                        int row_index = pointer_list_view.Rows.Add();
                        DataGridViewCellCollection row = pointer_list_view.Rows[row_index].Cells;

                        for (int i = 0; i < pathOffset.Count; ++i)
                        {
                            row[i].Value = (pathOffset[i].ToString("X"));                           //offset
                            int sectionID = processManager.MappedSectionList.GetMappedSectionID(pathAddress[i].Address);
                        }

                        if (pathOffset.Count > 0)
                        {
                            row[row.Count - 2].Value = (pathAddress[pathAddress.Count - 1].Address.ToString("X"));                  //address
                            int sectionID = processManager.MappedSectionList.GetMappedSectionID(pathAddress[pathAddress.Count - 1].Address);
                            row[row.Count - 1].Value = (processManager.MappedSectionList.GetSectionName(sectionID));               //section
                        }
                    }

                    ++result_counter;

                    if (result_counter % 1024 == 0)
                    {
                        msg.Text = result_counter.ToString() + " results";
                    }
                    if (isFastNextScanBox.Checked)
                    {
                        for (int idx = 0; idx < pathAddress.Count; ++idx)
                        {
                            if (!mappedSections.Contains(pathAddress[idx].SectionID))
                            {
                                mappedSections.Add(pathAddress[idx].SectionID);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void pointer_finder_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string elapsed = ((TimeSpan)(DateTime.Now - timeStart)).TotalMilliseconds.ToString();
            msg.Text = result_counter.ToString() + " results, elapsed:" + elapsed;
            find_btn.Text = "First Scan";
            next_btn.Enabled = true;
        }

        private void next_pointer_finder_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            PointerFinderWorkerArgs pointerFinderWorkerArgs = (PointerFinderWorkerArgs)e.Argument;
            result_counter = 0;
            pointerList.Clear();
            next_pointer_finder_worker.ReportProgress(0);
            for (int section_idx = 0; section_idx < processManager.MappedSectionList.Count; ++section_idx)
            {
                if (next_pointer_finder_worker.CancellationPending) break;
                if (isFastNextScanBox.Checked && mappedSections.Count > 0 && !mappedSections.Contains(section_idx)) continue;
                
                MappedSection mappedSection = processManager.MappedSectionList[section_idx];
                if (isFilterBox.Checked && Util.SectionIsFilter(mappedSection.Name)) continue;
                if (!isFilterBox.Checked && mappedSection.Name.StartsWith("libSce")) continue;
                mappedSection.PointerSearchInit(processManager, MemoryHelper, pointerList);
                next_pointer_finder_worker.ReportProgress((int)(((float)section_idx / processManager.MappedSectionList.Count) * 65),
                    "Init scanning pointer..." + pointerList.Count + " (" + (section_idx + 1) + "/" + processManager.MappedSectionList.Count + "), " + mappedSection.ToString());
            }

            if (next_pointer_finder_worker.CancellationPending) return;

            next_pointer_finder_worker.ReportProgress(65);
            pointerList.Init();
            next_pointer_finder_worker.ReportProgress(70);

            NextFinderEvent?.Invoke(pointerList, pointerFinderWorkerArgs);
        }

        /// <summary>
        /// Invoke thread-safe calls with a delegate event
        /// </summary>
        private void NextFinderWorkerEvent(PointerList pointerList, PointerFinderWorkerArgs pointerFinderWorkerArgs)
        {
            if (InvokeRequired)
            {
                NextFinderHandler ng = new NextFinderHandler(NextFinderWorkerEvent);
                this.Invoke(ng, pointerList, pointerFinderWorkerArgs);
            }
            else
            {
                List<PointerResult> newPointerResultList = new List<PointerResult>();
                pointer_list_view.Rows.Clear();

                for (int i = 0; i < pointerResults.Count; ++i)
                {
                    if (i % 100 == 0)
                    {
                        next_pointer_finder_worker.ReportProgress((int)(30 * (float)(i) / pointerResults.Count) + 70);
                    }

                    PointerResult pointerResult = pointerResults[i];
                    ulong tailAddress = pointerList.GetTailAddress(pointerResult, processManager.MappedSectionList);
                    if (tailAddress == pointerFinderWorkerArgs.Address)
                    {
                        newPointerResultList.Add(pointerResult);
                        ++result_counter;

                        if (result_counter < 2000)
                        {
                            int row_index = pointer_list_view.Rows.Add();
                            DataGridViewCellCollection row = pointer_list_view.Rows[row_index].Cells;

                            for (int j = 0; j < pointerResult.Offsets.Length; ++j)
                            {
                                row[j].Value = (pointerResult.Offsets[j].ToString("X"));                           //offset
                            }

                            if (pointerResult.Offsets.Length > 0)
                            {
                                row[row.Count - 2].Value = (pointerResult.GetBaseAddress(processManager.MappedSectionList).ToString("X"));   //address
                                row[row.Count - 1].Value = (processManager.MappedSectionList.GetSectionName(pointerResult.BaseSectionID));   //section
                            }
                        }
                    }
                }
                if (newPointerResultList.Count == 0 && MessageBox.Show("Whether to continue?", "Next Scan results are zero", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes ||
                    newPointerResultList.Count > 0)
                {
                    pointerResults = newPointerResultList;
                }

                next_pointer_finder_worker.ReportProgress(100);
            }

        }

        private void next_pointer_finder_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null && e.UserState is string)
            {
                msg.Text = (string)e.UserState;
            } else if (result_counter % 1024 == 0)
            {
                msg.Text = result_counter.ToString() + " results";
            }
            progress_bar.Value = e.ProgressPercentage;
        }

        private void next_pointer_finder_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            msg.Text = pointerResults.Count.ToString() + " results";
            next_btn.Text = "Next Scan";
        }

        private void pointer_list_view_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            PointerResult pointerResult = pointerResults[e.RowIndex];

            ulong baseAddress = pointerResult.GetBaseAddress(processManager.MappedSectionList);
            ulong tailAddress = pointerList.GetTailAddress(pointerResult, processManager.MappedSectionList);
            string data = MemoryHelper.BytesToString(MemoryHelper.GetBytesByType(tailAddress));
            string dataType = MemoryHelper.GetStringOfValueType(MemoryHelper.ValueType);
            string description = "";
            for (int oIdx = 0; oIdx < pointerResult.Offsets.Length; ++oIdx)
            {
                description += "+" + pointerResult.Offsets[oIdx].ToString("X");
            }
            description = (string)baseAddress.ToString("X") + description;
            mainForm.new_pointer_cheat(baseAddress, pointerResult.Offsets.ToList(), dataType, data, false, description);
        }

        private bool fast_scan = true;

        private void fast_scan_box_CheckedChanged(object sender, EventArgs e)
        {
            fast_scan = fast_scan_box.Checked;
        }

        private void pointer_list_view_add_to_cheat_list_Click(object sender, EventArgs e)
        {
            if (pointer_list_view.SelectedRows == null)
                return;

            DataGridViewSelectedRowCollection items = pointer_list_view.SelectedRows;
            for (int idx = 0; idx < items.Count; ++idx)
            {
                try
                {
                    DataGridViewRow pointerRow = items[idx];
                    PointerResult pointerResult = pointerResults[pointerRow.Index];


                    ulong baseAddress = ulong.Parse((string)pointerRow.Cells[pointerRow.Cells.Count - 2].Value, NumberStyles.HexNumber); //"DC2630"
                    ulong tailAddress = pointerList.GetTailAddress(pointerResult, processManager.MappedSectionList);
                    string data = MemoryHelper.BytesToString(MemoryHelper.GetBytesByType(tailAddress));
                    string dataType = MemoryHelper.GetStringOfValueType(MemoryHelper.ValueType);
                    string description = "";
                    for (int oIdx = 0; oIdx < pointerResult.Offsets.Length; ++oIdx)
                    {
                        description += "+" + pointerResult.Offsets[oIdx].ToString("X");
                    }
                    description = (string)baseAddress.ToString("X") + description;
                    mainForm.new_pointer_cheat(baseAddress, pointerResult.Offsets.ToList(), dataType, data, false, description);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void filterRuleBtn_Click(object sender, EventArgs e)
        {
            string sectionFilterKeys = Config.getSetting("sectionFilterKeys");
            if (InputBox.Show("Section Filter", "Enter the value of the filter keys", ref sectionFilterKeys, null) != DialogResult.OK)
            {
                return;
            }
            Config.updateSeeting("sectionFilterKeys", sectionFilterKeys);
        }
    }

    public class PointerResult
    {
        public int BaseSectionID { get; }
        public ulong BaseOffset { get; }
        public long[] Offsets { get; }

        public PointerResult(int BaseSectionID, ulong BaseOffset, List<long> Offsets)
        {
            this.BaseSectionID = BaseSectionID;
            this.BaseOffset = BaseOffset;
            this.Offsets = new long[Offsets.Count];
            for (int i = 0; i < this.Offsets.Length; ++i)
            {
                this.Offsets[i] = Offsets[this.Offsets.Length - 1 - i];
            }

        }

        public ulong GetBaseAddress(MappedSectionList mappedSectionList)
        {
            if (BaseSectionID >= mappedSectionList.Count)
                return 0;

            MappedSection section = mappedSectionList[BaseSectionID];

            return section.Start + BaseOffset;
        }
    }
}
