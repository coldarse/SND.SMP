using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SND.SMP.Chibis.Dto
{
    public class ChibiUploadDto
    {
        public IFormFile? file { get; set; }
    }
}