using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            openFileDialog1.Filter = "all files |*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] files = openFileDialog1.FileNames;
                int count = this.fileList.Count;
                
                
                for (int i = 0; i < files.Length;i++ )
                {
                    FileInfo fileInfo = new FileInfo(files[i]);
                    VideoFile v = new VideoFile();
                    v.FileName = fileInfo.Name;
                    v.FileSize = fileInfo.Length;
                    v.FileLocal = files[i];
                    v.FileIndex = count + i;
                    v.FileStatus = "等待上传";
                    v.UploadProgress = 0;

                    this.fileList.Add(v);
                }
                this.currentFile = this.fileList.ElementAt(0);
                addFileToList();
            }
        }

        //将文件添加至ListView
        public void addFileToList()
        {
            int count = this.fileList.Count;
            listView1.Items.Clear();
            listView1.BeginUpdate();
            for (int i = 0; i < count; i++)
            {
                ListViewItem lvi = new ListViewItem(fileList.ElementAt(i).FileIndex + "");
                lvi.SubItems.Add(fileList.ElementAt(i).FileSize+"KB");
                lvi.SubItems.Add(fileList.ElementAt(i).FileName);
                lvi.SubItems.Add(fileList.ElementAt(i).FileStatus);
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        //开始上传按钮事件
        private void button2_Click(object sender, EventArgs e)
        {
            if(this.fileList.Count != 0)
            {
                this.progressBar1.Minimum = 0;
                this.progressBar1.Maximum = 100;
                this.progressBar1.BackColor = Color.Red;

                for (int i = 0; i < 100; i++)
                {
                    this.label1.Text = this.currentFile.FileName + " " + i + "%";
                    this.progressBar1.Value++;

                    Application.DoEvents();
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //初始化表头
            listView1.Columns.Add("编号", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("大小", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("名称", 200, HorizontalAlignment.Center);
            listView1.Columns.Add("状态", 100, HorizontalAlignment.Center);

            this.fileList = new List<VideoFile>();
        }
    }
}
