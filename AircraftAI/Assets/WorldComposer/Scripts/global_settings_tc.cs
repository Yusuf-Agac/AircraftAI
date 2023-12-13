using System;

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
#endif

namespace WorldComposer
{
    #if UNITY_EDITOR
    [Serializable]
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "GlobalSettings", order = 1)]
    public class global_settings_tc : ScriptableObject
    {
        public Material matTerrain;
        public int tc_script_id;
        public bool tc_installed;
        public string[] examples = new string[]
        {
        "Procedural Mountains",
        "Procedural Canyons",
        "Procedural Plateaus",
        "Procedural Islands",
        "Island Example"
        };
        public bool layer_count = true;
        public bool placed_count = true;
        public bool display_project = true;
        public bool tabs = true;
        public bool color_scheme = true;
        public color_settings_class color_layout = new color_settings_class();
        public bool color_layout_converted;
        public bool box_scheme;
        public bool display_color_curves;
        public bool display_mix_curves = true;
        public bool filter_select_text = true;
        public string install_path;
        public string install_path_full;
        public bool object_fast = true;
        public bool preview_texture = true;
        public int preview_texture_buffer = 100;
        public bool preview_colors = true;
        public int preview_texture_resolution = 128;
        public int preview_texture_resolution1 = 128;
        public int preview_quick_resolution_min = 16;
        public float preview_splat_brightness = 1;
        public bool preview_texture_dock = true;
        public int preview_target_frame = 30;
        public List<Color> splat_colors = new List<Color>();
        public int splat_custom_texture_resolution = 128;
        public int splat_custom_texture_resolution1 = 128;
        public List<Color> tree_colors = new List<Color>();
        public List<Color> grass_colors = new List<Color>();
        public List<Color> object_colors = new List<Color>();
        public bool toggle_text_no;
        public bool toggle_text_short = true;
        public bool toggle_text_long;
        public bool tooltip_text_no;
        public bool tooltip_text_short;
        public bool tooltip_text_long = true;
        public int tooltip_mode = 2;
        public bool video_help = true;
        public bool run_in_background = true;
        public bool display_bar_auto_generate = true;
        public bool unload_textures;
        public bool clean_memory;
        public bool auto_speed = true;
        public int target_frame = 40;
        public bool auto_save = true;
        public int auto_save_tc_instances = 2;
        public int auto_save_scene_instances = 2;
        public bool auto_save_tc = true;
        public List<string> auto_save_tc_list = new List<string>();
        public bool auto_save_scene = true;
        public List<string> auto_save_scene_list = new List<string>();
        public float auto_save_timer = 10;
        public float auto_save_time_start;
        public bool auto_save_on_play = true;
        public string auto_save_path;
        public int terrain_tiles_max = 15;
        public List<auto_search_class> auto_search_list = new List<auto_search_class>();
        public map_class map = new map_class();

        public select_window_class select_window = new select_window_class();
        public List<int> preview_window = new List<int>();
        public bool map_combine;
        public bool map_load;
        public bool map_load2;
        public bool map_load3;
        public bool map_load4;
        public int map_zoom1;
        public int map_zoom2;
        public int map_zoom3;
        public int map_zoom4;

        public latlong_class map_latlong = new latlong_class();
        public latlong_class map_latlong_center = new latlong_class();
        public int map_zoom = 17;
        public int map_zoom_old;
        public global_settings_class settings = new global_settings_class();
    }
    [Serializable]
    public class gui_class
    {
        public List<float> column;

        public float y;

        public float x;

        public gui_class(int columns)
        {
            column = new List<float>();
            for (int i = 0; i < columns; i++)
            {
                column.Add(0);
            }
        }

        public Rect getRect(int column_index, float width, float y1, bool add_width, bool add_height)
        {
            float num = x;
            float num2 = y;
            if (add_width)
            {
                x += width;
            }
            if (add_height)
            {
                y += y1;
            }
            return new Rect(column[column_index] + num, num2, width, y1);
        }

        public Rect getRect(int column_index, float x1, float width, float y1, bool add_width, bool add_height)
        {
            float num = x;
            float num2 = y;
            if (add_width)
            {
                x += width + x1;
            }
            if (add_height)
            {
                y += y1;
            }
            return new Rect(column[column_index] + num + x1, num2, width, y1);
        }
    }

    [Serializable]
    public class image_edit_class
    {
        public Color color1_start;

        public Color color1_end;

        public AnimationCurve curve1;

        public Color color2_start;

        public Color color2_end;

        public AnimationCurve curve2;

        public float strength;

        public image_output_enum output;

        public bool active;

        public bool solid_color;

        public float radius;

        public int repeat;

        public image_edit_class()
        {
            color1_start = new Color(0, 0, 0, 1);
            color1_end = new Color(0.3f, 0.3f, 0.3f, 1);
            curve1 = AnimationCurve.Linear(0, 0, 1, 1);
            color2_start = new Color(1, 1, 1, 1);
            color2_end = new Color(1, 1, 1, 1);
            curve2 = AnimationCurve.Linear(0, 0, 1, 1);
            strength = 1;
            active = true;
            radius = 300;
            repeat = 4;
        }
    }

    [Serializable]
    public enum image_output_enum
    {
        add,
        subtract,
        change,
        multiply,
        divide,
        difference,
        average,
        max,
        min,
        content
    }

    [Serializable]
    public class JPGEncoder_class
    {
        private int[] ZigZag;

        private int[] YTable;

        private int[] UVTable;

        private float[] fdtbl_Y;

        private float[] fdtbl_UV;

        private BitString[] YDC_HT;

        private BitString[] UVDC_HT;

        private BitString[] YAC_HT;

        private BitString[] UVAC_HT;

        private int[] std_dc_luminance_nrcodes;

        private int[] std_dc_luminance_values;

        private int[] std_ac_luminance_nrcodes;

        private int[] std_ac_luminance_values;

        private int[] std_dc_chrominance_nrcodes;

        private int[] std_dc_chrominance_values;

        private int[] std_ac_chrominance_nrcodes;

        private int[] std_ac_chrominance_values;

        private BitString[] bitcode;

        private int[] category;

        private int bytenew;

        private int bytepos;

        private ByteArray byteout;

        private int[] DU;

        private float[] YDU;

        private float[] UDU;

        private float[] VDU;

        public bool isDone;

        private BitmapData image;

        private int sf;

        public JPGEncoder_class(Texture2D texture, float quality)
        {
            ZigZag = new int[]
            {
            0,
            1,
            5,
            6,
            14,
            15,
            27,
            28,
            2,
            4,
            7,
            13,
            16,
            26,
            29,
            42,
            3,
            8,
            12,
            17,
            25,
            30,
            41,
            43,
            9,
            11,
            18,
            24,
            31,
            40,
            44,
            53,
            10,
            19,
            23,
            32,
            39,
            45,
            52,
            54,
            20,
            22,
            33,
            38,
            46,
            51,
            55,
            60,
            21,
            34,
            37,
            47,
            50,
            56,
            59,
            61,
            35,
            36,
            48,
            49,
            57,
            58,
            62,
            63
            };
            YTable = new int[64];
            UVTable = new int[64];
            fdtbl_Y = new float[64];
            fdtbl_UV = new float[64];
            std_dc_luminance_nrcodes = new int[]
            {
            0,
            0,
            1,
            5,
            1,
            1,
            1,
            1,
            1,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0
            };
            std_dc_luminance_values = new int[]
            {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11
            };
            std_ac_luminance_nrcodes = new int[]
            {
            0,
            0,
            2,
            1,
            3,
            3,
            2,
            4,
            3,
            5,
            5,
            4,
            4,
            0,
            0,
            1,
            125
            };
            std_ac_luminance_values = new int[]
            {
            1,
            2,
            3,
            0,
            4,
            17,
            5,
            18,
            33,
            49,
            65,
            6,
            19,
            81,
            97,
            7,
            34,
            113,
            20,
            50,
            129,
            145,
            161,
            8,
            35,
            66,
            177,
            193,
            21,
            82,
            209,
            240,
            36,
            51,
            98,
            114,
            130,
            9,
            10,
            22,
            23,
            24,
            25,
            26,
            37,
            38,
            39,
            40,
            41,
            42,
            52,
            53,
            54,
            55,
            56,
            57,
            58,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            90,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            106,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            122,
            131,
            132,
            133,
            134,
            135,
            136,
            137,
            138,
            146,
            147,
            148,
            149,
            150,
            151,
            152,
            153,
            154,
            162,
            163,
            164,
            165,
            166,
            167,
            168,
            169,
            170,
            178,
            179,
            180,
            181,
            182,
            183,
            184,
            185,
            186,
            194,
            195,
            196,
            197,
            198,
            199,
            200,
            201,
            202,
            210,
            211,
            212,
            213,
            214,
            215,
            216,
            217,
            218,
            225,
            226,
            227,
            228,
            229,
            230,
            231,
            232,
            233,
            234,
            241,
            242,
            243,
            244,
            245,
            246,
            247,
            248,
            249,
            250
            };
            std_dc_chrominance_nrcodes = new int[]
            {
            0,
            0,
            3,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            0,
            0,
            0,
            0,
            0
            };
            std_dc_chrominance_values = new int[]
            {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11
            };
            std_ac_chrominance_nrcodes = new int[]
            {
            0,
            0,
            2,
            1,
            2,
            4,
            4,
            3,
            4,
            7,
            5,
            4,
            4,
            0,
            1,
            2,
            119
            };
            std_ac_chrominance_values = new int[]
            {
            0,
            1,
            2,
            3,
            17,
            4,
            5,
            33,
            49,
            6,
            18,
            65,
            81,
            7,
            97,
            113,
            19,
            34,
            50,
            129,
            8,
            20,
            66,
            145,
            161,
            177,
            193,
            9,
            35,
            51,
            82,
            240,
            21,
            98,
            114,
            209,
            10,
            22,
            36,
            52,
            225,
            37,
            241,
            23,
            24,
            25,
            26,
            38,
            39,
            40,
            41,
            42,
            53,
            54,
            55,
            56,
            57,
            58,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            90,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            106,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            122,
            130,
            131,
            132,
            133,
            134,
            135,
            136,
            137,
            138,
            146,
            147,
            148,
            149,
            150,
            151,
            152,
            153,
            154,
            162,
            163,
            164,
            165,
            166,
            167,
            168,
            169,
            170,
            178,
            179,
            180,
            181,
            182,
            183,
            184,
            185,
            186,
            194,
            195,
            196,
            197,
            198,
            199,
            200,
            201,
            202,
            210,
            211,
            212,
            213,
            214,
            215,
            216,
            217,
            218,
            226,
            227,
            228,
            229,
            230,
            231,
            232,
            233,
            234,
            242,
            243,
            244,
            245,
            246,
            247,
            248,
            249,
            250
            };
            bitcode = new BitString[65535];
            category = new int[65535];
            bytepos = 7;
            byteout = new ByteArray();
            DU = new int[64];
            YDU = new float[64];
            UDU = new float[64];
            VDU = new float[64];
            image = new BitmapData(texture);
            if (quality <= 0)
            {
                quality = 1;
            }
            if (quality > 100)
            {
                quality = 100;
            }
            if (quality < 50)
            {
                sf = (int)(5000 / quality);
            }
            else
            {
                sf = (int)(200 - quality * 2);
            }
            Thread thread = new Thread(new ThreadStart(doEncoding));
            thread.Start();
        }

        private void initQuantTables(int sf)
        {
            int i;
            float num;
            int[] array = new int[]
            {
            16,
            11,
            10,
            16,
            24,
            40,
            51,
            61,
            12,
            12,
            14,
            19,
            26,
            58,
            60,
            55,
            14,
            13,
            16,
            24,
            40,
            57,
            69,
            56,
            14,
            17,
            22,
            29,
            51,
            87,
            80,
            62,
            18,
            22,
            37,
            56,
            68,
            109,
            103,
            77,
            24,
            35,
            55,
            64,
            81,
            104,
            113,
            92,
            49,
            64,
            78,
            87,
            103,
            121,
            120,
            101,
            72,
            92,
            95,
            98,
            112,
            100,
            103,
            99
            };
            for (i = 0; i < 64; i++)
            {
                num = Mathf.Floor((array[i] * sf + 50) / 100);
                if (num < 1)
                {
                    num = 1;
                }
                else if (num > 255)
                {
                    num = 255;
                }
                YTable[ZigZag[i]] = (int)num;
            }
            int[] array2 = new int[]
            {
            17,
            18,
            24,
            47,
            99,
            99,
            99,
            99,
            18,
            21,
            26,
            66,
            99,
            99,
            99,
            99,
            24,
            26,
            56,
            99,
            99,
            99,
            99,
            99,
            47,
            66,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99,
            99
            };
            for (i = 0; i < 64; i++)
            {
                num = Mathf.Floor((array2[i] * sf + 50) / 100);
                if (num < 1)
                {
                    num = 1;
                }
                else if (num > 255)
                {
                    num = 255;
                }
                UVTable[ZigZag[i]] = (int)num;
            }
            float[] array3 = new float[]
            {
            1f,
            1.3870399f,
            1.306563f,
            1.17587554f,
            1f,
            0.785694957f,
            0.5411961f,
            0.27589938f
            };
            i = 0;
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    fdtbl_Y[i] = 1f / (YTable[ZigZag[i]] * array3[j] * array3[k] * 8f);
                    fdtbl_UV[i] = 1f / (UVTable[ZigZag[i]] * array3[j] * array3[k] * 8f);
                    i++;
                }
            }
        }

        private BitString[] computeHuffmanTbl(int[] nrcodes, int[] std_table)
        {
            int num = 0;
            int num2 = 0;
            BitString[] array = new BitString[256];
            for (int i = 1; i <= 16; i++)
            {
                for (int j = 1; j <= nrcodes[i]; j++)
                {
                    array[std_table[num2]] = new BitString();
                    array[std_table[num2]].val = num;
                    array[std_table[num2]].len = i;
                    num2++;
                    num++;
                }
                num *= 2;
            }
            return array;
        }

        private void initHuffmanTbl()
        {
            YDC_HT = computeHuffmanTbl(std_dc_luminance_nrcodes, std_dc_luminance_values);
            UVDC_HT = computeHuffmanTbl(std_dc_chrominance_nrcodes, std_dc_chrominance_values);
            YAC_HT = computeHuffmanTbl(std_ac_luminance_nrcodes, std_ac_luminance_values);
            UVAC_HT = computeHuffmanTbl(std_ac_chrominance_nrcodes, std_ac_chrominance_values);
        }

        private void initCategoryfloat()
        {
            int num = 1;
            int num2 = 2;
            int i;
            for (int j = 1; j <= 15; j++)
            {
                for (i = num; i < num2; i++)
                {
                    category[32767 + i] = j;
                    BitString bitString = new BitString();
                    bitString.len = j;
                    bitString.val = i;
                    bitcode[32767 + i] = bitString;
                }
                for (i = -(num2 - 1); i <= -num; i++)
                {
                    category[32767 + i] = j;
                    BitString bitString = new BitString();
                    bitString.len = j;
                    bitString.val = num2 - 1 + i;
                    bitcode[32767 + i] = bitString;
                }
                num <<= 1;
                num2 <<= 1;
            }
        }

        public byte[] GetBytes()
        {
            byte[] arg_28_0;
            if (!isDone)
            {
                Debug.LogError("JPEGEncoder not complete, cannot get bytes!");
                arg_28_0 = null;
            }
            else
            {
                arg_28_0 = byteout.GetAllBytes();
            }
            return arg_28_0;
        }

        private void writeBits(BitString bs)
        {
            int val = bs.val;
            int i = bs.len - 1;
            while (i >= 0)
            {
                if ((int)(val & Convert.ToUInt32(1 << i)) != 0)
                {
                    bytenew = bytenew | (int)Convert.ToUInt32(1 << bytepos);
                }
                i--;
                bytepos--;
                if (bytepos < 0)
                {
                    if (bytenew == 255)
                    {
                        writeByte(255);
                        writeByte(0);
                    }
                    else
                    {
                        writeByte((byte)bytenew);
                    }
                    bytepos = 7;
                    bytenew = 0;
                }
            }
        }

        private void writeByte(byte value)
        {
            byteout.writeByte(value);
        }

        private void writeWord(int value)
        {
            writeByte((byte)(value >> 8 & 255));
            writeByte((byte)(value & 255));
        }

        private float[] fDCTQuant(float[] data, float[] fdtbl)
        {
            float num;
            float num2;
            float num3;
            float num4;
            float num5;
            float num6;
            float num7;
            float num8;
            float num9;
            float num10;
            float num11;
            float num12;
            float num13;
            float num14;
            float num15;
            float num16;
            float num17;
            float num18;
            float num19;
            int i;
            int num20 = 0;
            for (i = 0; i < 8; i++)
            {
                num = data[num20 + 0] + data[num20 + 7];
                num8 = data[num20 + 0] - data[num20 + 7];
                num2 = data[num20 + 1] + data[num20 + 6];
                num7 = data[num20 + 1] - data[num20 + 6];
                num3 = data[num20 + 2] + data[num20 + 5];
                num6 = data[num20 + 2] - data[num20 + 5];
                num4 = data[num20 + 3] + data[num20 + 4];
                num5 = data[num20 + 3] - data[num20 + 4];
                num9 = num + num4;
                num12 = num - num4;
                num10 = num2 + num3;
                num11 = num2 - num3;
                data[num20 + 0] = num9 + num10;
                data[num20 + 4] = num9 - num10;
                num13 = (num11 + num12) * 0.707106769f;
                data[num20 + 2] = num12 + num13;
                data[num20 + 6] = num12 - num13;
                num9 = num5 + num6;
                num10 = num6 + num7;
                num11 = num7 + num8;
                num17 = (num9 - num11) * 0.382683426f;
                num14 = 0.5411961f * num9 + num17;
                num16 = 1.306563f * num11 + num17;
                num15 = num10 * 0.707106769f;
                num18 = num8 + num15;
                num19 = num8 - num15;
                data[num20 + 5] = num19 + num14;
                data[num20 + 3] = num19 - num14;
                data[num20 + 1] = num18 + num16;
                data[num20 + 7] = num18 - num16;
                num20 += 8;
            }
            num20 = 0;
            for (i = 0; i < 8; i++)
            {
                num = data[num20 + 0] + data[num20 + 56];
                num8 = data[num20 + 0] - data[num20 + 56];
                num2 = data[num20 + 8] + data[num20 + 48];
                num7 = data[num20 + 8] - data[num20 + 48];
                num3 = data[num20 + 16] + data[num20 + 40];
                num6 = data[num20 + 16] - data[num20 + 40];
                num4 = data[num20 + 24] + data[num20 + 32];
                num5 = data[num20 + 24] - data[num20 + 32];
                num9 = num + num4;
                num12 = num - num4;
                num10 = num2 + num3;
                num11 = num2 - num3;
                data[num20 + 0] = num9 + num10;
                data[num20 + 32] = num9 - num10;
                num13 = (num11 + num12) * 0.707106769f;
                data[num20 + 16] = num12 + num13;
                data[num20 + 48] = num12 - num13;
                num9 = num5 + num6;
                num10 = num6 + num7;
                num11 = num7 + num8;
                num17 = (num9 - num11) * 0.382683426f;
                num14 = 0.5411961f * num9 + num17;
                num16 = 1.306563f * num11 + num17;
                num15 = num10 * 0.707106769f;
                num18 = num8 + num15;
                num19 = num8 - num15;
                data[num20 + 40] = num19 + num14;
                data[num20 + 24] = num19 - num14;
                data[num20 + 8] = num18 + num16;
                data[num20 + 56] = num18 - num16;
                num20++;
            }
            for (i = 0; i < 64; i++)
            {
                data[i] = Mathf.Round(data[i] * fdtbl[i]);
            }
            return data;
        }

        private void writeAPP0()
        {
            writeWord(65504);
            writeWord(16);
            writeByte(74);
            writeByte(70);
            writeByte(73);
            writeByte(70);
            writeByte(0);
            writeByte(1);
            writeByte(1);
            writeByte(0);
            writeWord(1);
            writeWord(1);
            writeByte(0);
            writeByte(0);
        }

        private void writeSOF0(int width, int height)
        {
            writeWord(65472);
            writeWord(17);
            writeByte(8);
            writeWord(height);
            writeWord(width);
            writeByte(3);
            writeByte(1);
            writeByte(17);
            writeByte(0);
            writeByte(2);
            writeByte(17);
            writeByte(1);
            writeByte(3);
            writeByte(17);
            writeByte(1);
        }

        private void writeDQT()
        {
            writeWord(65499);
            writeWord(132);
            writeByte(0);
            int i;
            for (i = 0; i < 64; i++)
            {
                writeByte((byte)YTable[i]);
            }
            writeByte(1);
            for (i = 0; i < 64; i++)
            {
                writeByte((byte)UVTable[i]);
            }
        }

        private void writeDHT()
        {
            writeWord(65476);
            writeWord(418);
            int i;
            writeByte(0);
            for (i = 0; i < 16; i++)
            {
                writeByte((byte)std_dc_luminance_nrcodes[i + 1]);
            }
            for (i = 0; i <= 11; i++)
            {
                writeByte((byte)std_dc_luminance_values[i]);
            }
            writeByte(16);
            for (i = 0; i < 16; i++)
            {
                writeByte((byte)std_ac_luminance_nrcodes[i + 1]);
            }
            for (i = 0; i <= 161; i++)
            {
                writeByte((byte)std_ac_luminance_values[i]);
            }
            writeByte(1);
            for (i = 0; i < 16; i++)
            {
                writeByte((byte)std_dc_chrominance_nrcodes[i + 1]);
            }
            for (i = 0; i <= 11; i++)
            {
                writeByte((byte)std_dc_chrominance_values[i]);
            }
            writeByte(17);
            for (i = 0; i < 16; i++)
            {
                writeByte((byte)std_ac_chrominance_nrcodes[i + 1]);
            }
            for (i = 0; i <= 161; i++)
            {
                writeByte((byte)std_ac_chrominance_values[i]);
            }
        }

        private void writeSOS()
        {
            writeWord(65498);
            writeWord(12);
            writeByte(3);
            writeByte(1);
            writeByte(0);
            writeByte(2);
            writeByte(17);
            writeByte(3);
            writeByte(17);
            writeByte(0);
            writeByte(63);
            writeByte(0);
        }

        private float processDU(float[] CDU, float[] fdtbl, float DC, BitString[] HTDC, BitString[] HTAC)
        {
            BitString bs = HTAC[0];
            BitString bs2 = HTAC[240];
            int i;
            float[] array = fDCTQuant(CDU, fdtbl);
            for (i = 0; i < 64; i++)
            {
                DU[ZigZag[i]] = (int)array[i];
            }
            int num = (int)(DU[0] - DC);
            DC = DU[0];
            if (num == 0)
            {
                writeBits(HTDC[0]);
            }
            else
            {
                writeBits(HTDC[category[32767 + num]]);
                writeBits(bitcode[32767 + num]);
            }
            int num2 = 63;
            while (num2 > 0 && DU[num2] == 0)
            {
                num2--;
            }
            float arg_1A8_0;
            if (num2 == 0)
            {
                writeBits(bs);
                arg_1A8_0 = DC;
            }
            else
            {
                for (i = 1; i <= num2; i++)
                {
                    int num3 = i;
                    while (DU[i] == 0 && i <= num2)
                    {
                        i++;
                    }
                    int num4 = i - num3;
                    if (num4 >= 16)
                    {
                        for (int j = 1; j <= num4 / 16; j++)
                        {
                            writeBits(bs2);
                        }
                        num4 &= 15;
                    }
                    writeBits(HTAC[num4 * 16 + category[32767 + DU[i]]]);
                    writeBits(bitcode[32767 + DU[i]]);
                }
                if (num2 != 63)
                {
                    writeBits(bs);
                }
                arg_1A8_0 = DC;
            }
            return arg_1A8_0;
        }

        private void RGB2YUV(BitmapData img, int xpos, int ypos)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color pixelColor = img.getPixelColor(xpos + j, img.height - (ypos + i));
                    float num2 = pixelColor.r * 255;
                    float num3 = pixelColor.g * 255;
                    float num4 = pixelColor.b * 255;
                    YDU[num] = 0.299f * num2 + 0.587f * num3 + 0.114f * num4 - 128;
                    UDU[num] = -0.16874f * num2 + -0.33126f * num3 + 0.5f * num4;
                    VDU[num] = 0.5f * num2 + -0.41869f * num3 + -0.08131f * num4;
                    num++;
                }
            }
        }

        private void doEncoding()
        {
            isDone = false;
            Thread.Sleep(5);
            initHuffmanTbl();
            initCategoryfloat();
            initQuantTables(sf);
            encode();
            isDone = true;
            image = null;
            Thread.CurrentThread.Abort();
        }

        private void encode()
        {
            byteout = new ByteArray();
            bytenew = 0;
            bytepos = 7;
            writeWord(65496);
            writeAPP0();
            writeDQT();
            writeSOF0(image.width, image.height);
            writeDHT();
            writeSOS();
            float dC = 0;
            float dC2 = 0;
            float dC3 = 0;
            bytenew = 0;
            bytepos = 7;
            for (int i = 0; i < image.height; i += 8)
            {
                for (int j = 0; j < image.width; j += 8)
                {
                    RGB2YUV(image, j, i);
                    dC = processDU(YDU, fdtbl_Y, dC, YDC_HT, YAC_HT);
                    dC2 = processDU(UDU, fdtbl_UV, dC2, UVDC_HT, UVAC_HT);
                    dC3 = processDU(VDU, fdtbl_UV, dC3, UVDC_HT, UVAC_HT);
                    Thread.Sleep(0);
                }
            }
            if (bytepos >= 0)
            {
                writeBits(new BitString
                {
                    len = bytepos + 1,
                    val = (1 << bytepos + 1) - 1
                });
            }
            writeWord(65497);
            isDone = true;
        }
    }
#endif

    [Serializable]
    public class latlong_area_class
    {
        public latlong_class latlong1;

        public latlong_class latlong2;

        public latlong_area_class()
        {
            latlong1 = new latlong_class();
            latlong2 = new latlong_class();
        }
    }



    [Serializable]
    public class latlong_class
    {
        public double latitude;

        public double longitude;

        public latlong_class()
        {
        }

        public latlong_class(double latitude1, double longitude1)
        {
            latitude = latitude1;
            longitude = longitude1;
        }

        public void reset()
        {
            latitude = 0;
            longitude = 0;
        }
    }
}
