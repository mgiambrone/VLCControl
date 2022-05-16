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


namespace VLCControl
{
    public partial class mainForm : Form
    {
        int count1 = 0;
        int count2 = 0;
        string title = "";

        public mainForm()
        {
            m_vlcControl = new VLCRemoteController(); 
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            foreach (Control control in this.Controls)
            {
                control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
            }
        }

        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            m_vlcControl.sendCustomCommand("get_title");
            label5.Text = m_vlcControl.reciveAnswer();
            if (label5.Text != title)
            {
                txbLog.AppendText(count1 + " " + count2 + " " + title +"\r\n");
                title = label5.Text;
                count1 = 0;
                count2 = 0;
            }
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
            if (e.KeyCode == Keys.Space)
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
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Int32 port = 4444;
            TcpClient client = new TcpClient("127.0.0.1", port);
            NetworkStream stream = client.GetStream();
           // txbLog.AppendText("Sent help command\r\n");
            Byte[] data = System.Text.Encoding.UTF8.GetBytes("help\n");
            stream.Write(data, 0, data.Length);

            data = new Byte[2560];
            String responseData = String.Empty;            
            Int32 bytes = 0;
            while (client.Available > 0)
            {                
                bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
               // txbLog.AppendText(responseData);
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
            m_vlcControl.connect("127.0.0.1", 4444);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_vlcControl.disconnect();
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
    }
}
