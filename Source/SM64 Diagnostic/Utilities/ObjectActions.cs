﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM64_Diagnostic.Structs;

namespace SM64_Diagnostic.Utilities
{
    public static class ObjectActions
    {
        public static bool MoveMarioToObject(ProcessStream stream, Config config, uint objAddress)
        {
            // Move mario to object
            var marioAddress = config.Mario.MarioStructAddress;

            // Get object position
            float x, y, z;
            x = BitConverter.ToSingle(stream.ReadRam(objAddress + config.ObjectSlots.ObjectXOffset, 4), 0);
            y = BitConverter.ToSingle(stream.ReadRam(objAddress + config.ObjectSlots.ObjectYOffset, 4), 0);
            z = BitConverter.ToSingle(stream.ReadRam(objAddress + config.ObjectSlots.ObjectZOffset, 4), 0);

            // Add offset
            y += 300f;

            // Move mario to object
            bool success = true;
            success &= stream.WriteRam(BitConverter.GetBytes(x), marioAddress + config.Mario.XOffset);
            success &= stream.WriteRam(BitConverter.GetBytes(y), marioAddress + config.Mario.YOffset);
            success &= stream.WriteRam(BitConverter.GetBytes(z), marioAddress + config.Mario.ZOffset);
            return success;
        }

        public static bool MoveObjectToMario(ProcessStream stream, Config config, uint objAddress)
        {
            // Move object to Mario
            var marioAddress = config.Mario.MarioStructAddress;

            // Get Mario position
            float x, y, z;
            x = BitConverter.ToSingle(stream.ReadRam(marioAddress + config.Mario.XOffset, 4), 0);
            y = BitConverter.ToSingle(stream.ReadRam(marioAddress + config.Mario.YOffset, 4), 0);
            z = BitConverter.ToSingle(stream.ReadRam(marioAddress + config.Mario.ZOffset, 4), 0);

            // Add offset
            y += 300f;

            // Move object to Mario
            bool success = true;
            success &= stream.WriteRam(BitConverter.GetBytes(x), objAddress + config.ObjectSlots.ObjectXOffset);
            success &= stream.WriteRam(BitConverter.GetBytes(y), objAddress + config.ObjectSlots.ObjectYOffset);
            success &= stream.WriteRam(BitConverter.GetBytes(z), objAddress + config.ObjectSlots.ObjectZOffset);
            return success;
        }


        public static bool CloneObject(ProcessStream stream, Config config, uint objAddress)
        {
            bool success = true;
            var marioAddress = config.Mario.MarioStructAddress;

            // Make clone object mario's holding object
            success &= stream.WriteRam(BitConverter.GetBytes(objAddress), marioAddress + config.Mario.HoldingObjectPointerOffset);

            // Set clone action flags
            success &= stream.WriteRam(BitConverter.GetBytes(0x8000207), marioAddress + config.Mario.ActionOffset);

            return success;
        }

        public static bool UnloadObject(ProcessStream stream, Config config, uint address)
        {
            return stream.WriteRam(new byte[] { 0x00, 0x00 }, address + config.ObjectSlots.ObjectActiveOffset);
        }
    }
}