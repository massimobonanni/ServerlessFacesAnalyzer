﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Models
{
    public class FaceAnalyzerResult
    {
        public List<FaceInfo> Faces { get; set; }=new List<FaceInfo>();
    }
}