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
        string ipServer;
        string portServer;
        bool isPlayer = false;
        bool isGuest = false;

        public serverForm()
        {
            InitializeComponent();
        }

        private void connectBut_Click(object sender, EventArgs e)
        {
            ipServer = textBox1.Text;
            portServer = textBox2.Text;
            if((textBox1.Text =="") || (textBox2.Text == "") MessageBox.Show("Nieprawidłowa wartość");
            if (radioButton1.Checked) isPlayer = true;
            if (radioButton2.Checked) isGuest = true;
            else MessageBox.Show("Musisz wybrać!");
        }
    }
}
