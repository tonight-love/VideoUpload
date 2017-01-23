using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoUpload
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.progressBar1.Minimum = 0;
            //this.progressBar1.Maximum = 100;
            //this.progressBar1.BackColor = Color.Red;

            //for (int i = 0; i < 100;i++ )
            //{
            //    this.progressBar1.Value++;
            //    Application.DoEvents();
            //}

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "openFileDialog1";
            ofd.Multiselect = true;
        }

        
    }
}
