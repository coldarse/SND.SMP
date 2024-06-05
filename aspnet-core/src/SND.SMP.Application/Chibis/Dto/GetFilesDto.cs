using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SND.SMP.FileUploadAPI.Dto
{

    public class File
    {   public string name { get; set; }
        public DateTime createdAt { get; set; }
        public string ip { get; set; }
        public string original { get; set; }
        public string uuid { get; set; }
        public string hash { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string thumb { get; set; }
        public string thumbSquare { get; set; }
        public string preview { get; set; }
        public bool quarantine { get; set; }
        public List<object> albums { get; set; }
        public List<object> tags { get; set; }
    }

    public class GetFileDto
    {
        public string message { get; set; }
        public File file { get; set; }
    }

    public class GetFilesDto 
    {
        public string message { get; set; }
        public List<File> files { get; set; }
        public int count { get; set; }
    }
}
