using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

namespace VLCControl
{
    public partial class mainForm : Form
    {
        int count1 = 0;
        int count2 = 0;
        int videolength = 0;
        string title = "";
        int vlcid;
        DataTable dt = new DataTable();

        public mainForm()
        {
            m_vlcControl = new VLCRemoteController(); 
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            foreach (Control control in this.Controls)
            {
                control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
            }
            dt.Columns.AddRange(new DataColumn[5] { 
                new DataColumn("IN", typeof(int)),
                new DataColumn("OUT", typeof(int)),
                new DataColumn("Sum", typeof(int)),
                new DataColumn("Filename",typeof(string)) ,
                new DataColumn("Time", typeof(int))
            });
            

            this.dataGridView1.DataSource = dt;
            this.dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Columns[0].Width = 35;
            dataGridView1.Columns[1].Width = 35;
            dataGridView1.Columns[2].Width = 35;
            dataGridView1.Columns[3].Width = 260;
            dataGridView1.RowHeadersVisible = false;
            foreach(Control ctrl in this.Controls)
            {
                ctrl.GotFocus += Ctrl_GotFocus;
                ctrl.LostFocus += Ctrl_LostFocus;
            }
        }

        private void Ctrl_LostFocus(object sender, EventArgs e)
        {
            button1.BackColor = Color.Red;
            button3.BackColor = Color.Red;
        }

        private void Ctrl_GotFocus(object sender, EventArgs e)
        {
            button1.BackColor = Color.LightGray;
            button3.BackColor = Color.LightGray;
        }

        private Timer timer1;
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 200; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string path = @"c:\kld\countlog.csv";
            
            if (label5.Text != Process.GetProcessById(vlcid).MainWindowTitle.Split('-')[0])
            {                
                

                if (label5.Text != "" && !label5.Text.Contains("VLC media player"))
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(count1 + "," + count2 + "," + (count1 + count2) + "," + label5.Text + "\r\n");
                    }
                    txbLog.AppendText(count1 + " " + count2 + " " + (count1 + count2) + " " + label5.Text + "\r\n");
                    dt.Rows.Add(count1, count2, count1 + count2, label5.Text, videolength);
                    dataGridView1.FirstDisplayedCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0];
                }
                label5.Text = Process.GetProcessById(vlcid).MainWindowTitle.Split('-')[0];
                count1 = 0;
                count2 = 0;
                button1.Text = "0";
                button3.Text = "0";
            }
            else if(m_vlcControl.sendCustomCommand("get_length"))
            {
                int n;
                Int32.TryParse(m_vlcControl.reciveAnswer(), out n);
                if (n > 0)
                    videolength = n;

                // txbLog.AppendText("RECIVE:\r\n" + m_vlcControl.reciveAnswer() + "\r\n");
            }
        }

        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                e.IsInputKey = true;
            }
            if (e.KeyCode == Keys.OemOpenBrackets || e.KeyCode == Keys.Left)
            {
                button1.Text = (++count1).ToString();
            }
            if (e.KeyCode == Keys.OemCloseBrackets || e.KeyCode == Keys.Right)
            {
                button3.Text = (++count2).ToString();
            }
            if (e.KeyCode == Keys.P)
            {
                m_vlcControl.sendCustomCommand("pause");
            }
            if (e.KeyCode == Keys.Oemplus)
            {
                m_vlcControl.sendCustomCommand("faster");
            }
            if (e.KeyCode == Keys.OemMinus)
            {
                m_vlcControl.sendCustomCommand("slower");
            }
            if (e.KeyCode == Keys.C)
            {
                count1 = 0;
                count2 = 0;
                
                button1.Text = count1.ToString();
                button3.Text = count2.ToString();
            }
            if (e.KeyCode == Keys.Back)
            {
                if (dt.Rows.Count > 0)
                {
                    timer1.Stop();
                    m_vlcControl.sendCustomCommand("prev");
                    DataRow row = dt.Rows[dt.Rows.Count - 1];
                    count1 = (int)row[0];
                    count2 = (int)row[1];

                    label5.Text = (string)row[3];
                    button1.Text = count1.ToString();
                    button3.Text = count2.ToString();
                    dt.Rows.Remove(row);
                    timer1.Start();
                }

            }
            //m_vlcControl.sendCustomCommand("@name marq-marquee \"" + count1 + " " + count2 + "\"");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Int32 port = 4444;
            TcpClient client = new TcpClient("127.0.0.1", port);
            NetworkStream stream = client.GetStream();
            txbLog.AppendText("Sent help command\r\n");
            Byte[] data = System.Text.Encoding.UTF8.GetBytes("help\n");
            stream.Write(data, 0, data.Length);

            data = new Byte[2560];
            String responseData = String.Empty;            
            Int32 bytes = 0;
            while (client.Available > 0)
            {                
                bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                txbLog.AppendText(responseData);
            }
            stream.Close();
            client.Close();
        }

        private void btn_runVLC_Click(object sender, EventArgs e)
        {
            String exePath = m_vlcControl.getVLCExe();
            if (!String.IsNullOrEmpty(exePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = @"--control=rc --rc-host 127.0.0.1:4444";
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show("VLC is not found on PC. Please ran it from command line:\r\nvlc.exe --control=rc --rc-host 127.0.0.1:4444", "Cannot find VLC",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
            }
        }

        private VLCRemoteController m_vlcControl;

        private void btnConnect_Click(object sender, EventArgs e)
        {

            InitTimer();
            m_vlcControl.connect("127.0.0.1", 4444);
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.MainWindowTitle.Contains("VLC media player"))
                    {
                        btnConnect.Enabled = false;
                        btnConnect.BackColor = Control.DefaultBackColor;
                        Console.WriteLine("Connected: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                        vlcid = process.Id;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           // m_vlcControl.disconnect();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //txbLog.AppendText("SEND:" + txbCommand.Text + "\r\n");
            if (m_vlcControl.sendCustomCommand(txbCommand.Text))
            {
             //   txbLog.AppendText("RECIVE:\r\n" + m_vlcControl.reciveAnswer() + "\r\n");
            }
        }

        private void txbCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, e);
            }            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }


        private void buttonSaveTable_Click(object sender, EventArgs e)
        {
            //Table start.
            string html = "<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:arial'>";

            //Adding HeaderRow.
            html += "<tr>";
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>" + column.HeaderText + "</th>";
            }
            html += "</tr>";

            //Adding DataRow.
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                html += "<tr>";
                foreach (DataGridViewCell cell in row.Cells)
                {
                    html += "<td style='width:120px;border: 1px solid #ccc'>" + cell.Value.ToString() + "</td>";
                }
                html += "</tr>";
            }

            //Table end.
            html += "</table>";

            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;            

            File.WriteAllText(@"c:\kld\DataGridView-" + secondsSinceEpoch + ".htm", html);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 500,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400, Text= " " };
                Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
            label1.Text = Prompt.ShowDialog("Enter count name", "Enter count name");
            if (label1.Text != "")
                dt.Columns[0].ColumnName = label1.Text;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            label2.Text = Prompt.ShowDialog("Enter count name", "Enter count name");
            if(label2.Text != "")
                dt.Columns[1].ColumnName = label2.Text;
        }
    }
}
