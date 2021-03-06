﻿namespace PS4_Cheater
{
    using libdebug;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Linq;
    using System.Threading;
    using System.Text.RegularExpressions;

    public partial class main : Form
    {
        private ProcessManager processManager = new ProcessManager();
        private MemoryHelper memoryHelper = new MemoryHelper(true, 0);
        private CheatList cheatList = new CheatList();
        private readonly Dictionary<string, int> versions = new Dictionary<string, int>()
        {
            ["7.02"] = 702,
            ["6.72"] = 672,
            ["5.05"] = 505,
            ["4.55"] = 455,
            ["4.05"] = 405
        };

        private const int CHEAT_LIST_DEL = 0;
        private const int CHEAT_LIST_ADDRESS = 1;
        private const int CHEAT_LIST_TYPE = 2;
        private const int CHEAT_LIST_ENABLED = 3;
        private const int CHEAT_LIST_VALUE = 4;
        private const int CHEAT_LIST_SECTION = 5;
        private const int CHEAT_LIST_LOCK = 6;
        private const int CHEAT_LIST_DESC = 7;

        private const int RESULT_LIST_ADDRESS = 0;
        private const int RESULT_LIST_TYPE = 1;
        private const int RESULT_LIST_VALUE = 2;
        private const int RESULT_LIST_SECTION = 4;

        private string[] SEARCH_BY_FLOAT_FIRST = new string[]
        {
             CONSTANT.EXACT_VALUE,
             CONSTANT.FUZZY_VALUE,
             CONSTANT.BIGGER_THAN,
             CONSTANT.SMALLER_THAN,
             CONSTANT.BETWEEN_VALUE,
             CONSTANT.UNKNOWN_INITIAL_VALUE
        };

        private string[] SEARCH_BY_BYTES_FIRST = new string[]
        {
            CONSTANT.EXACT_VALUE,
            CONSTANT.BIGGER_THAN,
            CONSTANT.SMALLER_THAN,
            CONSTANT.BETWEEN_VALUE,
            CONSTANT.UNKNOWN_INITIAL_VALUE
        };

        private string[] SEARCH_BY_FLOAT_NEXT = new string[]
        {
             CONSTANT.EXACT_VALUE,
             CONSTANT.FUZZY_VALUE,
             CONSTANT.INCREASED_VALUE,
             CONSTANT.INCREASED_VALUE_BY,
             CONSTANT.DECREASED_VALUE,
             CONSTANT.DECREASED_VALUE_BY,
             CONSTANT.BIGGER_THAN,
             CONSTANT.SMALLER_THAN,
             CONSTANT.CHANGED_VALUE,
             CONSTANT.UNCHANGED_VALUE,
             CONSTANT.BETWEEN_VALUE,
        };

        private string[] SEARCH_BY_BYTES_NEXT = new string[]
        {
            CONSTANT.EXACT_VALUE,
            CONSTANT.INCREASED_VALUE,
            CONSTANT.INCREASED_VALUE_BY,
            CONSTANT.DECREASED_VALUE,
            CONSTANT.DECREASED_VALUE_BY,
            CONSTANT.BIGGER_THAN,
            CONSTANT.SMALLER_THAN,
            CONSTANT.CHANGED_VALUE,
            CONSTANT.UNCHANGED_VALUE,
            CONSTANT.BETWEEN_VALUE,
        };

        private string[] SEARCH_BY_HEX = new string[]
        {
            CONSTANT.EXACT_VALUE,
        };
        private string searchSectionTemp = "";

        public main()
        {
            this.InitializeComponent();
        }

        private void main_Load(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, int> ver in versions)
            {
                this.version_list.Items.Add(ver.Key);
            };
            valueTypeList.Items.AddRange(CONSTANT.SEARCH_VALUE_TYPE);
            valueTypeList.SelectedIndex = 2;

            string version = Config.getSetting("ps4 version");
            string ip = Config.getSetting("ip");
            var defaultVer = versions.First();
            if (!string.IsNullOrWhiteSpace(version))
            {
                defaultVer = versions.SingleOrDefault(v => v.Key == version);
            }

            version_list.SelectedIndex = version_list.Items.IndexOf(defaultVer.Key);
            Util.Version = defaultVer.Value;

            if (!string.IsNullOrEmpty(ip))
            {
                ip_box.Text = ip;
            }

            this.next_scan_btn.Text = CONSTANT.NEXT_SCAN;
            this.new_scan_btn.Text = CONSTANT.FIRST_SCAN;
            this.refresh_btn.Text = CONSTANT.REFRESH;
            this.Text += " " + CONSTANT.MAJOR_VERSION + "." + CONSTANT.SECONDARY_VERSION + "." + CONSTANT.THIRD_VERSION;

            if (Config.getSetting("sectionFilterKeys").Length == 0)
            {
                Config.updateSeeting("sectionFilterKeys", CONSTANT.DEFAULT_SECTION_FILTER);
            }
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {

            string ip = Config.getSetting("ip");
            string version = (string)version_list.SelectedItem;

            if (!string.IsNullOrWhiteSpace(version))
            {
                Config.updateSeeting("ps4 version", version);
            }

            if (!string.IsNullOrWhiteSpace(ip_box.Text))
            {
                Config.updateSeeting("ip", ip_box.Text);
            }

            //MemoryHelper.Disconnect();
        }

        class WorkerReturn
        {
            public List<ListViewItem> ListViewItems { get; set; }
            public bool[] MappedSectionCheckeSet { get; set; }
            public ulong Results { get; set; }
        }

        private void update_result_list_view(BackgroundWorker worker, bool refresh, int start, float percent)
        {
            worker.ReportProgress(start);

            List<ListViewItem> listViewItems = new List<ListViewItem>();
            bool[] mappedSectionCheckeSet = new bool[processManager.MappedSectionList.Count];

            ulong totalResultCount = processManager.MappedSectionList.TotalResultCount();
            ulong curResultCount = 0;
            string value_type = MemoryHelper.GetStringOfValueType(memoryHelper.ValueType);

            const int MAX_RESULTS_NUM = 0x1000;

            for (int idx = 0; idx < processManager.MappedSectionList.Count; ++idx)
            {
                MappedSection mapped_section = processManager.MappedSectionList[idx];
                if (!mapped_section.Check)
                {
                    continue;
                }
                List<ResultList> resultLists = memoryHelper.ValueType == ValueType.GROUP_TYPE ? mapped_section.ResultLists : new List<ResultList>() { mapped_section.ResultList };
                if (resultLists == null || resultLists.Count == 0)
                {
                    continue;
                }

                for (int rIdx = 0; rIdx < resultLists.Count; rIdx++)
                {
                    ResultList result_list = resultLists[rIdx];
                    mappedSectionCheckeSet[idx] = result_list.Count > 0;

                    for (result_list.Begin(); !result_list.End(); result_list.Next())
                    {
                        if (curResultCount >= MAX_RESULTS_NUM)
                        {
                            break;
                        }

                        uint memory_address_offset = 0;
                        byte[] memory_value = null;

                        result_list.Get(ref memory_address_offset, ref memory_value);

                        curResultCount++;
                        ListViewItem lvi = new ListViewItem();

                        lvi.Text = String.Format("{0:X}", memory_address_offset + mapped_section.Start);

                        if (refresh && !worker.CancellationPending)
                        {
                            if (memoryHelper.ValueType == ValueType.GROUP_TYPE)
                            {
                                List<MemoryHelper.ScanCommand> scanList = memoryHelper.ScanList;
                                memoryHelper.Length = scanList[rIdx].length;
                                memoryHelper.Alignment = scanList[rIdx].alignment;
                            }
                            memory_value = memoryHelper.GetBytesByType(memory_address_offset + mapped_section.Start);
                            result_list.Set(memory_value);
                            worker.ReportProgress(start + (int)(100.0f * curResultCount / MAX_RESULTS_NUM));
                        }

                        if (memoryHelper.ValueType != ValueType.GROUP_TYPE)
                        {
                            lvi.SubItems.Add(value_type);
                            lvi.SubItems.Add(memoryHelper.BytesToString(memory_value));
                            lvi.SubItems.Add(memoryHelper.BytesToHexString(memory_value));
                        }
                        else
                        {
                            List<MemoryHelper.ScanCommand> scanList = memoryHelper.ScanList;
                            if (rIdx % 2 == 0)
                            {
                                lvi.BackColor = Color.Azure;
                            } else
                            {
                                lvi.BackColor = Color.LightSkyBlue;
                            }
                            lvi.SubItems.Add(MemoryHelper.GetStringOfValueType(scanList[rIdx].scanType));
                            lvi.SubItems.Add(scanList[rIdx].bytesToString(memory_value));
                            lvi.SubItems.Add(scanList[rIdx].bytesToHexString(memory_value));
                        }
                        lvi.SubItems.Add(processManager.MappedSectionList.GetSectionName(idx));

                        listViewItems.Add(lvi);
                    }
                }
            }

            WorkerReturn workerReturn = new WorkerReturn();
            workerReturn.ListViewItems = listViewItems;
            workerReturn.MappedSectionCheckeSet = mappedSectionCheckeSet;
            workerReturn.Results = totalResultCount;

            worker.ReportProgress(start + (int)(100 * percent), workerReturn);
        }


        void setButtons(bool enabled)
        {
            new_scan_btn.Enabled = enabled;
            refresh_btn.Enabled = enabled;
            next_scan_btn.Enabled = enabled;
            processes_comboBox.Enabled = enabled;
            get_processes_btn.Enabled = enabled;
            section_list_menu.Enabled = enabled;
            compareTypeList.Enabled = enabled;
        }

        private void new_scan_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (new_scan_btn.Text == CONSTANT.FIRST_SCAN)
                {
                    if (MessageBox.Show("search size:" + (processManager.MappedSectionList.TotalMemorySize / (1024 * 1024)).ToString() + "MB", "First Scan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    memoryHelper.InitMemoryHandler((string)valueTypeList.SelectedItem,
                        (string)compareTypeList.SelectedItem, alignment_box.Checked, value_box.Text.Length);

                    setButtons(false);
                    new_scan_btn.Enabled = true;
                    valueTypeList.Enabled = false;
                    alignment_box.Enabled = false;
                    isFilterBox.Enabled = false;

                    new_scan_worker.RunWorkerAsync();

                    new_scan_btn.Text = CONSTANT.STOP;
                }
                else if (new_scan_btn.Text == CONSTANT.NEW_SCAN)
                {
                    valueTypeList.Enabled = true;
                    alignment_box.Enabled = true;
                    isFilterBox.Enabled = true;
                    refresh_btn.Enabled = false;
                    next_scan_btn.Enabled = false;
                    new_scan_btn.Text = CONSTANT.FIRST_SCAN;

                    result_list_view.Items.Clear();
                    processManager.MappedSectionList.ClearResultList();
                    memoryHelper.ScanList.Clear();
                    InitCompareTypeListOfFirstScan();
                }
                else if (new_scan_btn.Text == CONSTANT.STOP)
                {
                    new_scan_worker.CancelAsync();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (refresh_btn.Text == "Refresh")
                {
                    setButtons(false);
                    refresh_btn.Enabled = true;
                    refresh_btn.Text = CONSTANT.STOP;
                    update_result_list_worker.RunWorkerAsync();
                }
                else if (refresh_btn.Text == CONSTANT.STOP)
                {
                    update_result_list_worker.CancelAsync();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void next_scan_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (next_scan_btn.Text == "Next Scan")
                {
                    memoryHelper.InitNextScanMemoryHandler((string)compareTypeList.SelectedItem);
                    setButtons(false);
                    next_scan_btn.Enabled = true;
                    next_scan_btn.Text = CONSTANT.STOP;
                    next_scan_worker.RunWorkerAsync();
                }
                else if (next_scan_btn.Text == CONSTANT.STOP)
                {
                    next_scan_worker.CancelAsync();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void set_section_list_view(bool check)
        {
            for (int idx = 0; idx < sectionListView.Items.Count; ++idx)
            {
                sectionListView.Items[idx].Checked = check;
            }
        }

        private void select_all_check_box_Click(object sender, EventArgs e)
        {
            bool check = select_all.Checked;
            set_section_list_view(check);
        }

        private void update_result_list_view_ui(WorkerReturn ret)
        {
            result_list_view.Items.Clear();
            result_list_view.BeginUpdate();
            result_list_view.Items.AddRange(ret.ListViewItems.ToArray());
            result_list_view.EndUpdate();

            for (int idx = 0; idx < sectionListView.Items.Count; ++idx)
            {
                sectionListView.Items[idx].Checked = ret.MappedSectionCheckeSet[int.Parse(sectionListView.Items[idx].Name)];
            }
            msg.Text = ret.Results + " results";
        }
        

        private void scan_worker_DoWorker(object sender, DoWorkEventArgs e, bool isFirstScan)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            string value_0 = value_box.Text;
            string value_1 = value_1_box.Text;
            worker.ReportProgress(0);

            Mutex scan_wait_mutex = new Mutex();
            scan_wait_mutex.WaitOne();

            List<byte[]> buffer_queue = new List<byte[]>(CONSTANT.MAX_PEEK_QUEUE);

            Semaphore consumerMutex = new Semaphore(0, CONSTANT.MAX_PEEK_QUEUE);
            Semaphore producerMutex = new Semaphore(CONSTANT.MAX_PEEK_QUEUE, CONSTANT.MAX_PEEK_QUEUE);

            PeekThread peekThread = new PeekThread(processManager, buffer_queue, consumerMutex, producerMutex);
            ComparerThread comparer_thread = new ComparerThread(processManager, memoryHelper, buffer_queue,
                value_0, value_1, worker, consumerMutex, producerMutex, scan_wait_mutex);

            Thread producer = new Thread(peekThread.Peek);
            producer.Start();

            Thread consumer = null;
            if (isFirstScan)
            {
                consumer = new Thread(comparer_thread.ResultListOfNewScan);
            }
            else
            {
                consumer = new Thread(comparer_thread.ResultListOfNextScan);
            }
            consumer.Start();

            scan_wait_mutex.WaitOne();
            producer.Abort();
            consumer.Abort();

            worker.ReportProgress(80);

            update_result_list_view(worker, false, 80, 0.2f);
        }

        private void next_scan_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            long processed_memory_len = 0;
            ulong total_memory_size = processManager.MappedSectionList.TotalMemorySize + 1;
            string value_0 = value_box.Text;
            string value_1 = value_1_box.Text;
            next_scan_worker.ReportProgress(0);
            for (int section_idx = 0; section_idx < processManager.MappedSectionList.Count; ++section_idx)
            {
                if (next_scan_worker.CancellationPending) break;
                MappedSection mappedSection = processManager.MappedSectionList[section_idx];
                mappedSection.UpdateResultList(processManager, memoryHelper, value_0, value_1, hex_box.Checked, memoryHelper.ValueType == ValueType.GROUP_TYPE, false);
                if (mappedSection.Check) processed_memory_len += mappedSection.Length;
                next_scan_worker.ReportProgress((int)(((float)processed_memory_len / total_memory_size) * 80));
            }
            next_scan_worker.ReportProgress(80);

            update_result_list_view(next_scan_worker, false, 80, 0.2f);
        }

        private void new_scan_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //scan_worker_DoWorker(sender, e, true);
            long processed_memory_len = 0;
            ulong total_memory_size = processManager.MappedSectionList.TotalMemorySize + 1;
            string value_0 = value_box.Text;
            string value_1 = value_1_box.Text;
            new_scan_worker.ReportProgress(0);
            for (int section_idx = 0; section_idx < processManager.MappedSectionList.Count; ++section_idx)
            {
                if (new_scan_worker.CancellationPending) break;
                MappedSection mappedSection = processManager.MappedSectionList[section_idx];
                mappedSection.UpdateResultList(processManager, memoryHelper, value_0, value_1, hex_box.Checked, memoryHelper.ValueType == ValueType.GROUP_TYPE, true);
                if (mappedSection.Check) processed_memory_len += mappedSection.Length;
                new_scan_worker.ReportProgress((int)(((float)processed_memory_len / total_memory_size) * 80));
            }
            new_scan_worker.ReportProgress(80);
            update_result_list_view(new_scan_worker, false, 80, 0.2f);
        }

        private void update_result_list_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            update_result_list_view(update_result_list_worker, true, 0, 1.0f);
        }

        private void new_scan_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                msg.Text = "Peeking memory...";
            }

            if (e.ProgressPercentage == 50)
            {
                msg.Text = "Analysing memory...";
            }

            if (e.ProgressPercentage == 100)
            {
                if (e.UserState is WorkerReturn)
                {
                    update_result_list_view_ui((WorkerReturn)e.UserState);
                }
            }

            progressBar.Value = e.ProgressPercentage;
        }

        private void update_result_list_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                msg.Text = "Processing memory...";
            }

            if (e.ProgressPercentage == 100 && e.UserState is WorkerReturn)
            {
                update_result_list_view_ui((WorkerReturn)e.UserState);
            }

            progressBar.Value = e.ProgressPercentage;
        }

        private void dump_dialog(int sectionID)
        {
            if (sectionID >= 0)
            {
                MappedSection section = processManager.MappedSectionList[sectionID];

                save_file_dialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                save_file_dialog.FilterIndex = 1;
                save_file_dialog.RestoreDirectory = true;
                ListViewItem item = sectionViewItemGetByID(sectionID);
                save_file_dialog.FileName = string.Format("{0}-{1}-{2}-{3}",
                    item.SubItems[0].Text, item.SubItems[1].Text,
                    item.SubItems[2].Text, item.SubItems[3].Text);

                if (save_file_dialog.ShowDialog() == DialogResult.OK)
                {
                    byte[] buffer = memoryHelper.ReadMemory(section.Start, (int)section.Length);

                    FileStream myStream = new FileStream(save_file_dialog.FileName, FileMode.OpenOrCreate);
                    myStream.Write(buffer, 0, buffer.Length);
                    myStream.Close();
                }
            }
        }

        private void result_list_view_hex_item_Click(object sender, EventArgs e)
        {
            if (result_list_view.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = result_list_view.SelectedItems;

                ulong address = ulong.Parse(result_list_view.SelectedItems[0].
                    SubItems[RESULT_LIST_ADDRESS].Text, NumberStyles.HexNumber);
                int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

                if (sectionID >= 0)
                {
                    int offset = 0;

                    MappedSection section = processManager.MappedSectionList[sectionID];

                    offset = (int)(address - section.Start);
                    HexEditor hexEdit = new HexEditor(this, memoryHelper, offset, section);
                    hexEdit.Show(this);
                }
            }
        }


        private void dump_item_Click(object sender, EventArgs e)
        {
            if (result_list_view.SelectedItems.Count == 1)
            {
                ulong address = ulong.Parse(result_list_view.SelectedItems[0].
                    SubItems[RESULT_LIST_ADDRESS].Text, NumberStyles.HexNumber);

                int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

                dump_dialog(sectionID);
            }
        }

        private void sectionHexMenu_Click(object sender, EventArgs e)
        {
            int offset = 0;
            ListView.SelectedListViewItemCollection items = sectionListView.SelectedItems;
            if (items.Count > 0)
            {
                MappedSection section = processManager.MappedSectionList[int.Parse(items[items.Count-1].Name)];
                HexEditor hexEdit = new HexEditor(this, memoryHelper, offset, section);
                hexEdit.Show(this);
            }
        }

        private void sectionDumpMenu_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection items = sectionListView.SelectedItems;
            if (items.Count > 0)
            {
                for (int idx = 0; idx < items.Count; ++idx)
                {
                    dump_dialog(int.Parse(items[idx].Name));
                }
            }
        }

        private void sectionCheckMenu_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection items = sectionListView.SelectedItems;
            if (items.Count > 0)
            {
                for (int idx = 0; idx < items.Count; ++idx)
                {
                    items[idx].Checked = !items[idx].Checked;
                }
            }
        }

        private void sectionListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ListViewItem item = sectionListView.Items[e.Index];
            processManager.MappedSectionList.SectionCheck(int.Parse(item.Name), e.NewValue == CheckState.Checked);

            if (item.Checked)
            {
                item.BackColor = Color.Azure;
            }
            else
            {
                item.BackColor = Color.LightSkyBlue;
            }
        }

        private void sectionSearchBtn_Click(object sender, EventArgs e)
        {
            if (InputBox.Show("Search", "Enter the value of the search processes name", ref searchSectionTemp, null) != DialogResult.OK)
            {
                return;
            }
            int startIndex = 0;
            ListView.SelectedListViewItemCollection items = sectionListView.SelectedItems;
            if (items.Count > 0 && items[items.Count - 1].Index + 1 < sectionListView.Items.Count)
            {
                startIndex = items[items.Count - 1].Index + 1;
            }
            ListViewItem item = sectionListView.FindItemWithText(searchSectionTemp, true, startIndex);
            if (item != null)
            {
                sectionListView.Items[item.Index].Selected = true;
                sectionListView.Items[item.Index].EnsureVisible();
            }
        }

        private ListViewItem sectionViewItemGetByID(int sectionID)
        {
            if (!sectionListView.Items.ContainsKey(sectionID.ToString()))
            {
                return null;
            }
            ListViewItem[] items = sectionListView.Items.Find(sectionID.ToString(), false);
            return items[0];
        }

        void add_new_row_to_cheat_list_view(Cheat cheat)
        {
            int index = this.cheat_list_view.Rows.Add();

            DataGridViewRow cheat_list_view_item = cheat_list_view.Rows[index];
            CheatOperator destination = cheat.GetDestination();
            CheatOperator source = cheat.GetSource();

            cheat_list_view_item.Cells[CHEAT_LIST_ADDRESS].Value = destination.Display();
            cheat_list_view_item.Cells[CHEAT_LIST_TYPE].Value = MemoryHelper.GetStringOfValueType(source.ValueType);
            cheat_list_view_item.Cells[CHEAT_LIST_VALUE].Value = source.Display();
            cheat_list_view_item.Cells[CHEAT_LIST_SECTION].Value = processManager.MappedSectionList.GetSectionName(destination.GetSectionID());
            cheat_list_view_item.Cells[CHEAT_LIST_LOCK].Value = cheat.Lock;
            cheat_list_view_item.Cells[CHEAT_LIST_DESC].Value = cheat.Description;
        }

        public void new_data_cheat(ulong address, string type, string data, bool lock_, string description)
        {
            try
            {
                int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

                if (sectionID == -1)
                {
                    MessageBox.Show("Address is out of range!");
                    return;
                }

                if (cheatList.Exist(address))
                {
                    return;
                }

                ValueType valueType = MemoryHelper.GetValueTypeByString(type);

                DataCheatOperator dataCheatOperator = new DataCheatOperator(data, valueType, processManager);
                AddressCheatOperator addressCheatOperator = new AddressCheatOperator(address, processManager);

                DataCheat dataCheat = new DataCheat(dataCheatOperator, addressCheatOperator, lock_, description, processManager);
                add_new_row_to_cheat_list_view(dataCheat);
                cheatList.Add(dataCheat);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public void new_pointer_cheat(ulong address, List<long> offset_list, string type, string data, bool lock_, string description)
        {
            try
            {
                int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

                if (sectionID == -1)
                {
                    MessageBox.Show("Address is out of range!");
                    return;
                }

                if (cheatList.Exist(address))
                {
                    return;
                }

                ValueType valueType = MemoryHelper.GetValueTypeByString(type);

                DataCheatOperator sourceOperator = new DataCheatOperator(data, valueType, processManager);
                AddressCheatOperator addressCheatOperator = new AddressCheatOperator(address, processManager);
                List<OffsetCheatOperator> offsetOperators = new List<OffsetCheatOperator>();

                for (int i = 0; i < offset_list.Count; ++i)
                {
                    OffsetCheatOperator offsetOperator = new OffsetCheatOperator(offset_list[i],
                        ValueType.ULONG_TYPE, processManager);
                    offsetOperators.Add(offsetOperator);
                }

                SimplePointerCheatOperator destOperator = new SimplePointerCheatOperator(addressCheatOperator, offsetOperators, valueType, processManager);

                SimplePointerCheat simplePointerCheat = new SimplePointerCheat(sourceOperator, destOperator,
                    lock_, description, processManager);

                add_new_row_to_cheat_list_view(simplePointerCheat);
                cheatList.Add(simplePointerCheat);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void add_to_cheat_list(ListViewItem item)
        {
            string address = item.SubItems[RESULT_LIST_ADDRESS].Text;
            string type = item.SubItems[RESULT_LIST_TYPE].Text;
            string value = item.SubItems[RESULT_LIST_VALUE].Text;
            bool lock_ = false;
            string description = "";

            new_data_cheat(ulong.Parse(address, NumberStyles.HexNumber), type, value, lock_, description);
        }

        private void result_list_view_DoubleClick(object sender, EventArgs e)
        {
            if (result_list_view.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = result_list_view.SelectedItems;
                add_to_cheat_list(items[0]);
            }
        }

        private void cheat_list_view_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow edited_row = cheat_list_view.Rows[e.RowIndex];
            object edited_col = edited_row.Cells[e.ColumnIndex].Value;

            try
            {
                switch (e.ColumnIndex)
                {
                    case CHEAT_LIST_VALUE:
                        DataCheatOperator dataCheatOperator = (DataCheatOperator)cheatList[e.RowIndex].GetSource();
                        CheatOperator destOperator = cheatList[e.RowIndex].GetDestination();
                        dataCheatOperator.Set((string)edited_col);
                        destOperator.SetRuntime(dataCheatOperator);
                        break;
                    case CHEAT_LIST_DESC:
                        cheatList[e.RowIndex].Description = (string)edited_col;
                        break;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void cheat_list_view_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex < 0) return;

            DataGridViewRow edited_row = cheat_list_view.Rows[e.RowIndex];
            object edited_col = null;

            switch (e.ColumnIndex)
            {
                case CHEAT_LIST_ENABLED:
                    cheat_list_view.EndEdit();
                    DataCheatOperator dataCheatOperator = (DataCheatOperator)cheatList[e.RowIndex].GetSource();
                    CheatOperator destOperator = cheatList[e.RowIndex].GetDestination();
                    edited_col = edited_row.Cells[CHEAT_LIST_VALUE].Value;
                    dataCheatOperator.Set((string)edited_col);
                    destOperator.SetRuntime(dataCheatOperator);
                    break;
                case CHEAT_LIST_DEL:
                    cheat_list_view.Rows.RemoveAt(e.RowIndex);
                    break;
                case CHEAT_LIST_LOCK:
                    cheat_list_view.EndEdit();
                    edited_col = edited_row.Cells[e.ColumnIndex].Value;
                    cheatList[e.RowIndex].Lock = (bool)edited_col;
                    break;
            }
        }

        private void add_address_btn_Click(object sender, EventArgs e)
        {
            NewAddress newAddress = new NewAddress(processManager);
            if (newAddress.ShowDialog() != DialogResult.OK)
                return;

            ulong address = newAddress.Address;
            string value_type = newAddress.ValueTypeStr;
            string value = newAddress.Value;
            bool lock_ = newAddress.Lock;
            string description = newAddress.Descriptioin;

            int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

            if (sectionID < 0)
            {
                MessageBox.Show("Invalid Address!!");
                return;
            }

            if (!newAddress.Pointer)
            {
                new_data_cheat(address, value_type, value, lock_, description);
            }
            else
            {
                new_pointer_cheat(address, newAddress.OffsetList, value_type, value, lock_, description);
            }
        }

        private void cheat_list_view_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                cheatList.RemoveAt(e.RowIndex);
            }
        }

        private void save_address_btn_Click(object sender, EventArgs e)
        {
            save_file_dialog.Filter = "Cheat files (*.cht)|*.cht";
            save_file_dialog.FilterIndex = 1;
            save_file_dialog.RestoreDirectory = true;

            if (save_file_dialog.ShowDialog() == DialogResult.OK)
            {
                cheatList.SaveFile(save_file_dialog.FileName, (string)processes_comboBox.SelectedItem, processManager);
            }
        }

        private void load_address_btn_Click(object sender, EventArgs e)
        {
            open_file_dialog.Filter = "Cheat files (*.cht)|*.cht";
            open_file_dialog.FilterIndex = 1;
            open_file_dialog.RestoreDirectory = true;

            if (open_file_dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            cheat_list_view.Rows.Clear();
            cheatList.LoadFile(open_file_dialog.FileName, processManager, processes_comboBox);

            for (int i = 0; i < cheatList.Count; ++i)
            {
                Cheat cheat = cheatList[i];
                if (cheat.CheatType == CheatType.DATA_TYPE)
                {
                    add_new_row_to_cheat_list_view((DataCheat)cheat);
                }
                else if (cheat.CheatType == CheatType.SIMPLE_POINTER_TYPE)
                {
                    add_new_row_to_cheat_list_view((SimplePointerCheat)cheat);
                }
            }
        }

        private void refresh_cheat_list_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < cheatList.Count; ++i)
                {
                    DataGridViewRow row = cheat_list_view.Rows[i];

                    DataCheatOperator dataCheatOperator = (DataCheatOperator)cheatList[i].GetSource();
                    CheatOperator destOperator = cheatList[i].GetDestination();
                    dataCheatOperator.Set(destOperator.GetRuntime());
                    row.Cells[CHEAT_LIST_VALUE].Value = dataCheatOperator.Display();

                    if (cheatList[i].CheatType == CheatType.SIMPLE_POINTER_TYPE)
                    {
                        row.Cells[CHEAT_LIST_ADDRESS].Value = destOperator.Display();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void refresh_lock_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < cheatList.Count; ++i)
            {
                if (!cheatList[i].Lock)
                {
                    continue;
                }

                DataCheatOperator dataCheatOperator = (DataCheatOperator)cheatList[i].GetSource();
                CheatOperator destOperator = cheatList[i].GetDestination();
                destOperator.SetRuntime(dataCheatOperator);
            }
        }
        
        private void processes_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                sectionListView.Items.Clear();
                result_list_view.Items.Clear();
                new_scan_btn.Enabled = true;
                valueTypeList.Enabled = true;
                compareTypeList.Enabled = true;
                sectionListView.Enabled = true;

                ProcessInfo processInfo = processManager.GetProcessInfo(processes_comboBox.Text);
                Util.DefaultProcessID = processInfo.pid;
                ProcessMap pmap = MemoryHelper.GetProcessMaps(processInfo.pid);
                processManager.MappedSectionList.InitMemorySectionList(pmap);

                sectionListView.BeginUpdate();
                for (int sectionID = 0; sectionID < processManager.MappedSectionList.Count; ++sectionID)
                {
                    MappedSection mappedSection = processManager.MappedSectionList[sectionID];
                    if (isFilterBox.Checked && mappedSection.IsFilter)
                    {
                        continue;
                    }
                    string addr = String.Format("{0:X9}", mappedSection.Start);
                    int itemIdx = sectionListView.Items.Count;
                    sectionListView.Items.Add(sectionID.ToString(), addr, 0);
                    sectionListView.Items[itemIdx].SubItems.Add(mappedSection.Name);
                    sectionListView.Items[itemIdx].SubItems.Add(mappedSection.Prot.ToString("X"));
                    sectionListView.Items[itemIdx].SubItems.Add((mappedSection.Length / 1024).ToString() + "KB");
                    sectionListView.Items[itemIdx].SubItems.Add(sectionID.ToString());
                    if (mappedSection.Offset != 0)
                    {
                        sectionListView.Items[itemIdx].SubItems.Add(mappedSection.Offset.ToString("X"));
                    }
                    if (mappedSection.IsFilter)
                    {
                        sectionListView.Items[itemIdx].Tag = "filter";
                        sectionListView.Items[itemIdx].BackColor = Color.DarkGray;
                    }
                    if (mappedSection.Name.StartsWith("executable"))
                    {
                        sectionListView.Items[itemIdx].BackColor = Color.Honeydew;
                    }
                    else if (mappedSection.Name.Contains("NoName"))
                    {
                        sectionListView.Items[itemIdx].ForeColor = Color.Red;
                    }
                    else if (Regex.IsMatch(mappedSection.Name, @"^\[\d+\]$"))
                    {
                        sectionListView.Items[itemIdx].ForeColor = Color.DarkRed;
                    }
                }
                sectionListView.EndUpdate();
            }
            catch (Exception exception)
            {
                MessageBox.Show("SectionList init failed, " + exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
		
        private void get_processes_btn_Click(object sender, EventArgs e)
        {
            try
            {
                var connResult = MemoryHelper.Connect(ip_box.Text);
                if (!connResult.Item1)
                {
                    throw new Exception(connResult.Item2);
                }

                int selectedIdx = 0;
                this.processes_comboBox.Items.Clear();
                ProcessList pl = MemoryHelper.GetProcessList();
                foreach (Process process in pl.processes)
                {
                    int idx = this.processes_comboBox.Items.Add(process.name);
                    if (process.name == CONSTANT.DEFAULT_PROCESS)
                    {
                        selectedIdx = idx;
                    }
                    else if (selectedIdx == 0 && Regex.IsMatch(process.titleId, "CUSA|PCAS|PCJS|PLAS|PLJS|PLJM|PCCS|PCKS"))
                    {
                        selectedIdx = idx;
                    }
                }
                this.processes_comboBox.SelectedIndex = selectedIdx;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void send_pay_load(string IP, string payloadPath, int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(new IPEndPoint(IPAddress.Parse(IP), port));
            socket.SendFile(payloadPath);
            socket.Close();
        }

        private void send_payload_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.version_list.SelectedItem == null)
                {
                    throw new System.ArgumentException("Unknown version.");
                }
                string patchPath = string.Format(@"{0}\payloads\{1}\", Application.StartupPath, this.version_list.SelectedItem);
                if (!File.Exists(patchPath + @"ps4debug.bin"))
                {
                    throw new ArgumentException(string.Format("ps4debug.bin({0}) not found!", patchPath));
                }
                this.send_pay_load(this.ip_box.Text, patchPath + @"ps4debug.bin", Convert.ToInt32(this.port_box.Text));
                Thread.Sleep(1000);

                //if (File.Exists(patchPath + @"kdebugger.elf"))
                //{
                //    this.msg.Text = "Injecting kdebugger.elf...";
                //    this.send_pay_load(this.ip_box.Text, patchPath + @"kdebugger.elf", 9023);
                //    Thread.Sleep(2500);
                //}
                this.msg.ForeColor = Color.Green;
                this.msg.Text = "ps4debug.bin injected successfully!";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void InitCompareTypeListOfFirstScan()
        {
            string selected_type = (string)compareTypeList.SelectedItem;
            compareTypeList.Items.Clear();
            switch (MemoryHelper.GetValueTypeByString((string)valueTypeList.SelectedItem))
            {
                case ValueType.ULONG_TYPE:
                case ValueType.UINT_TYPE:
                case ValueType.USHORT_TYPE:
                case ValueType.BYTE_TYPE:
                    hex_box.Enabled = true;
                    compareTypeList.Items.AddRange(SEARCH_BY_BYTES_FIRST);
                    break;
                case ValueType.DOUBLE_TYPE:
                case ValueType.FLOAT_TYPE:
                    hex_box.Enabled = true;
                    compareTypeList.Items.AddRange(SEARCH_BY_FLOAT_FIRST);
                    break;
                case ValueType.HEX_TYPE:
                case ValueType.GROUP_TYPE:
                case ValueType.STRING_TYPE:
                    hex_box.Enabled = false;
                    hex_box.Checked = false;
                    compareTypeList.Items.AddRange(SEARCH_BY_HEX);
                    break;
                default:
                    throw new Exception("GetStringOfValueType!!!");
            }

            int list_idx = 0;
            int list_count = compareTypeList.Items.Count;

            for (; list_idx < list_count; ++list_idx)
            {
                if ((string)compareTypeList.Items[list_idx] == selected_type)
                {
                    break;
                }
            }

            compareTypeList.SelectedIndex = list_idx == list_count ? 0 : list_idx;
        }

        private void InitCompareTypeListOfNextScan()
        {
            string selected_type = (string)compareTypeList.SelectedItem;
            compareTypeList.Items.Clear();
            switch (MemoryHelper.GetValueTypeByString((string)valueTypeList.SelectedItem))
            {
                case ValueType.ULONG_TYPE:
                case ValueType.UINT_TYPE:
                case ValueType.USHORT_TYPE:
                case ValueType.BYTE_TYPE:
                    compareTypeList.Items.AddRange(SEARCH_BY_BYTES_NEXT);
                    break;
                case ValueType.DOUBLE_TYPE:
                case ValueType.FLOAT_TYPE:
                    compareTypeList.Items.AddRange(SEARCH_BY_FLOAT_NEXT);
                    break;
                case ValueType.HEX_TYPE:
                case ValueType.GROUP_TYPE:
                case ValueType.STRING_TYPE:
                    compareTypeList.Items.AddRange(SEARCH_BY_HEX);
                    break;
                default:
                    throw new Exception("GetStringOfValueType!!!");
            }

            int list_idx = 0;
            int list_count = compareTypeList.Items.Count;

            for (; list_idx < list_count; ++list_idx)
            {
                if ((string)compareTypeList.Items[list_idx] == selected_type)
                {
                    break;
                }
            }

            compareTypeList.SelectedIndex = list_idx == list_count ? 0 : list_idx;
        }

        private void valueTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitCompareTypeListOfFirstScan();
        }

        private void compareList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)compareTypeList.SelectedItem == CONSTANT.BETWEEN_VALUE)
            {
                value_1_box.Visible = true;
                value_label.Visible = true;
                and_label.Visible = true;
                value_box.Width = 115;
            }
            else
            {
                value_1_box.Visible = false;
                value_label.Visible = false;
                and_label.Visible = false;
                value_box.Width = 261;
            }
        }

        private void new_scan_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            new_scan_btn.Text = CONSTANT.NEW_SCAN;
            InitCompareTypeListOfNextScan();
            if (e.Error != null)
            {
                msg.Text = e.Error.Message;
            }
            setButtons(true);
        }

        private void update_result_list_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            refresh_btn.Text = CONSTANT.REFRESH;
            if (e.Error != null)
            {
                msg.Text = e.Error.Message;
            }
            setButtons(true);
        }

        private void next_scan_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            next_scan_btn.Text = CONSTANT.NEXT_SCAN;
            if (e.Error != null)
            {
                msg.Text = e.Error.Message;
            }
            setButtons(true);
        }

        private void result_list_view_add_to_cheat_list_Click(object sender, EventArgs e)
        {
            if (result_list_view.SelectedItems == null)
                return;

            ListView.SelectedListViewItemCollection items = result_list_view.SelectedItems;
            for (int i = 0; i < items.Count; ++i)
            {
                add_to_cheat_list(items[i]);
            }
        }

        private void cheat_list_item_active_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;
            for (int i = 0; i < items.Count; ++i)
            {
                Cheat cheat = cheatList[items[i].Index];

                cheat.GetDestination().SetRuntime(cheat.GetSource());
            }
        }

        private void cheat_list_item_delete_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;
            for (int i = 0; i < items.Count; ++i)
            {
                cheat_list_view.Rows.Remove(items[i]);
            }
        }

        private void cheat_list_item_lock_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;
            for (int i = 0; i < items.Count; ++i)
            {
                cheatList[items[i].Index].Lock = true;
                items[i].Cells[CHEAT_LIST_LOCK].Value = true;
            }
        }

        private void cheat_list_time_unlock_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;
            for (int i = 0; i < items.Count; ++i)
            {
                cheatList[items[i].Index].Lock = false;
                items[i].Cells[CHEAT_LIST_LOCK].Value = false;
            }
        }

        private void cheat_list_item_hex_view_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            if (cheat_list_view.SelectedRows.Count != 1)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;
            Cheat cheat = cheatList[items[0].Index];

            ulong address = 0;
            if (cheat.CheatType == CheatType.DATA_TYPE)
            {
                address = ulong.Parse((string)items[0].Cells[CHEAT_LIST_ADDRESS].Value, NumberStyles.HexNumber);
            }
            else if (cheat.CheatType == CheatType.SIMPLE_POINTER_TYPE)
            {
                SimplePointerCheatOperator destOperator = (SimplePointerCheatOperator)cheat.GetDestination();
                address = Convert.ToUInt64(destOperator.Display().Replace("p->", ""), 16); //destOperator.getBaseAddress();
            }
            int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);

            if (sectionID >= 0)
            {
                MappedSection section = processManager.MappedSectionList[sectionID];

                int offset = (int)(address - section.Start);
                HexEditor hexEdit = new HexEditor(this, memoryHelper, offset, section);
                hexEdit.Show(this);
            }
        }

        private void cheat_list_item_find_pointer_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;

            if (cheat_list_view.SelectedRows.Count != 1)
                return;

            DataGridViewSelectedRowCollection items = cheat_list_view.SelectedRows;

            try
            {
                ulong address = ulong.Parse((string)items[0].Cells[CHEAT_LIST_ADDRESS].Value, NumberStyles.HexNumber);
                string type = (string)items[0].Cells[CHEAT_LIST_TYPE].Value;

                PointerFinder pointerFinder = new PointerFinder(this, address, type, processManager, cheat_list_view);
                pointerFinder.Show();
            }
            catch
            {

            }
        }

        private void version_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace((string)version_list.SelectedItem))
            {
                Util.Version = versions[(string)version_list.SelectedItem];
            }
        }

        private void result_list_item_find_pointer_Click(object sender, EventArgs e)
        {
            if (result_list_view.SelectedItems == null)
                return;
            
            if (result_list_view.SelectedItems.Count != 1)
                return;

            try
            {
                ulong address = ulong.Parse(result_list_view.SelectedItems[0].
                                    SubItems[RESULT_LIST_ADDRESS].Text, NumberStyles.HexNumber);
                string type = result_list_view.SelectedItems[0].SubItems[RESULT_LIST_TYPE].Text;

                PointerFinder pointerFinder = new PointerFinder(this, address, type, processManager, cheat_list_view);
                pointerFinder.Show();
            }
            catch
            {

            }
        }

        private void isFilterBox_CheckedChanged(object sender, EventArgs e)
        {
            int idx = processes_comboBox.SelectedIndex;
            if (idx == -1) return;

            if (isFilterBox.Checked)
            {
                sectionListView.BeginUpdate();
                foreach (ListViewItem item in sectionListView.Items)
                {
                    if ("filter".Equals(item.Tag))
                    {
                        item.Checked = false; //Ensure that MappedSectionList is not selected
                        item.Remove();
                    }
                }
                sectionListView.EndUpdate();
            } else
            {
                processes_comboBox.SelectedIndex = 0;
                processes_comboBox.SelectedIndex = idx;
            }

        }

        private void cheat_list_item_edit_Click(object sender, EventArgs e)
        {
            if (cheat_list_view.SelectedRows == null)
                return;
            if (cheat_list_view.SelectedRows.Count != 1)
                return;
            try
            {
                DataGridViewRow item = cheat_list_view.SelectedRows[0];
                Cheat cheat = cheatList[item.Index];
                bool isPointer = false;
                ulong address = 0;
                List<long> offsets = null;
                if (cheat.CheatType == CheatType.DATA_TYPE)
                {
                    address = ulong.Parse((string)item.Cells[CHEAT_LIST_ADDRESS].Value, NumberStyles.HexNumber);
                    int sectionID = processManager.MappedSectionList.GetMappedSectionID(address);
                    if (sectionID < 0)
                    {
                        MessageBox.Show("Invalid Address!!");
                        return;
                    }
                }
                else if (cheat.CheatType == CheatType.SIMPLE_POINTER_TYPE)
                {
                    isPointer = true;
                    SimplePointerCheatOperator destOperator = (SimplePointerCheatOperator)cheat.GetDestination();
                    offsets = destOperator.getOffsets();
                    address = destOperator.getBaseAddress();
                }

                NewAddress newAddress = new NewAddress(processManager, address,
                    (string)item.Cells[CHEAT_LIST_VALUE].Value,
                    (string)item.Cells[CHEAT_LIST_TYPE].Value,
                    (string)item.Cells[CHEAT_LIST_DESC].Value,
                    (bool)item.Cells[CHEAT_LIST_LOCK].Value,
                    isPointer, offsets, true);
                if (newAddress.ShowDialog() != DialogResult.OK)
                    return;

                ValueType valueType = MemoryHelper.GetValueTypeByString(newAddress.ValueTypeStr);
                DataCheatOperator dataCheatOperator = new DataCheatOperator(newAddress.Value, valueType, processManager);
                AddressCheatOperator addressCheatOperator = new AddressCheatOperator(newAddress.Address, processManager);

                if (!newAddress.Pointer)
                {
                    cheat = new DataCheat(dataCheatOperator, addressCheatOperator, newAddress.Lock, newAddress.Descriptioin, processManager);
                    addressCheatOperator.SetRuntime(dataCheatOperator);
                } else
                {
                    List<OffsetCheatOperator> offsetOperators = new List<OffsetCheatOperator>();

                    for (int i = 0; i < offsets.Count; ++i)
                    {
                        OffsetCheatOperator offsetOperator = new OffsetCheatOperator(offsets[i],
                            ValueType.ULONG_TYPE, processManager);
                        offsetOperators.Add(offsetOperator);
                    }

                    SimplePointerCheatOperator destOperator = new SimplePointerCheatOperator(addressCheatOperator, offsetOperators, valueType, processManager);

                    cheat = new SimplePointerCheat(dataCheatOperator, destOperator,
                        newAddress.Lock, newAddress.Descriptioin, processManager);
                }
                cheatList[item.Index] = cheat;

                item.Cells[CHEAT_LIST_VALUE].Value = dataCheatOperator.Display();
                item.Cells[CHEAT_LIST_TYPE].Value = MemoryHelper.GetStringOfValueType(dataCheatOperator.ValueType);
                item.Cells[CHEAT_LIST_DESC].Value = newAddress.Descriptioin;
                item.Cells[CHEAT_LIST_LOCK].Value = newAddress.Lock;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
}

