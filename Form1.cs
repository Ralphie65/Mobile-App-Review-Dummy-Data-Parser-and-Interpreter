using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using GemBox.Spreadsheet;



namespace DataSetTesting1
{
    public partial class Main : Form
    {
        DataTable finalDT = new DataTable();
        BindingSource bS = new BindingSource();
        DataView dV = new DataView();
        public Main()
        {

            InitializeComponent();
            GemBox.Spreadsheet.SpreadsheetInfo.SetLicense("Free-Limited-Key");

        }
        private void Main_Load(object sender, EventArgs e)
        {
            string windowsUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string positiveFileName = $"C:\\Users\\{windowsUser}\\Documents\\Positive.xlsx";
            string negativeFileName = $"C:\\Users\\{windowsUser}\\Documents\\Negative.xlsx";
            
            
            //string fileName = $"C:\\Users\\{windowsUser}\\Documents\\Reviews.xlsx";

            MessageBox.Show("Please Select Positive Word list.xlsx");
            OpenFileDialog opfd = new OpenFileDialog();
            opfd.ShowDialog();
            positiveFileName = opfd.FileName;
            MessageBox.Show("Please Select Negative Word list.xlsx");
            opfd.ShowDialog();
            negativeFileName = opfd.FileName;
            MessageBox.Show("Please Select List of Reviews.xlsx");
            opfd.ShowDialog();
            String fileName = opfd.FileName;

            DataTable positiveDT = CreateWordDT(positiveFileName);
            DataTable negativeDT = CreateWordDT(negativeFileName);
            finalDT = Read_Excel(fileName, positiveDT, negativeDT);
            bS.DataSource = finalDT;
            dataGridView1.Columns[4].Width = 865;
            comboBox1.SelectedItem = "ALL";

        }
        private DataTable CreateWordDT(string fileName)
        {
            var workbook = ExcelFile.Load(fileName);
            var worksheet = workbook.Worksheets[0];
            var rowCount = worksheet.Rows.Count;
            var columnCount = worksheet.CalculateMaxUsedColumns();
            var wordDT = new DataTable();
            wordDT.PrimaryKey = new DataColumn[] { wordDT.Columns.Add("Word", typeof(string)) };
            int rC = worksheet.Rows.Count;
            for (int i = 0; i < rC; i++)
            {
                var value = worksheet.Cells[i, 0].Value?.ToString();
                wordDT.Rows.Add(value);
            }
            return wordDT;
        }
        private int CheckIfPositiveOrNegative(DataTable positive, DataTable negative, string[] wordsInInput)
        {
            foreach (string word in wordsInInput)
            {
                if (positive.Rows.Contains(word))
                {
                    return 1;
                }
                else if (negative.Rows.Contains(word))
                {
                    return 2;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }
        private DataTable Read_Excel(string fileName, DataTable positive, DataTable negative)
        {
            int lblPositiveCount = 0;
            int lblNegativeCount = 0;
            int lblUnCategorizedCount = 0;
            var workbook = ExcelFile.Load(fileName);
            var worksheet = workbook.Worksheets[0];
            var rowCount = worksheet.Rows.Count;
            var columnCount = worksheet.CalculateMaxUsedColumns();
            var dT = new DataTable();
            dT.Columns.Add("ID", typeof(int));
            dT.Columns.Add("Date", typeof(string));
            dT.Columns.Add("Word_Count", typeof(int));
            dT.Columns.Add("Rating", typeof(int));
            dT.Columns.Add("Review", typeof(string));
            dT.Columns.Add("Positive_Negative", typeof(int));
            int rC = worksheet.Rows.Count;
            for (int i = 1; i < rC; i++)
            {
                string iD = worksheet.Cells[i, 0].Value?.ToString();
                string date = worksheet.Cells[i, 1].Value?.ToString();
                string wordCount = worksheet.Cells[i, 2].Value?.ToString();
                string rating = worksheet.Cells[i, 3].Value?.ToString();
                string review = worksheet.Cells[i, 4].Value?.ToString();
                string[] wordsInReview = review.Split(new[] { ' ', ',', '.', '!' }, StringSplitOptions.RemoveEmptyEntries);
                int check = CheckIfPositiveOrNegative(positive, negative, wordsInReview);
                switch (check)
                {
                    case 0:
                        lblUnCategorizedCount += 1;
                        lblUnCategorizedData.Text = lblUnCategorizedCount.ToString();
                        break;
                    case 1:
                        lblPositiveCount += 1;
                        lblPositiveData.Text = lblUnCategorizedCount.ToString();
                        break;
                    case 2:
                        lblNegativeCount += 1;
                        lblNegativeData.Text = lblNegativeCount.ToString();
                        break;
                }
                dT.Rows.Add(iD, date, wordCount, rating, review, check);
            }
            dataGridView1.DataSource = dT;
            return dT;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                return;
            }
            DataView dV = new DataView(finalDT);
            if (comboBox1.SelectedItem.Equals("Positive"))
            {

                dV.RowFilter = $"[Positive_Negative] = '{1}'";
                dataGridView1.DataSource = dV;
            }
            else if (comboBox1.SelectedItem.Equals("Negative"))
            {
                dV.RowFilter = $"[Positive_Negative] = '{2}'";
                dataGridView1.DataSource = dV;
            }
            else if (comboBox1.SelectedItem.Equals("Uncategorized"))
            {
                dV.RowFilter = $"[Positive_Negative] = '{0}'";
                dataGridView1.DataSource = dV;
            }
            else if (comboBox1.SelectedItem.Equals("All"))
            {
                dV.RowFilter = null;
                dataGridView1.DataSource = (finalDT);
                dataGridView1.Columns[4].Width = 865;

            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string cboValue = comboBox1.SelectedItem.ToString();

        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created By: Ralph Romano. Sources For Word Lists: Bing Liu, Minqing Hu and Junsheng Cheng.Opinion Observer: Analyzing and Comparing Opinions on the Web. Proceedings of the 14th International World Wide Web conference(WWW - 2005), May 10 - 14,  2005, Chiba, Japan");
        }
        private void btnCalc_Click(object sender, EventArgs e)
        {
            try
            {
                var filteredrows = finalDT.AsEnumerable()
                    .Where(row => row
                    .Field<string>("Review")
                    .Contains(textBox1.Text))
                    .CopyToDataTable();
                dataGridView1.DataSource = filteredrows;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            dV.RowFilter = null;
            dataGridView1.DataSource = (finalDT);
            dataGridView1.Columns[4].Width = 865;
        }
    }

}
