﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM64_Diagnostic.Structs.Configurations
{
    public struct TriangleOffsetsConfig
    {
        public uint SurfaceType;
        public uint Flags;
        public uint WindDirection;
        public uint YMin;
        public uint YMax;
        public uint WallProjection;
        public uint X1, Y1, Z1;
        public uint X2, Y2, Z2;
        public uint X3, Y3, Z3;
        public uint NormX, NormY, NormZ;
        public uint Offset;
        public uint AssociatedObject;
    }
}
