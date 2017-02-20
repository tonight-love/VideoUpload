using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
                if (fileList.ElementAt(i).FileStatus.Equals("等待上传"))
                {
                    lvi.BackColor = Color.Yellow;
                }
                if (fileList.ElementAt(i).FileStatus.Equals("正在上传"))
                {
                    lvi.BackColor = Color.Orange;
                }
                if (fileList.ElementAt(i).FileStatus.Equals("上传完成"))
                {
                    lvi.BackColor = Color.GreenYellow;
                }
                
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
            if (this.fileList.Count != 0)
            {
                int count = this.fileList.Count();
                for (int i = 0; i < count;i++ )
                {
                    //上传初始化
                    jsonout json = uploadInit(i);
                    while (!json.code.Equals("0"))
                    {
                        Console.Write("code = 0!  初始化失败!");
                        json = uploadInit(i);
                    }
                    Console.Write("初始化成功!");
                    this.label1.Text = "初始化成功!";

                    this.button1.Hide();
                    this.button2.Hide();
                    this.ControlBox = false;

                    FileStream file = new FileStream(this.fileList.ElementAt(i).FileLocal, FileMode.Open);
                    string suburl = json.data.upload_url.Substring(0, (json.data.upload_url.Length - 10));
                    Uri url = new Uri(suburl);

                    //进度条初始化
                    prograssBarInit();
                    this.currentFile = this.fileList.ElementAt(i);

                    this.fileList.ElementAt(i).FileStatus = "正在上传";
                    addFileToList();

                    //开始上传
                    string uploadresult = uploadPost(url, file, json.data.token);

                    string res = sendVideoToSite(json.data.video_id, json.data.video_unique, this.fileList.ElementAt(i).FileName);
                    if (!res.Equals("ok"))
                    {
                        this.fileList.ElementAt(i).FileStatus = "上传失败";
                        addFileToList();
                        break;
                    }
                    else
                    {
                        this.fileList.ElementAt(i).FileStatus = "上传完成";
                        addFileToList();
                        this.label1.Text = "上传成功";
                    }

                    //this.fileList.ElementAt(i).FileStatus = "上传完成";
                    //addFileToList();
                    //this.label1.Text = "上传成功";

                    Application.DoEvents();
                }
                this.button1.Show();
                this.button2.Show();
                this.ControlBox = true;
                Application.Exit();
            }
        }

        public string sendVideoToSite(string video_id, string video_uuid, string video_name)
        {
            string uri = "http://211.149.188.34/VideoManager/UploadVideo";
            string token = "123456789";
            string param = "videoInfo=" + video_id + "_" + video_uuid + "_" + video_name;
            param += "&token=" + token;
            string res = LetvCloud.doGet(uri + "?" + param);

            if (res.Equals("ok"))
            {
                res = "ok";
            }
            else
            {
                res = "error";
            }

            return res;
        }

        public string uploadPost(Uri uri, FileStream file, string token)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AllowWriteStreamBuffering = false;
            //设置获得响应的超时时间（300秒） 
            request.Timeout = 300000;
            string strPostHeader = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + System.Environment.NewLine + "Content-Type: application/octet-stream"
                        + System.Environment.NewLine + System.Environment.NewLine;
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endline = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            long length = file.Length + postHeaderBytes.Length + boundaryBytes.Length;
            long fileLength = file.Length;
            request.ContentLength = length;
            MemoryStream stream = new MemoryStream();
            //提交文件
            try
            {
                //每次上传4K
                int bufferLength = 4028;
                byte[] buffer = new byte[bufferLength];
                DateTime startTime = DateTime.Now;
                //已上传字节数
                BinaryReader r = new BinaryReader(file);
                long offset = 0;
                int size = r.Read(buffer, 0, bufferLength);
                Stream postStream = request.GetRequestStream();
                //发送请求头部消息 
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    TimeSpan span = DateTime.Now - startTime;
                    double second = span.TotalSeconds;
                    String speed = (offset / 1024 / second).ToString("0.00");
                    String progress = "已上传：" + (offset * 100.0 / length).ToString("F2") + "%";
                    String sumprogress = (offset / 1048576.0).ToString("F2") + "M/" + (fileLength / 1048576.0).ToString("F2") + "M";
                    size = r.Read(buffer, 0, bufferLength);
                    Console.Write(progress);
                    Console.Write(System.Environment.NewLine);

                    this.progressBar1.Value = (int)(offset * 100.0 / length);
                    this.label1.Text = this.currentFile.FileName;

                    Application.DoEvents();
                }
                postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                postStream.Close();
                WebResponse webRespon = request.GetResponse();
                Stream s = webRespon.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                String sReturnString = sr.ReadLine();
                s.Close();
                sr.Close();
                Console.Write("上传成功!");

                //return Encoding.ASCII.GetBytes(sReturnString);
                return sReturnString;
            }
            //error then write error log file
            catch
            {
                LetvCloud.writeToken("", token);
                //byte[] wrongResult = Encoding.ASCII.GetBytes("");
                string wrongResult = "";
                return wrongResult;
            }
        }

        //进度条初始化
        public void prograssBarInit()
        {
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 100;
            this.progressBar1.BackColor = Color.Red;
        }

        //上传初始化
        public jsonout uploadInit(int i)
        {
            String result = LetvCloud.videoUploadInit(this.fileList.ElementAt(i).FileName, "192.168.1.1");
            jsonout json = LetvCloud.jsonGet(result);
            
            return json;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //初始化表头
            listView1.Columns.Add("编号", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("大小", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("名称", 250, HorizontalAlignment.Center);
            listView1.Columns.Add("状态", 100, HorizontalAlignment.Center);

            this.fileList = new List<VideoFile>();
        }
    }
}
