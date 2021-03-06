﻿using System.Collections.Generic;
using System.Drawing;
using TRTexture16Importer.Textures.Grouping;

namespace TRTexture16Importer.Textures.Target
{
    public class DynamicTextureTarget
    {
        public Dictionary<int, List<Rectangle>> DefaultTileTargets { get; set; }
        public Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>> OptionalTileTargets { get; set; }

        public DynamicTextureTarget()
        {
            DefaultTileTargets = new Dictionary<int, List<Rectangle>>();
            OptionalTileTargets = new Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>>();
        }
    }
}