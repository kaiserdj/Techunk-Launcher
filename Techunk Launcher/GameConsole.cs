using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Techunk_Launcher
{
    public partial class GameConsole : Form
    {
        public GameConsole()
        {
            InitializeComponent();
        }

        public void AddLog(string msg)
        {
            richTextBox1.AppendText(msg + "\n");
            richTextBox1.ScrollToCaret();
        }
    }
}
