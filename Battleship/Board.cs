using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship
{
    class Board: Form1
    {
        List<Button> fields;
        int i = 1;
        public System.Windows.Forms.Button AddViewButton()
        {
            System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
            this.fields.Add(btn);
            btn.Top = i * 20;
            btn.Left = 50;
            btn.Text = "";
            i++;
            return btn;
        }

    }
}
