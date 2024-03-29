﻿//TODO:create code to build kind a cave
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Data;

namespace LibOpenCraft.Biomes
{
    public enum BiomeType
    {
        Forest = 0,
        Taiga = 1,
        Swampland = 2,
        Mountains = 3,
        Desert = 4,
        Plains = 5,
        Ocean = 6,
        Tundra = 7,
        MushroomIsland = 8
    }

    public enum FormationType
    {
        ThreeDCross = 0,
        BigL = 1,
        Cube = 2,
        SmallL = 3
    }

    [XmlInclude(typeof(Biomes.Desert))]
    public class Biome : Chunk
    {
        public BiomeType Type { get; set; }

        public int X_Start { get; set; }

        public int Z_Start { get; set; }

        public int X_End { get; set; }

        public int Z_End { get; set; }

        public FastRandom RandomGenerator { get; set; }

        public int DefaultHeigth { get; set; }

        public virtual void CreateChunk() { }

        public Biome(short x, short z, FastRandom rnd)
            : base(x, z)
        {
            X_Start = x * 10 * 64;
            Z_Start = z * 10 * 64;

            X_End = (x + 1) * 10 * 64;
            Z_End = (z + 1) * 10 * 64;

            RandomGenerator = rnd;

            DefaultHeigth = 64;//Has to be a multiple of 2's 4's 8's 16's
            base.IsAir = new bool[Depth / 16];
            int i = 0;
            for (; i < DefaultHeigth / 16; i++)
            {
                IsAir[i] = false;
            }
            for (; i < Depth / 16; i++)
            {
                IsAir[i] = true;
            }
        }

        private void CreateFormat(BlockTypes type, int x, int y, int z, FormationType format)
        {
            switch (format)
            {
                case FormationType.ThreeDCross:
                    //3D Cross
                    SetBlocktype(x, y, z, (byte)type);
                    SetBlocktype(x - 1, y, z, (byte)type);
                    SetBlocktype(x + 1, y, z, (byte)type);
                    SetBlocktype(x, y, z + 1, (byte)type);
                    SetBlocktype(x, y, z - 1, (byte)type);
                    SetBlocktype(x, y - 1, z, (byte)type);
                    SetBlocktype(x, y + 1, z, (byte)type);
                    break;
                case FormationType.BigL:
                    //Kind of laying big "L"
                    SetBlocktype(x, y, z, (byte)type);
                    SetBlocktype(x + 1, y + 1, z, (byte)type);
                    SetBlocktype(x - 1, y, z, (byte)type);
                    SetBlocktype(x + 1, y, z, (byte)type);
                    break;
                case FormationType.Cube:
                    //Cube
                    SetBlocktype(x, y + 1, z, (byte)type);
                    SetBlocktype(x, y - 1, z, (byte)type);
                    SetBlocktype(x, y - 1, z - 1, (byte)type);
                    SetBlocktype(x, y + 1, z - 1, (byte)type);
                    break;
                case FormationType.SmallL:
                    //Kind of staying small "L"
                    SetBlocktype(x, y + 2, z, (byte)type);
                    SetBlocktype(x, y + 1, z, (byte)type);
                    SetBlocktype(x, y, z - 1, (byte)type);
                    break;
            }
        }

        public void CreateOres()
        {
            for (int block_y = 1; block_y < DefaultHeigth - 4; block_y++)
            {
                for (int block_x = 0; block_x < Width; block_x++)
                {
                    for (int block_z = 0; block_z < Height; block_z++)
                    {

                        int helper = RandomGenerator.Next(200000);
                        int formathelper = RandomGenerator.Next(0, 3);
                        if (Blocks[GetIndex(block_x, block_y, block_z)] != ((byte)BlockTypes.RedstoneOre | (byte)BlockTypes.GoldOre | (byte)BlockTypes.LapisLazuliBlock | (byte)BlockTypes.IronOre | (byte)BlockTypes.DiamondOre))
                        {
                            if (block_y <= 24)
                            {
                                //Diamond (2-14)
                                if (helper >= 34 & helper <= 48)
                                {
                                    CreateFormat(BlockTypes.DiamondOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }

                                //Red Stone (2-16)
                                if (helper >= 11 & helper <= 20)
                                {
                                    CreateFormat(BlockTypes.RedstoneOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }

                            }
                            if (block_y <= 36)
                            {
                                //Gold (2-28)
                                if (helper >= 8 & helper <= 12)
                                {
                                    CreateFormat(BlockTypes.GoldOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }
                            }
                            if (block_y <= 39)
                            {
                                //Lapiz Lazuli (2-31)
                                if (helper >= 33 & helper <= 36)
                                {
                                    CreateFormat(BlockTypes.LapisLazuliOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }
                            }
                            if (block_y <= 60)
                            {
                                //Iron (2-64)
                                if (helper <= 6)
                                {
                                    CreateFormat(BlockTypes.IronOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }
                            }
                            if (block_y <= 60)
                            {
                                //Coal (everywhere)
                                if (helper >= 22 & helper <= 32)
                                {
                                    CreateFormat(BlockTypes.CoalOre, block_x, block_y, block_z, (FormationType)formathelper);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreateLakes(int start_heigth, int frequency = 6,int min_size = 6, int max_size = 11)
        {
            if (RandomGenerator.Next(frequency) == 1)
            {
                int x_start = RandomGenerator.Next(min_size, max_size);
                int z_start = RandomGenerator.Next(min_size, max_size);

                if (x_start < z_start)
                {
                    z_start = RandomGenerator.Next(x_start - 2, x_start + 2);
                }
                else
                {
                    x_start = RandomGenerator.Next(z_start - 2, z_start + 2);
                }

                int depth = RandomGenerator.Next(5);

                for (int z = 0; z < z_start; z++)
                {
                    for (int x = 0; x < x_start; x++)
                    {
                        int current_depth = RandomGenerator.Next(1,5);

                        for (int y = 0; y < current_depth; y++)
                        {
                            SetBlocktype(x, start_heigth - y, z, (byte)BlockTypes.Water);
                        }     
                    }
                }
            }      
        }

        public Biome()
            : base()
        {
            DefaultHeigth = 64;//Has to be a multiple of 2's 4's 8's 16's
            base.IsAir = new bool[Depth / 16];
            int i = 0;
            for (; i < DefaultHeigth / 16; i++)
            {
                IsAir[i] = false;
            }
            for (; i < Depth / 16; i++)
            {
                IsAir[i] = true;
            }
        }
    }
}
