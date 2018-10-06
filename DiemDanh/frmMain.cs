using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

namespace DiemDanh
{
    public partial class frmMain : Form
    {
        public struct CharTick
        {
            public string _vang, _phep, _congdiem, _bodau;
        }
        private CharTick _charTick;
        private System.Media.SoundPlayer _sound;
        private DataTable myDataTable;
        private bool _play, _skip; // biến báo hiệu không đọc nữa để nhảy ô khác
        private string TableName;
        private DataServices myDataServices;
        public frmMain()
        {
            InitializeComponent();
            _play = false;
            GetCharStick();    
            
        }
       
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            //dlg.InitialDirectory = @"D:\";
            dlg.Filter = "File Excel |*xls; *xlsx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = dlg.FileName;
                myDataServices = new DataServices(dlg.FileName, ref TableName);
                Display();
            }
        }
        private void Read(string word)
        {
            
            _sound = new System.Media.SoundPlayer(Application.StartupPath + @"\Data\FileAudio\" + word + ".wav");
            _sound.Play();
            if(!_skip) Thread.Sleep(400);
        }
        private void StopRead()
        {
            if (_sound != null)
            {
                _sound.Stop();
                _sound = null;
            }
        }
        private void btnDiemDanh_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount == 0) return;
            if (!_play)
            {
                _skip = false;
                timerNextrowAndRead.Start();
                _play = true;
                btnDiemDanh.Text = "Tạm dừng (Space)";
            }
            else
            {
                _skip = true;
                _play = false;
                timerNextrowAndRead.Stop();
                StopRead();
                btnDiemDanh.Text = "Điểm danh (Space)";
            }
        }
        
        #region Xử lý data grid view
        private void Display()
        {
            dataGridView1.Columns.Clear();
            bool edit = true;
            myDataTable = new DataTable();
            myDataTable = myDataServices.RunQuery("SELECT * FROM [" + TableName + "];");
            int index;
            if (myDataTable.Columns[0].ColumnName != "TT")
            {    //Xóa bỏ tiêu đề
                edit = false;
                while (myDataTable.Rows[0][0].ToString().ToUpper() != "TT")
                {
                    myDataTable.Rows.Remove(myDataTable.Rows[0]);
                    if (myDataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Bảng không có cấu trúc của file danh sách điểm danh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                //Xử lý xóa các dòng thừa
                for (index = 0; index < 22 && index < myDataTable.Columns.Count; index++)
                {
                    string field = myDataTable.Rows[0][index].ToString().Trim();
                    if (field == "") continue;
                    DataGridViewTextBoxColumn myCol = new DataGridViewTextBoxColumn();
                    myCol.Name = myCol.DataPropertyName = myDataTable.Columns[index].ColumnName;
                    myCol.HeaderText = field;
                    dataGridView1.Columns.Add(myCol);
                }
                myDataTable.Rows.Remove(myDataTable.Rows[0]);
            }
            //
            index = 5;
            while (index < myDataTable.Columns.Count)
            {
                if (myDataTable.Columns[index].ColumnName.Substring(0, 1) == "F")
                    myDataTable.Columns.RemoveAt(index);
                else
                    index++;
            }
            
            this.Text = "Điểm danh - " + TableName;
            dataGridView1.AutoGenerateColumns = edit;
            dataGridView1.DataSource = myDataTable;
            //Không cho click sort trên hàng tiêu đề và Xóa bỏ những cột trắng
            index = 0;
            while (index < dataGridView1.ColumnCount)
            //{
            //    if (dataGridView1.Columns[index].HeaderText.Substring(0, 1) == "F")
            //    {
            //        dataGridView1.Columns.RemoveAt(index);
            //    }
            //    else
            //    {
                   dataGridView1.Columns[index++].SortMode = DataGridViewColumnSortMode.NotSortable;
            //    }
            //}
            PaintRow(0, Color.Red, Color.Yellow);
        }
        
        private void PaintRow(int RowIndex, Color ForeColor, Color BackColor)
        {
            if (dataGridView1.RowCount == 0) return;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Rows[RowIndex].Cells[i].Style.BackColor = BackColor;
                dataGridView1.Rows[RowIndex].Cells[i].Style.ForeColor = ForeColor;
            }
        }
        private void dataGridView1_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            StopWait();
            StopRead();
            PaintRow(e.RowIndex, Color.Black, Color.White);
        }
      
        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (_play)
                    timerNextrowAndRead.Start();
                if (dataGridView1.Rows.Count == 1) return;
                PaintRow(e.RowIndex, Color.Red, Color.Yellow);
            }
            catch { }
        }

        #endregion
        #region Xử lý ấn phím
        private bool NextRow()
        {
            if (dataGridView1.RowCount == 0) return false;
            if (dataGridView1.CurrentRow.Index == dataGridView1.Rows.Count - 1)
            {
                if(_play)
                btnDiemDanh.PerformClick();
                MessageBox.Show("Hết danh sách!");
                return false;
            }
            int col = dataGridView1.CurrentCell.ColumnIndex;
            dataGridView1.CurrentCell = dataGridView1[col, dataGridView1.CurrentRow.Index + 1];
            return true;
        }
        private bool PreviousRow()
        {
            if (dataGridView1.RowCount == 0 || dataGridView1.CurrentRow.Index == 0) return false;
            int col = dataGridView1.CurrentCell.ColumnIndex;
            dataGridView1.CurrentCell = dataGridView1[col, dataGridView1.CurrentRow.Index - 1];
            return true;
        }
        private bool LeftRow()
        {
            if (dataGridView1.Columns.Count == 0 || dataGridView1.CurrentCell.ColumnIndex == 0) return false;
            int col = dataGridView1.CurrentCell.ColumnIndex;
            dataGridView1.CurrentCell = dataGridView1[col - 1, dataGridView1.CurrentRow.Index];
            return true;
        }
        private bool RightRow()
        {
            if (dataGridView1.Columns.Count == 0 || dataGridView1.CurrentCell.ColumnIndex == dataGridView1.Columns.Count - 1) return false;
            int col = dataGridView1.CurrentCell.ColumnIndex;
            dataGridView1.CurrentCell = dataGridView1[col + 1, dataGridView1.CurrentRow.Index];
            return true;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up: _skip = true; return PreviousRow();
                case Keys.Down: _skip = true; return NextRow();
                case Keys.Left: return LeftRow();
                case Keys.Right: return RightRow();
                case Keys.Space: btnDiemDanh.PerformClick(); return true;
                case Keys.V: btnDanhdau.PerformClick(); return true;
                case Keys.B: btnVangphep.PerformClick(); return true;
                case Keys.N: btnCongdiem.PerformClick(); return true;
                case Keys.M: btnBodanh.PerformClick(); return true;
                default: return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        #endregion
              #region Thiết lập thời gian
        //Thời gian chuyển dòng
        int _timeNext = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_timeNext == 0)
            {
                try
                {
                    int row = dataGridView1.CurrentRow.Index;
                    string hoten = dataGridView1.Rows[row].Cells[1].Value.ToString().Trim();
                    string[] words = hoten.Split(' ');
                    foreach (string word in words)
                    {
                        if (word == "") continue;
                        try
                        {
                            if (_skip) break;
                            Read(word);
                        }
                        catch { _timewait += 3; }
                    }
                    timerWait.Start();
                    _skip = false;
                }
                catch { }
                _timeNext = 1;
                timerNextrowAndRead.Stop();
            }
            _timeNext--;
        }

        //Thời gian chờ nghe điểm danh
        int _timewait = 3;
        private void timerWait_Tick(object sender, EventArgs e)
        {
            
            if (_timewait == 0)
            {
                StopWait();
                NextRow();
            }
            _timewait--;
            lblTimeWait.Text = _timewait.ToString();
        }
        private void StopWait()
        {
            timerWait.Stop();
            _timewait = 3;
            lblTimeWait.Text = _timewait.ToString();
        }
        #endregion
#region Đánh dấu
        //Tạo ký tự để đánh dấu
        private void CreateCharTick(string v, string p, string c, string b)
        {
            _charTick._vang = v; _charTick._phep = p; _charTick._congdiem = c; _charTick._bodau = b;
        }
        //Đọc ký hiệu đánh dấu
        private void GetCharStick()
        {
            try
            {
                FileStream myFileStream = new FileStream(Application.StartupPath + @"\Data\CharStick.DAT", FileMode.Open);
                BinaryReader myBinaryReader = new BinaryReader(myFileStream);
                string v = myBinaryReader.ReadString();
                string vp = myBinaryReader.ReadString();
                string c = myBinaryReader.ReadString();
                string b = myBinaryReader.ReadString();
                CreateCharTick(v, vp, c, b);
                myBinaryReader.Close();
            }
            catch
            {
                CreateCharTick("v", "vp", "+", "");
            }
        }


        private void Update(string st)
        {
            if (dataGridView1.RowCount == 0) return;
            int a = dataGridView1.CurrentCell.ColumnIndex;
            string ColumnName = dataGridView1.Columns[a].DataPropertyName;
            string stt = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value.ToString();
            if (stt.Trim() == "") return;
            try //stt là kiểu chữ
            {
                string cmd = "Update [" + TableName + "] set [" + ColumnName + "] = '" + st + "' Where [" + dataGridView1.Columns[0].DataPropertyName + "] ='" + stt + "'";
                myDataServices.ExecuteNonQuery(cmd);
                dataGridView1.CurrentCell.Value = st;
            }
            catch (OleDbException) // stt là kiểu chữ
            {
                string cmd = "Update [" + TableName + "] set [" + ColumnName + "] = '" + st + "' Where [" + dataGridView1.Columns[0].DataPropertyName + "] =" + stt;
                myDataServices.ExecuteNonQuery(cmd);
                dataGridView1.CurrentCell.Value = st;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi truy vấn cập nhật");
            }
        }
        private void btnDanhdau_Click(object sender, EventArgs e)
        {
           _skip = true;
           Update(_charTick._vang); _skip = false;
        }
        private void btnVangphep_Click(object sender, EventArgs e)
        {
            _skip = true;
            Update(_charTick._phep);
            _skip = false;
        }

        private void btnCongdiem_Click(object sender, EventArgs e)
        {
            _skip = true;
            Update(_charTick._congdiem);
            _skip = false;
        }

        private void btnBodanh_Click(object sender, EventArgs e)
        {
            _skip = true;
            Update(_charTick._bodau);
            _skip = false;
        }
#endregion

        private void trợGiúpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(myDataTable!=null) myDataTable.Dispose();
            StopRead();
            StopWait();
            if(myDataServices !=null) myDataServices.Dispose();
        }

        private void kýHiệuĐánhDấuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCharStick frm = new frmCharStick(_charTick._vang, _charTick._phep, _charTick._congdiem, _charTick._bodau);
            frm.Move = new frmCharStick.ChuyenKyHieu(CreateCharTick);
            frm.ShowDialog();
        }

    }
}
