using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Be.Windows.Forms;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using librpc;

namespace PS4_Cheater
{
    public partial class HexEditor : Form
    {
        private main mainForm;
        private MappedSection section;
        private MemoryHelper memoryHelper;

        private int page;
        private int page_count;
        private long line;
        private int column;

        const int page_size = 8 * 1024 * 1024;

        public HexEditor(main mainForm, MemoryHelper memoryHelper, int offset, MappedSection section)
        {
            InitializeComponent();

            this.mainForm = mainForm;
            this.memoryHelper = memoryHelper;
            this.section = section;
            this.page = offset / page_size;
            this.line = (offset - page * page_size) / hexBox.BytesPerLine;
            this.column = (offset - page * page_size) % hexBox.BytesPerLine;

            this.page_count = divup((int)section.Length, page_size);

            for (int i = 0; i < page_count; ++i)
            {
                ulong start = section.Start + (ulong)i * page_size;
                ulong end = section.Start + (ulong)(i + 1) * page_size;
                page_list.Items.Add((i + 1).ToString() + String.Format(" {0:X}-{1:X}", start, end));
            }
        }

        private void update_ui(int page, long line)
        {
            hexBox.LineInfoOffset = (uint)((ulong)section.Start + (ulong)(page_size * page));

            int mem_size = page_size;

            if (section.Length - page_size * page < mem_size)
            {
                mem_size = section.Length - page_size * page;
            }

            byte[] dst = memoryHelper.ReadMemory(section.Start + (ulong)page * page_size, (int)mem_size);
            hexBox.ByteProvider = new MemoryViewByteProvider(dst);

            if (line != 0)
            {
                hexBox.SelectionStart = line * hexBox.BytesPerLine + column;
                hexBox.SelectionLength = 4;
                if (hexBox.ColumnInfoVisible)
                {
                    line -= 2;
                }
                hexBox.ScrollByteIntoView((line + hexBox.Height / (int)hexBox.CharSize.Height - 1) * hexBox.BytesPerLine + column);
            }
        }

        private void HexEdit_Load(object sender, EventArgs e)
        {
            page_list.SelectedIndex = page;
        }

        private void HexEdit_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        int divup(int sum, int div)
        {
            return sum / div + ((sum % div != 0) ? 1 : 0);
        }

        private void next_btn_Click(object sender, EventArgs e)
        {
            if (page + 1 >= page_count)
            {
                return;
            }

            page++;
            line = 0;
            column = 0;

            page_list.SelectedIndex = page;
        }

        private void previous_btn_Click(object sender, EventArgs e)
        {
            if (page <= 0)
            {
                return;
            }

            page--;
            line = 0;
            column = 0;
            page_list.SelectedIndex = page;
        }

        private void page_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            page = page_list.SelectedIndex;

            update_ui(page, line);
        }

        private void commit_btn_Click(object sender, EventArgs e)
        {
            
            MemoryViewByteProvider mvbp = (MemoryViewByteProvider)this.hexBox.ByteProvider;
            if (mvbp.HasChanges())
            {
                byte[] buffer = mvbp.Bytes.ToArray();
                List<int> change_list = mvbp.change_list;

                for (int i = 0; i < change_list.Count; ++i)
                {
                    byte[] b = { buffer[change_list[i]]  };
                    memoryHelper.WriteMemory(section.Start + (ulong)(page * page_size + change_list[i]), b);
                }
                mvbp.change_list.Clear();
            }
        }

        private void refresh_btn_Click(object sender, EventArgs e)
        {
            page_list.SelectedIndex = page;
            line = hexBox.CurrentLine - 1;
            column = 0;
            update_ui(page, line);
        }

        private void find_Click(object sender, EventArgs e)
        {
            FindOptions findOptions = new FindOptions();
            findOptions.Type = FindType.Hex;
            findOptions.Hex = MemoryHelper.string_to_hex_bytes(input_box.Text);
            hexBox.Find(findOptions);
        }

        private void hexBox_SelectionStartChanged(object sender, EventArgs e)
        {
            if (hexBox.SelectionStart <= 0)
            {
                return;
            }
            List<byte> tmpBList = new List<byte>();
            int info1 = 0, info2 = 0;
            uint info4 = 0;
            ulong info8 = 0;
            float infoF = 0;
            double infoD = 0;
            info_box.Text = hexBox.ByteProvider.ReadByte(hexBox.SelectionStart).ToString("X");
            for (int idx = 0; idx <= 8; ++idx)
            {
                switch (idx)
                {
                    case 1:
                        info1 = Convert.ToUInt16(tmpBList[0]);
                        break;
                    case 2:
                        info2 = BitConverter.ToUInt16(tmpBList.ToArray(), 0);
                        break;
                    case 4:
                        info4 = BitConverter.ToUInt32(tmpBList.ToArray(), 0);
                        infoF = BitConverter.ToSingle(tmpBList.ToArray(), 0);
                        break;
                    case 8:
                        info8 = BitConverter.ToUInt64(tmpBList.ToArray(), 0);
                        infoD = BitConverter.ToDouble(tmpBList.ToArray(), 0);
                        break;
                }
                tmpBList.Add(hexBox.ByteProvider.ReadByte(hexBox.SelectionStart + idx));
            }
            info_box.Text = string.Format(@"1: {0:X8}={0}
2: {1:X8}={1}
4: {2:X8}={2}
8: {3:X8}={3}
F: {4}
D: {4}", info1, info2, info4, info8, infoF, infoD);
        }

        private void add_to_cheat_list_btn_Click(object sender, EventArgs e)
        {
            if (hexBox.SelectionStart <= 0)
            {
                return;
            }
            ulong address = section.Start + (ulong)hexBox.ByteProvider.Length + (ulong)hexBox.SelectionStart;
            string value = "", valueTypeStr = "";

            List<byte> tmpBList = new List<byte>();
            for (int idx = 0; idx <= 8; ++idx)
            {
                tmpBList.Add(hexBox.ByteProvider.ReadByte(hexBox.SelectionStart + idx));
            }
            switch (hexBox.SelectionLength)
            {
                case 1:
                    
                    value = Convert.ToUInt16(tmpBList.GetRange(0, 1).ToArray()[0]).ToString();
                    valueTypeStr = CONSTANT.BYTE_TYPE;
                    break;
                case 2:
                    value = BitConverter.ToUInt16(tmpBList.GetRange(0, 2).ToArray(), 0).ToString();
                    valueTypeStr = CONSTANT.BYTE_2_TYPE;
                    break;
                case 4:
                    value = BitConverter.ToUInt32(tmpBList.GetRange(0, 4).ToArray(), 0).ToString();
                    valueTypeStr = CONSTANT.BYTE_4_TYPE;
                    break;
                case 8:
                    value = BitConverter.ToUInt64(tmpBList.GetRange(0, 8).ToArray(), 0).ToString();
                    valueTypeStr = CONSTANT.BYTE_8_TYPE;
                    break;
                default:
                    value = BitConverter.ToUInt32(tmpBList.GetRange(0, 4).ToArray(), 0).ToString();
                    valueTypeStr = CONSTANT.BYTE_4_TYPE;
                    break;
            }
            NewAddress newAddress = new NewAddress(null, address,
                value,
                valueTypeStr,
                "",
                false,
                false, null, false);
            if (newAddress.ShowDialog() != DialogResult.OK)
                return;

            if (!newAddress.Pointer)
            {
                mainForm.new_data_cheat(newAddress.Address, newAddress.ValueTypeStr, newAddress.Value, newAddress.Lock, newAddress.Descriptioin);
            }
            else
            {
                mainForm.new_pointer_cheat(address, newAddress.OffsetList, newAddress.ValueTypeStr, newAddress.Value, newAddress.Lock, newAddress.Descriptioin);
            }
        }
    }
}
