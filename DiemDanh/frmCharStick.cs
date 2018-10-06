using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DiemDanh
{
    public partial class frmCharStick : Form
    {
        private string[] myString;
        public delegate void ChuyenKyHieu(string v, string vp, string c, string b);
        public ChuyenKyHieu Move;
        public frmCharStick()
        {
            InitializeComponent();
        }
        public frmCharStick(string v, string vp, string c, string b)
        {
            InitializeComponent();
            txtCongdiem.Text = c;
            txtVang.Text = v;
            txtVangphep.Text = vp;
            txtXoa.Text = b;
        }

        private void WriteFile()
        {
            FileStream myFileStream = new FileStream(Application.StartupPath + @"\Data\CharStick.DAT", FileMode.Create);
            BinaryWriter myBinaryWriter = new BinaryWriter(myFileStream);
            myBinaryWriter.Write(txtVang.Text.Trim());
            myBinaryWriter.Write(txtVangphep.Text.Trim());
            myBinaryWriter.Write(txtCongdiem.Text.Trim());
            myBinaryWriter.Write(txtXoa.Text.Trim());
            myBinaryWriter.Close();
        }
        

        private void btnOK_Click(object sender, EventArgs e)
        {
            string v = txtVang.Text.Trim();
            string vp = txtVangphep.Text.Trim();
            string c = txtCongdiem.Text.Trim();
            string b = txtXoa.Text.Trim();
            //Kiểm tra trùng
            myString = new string[4] { v, vp, c, b};
            for (int i = 0; i < 3; i++)
            {
                for(int j=i+1;j<4;j++)
                    if(myString[i].ToUpper()==myString[j].ToUpper())
                    {
                        MessageBox.Show("Trùng ký hiệu\"" + myString[i] + "\"", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
            }
            WriteFile();
            Move(v, vp, c, b);
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            txtCongdiem.Text = "+";
            txtVang.Text = "v";
            txtVangphep.Text = "vp";
            txtXoa.Text = "";
        }
    }
}
