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

            GameStageCheck();
        }

        Game game = new Game();
        Serwer serwer = new Serwer();

        private void NextGameStage()
        {
            game.NextGameStage();
            GameStageCheck();
        }

        private void ChangeGameStage(int nr)
        {
            game.ChangeGameStage(nr);
            GameStageCheck();
        }

        private void GameStageCheck()
        {
            switch (game.gameStage)
            {
                case -1:
                    ordersLabel.Text = "You are only guest";
                    break;
                case 0:
                    ordersLabel.Text = "You have to choose your server and gamemode";
                    break;
                case 1:
                    ordersLabel.Text = "Place: Battleship";
                    break;

                case 2:
                    ordersLabel.Text = "Place: Destroyer";
                    break;

                case 3:
                    ordersLabel.Text = "Place: Destroyer";
                    break;

                case 4:
                    ordersLabel.Text = "Place: Submarine";
                    break;

                case 5:
                    break;

                default:
                    ordersLabel.Text = "IN PROGRESS";
                    break;
            }
        }

        private void player1fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
            //click.BackColor = Color.Green;
            foreach (var list in this.game.playerFields)
            {
                foreach (Field search in list)
                {
                    if (search.Button == click)
                    {
                        field = search;
                        break;
                    }
                }
            }

            click.Text = field.kol + ":" + field.wier;

            if(game.placingShip)
            {
                game.FillShip(field, game.lastField);
                game.CleanEmpty();
                game.placingShip = false;
                NextGameStage();
            }
            else
            {
                switch (game.gameStage)
                {
                    case 1:
                        game.PlaceShip(field, 4, "D");
                        break;

                    case 2:
                        game.PlaceShip(field, 3, "C");
                        break;

                    case 3:
                        game.PlaceShip(field, 3, "B");
                        break;

                    case 4:
                        game.PlaceShip(field, 2, "A");
                        break;

                    default:
                        game.DisableAllButtons();
                        break;
                }
            }
                 
       
                game.lastField = field;

        }

        private void AddFields()
        {
            List<List<Field>> listaField = new List<List<Field>>();
            for (int i = 0; i < 8; i++)
            {
                listaField.Add(new List<Field>());
                for (int j=0;j<8;j++)
                {
                    int bok = 45;
                    System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                    Field field = new Field(j,i);
                    System.Drawing.Size size = new System.Drawing.Size(bok,bok);
                    btn.Size = size;
                    btn.Top = 75 + i * bok;
                    btn.Left = 45 + j * bok;
                    btn.Name = i.ToString() +"."+ j.ToString();
                    btn.Text = "";
                    btn.Click += new EventHandler(this.player1fields);
                    btn.Enabled = false;
                    field.Button = btn;
                    listaField.Last().Add(field);
                    this.Controls.Add(listaField.Last().Last().Button);
                }
            }
            this.game.playerFields = listaField;

        }

        private void player2fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
            click.BackColor = Color.Green;
            foreach (var list in this.game.enemyFields)
            {
                foreach (Field search in list)
                {
                    if (search.Button == click)
                    {
                        field = search;
                        break;
                    }
                }
            }

            if (game.gameStage < 5)
            {
            
            }
            else
            {
                switch (game.gameStage)
                {
                    case 10:
                        
                        break;

                    case 11:
                        
                        break;

                    case 12:

                        break;

                    case 13:

                        break;

                    default:
                        break;
                }
            }

            click.Text = field.kol + ":" + field.wier;


        }

        private void AddEnemyFields()
        {
            List<List<Field>> listaField = new List<List<Field>>();
            for (int i = 0; i < 8; i++)
            {
                listaField.Add(new List<Field>());
                for (int j = 0; j < 8; j++)
                {
                    int bok = 45;
                    System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
                    Field field = new Field(j,i);
                    System.Drawing.Size size = new System.Drawing.Size(bok, bok);
                    btn.Size = size;
                    btn.Top = 75 + i * bok;
                    btn.Left = 508 + j * bok;
                    btn.Name = i.ToString() + "." + j.ToString();
                    btn.Text = "";
                    btn.Click += new EventHandler(this.player2fields);
                    btn.Enabled = false;
                    field.Button = btn;
                    listaField.Last().Add(field);
                    this.Controls.Add(listaField.Last().Last().Button);
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
                    int i = 0;
                    if (!Int32.TryParse(form.PortServer, out i)) 
                    {
                        i = -1;
                    }
                    if (!serwer.ConnectToServer(form.IpServer, i)) return;
                    Button click = sender as Button;
                    click.Enabled = false;
                    if (form.IsPlayer)
                        ChangeGameStage(1);
                    else
                        ChangeGameStage(-1);

                }
                else
                {
                    MessageBox.Show("Wystapil blad");
                    return;
                }
            }
            
        }



    }
}
