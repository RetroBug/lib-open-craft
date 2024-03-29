﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibOpenCraft;
using LibOpenCraft.ServerPackets;

using System.IO.Compression;
using System.IO;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Threading;

namespace LibOpenCraft.WorldPhysics
{
    public struct PhysicalObject
    {
        /// <summary>
        /// this is the format of our XYZ for physics
        /// X, Y, Z = meter.cm.mm
        /// </summary>
        public Vector3D v_pos;

        /// <summary>
        /// this is the position to were the force is from and how much is less.
        /// the values are 0.1 - 255.0 or else there not valid.
        /// </summary>
        public Vector3D v_force_pos;

        /// <summary>
        /// This is the amount of mass this object is being hit with.
        /// </summary>
        public double d_mass_op;

        public byte b_face;

        public int i_position;

        /// <summary>
        /// This is the objects mass,
        /// it will weigh in kilograms
        /// </summary>
        public const double d_mass = 1.0;

        /// <summary>
        /// This is how much it get's deducted persecond.
        /// </summary>
        public const double resistance = 0.1;

        /// <summary>
        /// This is how many cm persecond the object moves
        /// </summary>
        public double d_speed;
    }
}
