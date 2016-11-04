using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TelecomClientC
{
   
    public partial class Form1 : Form
    {
        TelecomClient client;
        public Form1()
        {
            InitializeComponent();
        }

        public void connectUDP()
        {
            int udpTimeoutCount = 0;
            while (client.connected == false)
            {
                Console.WriteLine("Connecting to server...");
                client.connect(txtMyName.Text);
                Thread.Sleep(2000);
                udpTimeoutCount += 1;
                if (udpTimeoutCount > 5)
                {
                    break;
                }
            }
            client.connect2();
            Console.WriteLine("Connection success");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            client = new TelecomClient(txtServerIP.Text);
            Thread trd = new Thread(connectUDP);
            trd.Start();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            client.sendMessageTCP("userList");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            client.sendMessageTCP("connect:" + this.TextBox1.Text);
        }

    }
}
