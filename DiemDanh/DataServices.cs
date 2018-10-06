using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
namespace DiemDanh
{
    class DataServices
    {
        private static OleDbConnection myOleDbConnection;
        public DataServices(string filePath, ref string tableName)
        {
            try
            {
                string Excel97_2003 = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Extended Properties = Excel 8.0";
                string Excel2007 = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = {0}; Extended Properties = Excel 8.0";
                string extension = System.IO.Path.GetExtension(filePath);
                string ExcelConnectionString;
                if (extension == "xls")
                    ExcelConnectionString = string.Format(Excel97_2003, filePath);
                else
                    ExcelConnectionString = string.Format(Excel2007, filePath);
                myOleDbConnection = new OleDbConnection(ExcelConnectionString);
                myOleDbConnection.Open();
                DataTable excelSchema = myOleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                tableName = excelSchema.Rows[0]["TABLE_NAME"].ToString();
            }
            catch(InvalidOperationException)
            {
                System.Windows.Forms.MessageBox.Show("Chưa cài Access DB engine", "Lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Lỗi");
            }
        }
        public void Dispose()
        {
            CloseDB();
            myOleDbConnection.Dispose();
            myOleDbConnection = null;
        }
        public void OpenDB()
        {
            try
            {
                if (myOleDbConnection.State != ConnectionState.Open)
                    myOleDbConnection.Open();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Lỗi");
            }
        }
        public void CloseDB()
        {
            try
            {
                if (myOleDbConnection.State == ConnectionState.Open)
                    myOleDbConnection.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Lỗi");
            }
        }
        public DataTable RunQuery(string cmd)
        {
            try
            {
                OpenDB();
                OleDbDataAdapter myOleDbDataAdapter = new OleDbDataAdapter(cmd, myOleDbConnection);
                //Doi
                DataTable myDataTable = new DataTable();
                myDataTable.Clear();
                myOleDbDataAdapter.Fill(myDataTable);
                CloseDB();
                return myDataTable;
            }
            catch (Exception ex)
            {
                CloseDB();
                return null;
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Lỗi");
            }

        }
        public void ExecuteNonQuery(string cmd)
        {
            OpenDB();
            OleDbCommand myOleDbCommand = new OleDbCommand(cmd, myOleDbConnection);
            myOleDbCommand.ExecuteNonQuery();
            CloseDB();
        }
    }
}
