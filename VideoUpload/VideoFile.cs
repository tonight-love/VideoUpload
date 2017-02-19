using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoUpload
{
    class VideoFile
    {
        private int fileIndex;
        private long fileSize;
        private string fileName;
        private string fileLocal;
        private string fileStatus;
        private float uploadProgress;

        public int FileIndex
        {
            get { return this.fileIndex; }
            set { this.fileIndex = value; }
        }

        public long FileSize 
        {
            get { return this.fileSize; }
            set { this.fileSize = value; }
        }

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public string FileLocal
        {
            get { return this.fileLocal; }
            set { this.fileLocal = value; }
        }

        public string FileStatus
        {
            get { return this.fileStatus; }
            set { this.fileStatus = value; }
        }

        public float UploadProgress
        {
            get { return this.uploadProgress; }
            set { this.uploadProgress = value; }
        }
    }
}
