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

        public Form1()
        {
            InitializeComponent();
        }



        private void player1fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
            click.BackColor = Color.Green;
            foreach (Field search in this.fields)
            {
                if (search.Button==click)
                {
                    field = search;
                    break;
                }
            }
        
        click.Text = field.kol + ":" + field.wier;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void a1_Click(object sender, EventArgs e)
        {

        }

        private void fieldsBut_Click(object sender, EventArgs e)
        {
            AddViewButton();
        }

        List<Field> fields;

        public void AddViewButton()
        {
            List<Field> listaField = new List<Field>();
            for (int i=0;i<8;i++)
            {
                for (int j=0;j<8;j++)
                {
                    int bok = 45;
                    System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                    Field field = new Field(i,j);
                    System.Drawing.Size size = new System.Drawing.Size(bok,bok);
                    btn.Size = size;
                    btn.Top = 75 + i * bok;
                    btn.Left = 45 + j * bok;
                    btn.Name = i.ToString() +"."+ j.ToString();
                    btn.Text = "";
                    btn.Click += new EventHandler(this.player1fields);
                    field.Button = btn;
                    listaField.Add(field);
                    this.Controls.Add(listaField.Last().Button);
                }
            }

            this.fields = listaField;

        }

        private void serverBut_Click(object sender, EventArgs e)
        {
            serverForm server = new serverForm();
            server.ShowDialog();
        }
    }
}
