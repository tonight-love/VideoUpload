using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Data;
using System.Windows.Forms;
namespace VideoUpload
{
    public struct jsonout
    {
        public string code { get; set; }
        public string message { get; set; }
        public string total { get; set; }
        public jsonin data;
    }
    public struct jsonin
    {
        public string video_id { get; set; }
        public string video_unique { get; set; }
        public string upload_url { get; set; }
        public string progress_url { get; set; }
        public string token { get; set; }
        public string uploadtype { get; set; }
        public string isdrm { get; set; }
    }
    public struct jsonResume
    {
        public string code { get; set; }
        public string message { get; set; }
        public string total { get; set; }
        public jsonResumein data;
        public string status { get; set; }
    }

    public struct jsonResumein
    {
        public string upload_url { get; set; }
        public string progress_url { get; set; }
        public string token { get; set; }
        public string video_id { get; set; }
        public string video_unique { get; set; }
        public string uploadtype { get; set; }
        public string isdrm { get; set; }
        public long upload_size { get; set; }
    }
    class LetvCloud
    {
        /*  基本配置信息
        public string userUnique { get; set; }
        public string secretKey { get; set; }
        public string restUrl { get; set; }
        public string format { get; set; }
        public string apiVersion { get; set; }
        */
        private static String userUnique = "ievel39qfn";
        private static String secretKey = "400c0826066c64f8a3b8c64d55342ea1";
        private static String restUrl = "http://api.letvcloud.com/open.php";
        private static String format = "json";
        private static String apiVersion = "2.0";
        private static string tokenFile;//记录上传失败token为了续传
        /// <summary>
        /// 带有失败记录文件的构造函数
        /// </summary>
        /// <param name="_tokenFile">失败记录token的文件名称</param>
        public LetvCloud(string _tokenFile)
        {
            tokenFile = _tokenFile;
        }
        /// <summary>
        /// 空构造函数
        /// </summary>
        public LetvCloud()
        {
        }
        /// <summary>
        /// 配置失败记录token的文件
        /// </summary>
        /// <param name="_tokenFile"></param>
        public void setTokenFile(string _tokenFile)
        {
            tokenFile = _tokenFile;
        }
        /// <summary>
        /// 视频上传初始化，使用HttpPost
        /// </summary>
        /// <param name="video_name">上传视频名称</param>
        /// <param name="client_ip">客户端ip地址</param>
        /// <returns>上传URL</returns>
        public static String videoUploadInit(String video_name, String client_ip)
        {

            return videoUploadInit(video_name, client_ip, 0);
        }
        /// <summary>
        /// 断点续传初始化
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>上传URL</returns>
        public static String videoUploadResume(String token)
        {
            String api = "video.upload.resume";
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("token", Encoding.UTF8.GetString(Encoding.Default.GetBytes(token)));
            return makeRequest(api, args);
        }
        /// <summary>
        /// 上传初始化实现
        /// </summary>
        /// <param name="video_name">上传视频名称</param>
        /// <param name="client_ip">客户端ip地址</param>
        /// <param name="file_size">文件大小</param>
        /// <returns></returns>
        public static String videoUploadInit(String video_name, String client_ip, int file_size)
        {
            String api = "video.upload.init";
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("video_name", video_name);
            if (client_ip.Length > 0)
            {
                args.Add("client_ip", client_ip);
            }
            if (file_size > 0)
            {
                args.Add("file_size", file_size + "");
            }
            Application.DoEvents();
            return makeRequest(api, args);
        }
        /// <summary>
        /// Post请求，获取上传URL
        /// </summary>
        /// <param name="api">应用程序接口，包括上传、续传等</param>
        /// <param name="args">上传文件信息字典</param>
        /// <returns>上传URL</returns>
        private static String makeRequest(String api, Dictionary<string, string> args)
        {
            args.Add("user_unique", userUnique);
            //获取时间戳

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            String timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();
            args.Add("timestamp", timestamp);
            args.Add("ver", apiVersion);
            args.Add("format", format);
            args.Add("api", api);
            //签名	
            args.Add("sign", generateSign(args));
            //构造请求URL
            String resurl = "";
            resurl += restUrl + "?" + mapToQueryString(args);
            //Console.Write(resurl);
            return doGet(resurl);
        }
        /// <summary>
        /// 计算sign 签名值(md5)
        /// </summary>
        /// <param name="args">上传文件信息字典</param>
        /// <returns>签名</returns>
        private static String generateSign(Dictionary<String, String> args)
        {

            Dictionary<string, string> argsAsc = args.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            //Dictionary<string, string> dic1desc = args.OrderByDescending(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            String keyStr = "";
            foreach (var key in argsAsc.Keys)
            {
                keyStr += key.ToString() + argsAsc[key];
            }
            keyStr += secretKey;
            return MD5Encrypt(keyStr);
        }
        ///   <summary>  
        ///   给一个字符串进行MD5加密  
        ///   </summary>  
        ///   <param   name="strText">待加密字符串</param>  
        ///   <returns>加密后的字符串</returns>  
        private static string MD5Encrypt(string strText)
        {
            char[] md5Chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
            char[] chars = new char[result.Length * 2];
            int i = 0;
            foreach (byte b in result)
            {
                char c0 = md5Chars[(b & 0xf0) >> 4];
                chars[i++] = c0;
                char c1 = md5Chars[b & 0xf];
                chars[i++] = c1;
            }
            return new String(chars);
        }
        /// <summary>
        /// 将文件信息字典转为字符串
        /// </summary>
        /// <param name="args">文件信息字典</param>
        /// <returns>URL中的字符串</returns>
        private static String mapToQueryString(Dictionary<String, String> args)
        {
            List<string> keyList = new List<string>(args.Keys);
            keyList.Sort();
            String str = "";
            for (int i = 0; i < keyList.Count; i++)
            {
                String key = keyList[i];
                if (i != keyList.Count - 1)
                {
                    str += key + "=" + args[key] + "&";
                }
                else
                {
                    str += key + "=" + args[key];
                }//异常没有写
            }
            return str;
        }
        /// <summary>
        /// HTTP GET 
        /// </summary>
        /// <param name="url">访问URL</param>
        /// <returns>响应（Response）</returns>
        public static String doGet(String url)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var mstream = new MemoryStream())
                {
                    responseStream.CopyTo(mstream);
                    return System.Text.Encoding.UTF8.GetString(mstream.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.Write("GET 请求失败！");
                return "";
            }
        }
        /// <summary>
        /// 上传字符串解析json
        /// </summary>
        /// <param name="src">字符串</param>
        /// <returns>json结构体</returns>
        public static jsonout jsonGet(String src)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            jsonout json = serializer.Deserialize<jsonout>(src);
            return json;
        }
        /// <summary>
        /// 续传字符串解析json
        /// </summary>
        /// <param name="src">字符串</param>
        /// <returns>json结构体</returns>
        public static jsonResume jsonGetResume(String src)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            jsonResume json = serializer.Deserialize<jsonResume>(src);
            return json;
        }
        ///   <summary>  
        ///   上传Post
        ///   </summary>  
        ///   <param   name="uri">上传post URL</param>  
        ///   <param   name="file">上传文件名</param>  
        ///   <returns>上传结束响应字符</returns>
        public static byte[] uploadPost(Uri uri, FileStream file)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            MemoryStream stream = new MemoryStream();
            byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endline = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            //提交文件
            if (file != null)
            {
                string fformat = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + System.Environment.NewLine + "Content-Type: application/octet-stream"
                    + System.Environment.NewLine + System.Environment.NewLine;
                string s = string.Format(fformat, file.Name, file.Name);
                byte[] sdata = Encoding.ASCII.GetBytes(s);
                stream.Write(sdata, 0, sdata.Length);
                byte[] filedata = new byte[file.Length];
                file.Read(filedata, 0, filedata.Length);
                stream.Write(filedata, 0, filedata.Length);
                stream.Write(endline, 0, endline.Length);
            }
            request.ContentLength = stream.Length;
            Stream requestStream = request.GetRequestStream();
            stream.Position = 0L;
            stream.CopyTo(requestStream);
            stream.Close();
            requestStream.Close();
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var mstream = new MemoryStream())
            {
                responseStream.CopyTo(mstream);
                return mstream.ToArray();
            }
        }
        /// <summary>
        /// 带offset的 部分上传Post
        /// </summary>
        ///   <param   name="uri">上传post URL</param>  
        ///   <param   name="file">上传文件名</param>
        ///   <param   name="token">上传token</param>
        /// <param name="offset">上传文件偏移量</param>
        ///   <returns>上传结束响应字符</returns>
        public static byte[] uploadPost(Uri uri, FileStream file, string token, long offset)
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
                return Encoding.ASCII.GetBytes(sReturnString);
            }
            //error then write error log file
            catch
            {
                writeToken(tokenFile, token);
                byte[] wrongResult = Encoding.ASCII.GetBytes("");
                return wrongResult;
            }
        }
        ///   <summary>  
        ///   部分上传Post
        ///   </summary>  
        ///   <param   name="uri">上传post URL</param>  
        ///   <param   name="file">上传文件名</param>
        ///   <param   name="token">上传token</param>
        ///   <returns>上传结束响应字符</returns>
        public static string uploadPost(Uri uri, FileStream file, string token)
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
                writeToken(tokenFile, token);
                //byte[] wrongResult = Encoding.ASCII.GetBytes("");
                string wrongResult = "";
                return wrongResult;
            }
        }
        ///   <summary>  
        ///   断点续传Post
        ///   </summary>  
        ///   <param   name="uri">上传post URL</param>  
        ///   <param   name="file">上传文件名</param>  
        ///   <returns>上传结束响应字符</returns>
        public static byte[] resumeUploadPost(Uri uri, FileStream file, long upload_size)
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
            MemoryStream stream = new MemoryStream();
            byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endline = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            //上传视频文件
            if (file != null)
            {
                string fformat = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + System.Environment.NewLine + "Content-Type: application/octet-stream"
                    + System.Environment.NewLine + System.Environment.NewLine;
                string s = string.Format(fformat, file.Name, file.Name);
                byte[] sdata = Encoding.ASCII.GetBytes(s);
                stream.Write(sdata, 0, sdata.Length);
                file.Seek(upload_size, SeekOrigin.Begin);
                byte[] filedata = new byte[file.Length];
                file.Read(filedata, 0, filedata.Length);
                stream.Write(filedata, 0, filedata.Length);
                stream.Write(endline, 0, endline.Length);
            }
            request.ContentLength = stream.Length;
            Stream requestStream = request.GetRequestStream();
            stream.Position = 0L;
            stream.CopyTo(requestStream);
            stream.Close();
            requestStream.Close();
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var mstream = new MemoryStream())
            {
                responseStream.CopyTo(mstream);
                return mstream.ToArray();
            }
        }
        ///   <summary>
        ///   单文件上传Demo测试
        ///   </summary>
        public static void TestUploadOneFile()
        {
            String result = videoUploadInit("letv3", "192.168.1.1");
            jsonout json = jsonGet(result);
            string code = json.code;
            string mes = json.message;
            string total = json.total;
            string data = json.data.token;
            if (!code.Equals("0"))
            {
                Console.Write("code = 0! Initial failed!");
                return;
            }
            Console.Write("Initial Success!");
            FileStream file = new FileStream("E:\\甜甜圈1.mp4", FileMode.Open);
            Uri url = new Uri(json.data.upload_url);
            byte[] uploadresult = uploadPost(url, file);
            Console.Write(System.Text.Encoding.UTF8.GetString(uploadresult));
            Console.Write(uploadresult);
            Console.Write("Upload Success!");
            Console.Write(json.data.video_id);
        }
        /// <summary>
        /// 单个文件上传（网络中断可以捕获异常）
        /// </summary>
        /// <param name="video_name">视频文件名称</param>
        /// <param name="client_ip">客户端ip</param>
        /// <param name="file_path">待上传文件路径</param>
        public static string upload(string video_name, string client_ip, string file_path)
        {
            String result = videoUploadInit(video_name, client_ip);
            jsonout json = jsonGet(result);
            string code = json.code;
            string mes = json.message;
            string total = json.total;
            string token = json.data.token;
            if (!code.Equals("0"))
            {
                Console.Write("code = 0!  初始化失败!");
                return null;
            }
            Console.Write("初始化成功!");
            FileStream file = new FileStream(file_path, FileMode.Open);
            //
            string suburl = json.data.upload_url.Substring(0, (json.data.upload_url.Length - 10));
            //
            Uri url = new Uri(suburl);
            string uploadresult = uploadPost(url, file, token);
            //Console.Write(System.Text.Encoding.UTF8.GetString(uploadresult));
            Console.Write(uploadresult);
            Console.Write("上传成功!");
            Console.Write(json.data.video_id);
            return uploadresult;
        }
        /// <summary>
        /// 单个文件断点续传
        /// </summary>
        /// <param name="file_path">上传文件路径</param>
        public void uploadResume(string file_path)
        {
            ///续传
            String token = readToken(tokenFile);
            String resumeresult = videoUploadResume(token);
            jsonResume resumejson = jsonGetResume(resumeresult);
            Uri resumeurl = new Uri(resumejson.data.upload_url);
            FileStream file = new FileStream(file_path, FileMode.Open);
            byte[] resumeResultbyte = uploadPost(resumeurl, file, token, resumejson.data.upload_size);
            Console.Write("续传成功!");
            Console.Write(resumeResultbyte);
        }

        /// <summary>
        /// 单个文件断点续传 需要token
        /// </summary>
        /// <param name="file_path">上传文件路径</param>
        /// <param name="token">token</param>
        public void uploadResume(string file_path, string token)
        {
            String resumeresult = videoUploadResume(token);
            jsonResume resumejson = jsonGetResume(resumeresult);
            Uri resumeurl = new Uri(resumejson.data.upload_url);
            FileStream file = new FileStream(file_path, FileMode.Open);
            //byte[] resumeResultbyte = resumeUploadPost(resumeurl, file, resumejson.data.upload_size);
            byte[] resumeResultbyte = uploadPost(resumeurl, file, token, resumejson.data.upload_size);
            Console.Write("续传成功!");
            Console.Write(resumeResultbyte);
        }

        public void folderReader(string folderFullName)
        {
            try
            {
                DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);

                //遍历文件
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    Console.WriteLine(NextFile.Name);
                    //upload();
                    string videoname = NextFile.Name;
                    string filename = folderFullName + "//" + NextFile.Name;
                    upload(videoname, "10.58.180.188", filename);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("文件夹读取有问题！");
                return;
            }
        }




        public void TestUploadResume()
        {
            ///续传
            String token = readToken(tokenFile);
            String resumeresult = videoUploadResume(token);
            jsonResume resumejson = jsonGetResume(resumeresult);
            Uri resumeurl = new Uri(resumejson.data.upload_url);
            //Uri resumeurl = new Uri("http://118.26.58.28/api/fileupload?offset=0&token=kFkAJgY1I7mfe9GItjXTiRs2_orljicxvcecXULwNNq9ZqZz1CX0-OD8s-o-c261wTqTs0MtKb3FiOsdqRCCBuOpLO4JSQMRARIFXYV0HsCenwsEjMHBX4SNG6q-qpZgSsPYBqlfxBaTmJ4-bUDPHRlnJZv4qWewF0wcGc3_4jjMRDQOqD34SwxZMSEPM8UhNg9xOxnDTasdXecW4SgyEvkA71jm5eFIQgvu8TSWl09XaBBA_VvYf6Vqj7u7i2jguESZfK1W94Ctpa4D0DtliY3JfHlx4mr2JseVMhQ-3gRC0b0LtRBql22h-8cbcAvSENlHi33LOahlZm2HcEDNwl09XP9SiMU6XyPnXCPjgX4LSgMGNCPgGZqD92kP0EtjNds-sALkP_GS_cMl8giH_kQfnwr8LEEDNqnl8pN6xm76AUJdZDm1lVLmotSUmucZRiyOnx4KIkFQDr1D_IZGm6x5V4gua3zZB_kQke_adUN915R68Gg1fRI756ic72-ghr2UhTlQJdcwkFFXQOtk9g~~");
            FileStream file = new FileStream("C://tmp//test.flv", FileMode.Open);
            byte[] resumeResultbyte = resumeUploadPost(resumeurl, file, resumejson.data.upload_size);
            Console.Write("续传成功!");
            Console.Write(resumeResultbyte);
        }

        public void TestUploadResume(string token)
        {
            ///续传
            //String token = "K1YNU94eH_b4cU0LsicTnpuPQBoCkfrDfYYWEqxLdc4DqwJW2E4ZwogG31fp7iZ1Ty0ENprj5E3H4LfExdA3I7SganT4SROYPZUK2jHSWGNHO3SzuG_NC6cHzrTs9vwegY9sXRgrzCXsWPDZ_7LOuZ41WatMO8NEicWwIHEsMTZe8i7P4ha70NwmaQX-zc4Gq2RlrzwwHfW-tTSg0Do5PhgoHIahDKCQz5MiVLmQr1Jh4uRyM1E9WsuIl1pnLUpZRklwsiQj5LCc9TtbV2hoQn--x8h3rTxefA97t3BbQKtjfoRG5_LcrsOqs9nMNIPcokxN3kevt7WTOrWSrAnCXHc99QCxF3vnsQSXv6RR4IOBOJgpiLHvGaKExzT3wsJT5PEGz5WKu2D7CjDs5CGobeMoJZ0uY-1Q_9malQ~~";
            //String token = readToken(tokenFile);
            String resumeresult = videoUploadResume(token);
            jsonResume resumejson = jsonGetResume(resumeresult);
            Uri resumeurl = new Uri(resumejson.data.upload_url);
            //Uri resumeurl = new Uri("http://118.26.58.28/api/fileupload?offset=0&token=kFkAJgY1I7mfe9GItjXTiRs2_orljicxvcecXULwNNq9ZqZz1CX0-OD8s-o-c261wTqTs0MtKb3FiOsdqRCCBuOpLO4JSQMRARIFXYV0HsCenwsEjMHBX4SNG6q-qpZgSsPYBqlfxBaTmJ4-bUDPHRlnJZv4qWewF0wcGc3_4jjMRDQOqD34SwxZMSEPM8UhNg9xOxnDTasdXecW4SgyEvkA71jm5eFIQgvu8TSWl09XaBBA_VvYf6Vqj7u7i2jguESZfK1W94Ctpa4D0DtliY3JfHlx4mr2JseVMhQ-3gRC0b0LtRBql22h-8cbcAvSENlHi33LOahlZm2HcEDNwl09XP9SiMU6XyPnXCPjgX4LSgMGNCPgGZqD92kP0EtjNds-sALkP_GS_cMl8giH_kQfnwr8LEEDNqnl8pN6xm76AUJdZDm1lVLmotSUmucZRiyOnx4KIkFQDr1D_IZGm6x5V4gua3zZB_kQke_adUN915R68Gg1fRI756ic72-ghr2UhTlQJdcwkFFXQOtk9g~~");
            FileStream file = new FileStream("C://tmp//test.wmv", FileMode.Open);
            byte[] resumeResultbyte = resumeUploadPost(resumeurl, file, resumejson.data.upload_size);
            Console.Write("续传成功!");
            Console.Write(resumeResultbyte);
        }
        /// <summary>
        /// 将上传失败的token记录在文件中
        /// </summary>
        /// <param name="filename">记录的文件名称</param>
        /// <param name="token">失败的token</param>
        public static void writeToken(string filename, string token)
        {
            try
            {
                FileStream aFile = new FileStream(filename, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(aFile);
                sw.WriteLine(token);
                sw.Close();
            }
            catch (IOException exception)
            {
                Console.WriteLine("没有将不成功的信息写入文件！");
                Console.WriteLine(exception.ToString());
                Console.ReadLine();
                return;
            }
        }
        /// <summary>
        /// 将上传失败的token从文件中读取出来
        /// </summary>
        /// <param name="filename">记录的文件名称</param>
        /// <returns>失败的token</returns>
        public static string readToken(string filename)
        {
            string token = null;
            try
            {
                FileStream aFile = new FileStream(filename, FileMode.OpenOrCreate);
                StreamReader sr = new StreamReader(aFile);
                token = sr.ReadLine();
                if (token != null)
                    return token;
                else
                {
                    Console.WriteLine("没有读到失败记录！");
                    return token;
                }
            }
            catch (IOException exception)
            {
                Console.WriteLine("没有读到失败记录！");
                Console.WriteLine(exception.ToString());
                Console.ReadLine();
                return token;
            }
        }


    }
}