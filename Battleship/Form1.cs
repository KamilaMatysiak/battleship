using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
//hello there
namespace Battleship
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            game = new Game();
            connect = new Connection(game);
            AddFields();
            AddEnemyFields();
            AddOther();
            GameStageCheck();
        }

        Game game;
        Connection connect;
        private void NextGameStage()
        {
            game.NextGameStage();
        }

        private void ChangeGameStage(int nr)
        {
            game.ChangeGameStage(nr);
        }

        private void GameStageCheck()
        {
            game.ChangeGameStage(game.gameStage);
        }

        private void player1fields(object sender, EventArgs e)
        {
            Button click = sender as Button;
            Field field = null;
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

            click.Text = field.wier + ":" + field.kol;

            if (game.placingShip)
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

            if(game.gameStage>5)
            {
                //connect.ReceiveShot(/*tu powinno być coś */);
                //game.ReceiveShot();
            }
            

        }
        private void AddOther()
        {
            game.connect = serverBut;
            game.info = ordersLabel;
            game.startGame = startGame;
            game.endGame = KoniecGry;
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

            game.DisableAllEnemyButtons();
            connect.SendShot(field);

            click.Text = field.wier + ":" + field.kol;


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
                    if (!connect.ConnectToServer(form.IpServer, i, form.IsPlayer))
                    {
                        ordersLabel.Text = "Nie polaczyles sie z serwerem, sprobuj ponownie.";
                        return;
                    }

                }
                else
                {
                    MessageBox.Show("Wystapil blad");
                    return;
                }
            }
            
        }

        private void startGame_Click(object sender, EventArgs e)
        {
            connect.GameBegin();
        }

        private void KoniecGry_Click(object sender, EventArgs e)
        {

            KoniecGry.Enabled = false;
            connect.sendGameEnd();
        }
    }
}
