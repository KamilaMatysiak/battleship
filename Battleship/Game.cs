using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship
{
    class Game
    {
        public Field lastField;
        public List<List<Field>> playerFields;
        public List<List<Field>> enemyFields;
        public Button connect;
        public Button startGame;
        public Label info;
        public bool placingShip = false;
        public int gameStage { get;private set; }

        public Game()
        {
            gameStage = 0;
        }

        public void ChangeGameStage(int nr)
        {
            gameStage = nr;
            GameStageCheck();
        }

        public void NextGameStage()
        {
            gameStage++;
            GameStageCheck();
        }

        private void GameStageCheck()
        {
            if (gameStage != 0)
                DisableButton(connect);
            if (gameStage != 9)
                DisableButton(startGame);

            switch (gameStage)
            {
                case -1:
                    ChangeText(info,"You are only guest");
                    break;
                case 0:
                    info.Text = "You have to choose your server and gamemode";
                    break;
                case 1:
                    ChangeText(info, "Place: Battleship");
                    break;

                case 2:
                    info.Text = "Place: Destroyer";
                    break;

                case 3:
                    info.Text = "Place: Destroyer";
                    break;

                case 4:
                    info.Text = "Place: Submarine";
                    break;
                case 5:
                    ChangeGameStage(9);
                    break;

                case 9:
                    EnableButton(startGame);
                    break;

                case 10:
                    EnableAllEnemyButtons();
                    break;

                default:
                    info.Text = "IN PROGRESS";
                    break;
            }
            if (this.gameStage>0 && this.gameStage < 9) EnableAllButtons();
            else if (this.gameStage == 9) 
            {
                DisableAllButtons();
            }
        }
        public void GameBegin()
        {
        }
        public void ShotResult(Field hitted, char result)
        {
            hitted.isHit = true;
            if (result == '0')
            {
                hitted.Button.BackColor = Color.LimeGreen;
            }
            else if (result == '1')
                hitted.Button.BackColor = Color.IndianRed;
            else
                hitted.Button.BackColor = Color.DarkRed;
        }
        public char ReceiveShot(Int16 wier, Int16 kol)
        {
            Field hitted = GetField(wier, kol);
            if(hitted.shipType == "none")
            {
                hitted.Button.BackColor = Color.LimeGreen;
                hitted.isHit = true;
                return '0';
            }

            else
            {
                hitted.isHit = true;
                if (SearchShip(hitted.shipType))
                {
                    hitted.Button.BackColor = Color.IndianRed;
                    return '1';
                }
                else
                {
                    hitted.Button.BackColor = Color.DarkRed;
                    return '2';
                }
            }
            
            //0 to nietrafiony, 1 to trafiony, 2 to trafiony i zatopiony
        }

        private bool SearchShip(String s)
        {
            foreach (var list in this.playerFields)
            {
                foreach (Field search in list)
                {
                    if((search.shipType == s) && (!search.isHit))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        public void PlaceShip(Field field, int size, string ship)
        {
            bool block = true;
            
            size = size - 1;
            if (!(field.shipType == "none")) return;
            DisableAllButtons();
            field.shipType = ship;
            if (field.kol + size < 8)
            {
                for (int i = field.kol + 1; i <= field.kol + size; i++)
                    if (playerFields[field.wier][i].shipType != "none") block = false;

                if (block)
                {
                    EnableButton(playerFields[field.wier][field.kol+size]);
                    playerFields[field.wier][field.kol + size].Button.BackColor = Color.Aquamarine;
                    placingShip = true;
                }
            }
            block = true;
            if (field.kol - size >= 0)
            {
                for (int i = field.kol - 1; i >= field.kol - size; i--)
                    if (playerFields[field.wier][i].shipType != "none") block = false;

                if (block)
                {
                    EnableButton(playerFields[field.wier][field.kol - size]);
                    playerFields[field.wier][field.kol - size].Button.BackColor = Color.Aquamarine;
                    placingShip = true;
                }
            }
            block = true;
            if (field.wier + size < 8)
            {
                for (int i = field.wier + 1; i <= field.wier + size; i++)
                    if (playerFields[i][field.kol].shipType != "none") block = false;

                if (block)
                {
                    EnableButton(playerFields[field.wier + size][field.kol]);
                    playerFields[field.wier + size][field.kol].Button.BackColor = Color.Aquamarine;
                    placingShip = true;
                }
            }
            block = true;
            if (field.wier - size >= 0)
            {
                for (int i = field.wier - 1; i >= field.wier - size; i--)
                    if (playerFields[i][field.kol].shipType != "none") block = false;

                if (block)
                {
                    EnableButton(playerFields[field.wier - size][field.kol]);
                    playerFields[field.wier - size][field.kol].Button.BackColor = Color.Aquamarine;
                    placingShip = true;
                }
            }

            if (!placingShip)
            {
                EnableAllButtons();
                CleanEmpty();
            }
        }

        public void FillShip(Field field, Field shipStart)
        {
            if (field.kol == shipStart.kol)
            {
                int begin = Math.Min(field.wier, shipStart.wier);
                int end = Math.Max(field.wier, shipStart.wier);
                for (int i = begin;i <= end; i++)
                {
                    playerFields[i][field.kol].Button.BackColor = Color.Purple;
                    playerFields[i][field.kol].shipType = shipStart.shipType;
                }
            }
            else if (field.wier == shipStart.wier)
            {
                int begin = Math.Min(field.kol, shipStart.kol);
                int end = Math.Max(field.kol, shipStart.kol);
                for (int i = begin; i <= end; i++)
                {
                    playerFields[field.wier][i].Button.BackColor = Color.Purple;
                    playerFields[field.wier][i].shipType = shipStart.shipType;
                }
            }
        }

        public void CleanEmpty()
        {
            foreach (var list in playerFields)
                foreach (Field f in list)
                {
                    if (f.shipType == "none")
                        f.Button.BackColor = Color.White;
                }
        }

        public void EnableButton(Field field)
        {
            field.Button.Enabled = true;            
        }

        public void DisableAllButtons()
        {
            foreach(var list in playerFields)
                foreach (Field f in list)
                {
                    DisableButton(f.Button);
                }
        }

        public void DisableAllEnemyButtons()
        {
            foreach (var list in enemyFields)
                foreach (Field f in list)
                {
                    DisableButton(f.Button);
                }
        }

        public void EnableAllButtons()
        {
            foreach (var list in playerFields)
                foreach (Field f in list)
                {
                    EnableButton(f.Button);
                }
        }

        public void EnableAllEnemyButtons()
        {
            foreach (var list in enemyFields)
                foreach (Field f in list)
                {
                    EnableButton(f.Button);
                }
        }

        public Field GetField(Coords coords)
        {
            return playerFields[coords.wier][coords.kol];
        }
        public Field GetField(int wier, int kol)
        {
            return playerFields[wier][kol];
        }
        public Field GetEnemyField(Coords coords)
        {
            return enemyFields[coords.wier][coords.kol];
        }
        public Field GetEnemyField(int wier, int kol)
        {
            return enemyFields[wier][kol];
        }

        public void DisableButton(Button b)
        {
            //Check if invoke requied if so return - as i will be recalled in correct thread
            if (ControlInvokeRequired(b, () => DisableButton(b))) return;
            b.Enabled = false;
        }
        public void EnableButton(Button b)
        {
            //Check if invoke requied if so return - as i will be recalled in correct thread
            if (ControlInvokeRequired(b, () => EnableButton(b))) return;
            b.Enabled = true;
        }
        public void ChangeText(Control b, string text)
        {
            //Check if invoke requied if so return - as i will be recalled in correct thread
            if (ControlInvokeRequired(b, () => ChangeText(b,text))) return;
            b.Text = text;
        }
        public bool ControlInvokeRequired(Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate { a(); }));
            else return false;

            return true;
        }
    }
}