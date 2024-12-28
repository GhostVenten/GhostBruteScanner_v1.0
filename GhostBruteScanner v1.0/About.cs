using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GhostBruteScanner_v1._0
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void scanbutton_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/FD3CHPkaZV");
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Ghostooo6");

        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            Process.Start("https://ghostdev.rf.gd/");
        }

        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.instagram.com/sharim_.ali/");
        }
    }
}
