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
            AddFields();
            AddEnemyFields();
        }

        private void player1fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
            click.BackColor = Color.Green;
            foreach (Field search in this.game.playerFields)
            {
                if (search.Button==click)
                {
                    field = search;
                    break;
                }
            }
        
        click.Text = field.kol + ":" + field.wier;
        }

        Game game = new Game();
        public void AddFields()
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

            this.game.playerFields = listaField;

        }

        private void player2fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
            click.BackColor = Color.Green;
            foreach (Field search in this.game.enemyFields)
            {
                if (search.Button == click)
                {
                    field = search;
                    break;
                }
            }

            click.Text = field.kol + ":" + field.wier;
        }

        public void AddEnemyFields()
        {
            List<Field> listaField = new List<Field>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int bok = 45;
                    System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                    Field field = new Field(i, j);
                    System.Drawing.Size size = new System.Drawing.Size(bok, bok);
                    btn.Size = size;
                    btn.Top = 75 + i * bok;
                    btn.Left = 508 + j * bok;
                    btn.Name = i.ToString() + "." + j.ToString();
                    btn.Text = "";
                    btn.Click += new EventHandler(this.player2fields);
                    field.Button = btn;
                    listaField.Add(field);
                    this.Controls.Add(listaField.Last().Button);
                }
            }
            this.game.enemyFields = listaField;
        }

        private void serverBut_Click(object sender, EventArgs e)
        {
            using (serverForm form = new serverForm())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.IsPlayer)
                    {
                        game.gameStage = 1;
                    }
                    else game.gameStage = -1;
                    int i = 0;
                    if (!Int32.TryParse(form.PortServer, out i)) 
                    {
                        i = -1;
                    }
                    game.ConnectToServer(form.IpServer, i);

                }
                else
                {
                    MessageBox.Show("Wystapil blad");
                    return;
                }
            }
        }

        public void placeShip()
        {
            foreach(Button field )
        }
    }
}
