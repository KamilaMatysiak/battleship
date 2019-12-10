using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Battleship
{
    struct Coords
    {
        public int kol;
        public int wier;
    }
    class Field
    {
        public System.Windows.Forms.Button Button { set; get; }

        public int kol { get; set; }
        public int wier { get; set; }
        public bool isHit = false;
        public string shipType;
        public int index { get; private set; }


        public Field(int kol_, int wier_)
        {
            kol = kol_;
            wier = wier_;
            shipType = "none";
            index = wier * 8 + kol;
        }

        public Coords GetCoords()
        {
            Coords coords = new Coords
            {
                kol = kol,
                wier = wier
            };
            return coords;
        }
    }
}
