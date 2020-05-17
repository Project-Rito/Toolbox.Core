﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IImportableModel
    {
        bool IdentfiyImport(string extension);
        STGenericModel Import(string filePath);
    }
}
