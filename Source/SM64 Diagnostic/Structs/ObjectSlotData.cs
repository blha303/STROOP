﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SM64_Diagnostic.Structs
{
    public class ObjectSlotData
    {
        public uint Address;
        public byte ObjectProcessGroup;
        public int ProcessIndex;
        public int? VacantSlotIndex;
        public float DistanceToMario;
        public bool IsActive;
        public uint Behavior;
    }
}
