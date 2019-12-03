using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Battleship
{
    class Field
    {
        public System.Windows.Forms.Button Button { set; get; }

        public int kol { get; set; }
        public int wier { get; set; }

        public Field(int kol_, int wier_)
        {
            kol = kol_;
            wier = wier_;
        }
    }
}
