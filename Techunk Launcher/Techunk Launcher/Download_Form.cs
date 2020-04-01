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
    public partial class Download_Form : Form
    {
        public Download_Form()
        {
            InitializeComponent();
        }
        public void ChangeProgress(int p)
        {
            try
            {
                progressBar1.Value = p;
            }
            catch
            {

            }
        }
    }
}
