using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship
{
    public partial class serverForm : Form
    {
        private string ipServer;
        private string portServer;
        private bool isPlayer = false;
        private bool isGuest = false;

        public string IpServer
        {
            get { return ipServer; }
            set { ipServer = value; }
        }

        public string PortServer
        {
            get { return portServer; }
            set { portServer = value; }
        }

        public bool IsPlayer
        {
            get { return isPlayer; }
            set { isPlayer = value; }
        }

        public bool IsGuest
        {
            get { return isGuest; }
            set { isGuest = value; }
        }


        public serverForm()
        {
            InitializeComponent();
        }

        private void connectBut_Click(object sender, EventArgs e)
        {
            ipServer = textBox1.Text;
            portServer = textBox2.Text;
            if ((textBox1.Text == "") || (textBox2.Text == ""))
            {
                MessageBox.Show("Nieprawidłowa wartość");
                return;
            }
            if (radioButton1.Checked) isPlayer = true;
            else if (radioButton2.Checked) isGuest = true;
            else
            {
                MessageBox.Show("Musisz wybrać!");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();

        }
    }
}
