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
    public partial class Form1 : Form
    {
        bool isShipPPlaced = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void a1_MouseClick(object sender, MouseEventArgs e)
        {
            a1.Text = "";
            a1.BackColor = Color.Green;
        }
    }
}
