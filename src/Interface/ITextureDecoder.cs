﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface ITextureDecoder
    {
        bool CanEncode(TexFormat format);
        bool IsSupportedPlatform();
        bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output);
        bool Encode(TexFormat format, byte[] input, int width, int height, out byte[] output);
    }
}
