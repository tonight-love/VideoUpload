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
            listView1.View = View.Details;
        }

        //选择视频按钮事件
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();     //显示选择文件对话框
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowDialog();
        }

        //开始上传按钮事件
        private void button2_Click(object sender, EventArgs e)
        {
            //this.progressBar1.Minimum = 0;
            //this.progressBar1.Maximum = 100;
            //this.progressBar1.BackColor = Color.Red;

            //for (int i = 0; i < 100; i++)
            //{
            //    this.progressBar1.Value++;
            //    Application.DoEvents();
            //}

            

            listView1.BeginUpdate();

            

            

            for (int i = 0; i < 10;i++ )
            {
                listView1.Items.Add("row1"+i, i+"", i);
                listView1.Items["row1" + i].SubItems.Add("200KB" + i);
                listView1.Items["row1" + i].SubItems.Add("test.mp4" + i);
                

               

                listView1.Items["row1" + i].SubItems.Add("4" + i);
                listView1.Items["row1" + i].BackColor = Color.Green;

                listView1.Items["row1" + i].SubItems.Add("等待上传" + i);
            }

            listView1.EndUpdate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //初始化表头
            listView1.Columns.Add("编号", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("大小", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("名称", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("进度", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("状态", 100, HorizontalAlignment.Center);
        }
    }
}
