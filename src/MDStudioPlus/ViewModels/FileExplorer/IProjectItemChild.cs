﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.FileExplorer
{
    public interface IProjectItemChild
    {
        Project Project { get; }
    }
}