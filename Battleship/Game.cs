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
            if (this.gameStage>0 && this.gameStage < 5) EnableAllButtons();
            else if (this.gameStage == 5) 
            {
                EnableAllEnemyButtons();
                DisableAllButtons();
            }
        }

        public string ReceiveShot(Int16 wier, Int16 kol)
        {
            Field hitted = GetField(wier, kol);
            if(hitted.shipType == "none")
            {
                hitted.Button.BackColor = Color.LimeGreen;
                hitted.isHit = true;
                return "0";
            }

            else
            {
                hitted.isHit = true;
                if (SearchShip(hitted.shipType))
                {
                    hitted.Button.BackColor = Color.IndianRed;
                    return "1";
                }
                else
                {
                    hitted.Button.BackColor = Color.LightGray;
                    return "2";
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
                    f.Button.Enabled = false;
                }
        }

        public void DisableAllEnemyButtons()
        {
            foreach (var list in enemyFields)
                foreach (Field f in list)
                {
                    f.Button.Enabled = false;
                }
        }

        public void EnableAllButtons()
        {
            foreach (var list in playerFields)
                foreach (Field f in list)
                {
                    f.Button.Enabled = true;
                }
        }

        public void EnableAllEnemyButtons()
        {
            foreach (var list in enemyFields)
                foreach (Field f in list)
                {
                    f.Button.Enabled = true;
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
    }
}