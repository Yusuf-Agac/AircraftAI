using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.Networking;

namespace WorldComposer
{
    [Serializable]
    public class Map_tc : EditorWindow
    {
        public string install_path;
        public Texture2D texture;
        public bool display_text; 
        public string text;
        public float rotAngle;
        public float label_old;
        public bool scroll;
        public int scroll_mode;
        public Vector2 mouse_position;
        public Vector2 mouse_position_old;
        public Vector2 mouse_move;
        public Event key;
        public bool button0;
        public bool button2;
        public int count_curve;
        public Vector2 offmap1;
        public Vector2 offmap2;
        public Vector2 offmap3;
        public Vector2 offmap4;
        public Vector2 offmap;
        public Rect hFileRect1;
        public Rect hFileRect2;
        public Rect iFileRect1;
        public Rect iFileRect2;
        public bool content_checked;
        public Vector2 offset;
        public Vector2 pixel3;
        public Vector2 pixel4;
        public Color[] pixels;
        public Vector2 offset2;
        public float time1;
        public GameObject Global_Settings_Scene;
        public global_settings_tc global_script;
        public GameObject TerrainComposer_Scene;
        public GameObject TerrainComposer_Parent;
        public EditorWindow tc_script;
        public Info_tc info_window;
        public bool terraincomposer;
        public Material material;
        public bool zooming;
        public double zoom;
        public double zoom1;
        public double zoom2;
        public double zoom3;
        public double zoom4;
        public double zoom_step;
        public double zoom1_step;
        public double zoom2_step;
        public double zoom3_step;
        public double zoom4_step;
        public double zoom_pos;
        public double zoom_pos1;
        public double zoom_pos2;
        public double zoom_pos3;
        public double zoom_pos4;
        public bool request1;
        public bool request2;
        public bool request3;
        public bool request4;
        public bool request_load1;
        public bool request_load2;
        public bool request_load3;
        public bool request_load4;
        public Rect screen_rect;
        public Rect screen_rect2;
        public Rect map_parameters_rect;
        public Rect regions_rect;
        public Rect areas_rect;
        public Rect heightmap_export_rect;
        public Rect image_export_rect;
        public Rect image_editor_rect;
        public Rect converter_rect;
        public Rect settings_rect;
        public Rect rectWindow;
        public Rect help_rect;
        public Rect update_rect;
        public bool animate;
        public latlong_class latlong_animate;
        public Vector2 animate_pixel;
        public float animate_time_start;
        public bool script_set;
        public float tt1;
        public int gui_y;
        public int gui_y2;
        public int gui_height;
        public int guiWidth1;
        public int guiWidth2;
        public int guiWidth3;
        public int guiAreaHeight;
        public map_region_class current_region;
        public map_area_class current_area;
        public map_region_class create_region;
        public map_area_class create_area;
        public map_area_class convert_area;
        public Texture2D convert_texture;
        public tile_class convert_tile;
        public map_region_class export_heightmap_region;
        public map_area_class export_heightmap_area;
        public map_region_class export_image_region;
        public map_area_class export_image_area;
        public map_area_class import_image_area;
        public bool gui_changed_old;
        public map_area_class requested_area;
        public latlong_class latlong_mouse;
        public latlong_class latlong_center;
        public latlong_area_class latlong_area;
        public FileStream fs;
        public byte[] bytes;
        public Vector2 export_p1;
        public Vector2 export_p2;
        public float width_p1;
        public float height_p1;
        public terrain_region_class terrain_region;
        public bool colormap;
        public string[] heightmap_resolution_list;
        public int heightmap_resolution_select;
        public string[] image_resolution_list;
        public int image_resolution_select;
        public string path_old;
        public bool import_settings_call;
        public bool import_jpg_call;
        public bool import_png_call;
        public string import_jpg_path;
        public string import_png_path;
        public bool map_scrolling;
        public bool area_rounded;
        public bool generate_manual;
        public int old_fontSize;
        public FontStyle old_fontStyle;
        public float save_global_time;
        public bool focus;
        public Texture button_settings;
        public Texture button_help;
        public Texture button_discord;
        public Texture button_heightmap;
        public Texture button_update;
        public Texture button_terrain;
        public Texture button_map;
        public Texture button_region;
        public Texture button_edit;
        public Texture button_image;
        public Texture button_converter;
        public gui_class wc_gui;
        public Vector2 area_size_old;
        public string notify_text;
        public int notify_frame;
        public Vector2 scrollPos;
        public ulong combine_width;
        public ulong combine_height;
        public ulong combine_length;
        public map_area_class combine_area;
        public string combine_import_filename;
        public string combine_import_path;
        public byte[] combine_byte;
        public FileStream combine_import_file;
        public FileStream combine_export_file;
        public string combine_export_path;
        public string combine_export_filename;
        public int combine_x;
        public int combine_y;
        public int combine_y1;
        public float combine_time;
        public bool combine_generate;
        public bool slice_generate;
        public float combine_progress;
        public bool combine_call;
        public Color[] combine_pixels;
        public Vector3 terrain_size;
        public GameObject terrain_parent;
        public float frames;
        public float auto_speed_time;
        public bool generate;
        public int generate_speed;
        public float generate_time_start;
        public float[,] heights;
        public float heightmap_x;
        public float heightmap_y;
        public float heightmap_res_x;
        public float heightmap_res_y;
        public int heightmap_resolution;
        public int heightmap_break_x_value;
        public int heightmap_count_terrain;
        public float tarframe;
        public int h_local_x;
        public int h_local_y;
        public RawFile rawFile;
        public Vector2 conversion_step;
        public bool create_terrain_loop;
        public bool apply_import_settings;
        public int import_settings_count;
        public int create_terrain_count;
        public int create_splat_count;
        public int x_old;

        Texture2D map0b;

        Texture2D map0;
        Texture2D map1;
        Texture2D map2;
        Texture2D map3;

        
        Texture2D tex2;
        Texture2D tex3;

        map_class map;

        public Map_tc()
        {
            install_path = string.Empty;
            rotAngle = 90;
            scroll_mode = 1;
            guiWidth1 = 120;
            guiWidth2 = 335;
            convert_tile = new tile_class();
            terrain_region = new terrain_region_class();
            colormap = true;
            heightmap_resolution_list = new string[]
            {
            "33",
            "65",
            "129",
            "257",
            "513",
            "1025",
            "2049",
            "4097"
            };
            image_resolution_list = new string[]
            {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096",
            "8192"
            };
            wc_gui = new gui_class(3);
            notify_text = string.Empty;
            generate_speed = 10000;
            tarframe = 30;
            rawFile = new RawFile();
        }

        [MenuItem("Window/World Composer")]
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(Map_tc));
            window.titleContent = new GUIContent("World");
        }

        public void OnEnable()
        {
            install_path = GetInstallPath();
            if (Drawing_tc1.lineMaterial == null)
            {
                Drawing_tc1.lineMaterial = (((Material)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Drawing_tc.mat", typeof(Material))) as Material);
            }
            Get_TerrainComposer_Scene();
            load_button_textures();
            if (Type.GetType("TerrainComposer") != null)
            {
                terraincomposer = true;
            }
            else
            {
                terraincomposer = false;
            }
            content_startup();

            WebRequest.ResetStampTime();

            request_map();
        }

        public string GetInstallPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            int length = assetPath.LastIndexOf("/Editor");
            return assetPath.Substring(0, length);
        }

        public void Get_TerrainComposer_Scene()
        {
            load_global_settings();
        }

        public void OnInspectorUpdate()
        {
            if (focus)
            {
                Repaint();
            }
            if (global_script && map.preimage_edit.generate)
            {
                Repaint();
            }
        }

        public void OnFocus()
        {
            focus = true;
            install_path = GetInstallPath();
            Get_TerrainComposer_Scene();
            init_paths();
        }

        public void OnLostFocus()
        {
            focus = false;
        }

        public void OnDisable()
        {
            if (tc_script != null)
            {
                tc_script.Repaint();
            }
        }

        public void OnDestroy()
        {
            save_global_settings();
            stop_all_elevation_pull();
            StopAllImagePull();

            Draw.DisposeTextures();
            Draw.DisposeTexture(ref map0b);
            Draw.DisposeTexture(ref map0);
            Draw.DisposeTexture(ref map1);
            Draw.DisposeTexture(ref map2);
            Draw.DisposeTexture(ref map3);
            Draw.DisposeTexture(ref tex2);
            Draw.DisposeTexture(ref tex3);
        }

        void CopyMap0bIntoMap0()
        {
            pixels = map0b.GetPixels(0, 32, 800, 768);
            if (CheckImageError()) global_script.settings.myExt2.RedoRequest();
            else
            {
                map0.SetPixels(800, 0, 800, 768, pixels);
            }
        }

        Color GetAnimColor()
        {
            return new Color(Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup)), Mathf.Abs(Mathf.Cos(Time.realtimeSinceStartup)), 0);
        }

        public void OnGUI()
        {
            key = Event.current;
            if (!global_script)
            {
                if (key.type == EventType.Repaint)
                {
                    Get_TerrainComposer_Scene();
                }
                return;
            }

            Draw.InitTextures(global_script);

            gui_y = 0;
            gui_y2 = 0;
            guiWidth2 = 335;
            guiWidth3 = 359;
            int num = 0;
            int num2 = 0;
            if (map.region_select > map.region.Count - 1)
            {
                map.region_select = map.region.Count - 1;
            }
            current_region = map.region[map.region_select];
            if (current_region.area_select > current_region.area.Count - 1)
            {
                current_region.area_select = current_region.area.Count - 1;
            }
            current_area = current_region.area[current_region.area_select];
            if (map.warnings && current_area != null && (current_area.heightmap_resolution.x > 4096 || current_area.heightmap_resolution.y > 4096))
            {
                if (notify_text.Length != 0)
                {
                    notify_text += "\n\n";
                }
                notify_text = "The heightmap resolution is bigger then 4096, please keep in mind the Unity terrain performance and the 50k transaction limit of Bing, exceeding this amount by magnitudes within 24 hours might block your Bing key." + "\nMake your heightmap a lower resolution in 'Heightmap Export' -> 'Heightmap Zoom' -> Clicking the '-' button.\n\nPlease read page 7 in the WC manual, after reading and understanding you can disable the warnings in the 'Settings' tab -> Show Warnings";
            }

            GUI.color = Color.white;
            if (key.keyCode == KeyCode.F5 && key.type == EventType.KeyDown)
            {
                request_map();
            }
            latlong_mouse = Mathw.pixel_to_latlong(new Vector2(key.mousePosition.x - position.width / 2 + offmap.x, key.mousePosition.y - position.height / 2 - offmap.y), global_script.map_latlong_center, zoom);
            latlong_center = Mathw.pixel_to_latlong(new Vector2(offmap.x, -offmap.y), global_script.map_latlong_center, zoom);
            if (map3 && global_script.map_zoom_old > 3 && !map.button_image_editor)
            {
                num = (int)((zoom_pos4 + 1) * map3.width * 4 * 2);
                num2 = (int)((zoom_pos4 + 1) * map3.height * 4 * 2);
                if (num < position.width * 12)
                {
                    EditorGUI.DrawPreviewTexture(new Rect((float)(-offmap4.x - zoom_pos4 * (map3.width * 2 * 2 + offmap4.x) - 2400 - (800 - position.width / 2)), (float)(offmap4.y - zoom_pos4 * (3200 - offmap4.y) - 2800 - (400 - position.height / 2)), num, num2), map3);
                }
            }
            if (map2 && global_script.map_zoom_old > 2 && !map.button_image_editor)
            {
                if (global_script.map_zoom <= global_script.map_zoom_old + 2 || !global_script.map_load4 || global_script.map_load3)
                {
                    num = (int)((zoom_pos3 + 1) * map2.width * 4);
                    num2 = (int)((zoom_pos3 + 1) * map2.height * 4);
                    if (num < position.width * 12)
                    {
                        EditorGUI.DrawPreviewTexture(new Rect((float)(-offmap3.x - zoom_pos3 * (map2.width * 2 + offmap3.x) - 800 - (800 - position.width / 2)), (float)(offmap3.y - zoom_pos3 * (1600 - offmap3.y) - 1200 - (400 - position.height / 2)), num, num2), map2);
                    }
                }
            }
            if (map0)
            {
                if ((global_script.map_zoom <= global_script.map_zoom_old + 2 || !global_script.map_load3) && (global_script.map_zoom <= global_script.map_zoom_old + 3 || !global_script.map_load4 || global_script.map_combine))
                {
                    EditorGUI.DrawPreviewTexture(new Rect((float)(-offmap1.x - zoom_pos1 * (map0.width / 2 + offmap1.x) - (800 - position.width / 2)), (float)(offmap1.y - zoom_pos1 * (400 - offmap1.y) - (400 - position.height / 2)), (float)(map0.width + zoom_pos1 * map0.width), (float)(map0.height + zoom_pos1 * map0.height)), map0);
                }
            }

            // int ww = 1024;
            // EditorGUI.DrawPreviewTexture(new Rect(ww + 10, 40, 256, 256), tex2);
            // EditorGUI.DrawPreviewTexture(new Rect(ww + 10 + 266, 40, 256, 256), tex3);

            zoom = Mathf.Log((float)(zoom_pos1 + 1), 2) + global_script.map_zoom_old;
            if (!map.button_image_editor)
            {
                for (int i = 0; i < map.region.Count; i++)
                {
                    current_region = map.region[i];
                    for (int j = 0; j < current_region.area.Count; j++)
                    {
                        if (current_region.area[j].created)
                        {
                            pixel3 = Mathw.latlong_to_pixel(current_region.area[j].upper_left, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                            if (current_region.area[j].select == 1)
                            {
                                latlong_area = Mathw.calc_latlong_area_rounded(current_region.area[j].upper_left, latlong_mouse, current_region.area[j].image_zoom, current_region.area[j].resolution, key.shift, 8);
                                current_region.area[j].lower_right = latlong_area.latlong2;
                            }
                            current_region.area[j].tiles = Mathw.calc_latlong_area_tiles(current_region.area[j].upper_left, current_region.area[j].lower_right, current_region.area[j].image_zoom, current_region.area[j].resolution);
                            pixel4 = Mathw.latlong_to_pixel(current_region.area[j].lower_right, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                            float num3 = pixel4.x - pixel3.x;
                            float num4 = pixel4.y - pixel3.y;
                            Color color = default(Color);
                            if (j == current_region.area_select && i == map.region_select)
                            {
                                current_area.size = Mathw.calc_latlong_area_size(current_area.upper_left, current_area.lower_right, current_area.center);
                                color = Color.yellow;
                                area_rounded = Draw.draw_latlong_raster(global_script.map_latlong_center, current_region.area[j].upper_left, current_region.area[j].lower_right, offmap, current_region.area[j].image_zoom, zoom, current_region.area[j].resolution, new Rect(0, 0, position.width, position.height), new Color(1, 1, 0, 0.5f), 2);
                                if (current_region.area[j].export_heightmap_active || current_region.area[j].export_image_active)
                                {
                                    color = new Color(1, 1, 1, 1);
                                }
                                else if (current_region.area[j].export_heightmap_call || current_region.area[j].export_image_call)
                                {
                                    color = new Color(1, 0.5f, 0, 1);
                                }
                                Draw.DrawRect(new Rect(-offmap.x + pixel3.x, offmap.y + pixel3.y, num3, num4), color, 2, new Rect(0, 0, position.width, position.height));
                                color = Color.yellow;
                                if (key.button == 0 && key.isMouse && key.type == 0 && current_area.start_tile_enabled && new Rect(pixel3.x, pixel3.y, num3, num4).Contains(key.mousePosition))
                                {
                                    current_area.start_tile.x = (int)Mathf.Floor((key.mousePosition.x - pixel3.x) / (num3 / current_area.tiles.x));
                                    current_area.start_tile.y = (int)Mathf.Floor((key.mousePosition.y - pixel3.y) / (num4 / current_area.tiles.y));
                                    current_area.start_tile_enabled = false;
                                    Repaint();
                                }
                                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate && (export_image_area != current_area) || (!current_area.export_image_active && !current_area.export_heightmap_active))
                                {
                                    if (map.mode == 2)
                                    {
                                        Repaint();
                                        if (current_area.start_tile.x > current_area.tiles.x - 1)
                                        {
                                            current_area.start_tile.x = current_area.tiles.x - 1;
                                        }
                                        else if (current_area.start_tile.x < 0)
                                        {
                                            current_area.start_tile.x = 0;
                                        }
                                        if (current_area.start_tile.y > current_area.tiles.y - 1)
                                        {
                                            current_area.start_tile.y = current_area.tiles.y - 1;
                                        }
                                        else if (current_area.start_tile.y < 0)
                                        {
                                            current_area.start_tile.y = 0;
                                        }
                                        if (!current_area.resize)
                                        {
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x - 10, pixel3.y + 20, 20, num4 - 40), MouseCursor.ResizeHorizontal);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + num3 - 10, pixel3.y + 20, 20, num4 - 40), MouseCursor.ResizeHorizontal);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + num3 / 2 - 20, pixel3.y + num4 / 2 - 20, 40, 40), MouseCursor.Link);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + 20, pixel3.y - 10, num3 - 40, 20), MouseCursor.ResizeVertical);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + 20, pixel3.y + num4 - 10, num3 - 40, 20), MouseCursor.ResizeVertical);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x - 20, pixel3.y - 20, 40, 40), MouseCursor.ResizeUpLeft);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + num3 - 20, pixel3.y - 20, 40, 40), MouseCursor.ResizeUpRight);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x - 20, pixel3.y - 20 + num4, 40, 40), MouseCursor.ResizeUpRight);
                                            EditorGUIUtility.AddCursorRect(new Rect(pixel3.x + num3 - 20, pixel3.y - 20 + num4, 40, 40), MouseCursor.ResizeUpLeft);
                                        }
                                        if (key.button == 0 && key.isMouse)
                                        {
                                            if (key.type == EventType.MouseDown)
                                            {
                                                if (!current_area.resize)
                                                {
                                                    if (new Rect(pixel3.x - 10, pixel3.y + 20, 20, num4 - 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_left = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + num3 - 10, pixel3.y + 20, 20, num4 - 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_right = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + 20, pixel3.y - 10, num3 - 40, 20).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_top = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + 20, pixel3.y + num4 - 10, num3 - 40, 20).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_bottom = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x - 20, pixel3.y - 20, 40, 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_topLeft = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + num3 - 20, pixel3.y - 20, 40, 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_topRight = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x - 20, pixel3.y - 20 + num4, 40, 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_bottomLeft = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + num3 - 20, pixel3.y - 20 + num4, 40, 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_bottomRight = true;
                                                        current_area.resize = true;
                                                    }
                                                    else if (new Rect(pixel3.x + num3 / 2 - 20, pixel3.y + num4 / 2 - 20, 40, 40).Contains(key.mousePosition))
                                                    {
                                                        current_area.resize_center = true;
                                                        current_area.resize = true;
                                                    }
                                                }
                                            }
                                            else if (key.type == EventType.MouseUp)
                                            {
                                                current_area.resize_left = false;
                                                current_area.resize_right = false;
                                                current_area.resize_top = false;
                                                current_area.resize_bottom = false;
                                                current_area.resize_topLeft = false;
                                                current_area.resize_topRight = false;
                                                current_area.resize_bottomLeft = false;
                                                current_area.resize_bottomRight = false;
                                                current_area.resize_center = false;
                                                current_area.resize = false;
                                                map.elExt_check_assign = true;
                                                requested_area = current_area;
                                                get_elevation_info(current_area.center);
                                            }
                                        }
                                    }
                                    calc_heightmap_settings(current_area);
                                    if (!current_area.terrain_heightmap_resolution_changed)
                                    {
                                        calc_terrain_heightmap_resolution();
                                    }
                                    if (current_area.resize_left)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeHorizontal);
                                        latlong_area = Mathw.calc_latlong_area_rounded(new latlong_class(current_area.upper_left.latitude, latlong_mouse.longitude), current_area.lower_right, current_area.image_zoom, current_area.resolution, key.shift, 1);
                                        current_area.upper_left.longitude = latlong_area.latlong1.longitude;
                                    }
                                    else if (current_area.resize_right)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeHorizontal);
                                        latlong_area = Mathw.calc_latlong_area_rounded(current_area.upper_left, new latlong_class(current_area.lower_right.latitude, latlong_mouse.longitude), current_area.image_zoom, current_area.resolution, key.shift, 2);
                                        current_area.lower_right.longitude = latlong_area.latlong2.longitude;
                                    }
                                    else if (current_area.resize_top)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeVertical);
                                        latlong_area = Mathw.calc_latlong_area_rounded(new latlong_class(latlong_mouse.latitude, current_area.upper_left.longitude), current_area.lower_right, current_area.image_zoom, current_area.resolution, key.shift, 3);
                                        current_area.upper_left.latitude = latlong_area.latlong1.latitude;
                                    }
                                    else if (current_area.resize_bottom)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeVertical);
                                        latlong_area = Mathw.calc_latlong_area_rounded(current_area.upper_left, new latlong_class(latlong_mouse.latitude, current_area.lower_right.longitude), current_area.image_zoom, current_area.resolution, key.shift, 4);
                                        current_area.lower_right.latitude = latlong_area.latlong2.latitude;
                                    }
                                    else if (current_area.resize_topLeft)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeUpLeft);
                                        latlong_area = Mathw.calc_latlong_area_rounded(latlong_mouse, current_area.lower_right, current_area.image_zoom, current_area.resolution, key.shift, 5);
                                        current_area.upper_left = latlong_area.latlong1;
                                    }
                                    else if (current_area.resize_topRight)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeUpRight);
                                        latlong_area = Mathw.calc_latlong_area_rounded(new latlong_class(latlong_mouse.latitude, current_area.upper_left.longitude), new latlong_class(current_area.lower_right.latitude, latlong_mouse.longitude), current_area.image_zoom, current_area.resolution, key.shift, 6);
                                        current_area.upper_left.latitude = latlong_area.latlong1.latitude;
                                        current_area.lower_right.longitude = latlong_area.latlong2.longitude;
                                    }
                                    else if (current_area.resize_bottomLeft)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeUpRight);
                                        latlong_area = Mathw.calc_latlong_area_rounded(new latlong_class(current_area.upper_left.latitude, latlong_mouse.longitude), new latlong_class(latlong_mouse.latitude, current_area.lower_right.longitude), current_area.image_zoom, current_area.resolution, key.shift, 7);
                                        current_area.upper_left.longitude = latlong_area.latlong1.longitude;
                                        current_area.lower_right.latitude = latlong_area.latlong2.latitude;
                                    }
                                    else if (current_area.resize_bottomRight)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.ResizeUpLeft);
                                        latlong_area = Mathw.calc_latlong_area_rounded(current_area.upper_left, latlong_mouse, current_area.image_zoom, current_area.resolution, key.shift, 8);
                                        current_area.lower_right = latlong_area.latlong2;
                                    }
                                    else if (current_area.resize_center)
                                    {
                                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.Link);
                                        Mathw.calc_latlong_area_from_center(current_area, latlong_mouse, current_area.image_zoom, new Vector2(current_area.resolution * current_area.tiles.x, current_area.resolution * current_area.tiles.y));
                                    }
                                }
                                if (current_area.start_tile.x != 0 || current_area.start_tile.y != 0)
                                {
                                    export_p1.x = pixel3.x + num3 / current_area.tiles.x * current_area.start_tile.x;
                                    export_p1.y = pixel3.y + num4 / current_area.tiles.y * current_area.start_tile.y;
                                    width_p1 = num3 / current_area.tiles.x;
                                    height_p1 = num4 / current_area.tiles.y;
                                    Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), new Color(1, 0, 0, 1), 2, new Rect(0, 0, position.width, position.height));
                                }
                            }
                            else
                            {
                                if (current_region.area[j].export_heightmap_active || current_region.area[j].export_image_active)
                                {
                                    color = new Color(1, 1, 1, 1);
                                }
                                else if (current_region.area[j].export_heightmap_call || current_region.area[j].export_image_call)
                                {
                                    color = new Color(1, 0.5f, 0, 1);
                                }
                                else
                                {
                                    color = Color.green;
                                }
                                Draw.DrawRect(new Rect(-offmap.x + pixel3.x, offmap.y + pixel3.y, num3, num4), color, 2, new Rect(0, 0, position.width, position.height));
                                color = Color.green;
                            }
                            current_region.area[j].center = Mathw.calc_latlong_center(current_region.area[j].upper_left, current_region.area[j].lower_right, zoom, new Vector2(position.width, position.height));
                            Vector2 vector = Mathw.latlong_to_pixel(current_region.area[j].center, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                            float num5 = num3 / 175 - 0.7f;
                            float num6 = num4 / 175 - 0.7f;
                            if (num5 > 1)
                            {
                                num5 = 1;
                            }
                            if (num6 > 1)
                            {
                                num6 = 1;
                            }
                            float fontSize = 0f;
                            if (zoom > 12)
                            {
                                fontSize = (float)(zoom + (zoom - 12) * 3);
                            }
                            else
                            {
                                fontSize = 12;
                            }
                            if (num5 / 2 > 0)
                            {
                                Draw.drawText((current_region.area[j].size.x / 1000).ToString("F3") + " (Km)", new Vector2(-offmap.x + pixel3.x + num3 / 2, offmap.y + pixel3.y), true, new Color(1, 0, 0, num5), new Color(color.r, color.g, color.b, num5 / 2), 0, fontSize, false, 2);
                                Draw.drawText((current_region.area[j].size.y / 1000).ToString("F3") + " (Km)", new Vector2(-offmap.x + pixel3.x - 30, offmap.y + pixel3.y + num4 / 2), true, new Color(1, 0, 0, num6), new Color(color.r, color.g, color.b, num6 / 2), -90, fontSize, false, 4);
                                if (current_area.resize && !current_area.resize_center && j == current_region.area_select && i == map.region_select)
                                {
                                    Draw.drawText(current_region.area[j].tiles.x.ToString() + "x" + current_region.area[j].tiles.y.ToString(), key.mousePosition + new Vector2(10, 20), true, new Color(1, 0, 0, 1), color + new Color(0, 0, 0, -0.5f), 0, 12, true, 3);
                                }
                                else
                                {
                                    Draw.drawText(current_region.area[j].tiles.x.ToString() + "x" + current_region.area[j].tiles.y.ToString(), new Vector2(pixel4.x - offmap.x, pixel4.y + offmap.y), true, new Color(1, 0, 0, num5), new Color(color.r, color.g, color.b, num5 / 2), 0, 12, true, 5);
                                }
                            }
                            Vector2 vector2 = Draw.drawText(current_region.area[j].name, new Vector2(-offmap.x + pixel3.x, offmap.y + pixel3.y), true, new Color(1, 0, 0, 1), color + new Color(0, 0, 0, -0.5f), 0, fontSize, true, 3);
                            if (key.button == 0 && key.type == 0 && new Rect(-offmap.x + pixel3.x, offmap.y + pixel3.y - vector2.y, vector2.x, vector2.y).Contains(key.mousePosition))
                            {
                                map.region_select = i;
                                map.region[i].area_select = j;
                                Repaint();
                            }
                            if (map.export_heightmap_active)
                            {
                                for (int k = 0; k < map.elExt.Count; k++)
                                {
                                    export_p1 = Mathw.latlong_to_pixel(map.elExt[k].latlong_area.latlong1, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                                    export_p2 = Mathw.latlong_to_pixel(map.elExt[k].latlong_area.latlong2, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                                    width_p1 = export_p2.x - export_p1.x;
                                    height_p1 = export_p2.y - export_p1.y;
                                    if (map.elExt[k].error == 1)
                                    {
                                        Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), new Color(0.8f, 0, 0, 1), 2, new Rect(0, 0, position.width, position.height));
                                    }
                                    else if (map.elExt[k].error == 2)
                                    {
                                        Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), new Color(0.8f, 0, 0.8f, 1), 2, new Rect(0, 0, position.width, position.height));
                                    }
                                    else
                                    {
                                        Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), new Color(0.95f, 0.62f, 0.04f, 1), 2, new Rect(0, 0, position.width, position.height));
                                    }
                                }
                            }
                            if (map.export_image_active)
                            {
                                for (int l = 0; l < map.texExt.Count; l++)
                                {
                                    export_p1 = Mathw.latlong_to_pixel(map.texExt[l].latlong_area.latlong1, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                                    export_p2 = Mathw.latlong_to_pixel(map.texExt[l].latlong_area.latlong2, global_script.map_latlong_center, zoom, new Vector2(position.width, position.height));
                                    width_p1 = export_p2.x - export_p1.x;
                                    height_p1 = export_p2.y - export_p1.y;
                                    if (map.texExt[l].error == 1)
                                    {
                                        Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), new Color(0.8f, 0, 0, 1), 2, new Rect(0, 0, position.width, position.height));
                                    }
                                    else
                                    {
                                        Draw.DrawRect(new Rect(-offmap.x + export_p1.x, offmap.y + export_p1.y, width_p1, height_p1), Color.green, 2, new Rect(0, 0, position.width, position.height));
                                    }
                                }
                            }
                            Drawing_tc1.DrawLine(new Vector2((float)(vector.x - 10 - offmap.x + (7 - zoom / 2.7f) / 1), (float)(vector.y - 10 + offmap.y + (7 - zoom / 2.7f) / 1)), new Vector2((float)(vector.x + 10 - offmap.x - (7 - zoom / 2.7f) / 1), (float)(vector.y + 10 + offmap.y - (7 - zoom / 2.7f) / 1)), color, 1, false, new Rect(0, 0, position.width, position.height));
                            Drawing_tc1.DrawLine(new Vector2((float)(vector.x - 10 - offmap.x + (7 - zoom / 2.7f) / 1), (float)(vector.y + 10 + offmap.y - (7 - zoom / 2.7f) / 1)), new Vector2((float)(vector.x + 10 - offmap.x - (7 - zoom / 2.7f) / 1), (float)(vector.y - 10 + offmap.y + (7 - zoom / 2.7f) / 1)), color, 1, false, new Rect(0, 0, position.width, position.height));
                        }
                    }
                }
            }

            current_region = map.region[map.region_select];
            if (current_region.area.Count > 0 && map.mode == 1)
            {
                if (key.button == 0 && key.isMouse && key.type == EventType.MouseDown && !check_in_rect())
                {
                    if (current_area.select == 0)
                    {
                        current_area.upper_left = latlong_mouse;
                        current_area.select = 1;
                        current_area.created = true;
                        requested_area = current_area;
                        map.elExt_check_assign = true;
                        get_elevation_info(current_area.upper_left);
                        current_area.start_tile_enabled = false;
                        current_area.start_tile.x = 0;
                        current_area.start_tile.y = 0;
                    }
                    else
                    {
                        pick_done();
                    }
                }
                if (key.button == 1)
                {
                    current_area.select = 0;
                    current_area.created = false;
                    current_area.reset();
                }
                if (current_area.select == 1)
                {
                    calc_heightmap_settings(current_area);
                    if (!current_area.terrain_heightmap_resolution_changed)
                    {
                        calc_terrain_heightmap_resolution();
                    }
                    Repaint();
                }
            }
            if (map.mode != 1 && key.button == 1 && key.type == EventType.MouseDown && map.elExt_check_loaded)
            {
                get_elevation_info(latlong_mouse);
            }
            Drawing_tc1.DrawLine(new Vector2(position.width / 2, position.height / 2 - 10), new Vector2(position.width / 2, position.height / 2 + 10), Color.green, 1, false, new Rect(0, 0, position.width, position.height));
            Drawing_tc1.DrawLine(new Vector2(position.width / 2 - 10, position.height / 2), new Vector2(position.width / 2 + 10, position.height / 2), Color.green, 1, false, new Rect(0, 0, position.width, position.height));
            GUI.color = map.titleColor;
            GUI.DrawTexture(new Rect(0, 0, 1453, 24), Draw.tex2);
            if (map.button_parameters || map.button_region || map.button_heightmap_export || map.button_image_export || map.button_settings || map.button_image_editor || map.button_update)
            {
                GUI.color = map.titleColor;
                if (map.button_image_editor)
                {
                    GUI.DrawTexture(new Rect(0, 24, guiWidth2 + 348, 19), Draw.tex2);
                }
                else
                {
                    GUI.DrawTexture(new Rect(0, 24, guiWidth2, 19), Draw.tex2); 
                }
            }
            GUI.color = new Color(1, 1, 1, map.alpha);
            int num7 = 0;
            if (map.button_image_editor)
            {
                num7 += 113 + map.preimage_edit.edit_color.Count * 18;
                if (current_area.preimage_save_new)
                {
                    num7 += 60;
                }
            }
            Rect rect;
            if (map.button_parameters && map.key_edit)
            {
                GUI.color = map.backgroundColor;
                rect = new Rect(guiWidth3, num7 + 45 - scrollPos.y, 799, 58);
                EditorGUI.DrawPreviewTexture(rect, Draw.tex2);
                GUI.color = Color.red;
                EditorGUI.LabelField(rect, "You need to create a free Bing key. Read the manual in the WorldComposer folder how to do this.\nIf you are in Webplayer build mode, read the troubleshooting in the WorldComposer manual to get it working.\nAfter you entered the key, press F5 to refresh. Then press the 'K' button in Map Parameters to hide the key and this text.\nThen follow the steps on page 6 of the manual to export and create a terrain.", EditorStyles.boldLabel);
                if (map.button_update)
                {
                    num7 += 86;
                }
                for (int m = 0; m < map.bingKey.Count; m++)
                {
                    GUI.color = map.backgroundColor;
                    rect = new Rect(guiWidth3, num7 + m * 19.9f + 220 - 96 - map.bingKey.Count * 0 - scrollPos.y, Draw.label_width("Key" + m.ToString() + " -> \"" + map.bingKey[m].key + "\"", true), 17);
                    EditorGUI.DrawPreviewTexture(rect, Draw.tex2);
                    GUI.color = Color.red;
                    EditorGUI.LabelField(rect, "Key" + m.ToString() + " -> \"" + map.bingKey[m].key + "\"", EditorStyles.boldLabel);
                }
                if (map.button_update)
                {
                    num7 -= 86;
                }
                GUI.color = Color.white;
            }
            if (map.path_display)
            {
                GUI.color = map.backgroundColor;
                rect = new Rect(heightmap_export_rect.x + guiWidth2 + 25, heightmap_export_rect.y + 60 + num7 + 46 - scrollPos.y, Draw.label_width(current_area.export_heightmap_path, true), 20);
                EditorGUI.DrawPreviewTexture(rect, Draw.tex2);
                GUI.color = map.color;
                EditorGUI.LabelField(rect, new GUIContent(current_area.export_heightmap_path), EditorStyles.boldLabel);
            }
            if (map.path_display)
            {
                GUI.color = map.backgroundColor;
                rect = new Rect(image_export_rect.x + guiWidth2 + 25, image_export_rect.y + num7 + 117 + 43 - scrollPos.y, Draw.label_width(current_area.export_image_path, true), 20);
                EditorGUI.DrawPreviewTexture(rect, Draw.tex2);
                GUI.color = map.color;
                EditorGUI.LabelField(rect, new GUIContent(current_area.export_image_path), EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUI.backgroundColor = new Color(1, 1, 1, 0.75f);
            if (map.button_parameters)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Map Parameters", button_map, "This shows the actual position on the map, with latitude/longitude and zoom level."), new GUILayoutOption[]
            {
            GUILayout.Width(150),
            GUILayout.Height(19)
            }))
            {
                map.button_parameters = !map.button_parameters;
            }
            if (map.button_region)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Regions", button_region, "This shows the Region window.\nA region can contain multiple areas and can be used for keeping good overview."), new GUILayoutOption[]
            {
            GUILayout.Width(120),
            GUILayout.Height(19)
            }))
            {
                map.button_region = !map.button_region;
            }
            if (map.button_heightmap_export)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Heightmap Export", button_heightmap, "This shows the Heightmap Export window.\nWith it you can export the heigthmap of an area."), new GUILayoutOption[]
            {
            GUILayout.Width(150),
            GUILayout.Height(19)
            }))
            {
                map.button_heightmap_export = !map.button_heightmap_export;
            }
            if (map.button_image_export)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Image Export", button_image, "This shows the Image Export window.\nWith it you can export the satellite images of an area."), new GUILayoutOption[]
            {
            GUILayout.Width(150),
            GUILayout.Height(19)
            }))
            {
                map.button_image_export = !map.button_image_export;
            }
            if (map.button_image_editor)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (map.preimage_edit.generate && map.preimage_edit.mode == 2)
            {
                GUI.backgroundColor = GUI.backgroundColor + new Color(1, -0.5f, -1);
            }
            if (GUILayout.Button(new GUIContent("Image Editor", button_edit, "This shows the Image Editor window.\nThis is for removing shadows from the exported satellite images.\nHow this works is explain in the manual on page 11."), GUILayout.Width(150), GUILayout.Height(19)))
            {
                map.button_image_editor = !map.button_image_editor;
                if (map.button_image_editor)
                {
                    image_generate_begin();
                }
                else
                {
                    if (map.preimage_edit.generate && map.preimage_edit.mode == 1)
                    {
                        map.preimage_edit.generate = false;
                    }
                    CopyMap0bIntoMap0();
                    map0.Apply();

                    request_map3();
                    request_map4();
                }
            }
            if (map.button_settings)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Settings", button_settings, "This shows the Settings window."), new GUILayoutOption[]
            {
            GUILayout.Width(120),
            GUILayout.Height(19)
            }))
            {
                map.button_settings = !map.button_settings;
            }
            if (map.button_create_terrain)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Create Terrain", button_terrain, "This shows the Create Terrain window.\nWith it you can create terrains from exported area."), new GUILayoutOption[]
            {
            GUILayout.Width(150),
            GUILayout.Height(19)
            }))
            {
                map.button_create_terrain = !map.button_create_terrain;
            }
            if (map.button_converter)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Converter", button_converter, "This shows the Converter window.\nWith it you can convert an ascii heightmap to a raw 16 bit heightmap, which is the format WorldComposer can use."), new GUILayoutOption[]
            {
            GUILayout.Width(150),
            GUILayout.Height(19)
            }))
            {
                map.button_converter = !map.button_converter;
            }
            if (map.button_help)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(new GUIContent("Help", button_help, "This shows how to navigate the map."), new GUILayoutOption[]
            {
            GUILayout.Width(120),
            GUILayout.Height(19)
            }))
            {
                map.button_help = !map.button_help;
            }
            if (map.button_update)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            //if (GUILayout.Button(new GUIContent("Update", button_update, "This shows the Update window. \nHere you can download and import the latest version of WorldComposer."), new GUILayoutOption[]
            //{
            //GUILayout.Width(120),
            //GUILayout.Height(19)
            //}))
            //{
            //    map.button_update = !map.button_update;
            //}
            map.button_update = false;

            if (!map.discordClicked) GUI.backgroundColor = GetAnimColor();

            if (GUILayout.Button(new GUIContent("Quick Discord Support", button_discord, "With instant chatting I'm able to give very fast support.\nWhen I'm online I try to answer right away."), GUILayout.Width(150), GUILayout.Height(19)))
            {
                if (Event.current.shift) map.discordClicked = false;
                else
                {
                    map.discordClicked = true;
                    Application.OpenURL("https://discord.gg/HhepjD9");
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            EditorGUILayout.Toggle(global_script.map_load, new GUILayoutOption[]
            {
            GUILayout.Width(25)
            });
            EditorGUILayout.Toggle(global_script.map_load2, new GUILayoutOption[]
            {
            GUILayout.Width(25)
            });
            EditorGUILayout.Toggle(global_script.map_load3, new GUILayoutOption[]
            {
            GUILayout.Width(25)
            });
            EditorGUILayout.Toggle(global_script.map_load4, new GUILayoutOption[]
            {
            GUILayout.Width(25)
            });
            EditorGUILayout.EndHorizontal();
            gui_y += 43;
            wc_gui.y = 64;
            wc_gui.x = 0;
            wc_gui.column[0] = 3;
            wc_gui.column[1] = guiWidth1 + 3;
            if (map.button_image_editor)
            {
                image_editor_rect = new Rect(0, gui_y, guiWidth2 + 348, 109 + (map.preimage_edit.edit_color.Count * 19 + Convert.ToInt32(current_area.preimage_save_new) * 36 + gui_height));
                drawGUIBox(image_editor_rect, "Combined Raw Image Editor", map.backgroundColor, map.titleColor, map.color);
                EditorGUI.LabelField(wc_gui.getRect(0, guiWidth1, 18, false, true), "Color Rules", EditorStyles.boldLabel);
                wc_gui.y = wc_gui.y + 2;
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(0, 4, 25, 15, true, false), new GUIContent("+", "Add a rule."), EditorStyles.miniButtonMid))
                {
                    map.preimage_edit.edit_color.Add(new image_edit_class());
                    if (key.shift)
                    {
                        map.preimage_edit.copy_color(map.preimage_edit.edit_color.Count - 1, map.preimage_edit.edit_color.Count - 2);
                    }
                    image_generate_begin();
                }
                if (GUI.Button(wc_gui.getRect(0, 25, 15, true, false), new GUIContent("-", "Remove this rule."), EditorStyles.miniButtonMid) && map.preimage_edit.edit_color.Count > 1)
                {
                    if (key.control)
                    {
                        Undo.RecordObject(global_script, "Erase Color Range");
                        map.preimage_edit.edit_color.RemoveAt(map.preimage_edit.edit_color.Count - 1);
                        image_generate_begin();
                        Repaint();
                        return;
                    }
                    notify_text = "Control click the '-' button to erase";
                }
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 2, 25, 19, true, false), "Act");
                GUI.color = Color.white;
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                map.preimage_edit.active = EditorGUI.Toggle(wc_gui.getRect(0, 2, 25, 19, true, true), map.preimage_edit.active);
                for (int n = 0; n < map.preimage_edit.edit_color.Count; n++)
                {
                    wc_gui.x = 4;
                    map.preimage_edit.edit_color[n].color1_start = EditorGUI.ColorField(wc_gui.getRect(0, 0, 55, 18, true, false), map.preimage_edit.edit_color[n].color1_start);
                    map.preimage_edit.edit_color[n].color1_end = EditorGUI.ColorField(wc_gui.getRect(0, 4, 55, 18, true, false), map.preimage_edit.edit_color[n].color1_end);
                    if (map.preimage_edit.edit_color[n].output != image_output_enum.content)
                    {
                        map.preimage_edit.edit_color[n].curve1 = EditorGUI.CurveField(wc_gui.getRect(0, 4, 50, 18, true, false), map.preimage_edit.edit_color[n].curve1);
                        map.preimage_edit.edit_color[n].solid_color = EditorGUI.Toggle(wc_gui.getRect(0, 6, 25, 18, true, false), map.preimage_edit.edit_color[n].solid_color);
                        EditorGUI.LabelField(wc_gui.getRect(0, -4, 20, 18, true, false), "->", EditorStyles.boldLabel);
                        map.preimage_edit.edit_color[n].color2_start = EditorGUI.ColorField(wc_gui.getRect(0, 4, 55, 18, true, false), map.preimage_edit.edit_color[n].color2_start);
                        map.preimage_edit.edit_color[n].color2_end = EditorGUI.ColorField(wc_gui.getRect(0, 4, 55, 18, true, false), map.preimage_edit.edit_color[n].color2_end);
                        map.preimage_edit.edit_color[n].curve2 = EditorGUI.CurveField(wc_gui.getRect(0, 4, 50, 18, true, false), map.preimage_edit.edit_color[n].curve2);
                        map.preimage_edit.edit_color[n].strength = EditorGUI.FloatField(wc_gui.getRect(0, 4, 50, 18, true, false), map.preimage_edit.edit_color[n].strength);
                        map.preimage_edit.edit_color[n].output = (image_output_enum)EditorGUI.EnumPopup(wc_gui.getRect(0, 4, 75, 18, true, false), map.preimage_edit.edit_color[n].output);
                        map.preimage_edit.edit_color[n].active = EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 18, true, false), map.preimage_edit.edit_color[n].active);
                    }
                    else
                    {
                        map.preimage_edit.edit_color[n].solid_color = EditorGUI.Toggle(wc_gui.getRect(0, 6, 25, 18, true, false), map.preimage_edit.edit_color[n].solid_color);
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, -4, 40, 18, true, false), "Edge", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        map.preimage_edit.edit_color[n].color2_start = EditorGUI.ColorField(wc_gui.getRect(0, 4, 55, 18, true, false), map.preimage_edit.edit_color[n].color2_start);
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 2, 50, 18, true, false), "Radius", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        gui_changed_old = GUI.changed;
                        GUI.changed = false;
                        map.preimage_edit.radiusSelect = EditorGUI.IntField(wc_gui.getRect(0, 4, 48, 18, true, false), map.preimage_edit.radiusSelect);
                        if (GUI.changed)
                        {
                            if (map.preimage_edit.radiusSelect < 50)
                            {
                                map.preimage_edit.radiusSelect = 50;
                            }
                            else if (map.preimage_edit.radiusSelect > 2000)
                            {
                                map.preimage_edit.radiusSelect = 2000;
                            }
                        }
                        GUI.changed = gui_changed_old;
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 2, 55, 18, true, false), "Repeat", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        gui_changed_old = GUI.changed;
                        GUI.changed = false;
                        map.preimage_edit.repeatAmount = EditorGUI.IntField(wc_gui.getRect(0, 4, 35, 18, true, false), map.preimage_edit.repeatAmount);
                        if (GUI.changed)
                        {
                            if (map.preimage_edit.repeatAmount < 2)
                            {
                                map.preimage_edit.repeatAmount = 2;
                            }
                            else if (map.preimage_edit.repeatAmount > 20)
                            {
                                map.preimage_edit.repeatAmount = 20;
                            }
                        }
                        GUI.changed = gui_changed_old;
                        map.preimage_edit.edit_color[n].output = (image_output_enum)EditorGUI.EnumPopup(wc_gui.getRect(0, 4, 75, 18, true, false), map.preimage_edit.edit_color[n].output);
                        map.preimage_edit.edit_color[n].active = EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 18, true, false), map.preimage_edit.edit_color[n].active);
                    }
                    if (map.preimage_edit.edit_color.Count > 1)
                    {
                        if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, false), new GUIContent("<", "Swap this rule with previous rule."), EditorStyles.miniButtonMid) && n > 0)
                        {
                            map.preimage_edit.swap_color(n - 1, n);
                            image_generate_begin();
                            Repaint();
                        }
                        if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, false), new GUIContent(">", "Swap this rule with next rule."), EditorStyles.miniButtonMid) && n < map.preimage_edit.edit_color.Count - 1)
                        {
                            map.preimage_edit.swap_color(n, n + 1);
                            image_generate_begin();
                            Repaint();
                        }
                    }
                    if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, false), new GUIContent("+", "Insert a rule."), EditorStyles.miniButtonMid))
                    {
                        map.preimage_edit.edit_color.Insert(n + 1, new image_edit_class());
                        if (key.shift)
                        {
                            map.preimage_edit.copy_color(n + 1, n);
                        }
                        image_generate_begin();
                    }
                    if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, true), new GUIContent("-", "Remove this rule."), EditorStyles.miniButtonMid) && map.preimage_edit.edit_color.Count > 1)
                    {
                        if (key.control)
                        {
                            Undo.RecordObject(global_script, "Erase Color Range");
                            map.preimage_edit.edit_color.RemoveAt(n);
                            image_generate_begin();
                            Repaint();
                            return;
                        }
                        notify_text = "Control click the '-' button to erase";
                    }
                    wc_gui.y = wc_gui.y + 3;
                }
                if (GUI.changed)
                {
                    image_generate_begin();
                }
                GUI.changed = gui_changed_old;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 23;
                current_area.preimage_save_new = false;
                if (!map.preimage_edit.generate || map.preimage_edit.mode != 2)
                {
                    if (GUI.Button(wc_gui.getRect(0, 70, 18, true, false), new GUIContent("Apply", "Apply the rules to the combined raw image.")))
                    {
                        save_global_settings();
                        Application.runInBackground = true;
                        convert_area = current_area;
                        if (convert_textures_begin(convert_area))
                        {
                            convert_area.preimage_count = 0;
                        }
                        return;
                    }
                }
                else if (GUI.Button(wc_gui.getRect(0, 0, 70, 18, true, false), new GUIContent("Stop", "Stop the execution of the image processing.")))
                {
                    Application.runInBackground = false;
                    if (map.preimage_edit.inputBuffer != null) { map.preimage_edit.inputBuffer.file.Close(); }
                    if (map.preimage_edit.outputBuffer != null) { map.preimage_edit.outputBuffer.file.Close(); }
                    map.preimage_edit.loop = false;
                    map.preimage_edit.generate = false;
                }
                if (!map.preimage_edit.loop_active)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUI.Button(wc_gui.getRect(0, 4, 55, 16, true, false), new GUIContent("Pause", "Pause the image processing."), EditorStyles.miniButtonMid))
                {
                    map.preimage_edit.loop_active = !map.preimage_edit.loop_active;
                }
                if ((!map.preimage_edit.generate || map.preimage_edit.mode != 2) && GUI.Button(wc_gui.getRect(0, 4, 55, 16, true, false), new GUIContent("Refresh", "Refresh the preview of after image processing."), EditorStyles.miniButtonMid))
                {
                    map.preimage_edit.generate = false;
                    image_generate_begin();
                }
                GUI.backgroundColor = Color.white;
                if (map.preimage_edit.generate && map.preimage_edit.mode == 2)
                {
                    map.preimage_edit.progress = map.preimage_edit.repeat * 1f / map.preimage_edit.repeatAmount + (map.preimage_edit.tile.y * map.preimage_edit.inputBuffer.tiles.x + map.preimage_edit.tile.x) * 1f / (map.preimage_edit.inputBuffer.tiles.x * map.preimage_edit.inputBuffer.tiles.y * 1f) / (map.preimage_edit.repeatAmount * 1f);
                    EditorGUI.ProgressBar(wc_gui.getRect(0, 4, 521, 19, false, false), map.preimage_edit.progress, (map.preimage_edit.progress * 100).ToString("F0") + "%");
                }
                else
                {
                    GUI.color = map.color;
                }
                if (map.preimage_edit.time < 0)
                {
                    map.preimage_edit.time = 0;
                }
                EditorGUI.LabelField(wc_gui.getRect(0, 6, 70, 19, false, false), sec_to_timeMin(map.preimage_edit.time, true));
                wc_gui.y = wc_gui.y + 19;
            }
            else if (map.preimage_edit.generate && map.preimage_edit.mode == 2)
            {
                map.preimage_edit.progress = map.preimage_edit.repeat * 1f / map.preimage_edit.repeatAmount + (map.preimage_edit.tile.y * map.preimage_edit.inputBuffer.tiles.x + map.preimage_edit.tile.x) * 1f / (map.preimage_edit.inputBuffer.tiles.x * map.preimage_edit.inputBuffer.tiles.y * 1f) / (map.preimage_edit.repeatAmount * 1f);
                EditorGUI.ProgressBar(new Rect(guiWidth2 + 15, 23, 490, 19), map.preimage_edit.progress, (map.preimage_edit.progress * 100).ToString("F0") + "%");
                EditorGUI.LabelField(new Rect(guiWidth2 + 17, 23, 100, 19), sec_to_timeMin(map.preimage_edit.time, true));
            }
            GUILayout.BeginArea(new Rect(0, gui_y, Screen.width, Screen.height - gui_y));
            scrollPos = GUI.BeginScrollView(new Rect(0, 0, guiWidth2 + 15, Screen.height - gui_y - 23), scrollPos, new Rect(0, 0, guiWidth2, guiAreaHeight - 1));
            gui_y = 0;
            if (map.button_update)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 20;
                update_rect = new Rect(0, gui_y, guiWidth2, 86 + gui_height);
                drawGUIBox(update_rect, "Update", map.backgroundColor, map.titleColor, map.color);
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Updates", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                int num8 = read_check();
                GUI.color = Color.white;
                wc_gui.y = wc_gui.y + 2;
                num8 = EditorGUI.Popup(wc_gui.getRect(1, 0, 206, 19, false, true), num8, global_script.settings.update);
                if (GUI.changed)
                {
                    write_check(num8.ToString());
                }
                GUI.changed = gui_changed_old;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Current Version", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 80, 19, false, false), "Final " + read_version().ToString("F3"));
                GUI.color = Color.white;
                if (info_window)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUI.Button(wc_gui.getRect(1, 126, 80, 18, false, true), new GUIContent("Info", "Shows the release notes.")))
                {
                    create_info_window();
                }
                GUI.backgroundColor = Color.white;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 1;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Available Version", EditorStyles.boldLabel);
                if (global_script.settings.new_version == 0 || global_script.settings.wc_loading == 1)
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 80, 19, false, false), "---");
                }
                else
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 80, 19, false, false), "Final " + global_script.settings.new_version.ToString("F3"));
                }
                if (global_script.settings.wc_loading == 1)
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 120, 70, 16, false, false), "Checking...");
                }
                if (global_script.settings.wc_loading == 2)
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 120, 70, 16, false, true), "Downloading...");
                }
                GUI.color = Color.white;
                if ((global_script.settings.wc_loading == 0 || global_script.settings.wc_loading == 3) && !global_script.settings.update_version && !global_script.settings.update_version2 && GUI.Button(wc_gui.getRect(1, 126, 80, 18, false, true), new GUIContent("Check Now", "Checks for the latest WorldCompose version.")))
                {
                    check_content_version();
                }
                if (global_script.settings.update_version && global_script.settings.wc_loading == 0)
                {
                    if (GUI.Button(wc_gui.getRect(1, 126, 80, 18, false, true), new GUIContent("Download", "Download the latest WorldComposer version.")))
                    {
                        content_version();
                        global_script.settings.update_version = false;
                    }
                }
                else if (global_script.settings.update_version2 && GUI.Button(wc_gui.getRect(1, 126, 80, 19, false, true), new GUIContent("Import", "Import the latest WorldComposer version.")))
                {
                    import_contents(Application.dataPath + install_path.Substring(0, install_path.Length - 6) + "/Update/WorldComposer/WorldComposer.unitypackage", true);
                }
            }
            if (map.button_parameters)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 23;
                if (map.key_edit)
                {
                    gui_height += 38 + map.bingKey.Count * 19;
                }
                map_parameters_rect = new Rect(0, gui_y, guiWidth2, 137 + gui_height);
                drawGUIBox(map_parameters_rect, "Map Parameters", map.backgroundColor, map.titleColor, map.color);

                if (map.bingKey_selected >= map.bingKey.Count) map.bingKey_selected = map.bingKey.Count - 1;

                if (map.bingKey[map.bingKey_selected].pulls > 45000)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = map.color;
                }
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Transactions", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 150, 19, true, false), "Key" + map.bingKey_selected + " = " + map.bingKey[map.bingKey_selected].pulls.ToString() + " (" + calc_24_hours() + ")");
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 4, 42, 18, false, true), new GUIContent("Reset", "Reset the transaction counter for the Bing key.\nYou can use 50K transaction for 1 Bing key within 24 hours.\nIf you exceed this amount your Bing key can be blocked by Microsoft.\nYou can use multiple Bing keys if you used up one for the day."), EditorStyles.miniButtonMid))
                {
                    map.bingKey[map.bingKey_selected].reset();
                }
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Map Type", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                map.type = (map_type_enum)EditorGUI.EnumPopup(wc_gui.getRect(1, 0, 150, 19, true, false), map.type);
                if (GUI.changed)
                {
                    request_map();
                }
                GUI.changed = gui_changed_old;
                if (GUI.Button(wc_gui.getRect(1, 4, 25, 16, true, false), new GUIContent("K", "This is the Bing key editor more, which allows you to view and add more Bing keys."), EditorStyles.miniButtonMid))
                {
                    map.key_edit = !map.key_edit;
                }
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (!map.active)
                {
                    GUI.color = Color.red;
                }
                map.active = EditorGUI.Toggle(wc_gui.getRect(1, 4, 25, 19, false, true), map.active);
                if (GUI.changed && map.active)
                {
                    request_map();
                }
                GUI.changed = gui_changed_old;
                if (map.key_edit)
                {
                    wc_gui.x = 0;
                    if (GUI.Button(wc_gui.getRect(0, 4, 25, 16, true, false), new GUIContent("+", "Add a Bing key to the end."), EditorStyles.miniButtonMid))
                    {
                        map.bingKey.Add(new map_key_class());
                        map.bingKey[map.bingKey.Count - 1].key = "Enter your Key here";
                        map.bingKey[map.bingKey.Count - 1].reset();
                    }
                    if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, false, true), new GUIContent("-", "Remove the Bing key from the end."), EditorStyles.miniButtonMid) && map.bingKey.Count > 1)
                    {
                        if (key.control)
                        {
                            Undo.RecordObject(global_script, "Erase Bing Key");
                            map.bingKey.RemoveAt(map.bingKey.Count - 1);
                            Repaint();
                            return;
                        }
                        notify_text = "Control click the '-' button to erase";
                    }
                    wc_gui.y = wc_gui.y + 3;
                    for (int m = 0; m < map.bingKey.Count; m++)
                    {
                        wc_gui.x = 0;
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Bing Key " + m.ToString() + " ->", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        map.bingKey[m].key = EditorGUI.TextField(wc_gui.getRect(1, 0, 170, 18, true, false), map.bingKey[m].key);
                        wc_gui.y = wc_gui.y + 1;
                        if (GUI.Button(wc_gui.getRect(1, 4, 25, 16, false, true), new GUIContent("-", "Remove this Bing key."), EditorStyles.miniButtonMid) && map.bingKey.Count > 1)
                        {
                            if (key.control)
                            {
                                Undo.RecordObject(global_script, "Erase Bing Key");
                                map.bingKey.RemoveAt(m);
                                Repaint();
                                return;
                            }
                            notify_text = "Control click the '-' button to erase";
                        }
                        wc_gui.y = wc_gui.y + 3;
                    }
                    wc_gui.x = 0;
                    GUI.color = map.color;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Key Selected", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    map.bingKey_selected = EditorGUI.IntField(wc_gui.getRect(1, 0, 50, 18, false, true), map.bingKey_selected);
                    wc_gui.y = wc_gui.y + 1;
                    if (map.bingKey_selected > map.bingKey.Count - 1)
                    {
                        map.bingKey_selected = map.bingKey.Count - 1;
                    }
                    if (map.bingKey_selected < 0)
                    {
                        map.bingKey_selected = 0;
                    }
                }
                GUI.color = map.color;
                wc_gui.x = 0;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Latitude ", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (map.manual_edit)
                {
                    GUI.color = Color.white;
                    latlong_center.latitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 150, 19, false, false), (float)latlong_center.latitude);
                }
                else
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 150, 19, false, false), latlong_center.latitude.ToString("F7"));
                }
                if (GUI.changed)
                {
                    global_script.map_latlong_center = latlong_center;
                }
                GUI.changed = gui_changed_old;
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(0, guiWidth1 + 154, 25, 16, false, true), new GUIContent("E", "Type in a manual latitude/longitude to go to that location on the map."), EditorStyles.miniButtonMid))
                {
                    map.manual_edit = !map.manual_edit;
                }
                wc_gui.y = wc_gui.y + 3;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Longitude ", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (map.manual_edit)
                {
                    GUI.color = Color.white;
                    latlong_center.longitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 150, 19, false, true), (float)latlong_center.longitude);
                }
                else
                {
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 150, 19, false, true), latlong_center.longitude.ToString("F7"));
                }
                if (GUI.changed)
                {
                    global_script.map_latlong_center = latlong_center;
                }
                GUI.changed = gui_changed_old;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Zoom ", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 400, 19, false, true), zoom.ToString("F2"));
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Mouse ", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 400, 19, false, true), latlong_mouse.latitude.ToString("F5") + ", " + latlong_mouse.longitude.ToString("F5"));
            }
            if (map.button_region)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 23;
                if (map.region_popup_edit)
                {
                    gui_height += 19;
                }
                regions_rect = new Rect(0, gui_y, guiWidth2, 63 + gui_height);
                drawGUIBox(regions_rect, "Regions", map.backgroundColor, map.titleColor, map.color);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                map.region_select = EditorGUI.Popup(wc_gui.getRect(0, 0, guiWidth1 + 126, 19, true, false), map.region_select, map.region_popup);
                if (GUI.changed)
                {
                    GUI.FocusControl("GoButton");
                }
                GUI.changed = gui_changed_old;
                if (GUI.Button(wc_gui.getRect(0, 4, 25, 16, true, false), new GUIContent("E", "Rename this region name."), EditorStyles.miniButtonMid))
                {
                    map.region_popup_edit = !map.region_popup_edit;
                    if (!map.region_popup_edit)
                    {
                        GUI.FocusControl("GoButton");
                    }
                }
                if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, false), new GUIContent("+", "Add a new region.\nThe location of the region will be that of the current center location on the map.\n\nShift click will duplicate the current region"), EditorStyles.miniButtonMid))
                {
                    AddRegion(Event.current.shift);
                }
                if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, false, true), new GUIContent("-", "Remove this region."), EditorStyles.miniButtonMid))
                {
                    if (key.control)
                    {
                        Undo.RecordObject(global_script, "Region Erase");
                        map.region.RemoveAt(map.region_select);

                        if (map.region.Count == 0) AddRegion(false);
                        else
                        {
                            if (map.region_select > 0)
                            {
                                map.region_select = map.region_select - 1;
                            }
                            current_region = map.region[map.region_select];
                            map.make_region_popup();
                        }
                        Repaint();
                        return;
                    }
                    notify_text = "Control click the '-' button to erase";
                }
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 3;
                if (map.disable_region_popup_edit && key.type == EventType.Layout)
                {
                    map.disable_region_popup_edit = false;
                    map.region_popup_edit = false;
                    GUI.UnfocusWindow();
                    Repaint();
                }
                if (map.region_popup_edit)
                {
                    gui_changed_old = GUI.changed;
                    GUI.changed = false;
                    GUI.color = Color.white;
                    current_region.name = EditorGUI.TextField(wc_gui.getRect(0, 2, 210, 19, false, true), current_region.name);
                    if (GUI.changed)
                    {
                        if (current_region.name == string.Empty)
                        {
                            current_region.name = "Untitled" + map.region_select.ToString();
                        }
                        map.make_region_popup();
                    }
                    GUI.changed = gui_changed_old;
                    if (key.keyCode == KeyCode.Return)
                    {
                        map.disable_region_popup_edit = true;
                        GUI.FocusControl("GoButton");
                    }
                }
                GUI.color = map.color;
                wc_gui.x = 0;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Lat/Long", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 122, 19, true, false), current_region.center.latitude.ToString("F4") + ", " + current_region.center.longitude.ToString("F4"));
                GUI.SetNextControlName("GoButton");
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 8, 30, 16, true, false), new GUIContent("Go", "Set the center location on the map to the location of this region."), EditorStyles.miniButtonMid))
                {
                    stop_download();
                    latlong_animate = current_region.center;
                    animate_time_start = Time.realtimeSinceStartup;
                    animate = true;
                }
                if (GUI.Button(wc_gui.getRect(1, 0, 45, 16, false, true), new GUIContent("<Set>", "Set the location of the region to the current center location of the map."), EditorStyles.miniButtonMid) && key.shift)
                {
                    current_region.center = global_script.map_latlong_center;
                }
                wc_gui.y = wc_gui.y + 30;
                if (map.area_popup_edit)
                {
                    gui_height = 19;
                }
                areas_rect = new Rect(0, gui_y, guiWidth2 + 1, 155 + gui_height);
                if (current_area.manual_area)
                {
                    areas_rect.height = areas_rect.height + 80;
                }
                drawGUIBox(areas_rect, "Areas", map.backgroundColor, map.titleColor, map.color);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                wc_gui.x = 0;
                current_region.area_select = EditorGUI.Popup(wc_gui.getRect(0, 0, guiWidth1 + 126, 19, true, false), current_region.area_select, current_region.area_popup);
                if (GUI.changed)
                {
                    GUI.FocusControl("GoButton2");
                }
                GUI.changed = gui_changed_old;
                if (GUI.Button(wc_gui.getRect(0, 4, 25, 16, true, false), new GUIContent("E", "Change this area name"), EditorStyles.miniButtonMid))
                {
                    map.area_popup_edit = !map.area_popup_edit;
                    if (!map.area_popup_edit)
                    {
                        GUI.FocusControl("GoButton2");
                    }
                }
                if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, true, false), new GUIContent("+", "Add a new area.\n\nShift click to duplicate the current area."), EditorStyles.miniButtonMid))
                {
                    add_area(current_region, current_region.area.Count, "Untitled", Event.current.shift);
                    current_region.make_area_popup();
                }
                if (GUI.Button(wc_gui.getRect(0, 0, 25, 16, false, true), new GUIContent("-", "Remove this area."), EditorStyles.miniButtonMid))
                {
                    if (key.control)
                    {
                        Undo.RecordObject(global_script, "Area Erase");
                        current_region.area.RemoveAt(current_region.area_select);

                        if (current_region.area.Count == 0) add_area(current_region, 0, "Untitled");
                        
                        if (current_region.area_select > 0)
                        {
                            current_region.area_select = current_region.area_select - 1;
                        }
                        current_region.make_area_popup();
                        Repaint();
                        return;
                    }
                    notify_text = "Control click the '-' button to erase";
                }
                wc_gui.y = wc_gui.y + 3;
                if (map.disable_area_popup_edit && key.type == EventType.Layout)
                {
                    map.disable_area_popup_edit = false;
                    map.area_popup_edit = false;
                    GUI.UnfocusWindow();
                    Repaint();
                }
                if (current_region.area.Count > 0)
                {
                    if (map.area_popup_edit)
                    {
                        gui_changed_old = GUI.changed;
                        GUI.changed = false;
                        GUI.color = Color.white;
                        wc_gui.x = 0;
                        current_area.name = EditorGUI.TextField(wc_gui.getRect(0, 2, 210, 19, false, true), current_area.name);
                        if (GUI.changed)
                        {
                            if (current_area.name == string.Empty)
                            {
                                current_area.name = "Untitled" + current_region.area_select.ToString();
                            }
                            current_region.make_area_popup();
                            if (!current_area.preimage_path_changed)
                            {
                                current_area.preimage_path = current_area.name;
                            }
                            if (!current_area.export_heightmap_changed)
                            {
                                current_area.export_heightmap_filename = current_area.name;
                            }
                            if (!current_area.export_image_changed)
                            {
                                current_area.export_image_filename = current_area.name;
                            }
                            if (!current_area.export_terrain_changed)
                            {
                                current_area.terrain_scene_name = "_" + current_area.name;
                                current_area.terrain_scene_name = current_area.name;
                            }
                        }
                        GUI.changed = gui_changed_old;
                        if (key.keyCode == KeyCode.Return)
                        {
                            map.disable_area_popup_edit = true;
                            GUI.FocusControl("GoButton2");
                        }
                        GUI.color = map.color;
                    }
                    wc_gui.x = 0;
                    GUI.color = map.color;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Center", EditorStyles.boldLabel);
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 122, 19, true, false), current_area.center.latitude.ToString("G6") + ", " + current_area.center.longitude.ToString("G6"));
                    GUI.SetNextControlName("GoButton");
                    GUI.color = Color.white;
                    if (GUI.Button(wc_gui.getRect(1, 8, 30, 16, true, false), new GUIContent("Go", "Set the center location on the map to the center of this area."), EditorStyles.miniButtonMid))
                    {
                        if (current_area.tiles.x != 0 && current_area.tiles.y != 0)
                        {
                            stop_download();
                            latlong_animate = current_area.center;
                            animate_time_start = Time.realtimeSinceStartup;
                            animate = true;
                        }
                        else
                        {
                            notify_text = "The area is not created. Use the 'Pick' button to create an area";
                        }
                    }
                    if (GUI.Button(wc_gui.getRect(1, 0, 45, 16, false, true), new GUIContent("<Set>", "Set the center of the area to the center location on the map."), EditorStyles.miniButtonMid))
                    {
                        if (key.shift)
                        {
                            if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                            {
                                Mathw.calc_latlong_area_from_center(current_area, latlong_center, current_area.image_zoom, new Vector2(current_area.resolution * current_area.tiles.x, current_area.resolution * current_area.tiles.y));
                            }
                            else
                            {
                                notify_text = "It is not possible to relocate the area while exporting";
                            }
                        }
                        else
                        {
                            notify_text = "Shift click <Set> to change center of area to center of current map view";
                        }
                    }
                    GUI.color = map.color;
                    wc_gui.x = 0;
                    wc_gui.y = wc_gui.y + 3;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Upper Left", EditorStyles.boldLabel);
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, guiWidth1, 19, true, false), current_area.upper_left.latitude.ToString("G6") + ", " + current_area.upper_left.longitude.ToString("G6"));
                    GUI.color = Color.white;
                    GUI.SetNextControlName("GoButton");
                    if (GUI.Button(wc_gui.getRect(1, 10, 30, 16, true, false), new GUIContent("Go", "Set the center location on the map to this area upper left location."), EditorStyles.miniButtonMid))
                    {
                        if (current_area.tiles.x != 0 && current_area.tiles.y != 0)
                        {
                            stop_download();
                            latlong_animate = current_area.upper_left;
                            animate_time_start = Time.realtimeSinceStartup;
                            animate = true;
                        }
                        else
                        {
                            notify_text = "The area is not created. Use the 'Pick' button to create an area";
                        }
                    }
                    if (map.mode == 1)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    else if (current_area.tiles.x == 0 && current_area.tiles.y == 0)
                    {
                        GUI.backgroundColor = Color.red;
                    }
                    if (GUI.Button(wc_gui.getRect(1, 0, 45, 16, false, true), new GUIContent("Pick", "Select a new location for this area.\nFirst mouse click is for the upper left location of the area.\nSecond mouse click is for the lower right location of the area."), EditorStyles.miniButtonMid))
                    {
                        if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                        {
                            if (map.mode != 1)
                            {
                                map.mode = 1;
                            }
                            else
                            {
                                map.mode = 0;
                                if (current_area.select == 1)
                                {
                                    pick_done();
                                }
                            }
                        }
                        else
                        {
                            notify_text = "It is not possible to repick the area while exporting";
                        }
                    }
                    wc_gui.x = 0;
                    wc_gui.y = wc_gui.y + 3;
                    GUI.backgroundColor = Color.white;
                    if (current_area.manual_area)
                    {
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Latitude", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.upper_left.latitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 160, 19, false, true), (float)current_area.upper_left.latitude);
                        wc_gui.x = 0;
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Longitude", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.upper_left.longitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 160, 19, false, true), (float)current_area.upper_left.longitude);
                    }
                    wc_gui.x = 0;
                    GUI.color = map.color;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Lower Right", EditorStyles.boldLabel);
                    if (current_area.lower_right.latitude > current_area.upper_left.latitude)
                    {
                        current_area.lower_right.latitude = current_area.upper_left.latitude;
                    }
                    if (current_area.lower_right.longitude < current_area.upper_left.longitude)
                    {
                        current_area.lower_right.longitude = current_area.upper_left.longitude;
                    }
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 122, 19, true, false), current_area.lower_right.latitude.ToString("G6") + ", " + current_area.lower_right.longitude.ToString("G6"));
                    GUI.SetNextControlName("GoButton");
                    GUI.color = Color.white;
                    if (GUI.Button(wc_gui.getRect(1, 8, 30, 16, true, false), new GUIContent("Go", "Set the center location of the map to the lower right location of this area."), EditorStyles.miniButtonMid))
                    {
                        if (current_area.tiles.x != 0 && current_area.tiles.y != 0)
                        {
                            stop_download();
                            latlong_animate = current_area.lower_right;
                            animate_time_start = Time.realtimeSinceStartup;
                            animate = true;
                        }
                        else
                        {
                            notify_text = "The area is not created. Use the 'Pick' button to create an area";
                        }
                    }
                    if (map.mode == 2)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUI.Button(wc_gui.getRect(1, 0, 45, 16, false, true), new GUIContent("Resize", "Resize this area."), EditorStyles.miniButtonMid))
                    {
                        if (map.mode == 1)
                        {
                            notify_text = "Fist click 1 more time in the WC map to select the lower right of your area.";
                        }
                        else if (current_area.created)
                        {
                            if (map.mode == 2)
                            {
                                map.mode = 0;
                            }
                            else
                            {
                                map.mode = 2;
                            }
                        }
                        else
                        {
                            notify_text = "You need to create an area first with 'Pick'";
                        }
                    }
                    GUI.backgroundColor = Color.white;
                    GUI.color = map.color;
                    wc_gui.y = wc_gui.y + 3;
                    if (current_area.manual_area)
                    {
                        wc_gui.x = 0;
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Latitude", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.lower_right.latitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 160, 16, false, true), (float)current_area.lower_right.latitude);
                        wc_gui.x = 0;
                        GUI.color = map.color;
                        EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Longitude", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.lower_right.longitude = EditorGUI.FloatField(wc_gui.getRect(1, 0, 160, 19, false, true), (float)current_area.lower_right.longitude);
                    }
                    GUI.color = map.color;
                    wc_gui.x = 0;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Size", EditorStyles.boldLabel);
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 152, 19, true, false), (current_area.size.x / 1000).ToString("F2") + "(km), " + (current_area.size.y / 1000).ToString("F2") + "(km)");
                    GUI.color = Color.white;
                    if (GUI.Button(wc_gui.getRect(1, 8, 45, 16, false, true), new GUIContent("Edit", "Edit the size of the area with manually entering latitue and longitude."), EditorStyles.miniButtonMid))
                    {
                        current_area.manual_area = !current_area.manual_area;
                    }
                    wc_gui.x = 0;
                    wc_gui.y = wc_gui.y + 3;
                    GUI.color = map.color;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Height Center", EditorStyles.boldLabel);
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 150, 19, false, true), current_area.center_height.ToString() + " (m)");
                    wc_gui.x = 0;
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Heightmap Data", EditorStyles.boldLabel);
                    EditorGUI.LabelField(wc_gui.getRect(1, 0, 150, 19, false, true), current_area.elevation_zoom.ToString() + " -> " + current_area.heightmap_scale.ToString("F2") + " (m/p)");
                }
            }
            if (map.button_heightmap_export)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 23;
                heightmap_export_rect = new Rect(0, gui_y, guiWidth2 + 1, 184 + gui_height);
                drawGUIBox(heightmap_export_rect, "Heightmap Export", map.backgroundColor, map.titleColor, map.color);
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Heightmap Zoom", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                current_area.heightmap_zoom = EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 19, true, false), current_area.heightmap_zoom);
                if (GUI.Button(wc_gui.getRect(1, 4, 25, 16, true, false), new GUIContent("+", "Increase the heightmap zoom level.\nThis increases the heightmap size (resolution)."), EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_heightmap_active)
                    {
                        current_area.heightmap_zoom = current_area.heightmap_zoom + 1;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change heightmap resolution while exporting";
                    }
                }
                if (GUI.Button(wc_gui.getRect(1, 0, 25, 16, true, false), new GUIContent("-", "Lower the heightmap zoom level.\nThis lower the heightmap size (resolution)."), EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_heightmap_active)
                    {
                        current_area.heightmap_zoom = current_area.heightmap_zoom - 1;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change heightmap resolution while exporting";
                    }
                }
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(1, 12, 50, 19, true, false), new GUIContent("Manual", "Override the maximum heightmap zoom level of what Bing provides.\nThis can be used if inside the area is a more detailed resultion than the center of the area, as the maximum is sampled from the center from the area."));
                GUI.color = Color.white;
                current_area.heightmap_manual = EditorGUI.Toggle(wc_gui.getRect(1, 0, 50, 19, false, true), string.Empty, current_area.heightmap_manual);
                wc_gui.y = wc_gui.y + 3;
                if (GUI.changed)
                {
                    if (current_area.heightmap_zoom < 1)
                    {
                        current_area.heightmap_zoom = 1;
                    }
                    else if (current_area.heightmap_zoom > current_area.elevation_zoom && !current_area.heightmap_manual)
                    {
                        current_area.heightmap_zoom = current_area.elevation_zoom;
                    }
                    calc_heightmap_settings(current_area);
                    if (!current_area.terrain_heightmap_resolution_changed)
                    {
                        calc_terrain_heightmap_resolution();
                    }
                }
                GUI.changed = gui_changed_old;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Heightmap Size", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 400, 19, false, true), current_area.heightmap_resolution.x.ToString() + "x" + current_area.heightmap_resolution.y.ToString() + "  (" + current_area.heightmap_resolution.x * current_area.heightmap_resolution.y / 1024 + " transactions)");
                wc_gui.x = 0;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Export Path", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, guiWidth1 + 19, 19, true, false), new GUIContent(current_area.export_heightmap_path, current_area.export_heightmap_path));
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 5, 61, 18, false, true), new GUIContent("Change", "Change the folder where the heightmap is saved to.")))
                {
                    if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        if (current_area.export_heightmap_path.Length == 0)
                        {
                            current_area.export_heightmap_path = Application.dataPath;
                        }
                        path_old = current_area.export_heightmap_path;
                        if (!key.shift)
                        {
                            current_area.export_heightmap_path = EditorUtility.OpenFolderPanel("Export Heightmap Path", current_area.export_heightmap_path, string.Empty);
                            if (current_area.export_heightmap_path.Length == 0)
                            {
                                current_area.export_heightmap_path = path_old;
                            }
                        }
                        else
                        {
                            current_area.export_heightmap_path = Application.dataPath;
                        }
                        if (path_old != current_area.export_heightmap_path)
                        {
                            if (current_area.export_heightmap_path.IndexOf(Application.dataPath) == -1)
                            {
                                notify_text = "The path should be inside your Unity project. Reselect your path.";
                                current_area.export_heightmap_path = Application.dataPath;
                            }
                            current_area.export_heightmap_changed = true;
                            if (!current_area.preimage_path_changed)
                            {
                                current_area.preimage_path = current_area.export_heightmap_path;
                            }
                            if (!current_area.export_image_changed)
                            {
                                current_area.export_image_path = current_area.export_heightmap_path;
                            }
                            if (!current_area.export_terrain_changed)
                            {
                                current_area.export_terrain_path = current_area.export_heightmap_path + "/Terrains";
                            }
                        }
                    }
                    else
                    {
                        notify_text = "It is not possible to change an export folder while exporting.";
                    }
                }
                GUI.color = map.color;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 1;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Offset", EditorStyles.boldLabel);
                wc_gui.x = wc_gui.x + (guiWidth1 - 17);
                EditorGUI.LabelField(wc_gui.getRect(0, 0, 15, 19, true, false), "X");
                GUI.color = Color.white;
                current_area.heightmap_offset_e.x = EditorGUI.IntField(wc_gui.getRect(0, 4, 45, 18, true, false), (int)current_area.heightmap_offset_e.x);
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 4, 15, 19, true, false), "Y");
                GUI.color = Color.white;
                current_area.heightmap_offset_e.y = EditorGUI.IntField(wc_gui.getRect(0, 4, 45, 18, true, false), (int)current_area.heightmap_offset_e.y);
                GUI.color = map.color;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 19;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Heightmap File", EditorStyles.boldLabel);
                GUI.color = Color.white;
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    current_area.export_heightmap_filename = EditorGUI.TextField(wc_gui.getRect(1, 2, guiWidth1 + 21, 18, true, false), current_area.export_heightmap_filename);
                }
                else
                {
                    EditorGUI.TextField(wc_gui.getRect(1, 2, guiWidth1 + 21, 18, true, false), current_area.export_heightmap_filename);
                }
                if (GUI.changed)
                {
                    current_area.export_heightmap_changed = true;
                    if (!current_area.export_image_changed)
                    {
                        current_area.export_image_filename = current_area.export_heightmap_filename;
                    }
                    if (!current_area.terrain_name_changed)
                    {
                        current_area.terrain_scene_name = "_" + current_area.export_heightmap_filename;
                        current_area.terrain_scene_name = current_area.export_heightmap_filename;
                    }
                }
                GUI.changed = false;
                current_area.export_heightmap_changed = EditorGUI.Toggle(wc_gui.getRect(1, 4, 25, 19, true, false), current_area.export_heightmap_changed);
                if (GUI.changed && !current_area.export_heightmap_changed)
                {
                    current_area.export_heightmap_path = current_area.export_image_path;
                    current_area.export_heightmap_filename = current_area.export_image_filename;
                }
                GUI.changed = gui_changed_old;
                if (map.path_display)
                {
                    if (GUI.Button(wc_gui.getRect(1, 8, 25, 16, false, true), new GUIContent("<", "Hide the full path text of where the heightmap is stored into."), EditorStyles.miniButtonMid))
                    {
                        map.path_display = false;
                    }
                }
                else if (GUI.Button(wc_gui.getRect(1, 8, 25, 16, false, true), new GUIContent(">", "Show the full path text of where the heightmap is stored into."), EditorStyles.miniButtonMid))
                {
                    map.path_display = true;
                }
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 3;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), new GUIContent("Threads", "The amount of download threads that run at the same time.\nA heightmap is downloaded in small blocks, each block represent a download thread."), EditorStyles.boldLabel);
                GUI.color = Color.white;
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    map.export_elExt = EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, false, true), map.export_elExt);
                }
                else
                {
                    EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, false, true), map.export_elExt);
                    if (GUI.changed)
                    {
                        notify_text = "It is not possible to change image zoom while exporting";
                    }
                }
                if (GUI.changed)
                {
                    if (map.export_elExt < 1)
                    {
                        map.export_elExt = 1;
                    }
                    else if (map.export_elExt > 128)
                    {
                        map.export_elExt = 128;
                    }
                }
                GUI.changed = gui_changed_old;
                string text = "Export\n Heightmap";
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 4;
                if (current_area.export_heightmap_active || current_area.export_heightmap_call)
                {
                    text = "Stop";
                }
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(0, 0, guiWidth1, 38, false, false), new GUIContent(text, "This button is for starting/stopping the exporting of the heigthmap.")))
                {
                    if (current_area.tiles.x == 0 && current_area.tiles.y == 0)
                    {
                        notify_text = "The area is not created. Use the 'Pick' button to create an area";
                        return;
                    }
                    if (!area_rounded)
                    {
                        notify_text = "The area tiles are not rounded. Please resize the area";
                        map.mode = 2;
                    }
                    else if (key.shift)
                    {
                        save_global_settings();
                        start_elevation_pull_region(current_region);
                    }
                    else if (key.control)
                    {
                        stop_all_elevation_pull();
                    }
                    else
                    {
                        save_global_settings();
                        start_elevation_pull(current_region, current_area);
                    }
                }
                if (!map.export_heightmap_continue)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUI.Button(wc_gui.getRect(1, 3, 45, 16, true, false), new GUIContent("Pause", "Will pause the export of the heightmap."), EditorStyles.miniButtonMid))
                {
                    map.export_heightmap_continue = !map.export_heightmap_continue;
                }
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                if (map.export_heightmap_active)
                {
                    map.export_heightmap.progress = (map.export_heightmap.tile.x * 1f + map.export_heightmap.tile.y * map.export_heightmap.tiles.x * 1f) / (map.export_heightmap.tiles.x * map.export_heightmap.tiles.y * 1f);
                    EditorGUI.ProgressBar(wc_gui.getRect(1, 4, 153, 19, false, false), map.export_heightmap.progress, (map.export_heightmap.progress * 100).ToString("F0") + "%");
                }
                else
                {
                    GUI.color = map.color;
                }
                if (map.export_heightmap_timeEnd - map.export_heightmap_timeStart < 0)
                {
                    map.export_heightmap_timeEnd = (map.export_heightmap_timeStart = 0);
                }
                EditorGUI.LabelField(wc_gui.getRect(1, 4, 100, 19, false, true), sec_to_timeMin(map.export_heightmap_timeEnd - map.export_heightmap_timeStart, true));
                wc_gui.x = 0;
                GUI.color = Color.white;
                if (File.Exists(current_area.export_heightmap_path + "/" + current_area.export_heightmap_filename + ".Raw") && fs == null && GUI.Button(wc_gui.getRect(1, 3, 75, 16, false, false), new GUIContent("Normalize", "This normalizes the heightmap.\nIn the heightmap, it will make the lowest height the black color and the highest height the white color.")))
                {
                    current_area.normalizedHeight = NormalizeHeightmap(current_area.heightmap_resolution, current_area.export_heightmap_path + "/" + current_area.export_heightmap_filename + ".Raw");
                }
            }
            if (current_area.image_changed)
            {
                if (!current_area.terrain_heightmap_resolution_changed)
                {
                    calc_terrain_heightmap_resolution();
                }
                current_area.image_changed = false;
            }
            if (map.button_image_export)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 23;
                if (map.export_jpg)
                {
                    gui_height += 19;
                }
                if (map.export_raw)
                {
                    gui_height += 23;
                }
                image_export_rect = new Rect(0, gui_y, guiWidth2 + 1, 243 + gui_height);
                drawGUIBox(image_export_rect, "Image Export", map.backgroundColor, map.titleColor, map.color);
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), new GUIContent("Image Zoom", "Higher the image zoom level.\nThis create more terrain tiles."), EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                current_area.image_zoom = EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, true, false), current_area.image_zoom);
                if (GUI.Button(wc_gui.getRect(1, 4, 25, 16, true, false), "+", EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        current_area.image_zoom = current_area.image_zoom + 1;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change image zoom while exporting";
                    }
                }
                if (GUI.Button(wc_gui.getRect(1, 0, 25, 16, true, true), new GUIContent("-", "Lower the image zoom level.\nThis lowers the amount of terrain tiles."), EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        current_area.image_zoom = current_area.image_zoom - 1;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change image zoom while exporting";
                    }
                }
                if (GUI.changed)
                {
                    if (current_area.image_zoom < 1)
                    {
                        current_area.image_zoom = 1;
                    }
                    else if (current_area.image_zoom > 19)
                    {
                        current_area.image_zoom = 19;
                    }
                    current_area.image_changed = true;
                    check_area_resize();
                }
                GUI.changed = gui_changed_old;
                wc_gui.y = wc_gui.y + 3;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Resolution", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 60, 19, true, false), current_area.resolution.ToString());
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 6, 25, 16, true, false), new GUIContent("+", "This highers the image resolution."), EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        current_area.resolution = current_area.resolution * 2;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change image resolution while exporting";
                    }
                }
                if (GUI.Button(wc_gui.getRect(1, 0, 25, 16, false, true), new GUIContent("-", "This lowers the image resolution."), EditorStyles.miniButtonMid))
                {
                    if (!current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        current_area.resolution = current_area.resolution / 2;
                        GUI.changed = true;
                    }
                    else
                    {
                        notify_text = "It is not possible to change image resolution while exporting";
                    }
                }
                if (GUI.changed)
                {
                    if (current_area.resolution < 512)
                    {
                        current_area.resolution = 512;
                    }
                    check_area_resize();
                }
                GUI.changed = gui_changed_old;
                if (current_area.resolution > 8192 && (map.export_jpg || map.export_png))
                {
                    current_area.resolution = 8192;
                }
                if (!current_area.maxTextureSize_changed)
                {
                    if (current_area.resolution > 4096)
                    {
                        current_area.maxTextureSize = 4096;
                        current_area.maxTextureSize_select = 7;
                    }
                    else
                    {
                        current_area.maxTextureSize = current_area.resolution;
                        current_area.maxTextureSize_select = (int)(Mathf.Log(current_area.maxTextureSize, 2) - 5);
                    }
                }
                wc_gui.y = wc_gui.y + 3;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Tiles", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.tiles.x = EditorGUI.IntField(wc_gui.getRect(1, 2, 45, 18, true, false), current_area.tiles.x);
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(1, 4, 15, 19, true, false), "*", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.tiles.y = EditorGUI.IntField(wc_gui.getRect(1, 4, 45, 18, true, false), current_area.tiles.y);
                if (GUI.Button(wc_gui.getRect(1, 5, 22, 16, false, false), new GUIContent("R", "Reset the start export position of the image tiles."), EditorStyles.miniButtonMid))
                {
                    current_area.start_tile.reset();
                }
                if (current_area.start_tile_enabled)
                {
                    GUI.color = Color.green;
                }
                if (GUI.Button(wc_gui.getRect(1, 32, 58, 16, false, true), new GUIContent("Start", "Choose a start export position of the image tiles.\nThis can be used if the export was interupted or for a single tile download that can be enabled with the 'One' toggle."), EditorStyles.miniButtonMid))
                {
                    current_area.start_tile_enabled = !current_area.start_tile_enabled;
                }
                wc_gui.y = wc_gui.y + 3;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Offset", EditorStyles.boldLabel);
                wc_gui.x = wc_gui.x + (guiWidth1 - 17);
                EditorGUI.LabelField(wc_gui.getRect(0, 0, 15, 19, true, false), "X");
                GUI.color = Color.white;
                current_area.image_offset_e.x = EditorGUI.IntField(wc_gui.getRect(0, 4, 45, 18, true, false), (int)current_area.image_offset_e.x);
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 4, 15, 19, true, false), "Y");
                GUI.color = Color.white;
                current_area.image_offset_e.y = EditorGUI.IntField(wc_gui.getRect(0, 4, 45, 18, true, false), (int)current_area.image_offset_e.y);
                if (current_area.image_stop_one)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = map.color;
                }
                EditorGUI.LabelField(wc_gui.getRect(0, 29, 30, 19, true, false), "One");
                if (current_area.image_stop_one)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = Color.white;
                }
                current_area.image_stop_one = EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 19, true, false), current_area.image_stop_one);
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 19;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Image Size", EditorStyles.boldLabel);
                Vector2 vector3 = default(Vector2);
                vector3.x = current_area.resolution * current_area.tiles.x;
                vector3.y = current_area.resolution * current_area.tiles.y;
                if (map.warnings && (vector3.x > 16384 || vector3.y > 16384) && (map.export_jpg || map.export_png) && notify_text.IndexOf("The total") == -1)
                {
                    if (notify_text.Length != 0)
                    {
                        notify_text += "\n\n";
                    }
                    notify_text += "The total image size is bigger then 16k, please keep the performance in mind and texture memory. You can still go to at least 64k total image resolution in Unity 5." + "\nMake your image resolution lower in 'Image Export' -> 'Image Zoom' -> Click the '-' button.\n\nPlease read page 7 in the WC manual, after reading and understanding you can disable the warnings in the 'Settings' tab -> Show Warnings";
                }
                area_size_old.x = vector3.x;
                area_size_old.y = vector3.y;
                if ((vector3.x >= 16384 && vector3.x <= 32768) || (vector3.y >= 16384 && vector3.y <= 32768))
                {
                    if (map.export_jpg || map.export_png)
                    {
                        GUI.color = new Color(1, 0.5f, 0, 1);
                    }
                }
                else if ((vector3.x > 32768 || vector3.y > 32768) && (map.export_jpg || map.export_png))
                {
                    GUI.color = Color.red;
                }
                EditorGUI.LabelField(wc_gui.getRect(1, 0, 300, 19, false, true), vector3.x.ToString() + "x" + vector3.y.ToString() + "  (" + vector3.x * vector3.y / 262144 + " transactions)");
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Export Path", EditorStyles.boldLabel);
                EditorGUI.LabelField(wc_gui.getRect(1, 0, guiWidth1 + 19, 19, true, false), new GUIContent(current_area.export_image_path, current_area.export_image_path));
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 4, 62, 18, false, true), new GUIContent("Change", "Change the folder where the images are saved to.")))
                {
                    if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                    {
                        if (current_area.export_image_path.Length == 0)
                        {
                            current_area.export_image_path = Application.dataPath;
                        }
                        path_old = current_area.export_image_path;
                        if (key.shift)
                        {
                            current_area.export_image_path = Application.dataPath;
                        }
                        else if (key.alt)
                        {
                            current_area.export_image_path = current_area.export_heightmap_path;
                        }
                        else
                        {
                            current_area.export_image_path = EditorUtility.OpenFolderPanel("Export image Path", current_area.export_image_path, string.Empty);
                            if (current_area.export_image_path.Length == 0)
                            {
                                current_area.export_image_path = path_old;
                            }
                        }
                        if (path_old != current_area.export_image_path)
                        {
                            if (current_area.export_image_path.IndexOf(Application.dataPath) == -1)
                            {
                                notify_text = "The path should be inside your Unity project. Reselect your path.";
                                current_area.export_image_path = Application.dataPath;
                            }
                            current_area.export_image_changed = true;
                            if (!current_area.preimage_path_changed)
                            {
                                current_area.preimage_path = current_area.export_image_path;
                            }
                            if (!current_area.export_heightmap_changed)
                            {
                                current_area.export_heightmap_path = current_area.export_image_path;
                            }
                            if (!current_area.export_terrain_changed)
                            {
                                current_area.export_terrain_path = current_area.export_image_path + "/Terrains";
                            }
                        }
                    }
                    else
                    {
                        notify_text = "It is not possible to change an export folder while exporting.";
                    }
                }
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 1;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Image File", EditorStyles.boldLabel);
                GUI.color = Color.white;
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    current_area.export_image_filename = EditorGUI.TextField(wc_gui.getRect(1, 2, guiWidth1 + 21, 18, true, false), current_area.export_image_filename);
                }
                else
                {
                    EditorGUI.TextField(wc_gui.getRect(1, 2, guiWidth1 + 21, 18, true, false), current_area.export_image_filename);
                }
                if (GUI.changed)
                {
                    current_area.export_image_changed = true;
                    if (!current_area.export_heightmap_changed)
                    {
                        current_area.export_heightmap_filename = current_area.export_image_filename;
                    }
                    if (!current_area.export_terrain_changed)
                    {
                        current_area.terrain_scene_name = "_" + current_area.export_image_filename;
                        current_area.terrain_scene_name = current_area.export_image_filename;
                    }
                }
                GUI.changed = false;
                current_area.export_image_changed = EditorGUI.Toggle(wc_gui.getRect(1, 4, 25, 19, true, false), current_area.export_image_changed);
                if (GUI.changed && !current_area.export_image_changed)
                {
                    current_area.export_image_path = current_area.export_heightmap_path;
                    current_area.export_image_filename = current_area.export_heightmap_filename;
                }
                GUI.changed = gui_changed_old;
                if (map.path_display)
                {
                    if (GUI.Button(wc_gui.getRect(1, 8, 25, 16, false, true), new GUIContent("<", "Hide the full path text of where the images are stored into."), EditorStyles.miniButtonMid))
                    {
                        map.path_display = false;
                    }
                }
                else if (GUI.Button(wc_gui.getRect(1, 8, 25, 16, false, true), new GUIContent(">", "Show the full path text of where the images are stored into."), EditorStyles.miniButtonMid))
                {
                    map.path_display = true;
                }
                GUI.color = map.color;
                wc_gui.y = wc_gui.y + 3;
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Export World File", EditorStyles.boldLabel);
                GUI.color = Color.white;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    current_area.export_image_world_file = EditorGUI.Toggle(wc_gui.getRect(1, 2, 25, 19, false, true), current_area.export_image_world_file);
                }
                else
                {
                    EditorGUI.Toggle(wc_gui.getRect(1, 2, 25, 19, false, true), current_area.export_image_world_file);
                }
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Threads", EditorStyles.boldLabel);
                GUI.color = Color.white;
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    map.export_texExt = EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, false, true), map.export_texExt);
                }
                else
                {
                    EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, false, true), map.export_texExt);
                    if (GUI.changed)
                    {
                        notify_text = "It is not possible to change this while exporting";
                    }
                }
                if (GUI.changed)
                {
                    if (map.export_texExt < 1)
                    {
                        map.export_texExt = 1;
                    }
                    else if (map.export_texExt > 16)
                    {
                        map.export_texExt = 16;
                    }
                }
                GUI.changed = gui_changed_old;
                current_area.export_image_import_settings = false;
                wc_gui.y = wc_gui.y + 4;
                wc_gui.x = 0;
                string text = "Export\n Images";
                if (current_area.export_image_active || current_area.export_image_call)
                {
                    text = "Stop";
                }
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(0, 0, guiWidth1, 38, true, false), new GUIContent(text, "This button is for starting/stopping the exporting of the images.")))
                {
                    if (map.export_jpg && map.export_raw)
                    {
                        notify_text = "The Jpg and Raw are selected. This mode can only be used to slice the raw combined image into Jpg images. Read page 10 in the WorldComposer manual about this.";
                    }
                    else
                    {
                        if (current_area.tiles.x == 0 && current_area.tiles.y == 0)
                        {
                            notify_text = "The area is not created. Use the 'Pick' button to create an area";
                            return;
                        }
                        if (!check_area_resize())
                        {
                            if (key.shift)
                            {
                                save_global_settings();
                                start_image_pull_region(current_region);
                            }
                            else if (key.control)
                            {
                                stop_image_pull_region(current_region);
                            }
                            else
                            {
                                save_global_settings();
                                start_image_pull(current_region, current_area);
                            }
                        }
                    }
                }
                EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
                if (!map.export_image_continue)
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUI.Button(wc_gui.getRect(0, 4, 45, 16, false, false), new GUIContent("Pause", "This pauses the image export."), EditorStyles.miniButtonMid))
                {
                    map.export_image_continue = !map.export_image_continue;
                }
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                if (map.export_image_active)
                {
                    map.export_image.progress = (map.export_image.tile.x * 1f + map.export_image.tile.y * map.export_image.tiles.x * 1f) / (map.export_image.tiles.x * map.export_image.tiles.y * 1f);
                    EditorGUI.ProgressBar(wc_gui.getRect(0, 53, 153, 19, false, false), map.export_image.progress, (map.export_image.progress * 100).ToString("F0") + "%");
                }
                else
                {
                    GUI.color = map.color;
                }
                if (map.export_image_timeEnd - map.export_image_timeStart < 0)
                {
                    map.export_image_timeEnd = (map.export_image_timeStart = 0);
                }
                EditorGUI.LabelField(wc_gui.getRect(0, 53, 100, 19, false, true), sec_to_timeMin(map.export_image_timeEnd - map.export_image_timeStart, true));
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 4, 30, 19, true, false), "Jpg");
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    map.export_jpg = EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 19, true, false), map.export_jpg);
                }
                else
                {
                    EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 19, true, false), map.export_jpg);
                }
                if (GUI.changed && !map.export_jpg && !map.export_raw && !map.export_png)
                {
                    map.export_png = true;
                }
                GUI.changed = gui_changed_old;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 10, 30, 19, true, false), "Png");
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    map.export_png = EditorGUI.Toggle(wc_gui.getRect(0, 4, 24, 19, true, false), map.export_png);
                }
                else
                {
                    EditorGUI.Toggle(wc_gui.getRect(0, 4, 24, 19, true, false), map.export_png);
                }
                if (GUI.changed)
                {
                    if (!map.export_png && !map.export_raw && !map.export_jpg)
                    {
                        map.export_jpg = true;
                    }
                    if (map.export_png && map.export_raw)
                    {
                        map.export_raw = false;
                    }
                }
                GUI.changed = gui_changed_old;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 10, 30, 19, true, false), "Raw");
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                if (!current_area.export_heightmap_active && !current_area.export_image_active && !combine_generate && !slice_generate)
                {
                    map.export_raw = EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 19, true, true), map.export_raw);
                }
                else
                {
                    EditorGUI.Toggle(wc_gui.getRect(0, 4, 25, 19, true, true), map.export_raw);
                }
                if (GUI.changed)
                {
                    if (!map.export_raw)
                    {
                        if (!map.export_jpg)
                        {
                            map.export_jpg = true;
                        }
                    }
                    else
                    {
                        map.export_png = false;
                    }
                }
                GUI.changed = gui_changed_old;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 3;
                GUI.color = map.color;
                if (map.export_jpg)
                {
                    EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Jpeg Quality", EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    map.export_jpg_quality = (int)EditorGUI.Slider(wc_gui.getRect(1, 4, guiWidth1 + 80, 19, false, true), map.export_jpg_quality, 0, 100);
                }
                wc_gui.y = wc_gui.y + 3;
                if (map.export_raw)
                {
                    GUI.color = Color.white;
                    if (!combine_generate && !slice_generate)
                    {
                        if (map.export_jpg)
                        {
                            if (GUI.Button(wc_gui.getRect(0, 0, 120, 19, false, true), new GUIContent("Slice Images", "Slice the combined raw file back into single JPG images.")))
                            {
                                slice_textures_begin(current_area, current_area.export_image_path, current_area.export_image_filename);
                            }
                        }
                        else if (GUI.Button(wc_gui.getRect(0, 0, 120, 19, false, true), new GUIContent("Combine Images", "Combine the exported image tiles into 1 big raw image file.\nThis file can be used by the 'Image Editor' or you can edit it in photoshop.")))
                        {
                            combine_textures_begin(current_area, current_area.export_image_path, current_area.export_image_filename);
                        }
                    }
                    else
                    {
                        if (GUI.Button(wc_gui.getRect(0, 0, 120, 19, true, false), "Stop"))
                        {
                            combine_generate = false;
                            slice_generate = false;
                            if (combine_export_file != null)
                            {
                                combine_import_file.Close();
                            }
                            if (combine_export_file != null)
                            {
                                combine_export_file.Close();
                            }
                            Application.runInBackground = false;
                        }
                        combine_progress = (combine_y * combine_area.tiles.x + combine_x) * 1f / (combine_area.tiles.x * combine_area.tiles.y * 1f);
                        EditorGUI.ProgressBar(wc_gui.getRect(0, 4, 206, 19, false, true), combine_progress, (combine_progress * 100).ToString("F0") + "%");
                    }
                }
            }
            if (map.button_settings)
            {
                wc_gui.x = 0;
                wc_gui.y = gui_y + 23;
                settings_rect = new Rect(0, gui_y, guiWidth2 + 1, 200 + gui_height);
                drawGUIBox(settings_rect, "Settings", map.backgroundColor, map.titleColor, map.color);
                wc_gui.y = wc_gui.y + 1;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Stop all exports", EditorStyles.boldLabel);
                GUI.color = Color.white;
                if (GUI.Button(wc_gui.getRect(1, 154, 50, 18, false, true), new GUIContent("Stop", "Stops all current heightmap and image exports.")))
                {
                    reexports();
                }
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Request Timeout", EditorStyles.boldLabel);
                GUI.color = Color.white;
                GUI.changed = false;
                map.timeOut = EditorGUI.IntField(wc_gui.getRect(1, 2, 60, 18, false, true), map.timeOut);
                if (GUI.changed)
                {
                    if (map.timeOut < 2)
                    {
                        map.timeOut = 2;
                    }
                    else if (map.timeOut > 35)
                    {
                        map.timeOut = 35;
                    }
                }
                wc_gui.y = wc_gui.y + 2;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Error color", EditorStyles.boldLabel);
                GUI.color = Color.white;
                GUI.changed = false;
                wc_gui.x = wc_gui.x + 2;
                map.errorColor = EditorGUI.ColorField(wc_gui.getRect(1, 0, 50, 17, false, true), map.errorColor);
                wc_gui.x = wc_gui.x - 2;
                if (GUI.changed)
                {
                    notify_text = "This is the color that Bing sometimes returns as empty space within a satellite image. WorldComposer scans each requested image for this color and if it contains a certain amount of pixels in a row (the green export image box will turn red) it will resend the request to get a clean image. The default color is R 127,G 127, B 127. Change this only if Bing changes this color.";
                }
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Show Warnings", EditorStyles.boldLabel);
                GUI.color = Color.white;
                map.warnings = EditorGUI.Toggle(wc_gui.getRect(1, 2, guiWidth1 + 25, 19, false, true), map.warnings);
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Track Tiles", EditorStyles.boldLabel);
                GUI.color = Color.white;
                map.track_tile = EditorGUI.Toggle(wc_gui.getRect(1, 2, guiWidth1 + 25, 19, false, true), map.track_tile);
                wc_gui.y = wc_gui.y + 3;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Mouse Sensivity", EditorStyles.boldLabel);
                GUI.color = Color.white;
                map.mouse_sensivity = EditorGUI.Slider(wc_gui.getRect(1, 2, guiWidth1 + 84, 19, false, true), map.mouse_sensivity, 1, 10);
                wc_gui.x = 0;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Title Color", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                wc_gui.x = wc_gui.x + 2;
                map.titleColor = EditorGUI.ColorField(wc_gui.getRect(1, 0, 50, 17, false, false), map.titleColor);
                wc_gui.x = wc_gui.x - 2;
                if (GUI.changed)
                {
                    Draw.tex3.SetPixel(0, 0, map.titleColor);
                    Draw.tex3.Apply();
                }
                GUI.changed = gui_changed_old;
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 19;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Background Color", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                wc_gui.x = wc_gui.x + 2;
                map.backgroundColor = EditorGUI.ColorField(wc_gui.getRect(1, 0, 50, 17, false, false), map.backgroundColor);
                wc_gui.x = wc_gui.x - 2;
                if (GUI.changed)
                {
                    Draw.tex2.SetPixel(0, 0, map.backgroundColor);
                    Draw.tex2.Apply();
                }
                wc_gui.x = 0;
                wc_gui.y = wc_gui.y + 19;
                GUI.color = map.color;
                EditorGUI.LabelField(wc_gui.getRect(0, 0, guiWidth1, 19, false, false), "Font Color", EditorStyles.boldLabel);
                GUI.color = Color.white;
                wc_gui.x = wc_gui.x + 2;
                map.color = EditorGUI.ColorField(wc_gui.getRect(1, 0, 50, 17, false, false), map.color);
                wc_gui.x = wc_gui.x - 2;
            }
            guiAreaHeight = gui_y;
            GUI.EndScrollView();
            GUILayout.EndArea();
            if (map.button_image_editor)
            {
                gui_y2 += 113 + map.preimage_edit.edit_color.Count * 18;
                if (current_area.preimage_save_new)
                {
                    gui_y2 += 60;
                }
            }
            if (map.button_help)
            {
                keyHelp();
                gui_y2 += 125;
            }
            screen_rect2 = new Rect(0, 0, 0, 0);
            guiWidth2 += 10;
            gui_y2++;
            if (map.button_converter)
            {
                converter_rect = new Rect(guiWidth2 + 15, 42 + gui_y2, 490, 122);
                drawGUIBox(converter_rect, "Converter", map.backgroundColor, map.titleColor, map.color);
                EditorGUI.LabelField(new Rect(guiWidth2 + 14, 64 + gui_y2, 200, 20), "Source ascii heightmap", EditorStyles.boldLabel);
                if (map.path_display)
                {
                    GUI.color = map.backgroundColor;
                    EditorGUI.DrawPreviewTexture(new Rect(guiWidth2 + 529, 64 + gui_y2, Draw.label_width(current_area.converter_source_path_full, true), 20), Draw.tex2);
                    GUI.color = map.color;
                    EditorGUI.LabelField(new Rect(guiWidth2 + 528, 65 + gui_y2, Draw.label_width(current_area.converter_source_path_full, true), 20), new GUIContent(current_area.converter_source_path_full), EditorStyles.boldLabel);
                }
                EditorGUI.LabelField(new Rect(guiWidth2 + 204, 64 + gui_y2, 220, 20), new GUIContent(current_area.converter_source_path_full, current_area.converter_source_path_full));
                GUI.color = Color.white;
                if (GUI.Button(new Rect(guiWidth2 + 428, 64 + gui_y2, 70, 18), new GUIContent("Change", "Change the source ascii heightmap file.")))
                {
                    path_old = current_area.converter_source_path_full;
                    if (!key.shift)
                    {
                        current_area.converter_source_path_full = EditorUtility.OpenFilePanel("Source Ascii heightmap", current_area.converter_source_path_full, "asc");
                        if (current_area.converter_source_path_full.Length == 0)
                        {
                            current_area.converter_source_path_full = path_old;
                        }
                        else
                        {
                            current_area.converter_destination_path_full = current_area.converter_source_path_full.Replace(".asc", ".raw");
                        }
                    }
                    else
                    {
                        current_area.converter_source_path_full = Application.dataPath;
                    }
                }
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 14, 83 + gui_y2, 200, 20), "Destination raw heightmap", EditorStyles.boldLabel);
                if (map.path_display)
                {
                    GUI.color = map.backgroundColor;
                    EditorGUI.DrawPreviewTexture(new Rect(guiWidth2 + 529, 83 + gui_y2, Draw.label_width(current_area.converter_destination_path_full, true), 20), Draw.tex2);
                    GUI.color = map.color;
                    EditorGUI.LabelField(new Rect(guiWidth2 + 528, 83 + gui_y2, Draw.label_width(current_area.converter_destination_path_full, true), 20), new GUIContent(current_area.converter_destination_path_full), EditorStyles.boldLabel);
                }
                EditorGUI.LabelField(new Rect(guiWidth2 + 204, 83 + gui_y2, 220, 20), new GUIContent(current_area.converter_destination_path_full, current_area.converter_destination_path_full));
                GUI.color = Color.white;
                if (GUI.Button(new Rect(guiWidth2 + 428, 83 + gui_y2, 70, 18), new GUIContent("Change", "Change the destination raw heightmap file.")))
                {
                    path_old = current_area.converter_destination_path_full;
                    if (!key.shift)
                    {
                        int num9 = current_area.converter_source_path_full.LastIndexOf("/");
                        string text2 = current_area.converter_source_path_full.Substring(num9 + 1);
                        text2 = text2.Replace(".asc", ".raw");
                        current_area.converter_destination_path_full = EditorUtility.SaveFilePanel("Destination raw heightmap", current_area.converter_source_path_full, text2, "raw");
                        if (current_area.converter_destination_path_full.Length == 0)
                        {
                            current_area.converter_destination_path_full = path_old;
                        }
                    }
                    else
                    {
                        current_area.converter_source_path_full = Application.dataPath;
                    }
                }
                if (map.path_display)
                {
                    if (GUI.Button(new Rect(guiWidth2 + 471, 104 + gui_y2, 25, 15), new GUIContent("<", "Hide the full path texts."), EditorStyles.miniButtonMid))
                    {
                        map.path_display = false;
                    }
                }
                else if (GUI.Button(new Rect(guiWidth2 + 471, 104 + gui_y2, 25, 15), new GUIContent(">", "Show the full path texts."), EditorStyles.miniButtonMid))
                {
                    map.path_display = true;
                }
                if (GUI.Button(new Rect(guiWidth2 + 18, 121 + gui_y2, 130, 36), new GUIContent("Convert", "Convert the source ascii heightmap to destination raw heightmap.")))
                {
                    if (current_area.converter_source_path_full.Length == 0)
                    {
                        notify_text = "Choose a source ascii file";
                        return;
                    }
                    if (current_area.converter_destination_path_full.Length == 0)
                    {
                        notify_text = "Choose a destination raw file";
                        return;
                    }
                    asc_convert_to_raw(current_area.converter_source_path_full, current_area.converter_destination_path_full);
                }
                gui_y2 += 124;
            }
            if (map.button_create_terrain)
            {
                DrawCreateTerrain();
            }
            screen_rect = new Rect(0, 0, guiWidth2, gui_y);

            mouse_move = key.delta;
            if (key.button == 0 && key.clickCount == 2 && key.mousePosition.y > 20 && !current_area.resize && !check_in_rect())
            {
                stop_download();
                latlong_animate = latlong_mouse;
                animate_time_start = Time.realtimeSinceStartup;
                animate = true;
            }
            if (key.button == 0)
            {
                if (key.type == 0)
                {
                    if (!check_in_rect())
                    {
                        map_scrolling = true;
                    }
                }
                else if (key.type == EventType.MouseUp)
                {
                    map_scrolling = false;
                }
                if (key.type == EventType.MouseDrag && (!check_in_rect() || map_scrolling) && map_scrolling && key.mousePosition.y > 0 && !current_area.resize)
                {
                    animate = false;
                    move_center(new Vector2(-mouse_move.x / map.mouse_sensivity, mouse_move.y / map.mouse_sensivity), true);
                }
            }
            if (key.type == EventType.ScrollWheel || key.type == EventType.KeyDown)
            {
                bool flag = false;
                if (key.delta.y > 0 || key.keyCode == KeyCode.Minus || key.keyCode == KeyCode.KeypadMinus)
                {
                    if (global_script.map_zoom > 1)
                    {
                        if (zoom1 > 0)
                        {
                            zoom1 = (zoom1 - 1) / 2;
                            if (zoom1 < 1)
                            {
                                zoom1 = 0;
                            }
                        }
                        else if (zoom1 < 0)
                        {
                            zoom1_step /= 2;
                            zoom1 += zoom1_step;
                        }
                        else
                        {
                            zoom1 = -0.5f;
                            zoom1_step = -0.5f;
                        }
                        if (zoom2 > 0)
                        {
                            zoom2 = (zoom2 - 1) / 2;
                            if (zoom2 < 1)
                            {
                                zoom2 = 0;
                            }
                        }
                        else if (zoom2 < 0)
                        {
                            zoom2_step /= 2;
                            zoom2 += zoom2_step;
                        }
                        else
                        {
                            zoom2 = -0.5f;
                            zoom2_step = -0.5f;
                        }
                        if (zoom3 > 0)
                        {
                            zoom3 = (zoom3 - 1) / 2;
                            if (zoom3 < 1)
                            {
                                zoom3 = 0;
                            }
                        }
                        else if (zoom3 < 0)
                        {
                            zoom3_step /= 2;
                            zoom3 += zoom3_step;
                        }
                        else
                        {
                            zoom3 = -0.5f;
                            zoom3_step = -0.5f;
                        }
                        if (zoom4 > 0)
                        {
                            zoom4 = (zoom4 - 1) / 2;
                            if (zoom4 < 1)
                            {
                                zoom4 = 0;
                            }
                        }
                        else if (zoom4 < 0)
                        {
                            zoom4_step /= 2;
                            zoom4 += zoom4_step;
                        }
                        else
                        {
                            zoom4 = -0.5f;
                            zoom4_step = -0.5f;
                        }
                        convert_center();
                        global_script.map_zoom = global_script.map_zoom - 1;
                        flag = true;
                        request_map_timer();
                    }
                }
                else if ((key.delta.y < 0 || key.keyCode == KeyCode.Equals || key.keyCode == KeyCode.KeypadPlus) && global_script.map_zoom < 19)
                {
                    if (zoom1 < 0)
                    {
                        zoom1 -= zoom1_step;
                        zoom1_step *= 2;
                        if (zoom1 > -0.5f)
                        {
                            zoom1 = 0;
                        }
                    }
                    else if (zoom1 > 0)
                    {
                        zoom1 = zoom1 * 2 + 1;
                    }
                    else
                    {
                        zoom1 = 1;
                    }
                    if (zoom2 < 0)
                    {
                        zoom2 -= zoom2_step;
                        zoom2_step *= 2;
                        if (zoom2 > -0.5f)
                        {
                            zoom2 = 0;
                        }
                    }
                    else if (zoom2 > 0)
                    {
                        zoom2 = zoom2 * 2 + 1;
                    }
                    else
                    {
                        zoom2 = 1;
                    }
                    if (zoom3 < 0)
                    {
                        zoom3 -= zoom3_step;
                        zoom3_step *= 2;
                        if (zoom3 > -0.5f)
                        {
                            zoom3 = 0;
                        }
                    }
                    else if (zoom3 > 0)
                    {
                        zoom3 = zoom3 * 2 + 1;
                    }
                    else
                    {
                        zoom3 = 1;
                    }
                    if (zoom4 < 0)
                    {
                        zoom4 -= zoom4_step;
                        zoom4_step *= 2;
                        if (zoom4 > -0.5f)
                        {
                            zoom4 = 0;
                        }
                    }
                    else if (zoom4 > 0)
                    {
                        zoom4 = zoom4 * 2 + 1;
                    }
                    else
                    {
                        zoom4 = 1;
                    }
                    convert_center();
                    global_script.map_zoom = global_script.map_zoom + 1;
                    flag = true;
                    request_map_timer();
                }
                if (flag)
                {
                    stop_download();
                    button0 = true;
                    time1 = Time.realtimeSinceStartup;
                    zooming = true;
                }
            }
            if (map.preimage_edit.generate && map.preimage_edit.mode == 1)
            {
                float num12 = 0f;
                if (!map.preimage_edit.loop)
                {
                    num12 = 1 - map.preimage_edit.y1 * 1f / 768f;
                }
                else if (convert_texture)
                {
                    num12 = 1 - map.preimage_edit.y1 * 1f / convert_texture.height;
                }
                GUI.color = new Color(num12, 1 - num12, 0, 2);
                if (map.preimage_edit.loop)
                {
                    old_fontSize = GUI.skin.label.fontSize;
                    old_fontStyle = GUI.skin.label.fontStyle;
                    GUI.skin.label.fontSize = 17;
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    if (map.preimage_edit.loop_active)
                    {
                        GUI.Label(new Rect(position.width / 2 - 65, position.height / 2 + 10, 200, 25), "Generating " + (convert_area.tiles.x * convert_area.tiles.y - convert_area.preimage_count) + "...");
                    }
                    else
                    {
                        GUI.Label(new Rect(position.width / 2 - 52, position.height / 2 + 10, 200, 25), "Pause " + (convert_area.tiles.x * convert_area.tiles.y - convert_area.preimage_count) + "...");
                    }
                    GUI.skin.label.fontSize = old_fontSize;
                    GUI.skin.label.fontStyle = old_fontStyle;
                }
                else
                {
                    old_fontSize = GUI.skin.label.fontSize;
                    old_fontStyle = GUI.skin.label.fontStyle;
                    GUI.skin.label.fontSize = 17;
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    GUI.Label(new Rect(position.width / 2 + 350, position.height / 2 - 10, 200, 25), "Generating...");
                    GUI.skin.label.fontSize = old_fontSize;
                    GUI.skin.label.fontStyle = old_fontStyle;
                }
            }
            GUI.color = Color.white;
            if (notify_text.Length != 0)
            {
                if (notify_frame > 1)
                {
                    ShowNotification(new GUIContent(notify_text));
                    notify_text = string.Empty;
                    notify_frame = 0;
                }
                notify_frame++;
            }
            if (map.export_heightmap_active || map.export_image_active)
            {
                Repaint();
            }
        }

        public void DrawCreateTerrain()
        {
            int linesTerrainComposer = 0;
            if (terraincomposer)
            {
                linesTerrainComposer += 19;
            }
            linesTerrainComposer += 43;

            float guiY = 42 + gui_y2;
            float disabledLines = 0;

            if (!current_area.do_heightmap) disabledLines += 110;
            if (!current_area.do_image) disabledLines += 131;

            #if UNITY_5
            disabledLines += 19;
            #endif

            drawGUIBox(new Rect(guiWidth2 + 15, guiY, 490, 435 + linesTerrainComposer - disabledLines), "Exported Heightmap", map.backgroundColor, map.titleColor, map.color, true);
            
            guiY += 25;

            {
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Generate Heightmap", EditorStyles.boldLabel);
                GUI.color = Color.white;

                current_area.do_heightmap = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.do_heightmap); guiY += 20;
                GUI.color = map.color;
            }

            if (current_area.do_heightmap)
            {
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Normalize Heightmap", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.normalizeHeightmap = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 25, 20), current_area.normalizeHeightmap);

                //if (!this.current_area.normalizeHeightmap)
                //{
                //	GUI.color = map.color;
                //	this.gui_y2 += 20;
                //	EditorGUI.LabelField(new Rect((float)(this.guiWidth2 + 19), (float)(125 + this.gui_y2), (float)200, (float)20), "Terrain Height", EditorStyles.boldLabel);
                //	GUI.color = Color.white;
                //	this.gui_changed_old = GUI.changed;
                //	GUI.changed = false;
                //	this.current_area.terrain_height = EditorGUI.FloatField(new Rect((float)(this.guiWidth2 + 204), (float)(125 + this.gui_y2), (float)75, (float)17), this.current_area.terrain_height);
                //	if (GUI.changed && this.current_area.terrain_height < (float)1)
                //	{
                //		this.current_area.terrain_height = (float)1;
                //	}
                //	GUI.changed = gui_changed_old;
                //}

                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 282, guiY, 80, 20), "Scale", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.terrain_scale = EditorGUI.FloatField(new Rect(guiWidth2 + 324, guiY, 80, 17), current_area.terrain_scale);
                if (current_area.terrain_scale <= 0)
                {
                    current_area.terrain_scale = 1;
                }

                guiY += 20;

                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Heightmap Offset", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(guiWidth2 + 204, guiY, 20, 20), "X");
                GUI.color = Color.white;
                current_area.heightmap_offset_e.x = EditorGUI.FloatField(new Rect(guiWidth2 + 220, guiY, 80, 18), current_area.heightmap_offset_e.x);
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 304, guiY, 20, 20), "Y");
                GUI.color = Color.white;
                current_area.heightmap_offset_e.y = EditorGUI.FloatField(new Rect(guiWidth2 + 324, guiY, 80, 18), current_area.heightmap_offset_e.y); guiY += 21;
                GUI.color = map.color;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Heightmap Resolution", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                current_area.terrain_heightmap_resolution_select = EditorGUI.Popup(new Rect(guiWidth2 + 204, guiY, 200, 17), current_area.terrain_heightmap_resolution_select, heightmap_resolution_list);
                if (GUI.changed)
                {
                    current_area.terrain_heightmap_resolution = (int)(Mathf.Pow(2, current_area.terrain_heightmap_resolution_select + 5) + 1);
                    current_area.terrain_heightmap_resolution_changed = true;
                }
                if (current_area.terrain_heightmap_resolution < 33)
                {
                    current_area.terrain_heightmap_resolution = 33;
                }
                GUI.changed = false;
                current_area.terrain_heightmap_resolution_changed = EditorGUI.Toggle(new Rect(guiWidth2 + 407, guiY, 20, 20), current_area.terrain_heightmap_resolution_changed);
                if (GUI.changed && !current_area.terrain_heightmap_resolution_changed)
                {
                    calc_terrain_heightmap_resolution();
                }
                GUI.changed = gui_changed_old;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 424, guiY, 200, 20), "(" + (current_area.heightmap_resolution.x / current_area.tiles.x).ToString() + ")"); guiY += 19;

                {
                    GUI.color = map.color;
                    if (terraincomposer)
                    {
                        EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Export to TerrainComposer", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.export_to_terraincomposer = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 20, 20), current_area.export_to_terraincomposer); guiY += 19;
                        GUI.color = map.color;
                        if (current_area.export_to_terraincomposer)
                        {
                            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Add Perlin to heightmap", EditorStyles.boldLabel);
                            GUI.color = Color.white;
                            current_area.filter_perlin = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 20, 20), current_area.filter_perlin);
                            GUI.color = map.color;
                        }
                    }

                    guiY = (guiY - 43) + linesTerrainComposer;

                    if (!current_area.export_to_terraincomposer || !terraincomposer)
                    {
                        EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Heightmap Curve", EditorStyles.boldLabel);
                        GUI.color = Color.white;
                        current_area.terrain_curve = EditorGUI.CurveField(new Rect(guiWidth2 + 205, guiY + 1, 198, 17), current_area.terrain_curve);
                        if (GUI.Button(new Rect(guiWidth2 + 406, guiY + 1, 20, 17), new GUIContent("R", "Reset the heightmap curve.")))
                        {
                            current_area.terrain_curve = AnimationCurve.Linear(0, 0, 1, 1);
                            current_area.terrain_curve.AddKey(1, 0);
                            current_area.terrain_curve = current_area.set_curve_linear(current_area.terrain_curve);
                        }
                        GUI.color = map.color;
                    }
                    GUI.color = Color.white;
                    guiY += 25;
                }
                if (!create_terrain_loop && (!current_area.export_to_terraincomposer || !terraincomposer) && current_area.terrain_done)
                {
                    if (!generate)
                    {
                        if (GUI.Button(new Rect(guiWidth2 + 204, guiY, 130, 20), new GUIContent("Generate Heightmap", "Regenerate the heightmap of the terrain.")))
                        {
                            heightmap_count_terrain = 0;
                            generate_manual = true;
                            create_region = current_region;
                            create_area = current_area;
                            generate_begin();
                            heightmap_y = heightmap_resolution - 1;
                            generate = true;
                        }
                        if (GUI.Button(new Rect(guiWidth2 + 339, guiY, 65, 20), new GUIContent("Smooth", "Smoothen the terrain heightmap.")))
                        {
                            smooth_all_terrain(current_area.smooth_strength);
                        }
                        current_area.smooth_strength = EditorGUI.FloatField(new Rect(guiWidth2 + 409, guiY + 2, 63, 17), current_area.smooth_strength);
                        if (current_area.smooth_strength > 1) current_area.smooth_strength = 1;
                    }
                    else
                    {
                        if (GUI.Button(new Rect(guiWidth2 + 203, guiY, 130, 20), new GUIContent("Stop", "Stop generating the heightmap of the terrains.")))
                        {
                            generate = false;
                            generate_manual = false;
                        }
                        GUI.color = Color.red;
                        EditorGUI.LabelField(new Rect(guiWidth2 + 337, guiY, 130, 20), (create_area.tiles.x * create_area.tiles.y - heightmap_count_terrain).ToString(), EditorStyles.boldLabel);
                    }
                }
                guiY += 25;
            }

            drawGUIBox(new Rect(guiWidth2 + 15, guiY, 490, 19), "Exported Images", map.backgroundColor, map.titleColor, map.color, false);

            guiY += 25;

            GUI.color = map.color;
            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Use Satellite Images", EditorStyles.boldLabel);
            GUI.color = Color.white;
            current_area.do_image = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.do_image); guiY += 19;

            if (current_area.do_image)
            {
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Generate Mip Maps", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.mipmapEnabled = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.mipmapEnabled); guiY += 19;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Filter Mode", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.filterMode = (FilterMode)EditorGUI.EnumPopup(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.filterMode); guiY += 19;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Aniso Level", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.anisoLevel = (int)EditorGUI.Slider(new Rect(guiWidth2 + 204, guiY, 200, 17), current_area.anisoLevel, 0, 9); guiY += 19;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Max size", EditorStyles.boldLabel);
                gui_changed_old = GUI.changed;
                GUI.changed = false;
                GUI.color = Color.white;
                current_area.maxTextureSize_select = EditorGUI.Popup(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.maxTextureSize_select, image_resolution_list);
                if (GUI.changed)
                {
                    current_area.maxTextureSize = (int)Mathf.Pow(2, current_area.maxTextureSize_select + 5);
                    current_area.maxTextureSize_changed = true;
                }
                GUI.changed = false;
                current_area.maxTextureSize_changed = EditorGUI.Toggle(new Rect(guiWidth2 + 406, guiY, 20, 20), current_area.maxTextureSize_changed); guiY += 24;
                if (GUI.changed && !current_area.maxTextureSize_changed)
                {
                    if (current_area.resolution > 4096)
                    {
                        current_area.maxTextureSize = 4096;
                        current_area.maxTextureSize_select = 7;
                    }
                    else
                    {
                        current_area.maxTextureSize = current_area.resolution;
                        current_area.maxTextureSize_select = (int)(Mathf.Log(current_area.maxTextureSize, 2) - 5);
                    }
                }
                GUI.changed = gui_changed_old;
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Format", EditorStyles.boldLabel);
                GUI.color = Color.white;
                current_area.textureFormat = (TextureImporterFormat)EditorGUI.EnumPopup(new Rect(guiWidth2 + 204, guiY, 200, 20), current_area.textureFormat);
                guiY += 25;

                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Image Import Settings", EditorStyles.boldLabel);
                GUI.color = Color.white;
                if (!apply_import_settings)
                {
                    if (GUI.Button(new Rect(guiWidth2 + 204, guiY, 70, 18), new GUIContent("Apply", "Apply the image import settings for the exported images of this area.")))
                    {
                        save_global_settings();
                        start_image_import_settings(current_area);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(guiWidth2 + 204, guiY, 70, 18), new GUIContent("Stop", "Stop applying the image import settings.")))
                    {
                        apply_import_settings = false;
                    }
                    GUI.color = Color.red;
                    EditorGUI.LabelField(new Rect(guiWidth2 + 330, guiY, 130, 19), (create_area.tiles.x * create_area.tiles.y - import_settings_count).ToString());
                }
                GUI.color = map.color;
                EditorGUI.LabelField(new Rect(guiWidth2 + 278, guiY, 130, 19), new GUIContent("Auto", "Automatically apply the choosen image import settings after creating the terrains."));
                GUI.color = Color.white;
                current_area.auto_import_settings_apply = EditorGUI.Toggle(new Rect(guiWidth2 + 310, guiY, 25, 19), current_area.auto_import_settings_apply);
                GUI.color = map.color;

                guiY += 25;
            }

            drawGUIBox(new Rect(guiWidth2 + 15, guiY, 490, 19), "Create Terrain", map.backgroundColor, map.titleColor, map.color);

            guiY += 25;

            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Asset Path", EditorStyles.boldLabel);
            if (map.path_display)
            {
                GUI.color = map.backgroundColor;
                Rect rect = new Rect(guiWidth2 + 529, guiY, Draw.label_width(current_area.export_terrain_path, true), 20);
                EditorGUI.DrawPreviewTexture(rect, Draw.tex2);
                GUI.color = map.color;
                EditorGUI.LabelField(rect, new GUIContent(current_area.export_terrain_path), EditorStyles.boldLabel);
            }
            EditorGUI.LabelField(new Rect(guiWidth2 + 204, guiY, 200, 20), new GUIContent(current_area.export_terrain_path, current_area.export_terrain_path));
            GUI.color = Color.white;

            gui_changed_old = GUI.changed;
            current_area.export_terrain_changed = EditorGUI.Toggle(new Rect(guiWidth2 + 406, guiY, 20, 20), current_area.export_terrain_changed);
            if (GUI.changed && !current_area.export_terrain_changed)
            {
                current_area.export_terrain_path = current_area.export_heightmap_path + "/Terrains";
            }

            GUI.changed = gui_changed_old;
            if (GUI.Button(new Rect(guiWidth2 + 428, guiY, 70, 18), new GUIContent("Change", "Change the folder where to save the terrains to.")))
            {
                path_old = current_area.export_heightmap_path;
                if (!key.shift)
                {
                    current_area.export_terrain_path = EditorUtility.OpenFolderPanel("Terrain Asset Path", current_area.export_terrain_path, string.Empty);
                    if (current_area.export_terrain_path.Length == 0)
                    {
                        current_area.export_terrain_path = path_old;
                    }
                }
                else
                {
                    current_area.export_terrain_path = Application.dataPath;
                }
                if (path_old != current_area.export_heightmap_path)
                {
                    if (current_area.export_terrain_path.IndexOf(Application.dataPath) == -1)
                    {
                        notify_text = "The path should be inside your Unity project. Reselect your path.";
                        current_area.export_terrain_path = Application.dataPath;
                    }
                    current_area.export_terrain_changed = true;
                    if (!current_area.export_image_changed)
                    {
                        current_area.export_image_path = current_area.export_heightmap_path;
                    }
                    if (!current_area.export_terrain_changed)
                    {
                        current_area.export_terrain_path = current_area.export_heightmap_path + "/Terrains";
                    }
                }
            }
            guiY += 19;

            GUI.color = map.color;
            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Asset Name", EditorStyles.boldLabel);
            GUI.color = Color.white;
            gui_changed_old = GUI.changed;
            GUI.changed = false;
            current_area.terrain_scene_name = EditorGUI.TextField(new Rect(guiWidth2 + 204, guiY, 200, 17), current_area.terrain_scene_name);
            if (GUI.changed)
            {
                current_area.terrain_name_changed = true;
            }
            GUI.changed = false;
            current_area.terrain_name_changed = EditorGUI.Toggle(new Rect(guiWidth2 + 406, guiY, 20, 20), current_area.terrain_name_changed);
            if (GUI.changed && !current_area.terrain_name_changed)
            {
                current_area.terrain_scene_name = "_" + current_area.export_heightmap_filename;
                current_area.terrain_scene_name = current_area.export_heightmap_filename;
            }
            GUI.changed = gui_changed_old;

            if (map.path_display)
            {
                if (GUI.Button(new Rect(guiWidth2 + 471, guiY + 2, 25, 15), new GUIContent("<", "Hide the full path texts."), EditorStyles.miniButtonMid))
                {
                    map.path_display = false;
                }
            }
            else if (GUI.Button(new Rect(guiWidth2 + 471, guiY + 2, 25, 15), new GUIContent(">", "Show the full path texts."), EditorStyles.miniButtonMid))
            {
                map.path_display = true;
            }
            guiY += 19;

            GUI.color = map.color;
            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), "Scene Name", EditorStyles.boldLabel);
            GUI.color = Color.white;
            gui_changed_old = GUI.changed;
            GUI.changed = false;
            current_area.terrain_scene_name = EditorGUI.TextField(new Rect(guiWidth2 + 204, guiY, 200, 17), current_area.terrain_scene_name);
            if (GUI.changed)
            {
                current_area.terrain_name_changed = true;
            }
            GUI.changed = gui_changed_old;
            guiY += 23;

#if !UNITY_5
            GUI.color = map.color;
            EditorGUI.LabelField(new Rect(guiWidth2 + 19, guiY, 200, 20), new GUIContent("Disable Auto Lightmapping", "This will disable lightmapping, so Unity doesn't start to lightmap the terrains which can take ages and needs to be done manually."));
            GUI.color = Color.white;
            global_script.settings.disableLightmapping = EditorGUI.Toggle(new Rect(guiWidth2 + 204, guiY, 200, 20), global_script.settings.disableLightmapping); 

            guiY += 19;
#endif

            if (!create_terrain_loop)
            {
                if (GUI.Button(new Rect(guiWidth2 + 19, guiY, 130, 37), new GUIContent("Create Terrain", "Create the terrains from the exported heightmap and images.")))
                {
                    if (map.export_raw && !map.export_jpg && !map.export_png)
                    {
                        notify_text = "It is only possible to create terrains with Jpg (Recommended) or Png images. The raw is for photoshop editing";
                        return;
                    }
                    save_global_settings();
                    create_terrains_area();
                }
            }
            else if (GUI.Button(new Rect(guiWidth2 + 18, guiY, 130, 36), new GUIContent("Stop", "Stop creating terrains.")))
            {
                create_terrain_loop = false;
                if (!map.export_heightmap_active && !map.export_image_active)
                {
                    Application.runInBackground = false;
                }
                generate = false;
            }
            if (create_terrain_loop)
            {
                GUI.color = Color.red;
                EditorGUI.LabelField(new Rect(guiWidth2 + 150, guiY, 130, 36), (create_area.tiles.x * create_area.tiles.y - create_terrain_count).ToString());
            }
            GUI.color = map.color;
        }

        public bool move_to_latlong(latlong_class latlong, float speed)
        {
            latlong_class latlong_class = Mathw.pixel_to_latlong(new Vector2(0, 0), global_script.map_latlong_center, zoom);
            Vector2 vector = Mathw.latlong_to_pixel(latlong, latlong_class, zoom, new Vector2(position.width, position.height));
            float num = vector.x - position.width / 2 - offmap.x;
            float num2 = -(vector.y - position.height / 2 + offmap.y);
            bool arg_11E_0;
            if (Mathf.Abs(num) < 0.01f && Mathf.Abs(num2) < 0.01f)
            {
                global_script.map_latlong_center = latlong;
                offmap = new Vector2(0, 0);
                request_map();
                Repaint();
                arg_11E_0 = true;
            }
            else
            {
                num /= 250 / speed;
                num2 /= 250 / speed;
                move_center(new Vector2(num, num2), false);
                arg_11E_0 = false;
            }
            return arg_11E_0;
        }

        public void move_center(Vector2 offset2, bool map)
        {
            offset = offset2;
            offmap += offset;
            if (zoom_pos1 != 0)
            {
                offmap1 += offset / (float)(zoom_pos1 + 1);
            }
            else
            {
                offmap1 += offset;
            }
            if (zoom_pos2 != 0)
            {
                offmap2 += offset / (float)(zoom_pos2 + 1);
            }
            else
            {
                offmap2 += offset;
            }
            if (zoom_pos3 != 0)
            {
                offmap3 += offset / (float)(zoom_pos3 + 1);
            }
            else
            {
                offmap3 += offset;
            }
            if (zoom_pos4 != 0)
            {
                offmap4 += offset / (float)(zoom_pos4 + 1);
            }
            else
            {
                offmap4 += offset;
            }
            if (map)
            {
                stop_download();
                request_map_timer();
            }
            Repaint();
        }

        public void AddRegion(bool duplicate)
        {
            if (duplicate)
            {
                var region = current_region.GetShallowCopy();
                region.name = current_region.name + "_Copy";
                map.region.Add(region);
                current_region = map.region[map.region.Count - 1];
            }
            else
            {
                map.region.Add(new map_region_class(map.region.Count + 1));
                map.region[map.region.Count - 1].center = global_script.map_latlong_center;
                current_region = map.region[map.region.Count - 1];
                add_area(current_region, 0, "Untitled");
            }
            map.region_select = map.region.Count - 1;
            map.make_region_popup();
        }

        public void AddArea()
        {

        }

        public void convert_center()
        {
            global_script.map_latlong_center = Mathw.pixel_to_latlong(new Vector2(offmap.x, -offmap.y), global_script.map_latlong_center, zoom);
            offmap = new Vector2(0, 0);
        }

        public void request_map_timer()
        {
            time1 = Time.realtimeSinceStartup;
            request1 = true;
            request2 = true;
            if (!map.button_image_editor)
            {
                request3 = true;
                request4 = true;
            }
            Repaint();
        }

        public void request_map()
        {
            if (global_script)
            {
                request_map1();
                request_map2();
                if (!map.button_image_editor)
                {
                    request_map3();
                    request_map4();
                }
                Repaint();
            }
        }

        public void reset_texture(Texture2D texture)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
            texture.Apply();
        }

        public void request_map1()
        {
            if (map.active)
            {
                stop_download_map1();
                request_load1 = true;
                global_script.map_load = false;
                global_script.map_latlong = Mathw.pixel_to_latlong(new Vector2(-400, 0), global_script.map_latlong_center, global_script.map_zoom);
                string text = "https://dev.virtualearth.net/REST/v1/Imagery/Map/" + map.type.ToString() + "/" + global_script.map_latlong.latitude + "," + global_script.map_latlong.longitude + "/" + global_script.map_zoom + "?&mapSize=800,800&key=" + map.bingKey[map.bingKey_selected].key;
                global_script.settings.myExt.Request(text, true);
                map.bingKey[map.bingKey_selected].pulls = map.bingKey[map.bingKey_selected].pulls + 1;
            }
        }

        public void request_map2()
        {
            if (map.active)
            {
                stop_download_map2();
                request_load2 = true;
                global_script.map_load = false;
                global_script.map_latlong.longitude = Mathw.pixel_to_latlong(new Vector2(400, 0), global_script.map_latlong_center, global_script.map_zoom).longitude;
                string text = "https://dev.virtualearth.net/REST/v1/Imagery/Map/" + map.type.ToString() + "/" + global_script.map_latlong.latitude + "," + global_script.map_latlong.longitude + "/" + global_script.map_zoom + "?&mapSize=800,800&key=" + map.bingKey[map.bingKey_selected].key;
                global_script.settings.myExt2.Request(text, true);
                map.bingKey[map.bingKey_selected].pulls = map.bingKey[map.bingKey_selected].pulls + 1;
            }
        }

        public void request_map3()
        {
            if (map.active)
            {
                if (global_script.map_zoom > 2)
                {
                    stop_download_map3();
                    request_load3 = true;
                    global_script.map_load3 = false;
                    string text = "https://dev.virtualearth.net/REST/v1/Imagery/Map/" + map.type.ToString() + "/" + global_script.map_latlong_center.latitude + "," + global_script.map_latlong_center.longitude + "/" + (global_script.map_zoom - 2) + "?&mapSize=800,800&key=" + map.bingKey[map.bingKey_selected].key;
                    global_script.settings.myExt3.Request(text, true);
                    map.bingKey[map.bingKey_selected].pulls = map.bingKey[map.bingKey_selected].pulls + 1;
                }
            }
        }

        public void request_map4()
        {
            if (map.active)
            {
                if (global_script.map_zoom > 3)
                {
                    stop_download_map4();
                    request_load4 = true;
                    global_script.map_load4 = false;
                    string text = "https://dev.virtualearth.net/REST/v1/Imagery/Map/" + map.type.ToString() + "/" + global_script.map_latlong_center.latitude + "," + global_script.map_latlong_center.longitude + "/" + (global_script.map_zoom - 3) + "?&mapSize=800,800&key=" + map.bingKey[map.bingKey_selected].key;
                    global_script.settings.myExt4.Request(text, true);
                    map.bingKey[map.bingKey_selected].pulls = map.bingKey[map.bingKey_selected].pulls + 1;
                }
            }
        }

        public void stop_download()
        {
            stop_download_map1();
            stop_download_map2();
            stop_download_map3();
            stop_download_map4();
        }

        public void stop_download_map1()
        {
            if (request_load1)
            {
                global_script.map_load = false;
                global_script.settings.myExt.Abort();
            }
            request_load1 = false;
        }

        public void stop_download_map2()
        {
            global_script.map_load2 = false;

            if (request_load2)
            {
                global_script.settings.myExt2.Abort();
            }
            request_load2 = false;
        }

        public void stop_download_map3()
        {
            if (request_load3)
            {
                global_script.map_load3 = false;
                global_script.settings.myExt3.Abort();
            }
            request_load3 = false;
        }

        public void stop_download_map4()
        {
            if (request_load4)
            {
                global_script.map_load4 = false;
                global_script.settings.myExt4.Abort();
            }
            request_load4 = false;
        }

        public void get_elevation_info(latlong_class latlong)
        {
            map.elExt_check.Request("https://dev.virtualearth.net/REST/v1/Elevation/List?points=" + latlong.latitude.ToString() + "," + latlong.longitude.ToString() + "&heights=ellipsoid&key=" + map.bingKey[map.bingKey_selected].key, false);
            map.bingKey[map.bingKey_selected].pulls = map.bingKey[map.bingKey_selected].pulls + 1;
            map.elExt_check_loaded = false;
        }

        public void drawGUIBox(Rect rect, string text, Color backgroundColor, Color highlightColor, Color textColor, bool drawBox = true)
        {
            if (drawBox)
            {
                GUI.color = new Color(1, 1, 1, backgroundColor.a);
                GUI.DrawTexture(new Rect(rect.x, rect.y + 19, rect.width, rect.height - 19), Draw.tex2);
            }
            GUI.color = new Color(1, 1, 1, highlightColor.a);
            EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y, rect.width, 19), Draw.tex3);
            GUI.color = textColor;
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, rect.width, 19), text, EditorStyles.boldLabel);
            gui_y = (int)(gui_y + (rect.height + 2));
            gui_height = 0;
        }

        public void Update()
        {
            //Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

            if (global_script)
            {
                WebRequest.ProcessRequests();
                check_content_done();

                if (Time.realtimeSinceStartup > save_global_time + global_script.settings.save_global_timer * 60)
                {
                    save_global_settings();
                    save_global_time = Time.realtimeSinceStartup;
                }
                if (map.preimage_edit.generate)
                {
                    if (map.preimage_edit.mode == 2)
                    {
                        convert_textures_raw(convert_area);
                    }
                    else
                    {
                        image_edit_apply();
                    }
                }
                if (combine_generate)
                {
                    combine_textures();
                    Repaint();
                }
                if (slice_generate)
                {
                    slice_textures();
                    Repaint();
                }
                if (import_settings_call)
                {
                    import_settings_call = false;
                    if (import_jpg_call)
                    {
                        import_jpg_call = false;
                        Draw.set_image_import_settings(import_jpg_path, false, import_image_area.textureFormat, TextureWrapMode.Clamp, import_image_area.resolution, import_image_area.mipmapEnabled, import_image_area.filterMode, import_image_area.anisoLevel, 127);
                    }
                    if (import_png_call)
                    {
                        import_png_call = false;
                        Draw.set_image_import_settings(import_png_path, false, import_image_area.textureFormat, TextureWrapMode.Clamp, import_image_area.resolution, import_image_area.mipmapEnabled, import_image_area.filterMode, import_image_area.anisoLevel, 127);
                    }
                }
                if (zooming)
                {
                    zoom_pos = Mathf.Lerp((float)zoom_pos, (float)zoom1, 0.1f);
                    zoom_pos1 = Mathf.Lerp((float)zoom_pos1, (float)zoom1, 0.1f);
                    zoom_pos2 = Mathf.Lerp((float)zoom_pos2, (float)zoom2, 0.1f);
                    zoom_pos3 = Mathf.Lerp((float)zoom_pos3, (float)zoom3, 0.1f);
                    zoom_pos4 = Mathf.Lerp((float)zoom_pos4, (float)zoom4, 0.1f);
                    if (Mathf.Abs((float)(zoom_pos1 - zoom1)) < 0.001)
                    {
                        zoom_pos = zoom1;
                        zoom_pos1 = zoom1;
                        zoom_pos2 = zoom2;
                        zoom_pos3 = zoom3;
                        zoom_pos4 = zoom4;
                        zooming = false;
                    }
                    Repaint();
                }
                if (animate && move_to_latlong(latlong_animate, 45))
                {
                    animate = false;
                }
                if (create_terrain_loop && !generate)
                {
                    create_terrain(terrain_region.area[0], terrain_region.area[0].terrains[0], Application.dataPath + "/Terrains", terrain_parent.transform);
                    Repaint();
                }
                if (apply_import_settings)
                {
                    tile_class tile_class = calc_terrain_tile(import_settings_count, terrain_region.area[0].tiles_select);
                    if (tile_class == null)
                    {
                        return;
                    }
                    if (map.export_jpg)
                    {
                        string text = create_area.export_image_path.Replace(Application.dataPath, "Assets") + "/" + create_area.export_image_filename + "_x" + tile_class.x.ToString() + "_y" + tile_class.y.ToString() + ".jpg";
                        if (!File.Exists(text))
                        {
                            notify_text = text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles";
                            Debug.Log(text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles.");
                        }
                        else
                        {
                            Draw.set_image_import_settings(text, false, create_area.textureFormat, TextureWrapMode.Clamp, create_area.maxTextureSize, create_area.mipmapEnabled, create_area.filterMode, create_area.anisoLevel, 124);
                        }
                    }
                    if (map.export_png)
                    {
                        string text = create_area.export_image_path.Replace(Application.dataPath, "Assets") + "/" + create_area.export_image_filename + "_x" + tile_class.x.ToString() + "_y" + tile_class.y.ToString() + ".png";
                        if (!File.Exists(text))
                        {
                            notify_text = text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles";
                            Debug.Log(text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles.");
                        }
                        else
                        {
                            Draw.set_image_import_settings(text, false, create_area.textureFormat, TextureWrapMode.Clamp, create_area.maxTextureSize, create_area.mipmapEnabled, create_area.filterMode, create_area.anisoLevel, 124);
                        }
                    }
                    import_settings_count++;
                    if (import_settings_count > create_area.tiles.x * create_area.tiles.y - 1)
                    {
                        apply_import_settings = false;
                    }
                    Repaint();
                }
                if (generate)
                {
                    generate_heightmap2();
                }
                if (request1 && Time.realtimeSinceStartup - time1 > 1.5f)
                {
                    request1 = false;
                    convert_center();
                    request_map1();
                }
                if (request2 && Time.realtimeSinceStartup - time1 > 1.7f)
                {
                    request2 = false;
                    convert_center();
                    request_map2();
                }
                if (request3 && Time.realtimeSinceStartup - time1 > 1.9f)
                {
                    request3 = false;
                    convert_center();
                    request_map3();
                }
                if (request4 && Time.realtimeSinceStartup - time1 > 2.1f)
                {
                    request4 = false;
                    convert_center();
                    request_map4();
                }
                if (map.elExt_check.IsDone && !map.elExt_check_loaded)
                {
                    string errorText;
                    if (!map.elExt_check.IsError(out errorText))
                    {
                        string text2 = map.elExt_check.GetText();
                        string text3 = text2;
                        int num = text2.IndexOf("zoomLevel");
                        int num2 = 0;
                        int num3 = 0;
                        text2 = text2.Substring(num + 11);
                        num = text2.IndexOf("}");
                        text2 = text2.Substring(0, num);
                        num3 = short.Parse(text2);
                        num = text3.IndexOf("elevations");
                        text3 = text3.Substring(num + 13);
                        num = text3.IndexOf("]");
                        text3 = text3.Substring(0, num);
                        num2 = short.Parse(text3);
                        if (map.elExt_check_assign)
                        {
                            if (requested_area != null)
                            {
                                requested_area.center_height = num2;
                                requested_area.elevation_zoom = num3;
                                if (requested_area.heightmap_zoom == 0)
                                {
                                    requested_area.heightmap_zoom = requested_area.elevation_zoom;
                                }
                                calc_heightmap_settings(requested_area);
                            }
                            map.elExt_check_assign = false;
                        }
                        else
                        {
                            notify_text = "Zoom Level: " + num3 + "-> " + Mathf.Round((float)Mathw.calc_latlong_area_resolution(latlong_mouse, num3)) + " (m/p), Height: " + num2 + " (m)";
                            Repaint();
                        }
                    }
                    else
                    {
                        Debug.LogError(errorText);
                        Repaint();
                    }
                    map.elExt_check_loaded = true;
                }
                if (global_script.settings.myExt.IsDoneIfErrorRedo && !global_script.map_load)
                {
                    global_script.map_load = true;

                    Texture2D tex = global_script.settings.myExt.GetTexture(); 
                    if (!global_script.map_load2)
                    {
                        global_script.map_combine = false;
                    }
                    if (!map0)
                    {
                        map0 = new Texture2D(1600, 768, TextureFormat.RGBA32, false, false);
                    }
                    if (tex.height == 800 && tex.width == 800)
                    {
                        pixels = tex.GetPixels(0, 32, 800, 768);

                        if (CheckImageError()) global_script.settings.myExt.RedoRequest();
                        else
                        {
                            map0.SetPixels(0, 0, 800, 768, pixels);
                        }
                    }

                    Draw.DisposeTexture(ref tex);
                }
                if (global_script.settings.myExt2.IsDoneIfErrorRedo && !global_script.map_load2)
                {
                    if (!map1)
                    {
                        map1 = new Texture2D(800, 800, TextureFormat.RGBA32, false, false);
                        map1.wrapMode = TextureWrapMode.Clamp;
                    }
                    if (!map0)
                    {
                        map0 = new Texture2D(1600, 768, TextureFormat.RGBA32, false, false);
                    }

                    offmap2 = new Vector2(0, 0);
                    zoom2 = 0;
                    zoom_pos2 = 0;

                    if (!global_script.map_load)
                    {
                        global_script.map_combine = false;
                    }

                    Repaint();

                    global_script.map_load2 = true;

                    Draw.DisposeTexture(ref map0b);
                    map0b = global_script.settings.myExt2.GetTexture();

                    if (map0b.height == 800 && map0b.width == 800 && !map.button_image_editor)
                    {
                        CopyMap0bIntoMap0();
                    }
                }
                if (global_script.map_load && !global_script.map_combine && (global_script.map_load2 || map.button_image_editor))
                {
                    map0.Apply();
                    global_script.map_combine = true;
                    if (map.button_image_editor)
                    {
                        image_generate_begin();
                    }
                    global_script.map_zoom_old = global_script.map_zoom;
                    offmap1 = new Vector2(0, 0);
                    zoom1 = 0;
                    zoom_pos1 = 0;
                    Repaint();
                }
                if (global_script.settings.myExt3.IsDoneIfErrorRedo && !global_script.map_load3)
                {
                    if (!map2)
                    {
                        map2 = new Texture2D(800, 768, TextureFormat.RGBA32, false, false);
                        map2.wrapMode = TextureWrapMode.Clamp;
                    }
                    Texture2D tex = global_script.settings.myExt3.GetTexture();
                    global_script.map_load3 = true;
                    if (tex.width == 800 && tex.height == 800)
                    {
                        pixels = tex.GetPixels(0, 32, 800, 768);

                        if (CheckImageError()) global_script.settings.myExt3.RedoRequest();
                        else
                        {
                            map2.SetPixels(0, 0, 800, 768, pixels);
                            map2.Apply();
                        }
                    }
                    Draw.DisposeTexture(ref tex);

                    zoom3 = 0;
                    zoom_pos3 = 0;
                    offmap3 = new Vector2(0, 0);
                    global_script.map_zoom3 = global_script.map_zoom;
                    Repaint();
                }
                if (global_script.settings.myExt4.IsDoneIfErrorRedo && !global_script.map_load4)
                {
                    if (!map3)
                    {
                        map3 = new Texture2D(800, 768, TextureFormat.RGBA32, false, false);
                        map3.wrapMode = TextureWrapMode.Clamp;
                    }
                    Texture2D tex = global_script.settings.myExt4.GetTexture();
                    
                    if (tex.width == 800 && tex.height == 800)
                    {
                        pixels = tex.GetPixels(0, 32, 800, 768);

                        if (CheckImageError()) global_script.settings.myExt4.RedoRequest();
                        else
                        {
                            
                            global_script.map_load4 = true;
                            map3.SetPixels(0, 0, 800, 768, pixels);
                            map3.Apply();
                        }
                    }
                    Draw.DisposeTexture(ref tex);

                    offmap4 = new Vector2(0, 0);
                    zoom_pos4 = 0;
                    zoom4 = 0;
                    Repaint();
                }
                if (map.export_heightmap_active)
                {
                    if (map.bingKey[map.bingKey_selected].pulls > 48000)
                    {
                        check_free_key();
                    }
                    if (map.export_heightmap_continue)
                    {
                        map.export_heightmap_timeEnd = Time.realtimeSinceStartup;
                    }
                    for (int i = 0; i < map.elExt.Count; i++)
                    {
                        ext_class elExt = map.elExt[i];

                        if (elExt.loaded && !map.export_heightmap.last_tile)
                        {
                            if (map.export_heightmap_continue && Time.realtimeSinceStartup > elExt.startTime)
                            {
                                elExt.bres = new Vector2(32, 32);
                                if (export_heightmap_area.export_heightmap_not_fit)
                                {
                                    if (map.export_heightmap.tile.x == map.export_heightmap.tiles.x - 1)
                                    {
                                        elExt.bres.x = export_heightmap_area.export_heightmap_bres.x;
                                    }
                                    if (map.export_heightmap.tile.y == map.export_heightmap.tiles.y - 1)
                                    {
                                        elExt.bres.y = export_heightmap_area.export_heightmap_bres.y;
                                    }
                                }
                                elExt.latlong_area = Mathw.calc_latlong_area_by_tile(map.export_heightmap_area.latlong1, map.export_heightmap.tile, map.export_heightmap_zoom, 32, new Vector2(elExt.bres.x, elExt.bres.y), export_heightmap_area.heightmap_offset_e);
                                elExt.tile.x = map.export_heightmap.tile.x;
                                elExt.tile.y = map.export_heightmap.tile.y;
                                elExt.url = "https://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds=" + elExt.latlong_area.latlong2.latitude.ToString() + "," + elExt.latlong_area.latlong1.longitude.ToString() + "," + elExt.latlong_area.latlong1.latitude.ToString() + "," + elExt.latlong_area.latlong2.longitude.ToString() + "&rows=" + elExt.bres.y.ToString() + "&cols=" + elExt.bres.x.ToString() + "&heights=ellipsoid&key=" + map.bingKey[map.bingKey_selected].key;
                                pull_elevation(i);
                                elExt.loaded = false;
                                if (jump_export_heightmap_tile())
                                {
                                    map.export_heightmap.last_tile = true;
                                }
                            }
                        }
                        else if (elExt.pull.HasRequested)
                        {
                            string errorText;
                            if (elExt.pull.IsError(out errorText))
                            {
                                pull_elevation(i);
                                elExt.error = 2;
                                if (!errorText.Contains("429"))
                                {
                                    Debug.LogError(errorText);
                                    if (elExt.download_error++ > 24)
                                    {
                                        ElevationConvertZero(i);
                                        elExt.loaded = true;
                                        elExt.download_error = 0;
                                    }
                                }
                            }
                            if (Time.realtimeSinceStartup > elExt.startTime + map.timeOut)
                            {
                                pull_elevation(i);
                                elExt.download_error = 0;
                            }
                            if (elExt.pull.IsDone)
                            {
                                if (ElevationConvert(i))
                                {
                                    elExt.loaded = true;
                                    elExt.startTime = Time.realtimeSinceStartup + 0.1f;
                                }
                                elExt.download_error = 0;
                            }
                        }
                    }
                    if (map.export_heightmap.last_tile && check_elevation_pulls_done())
                    {
                        Debug.Log("Exporting heightmap Region: " + export_heightmap_region.name + " -> Area: " + export_heightmap_area.name + " done.");
                        stop_elevation_pull(export_heightmap_region, export_heightmap_area);
                    }
                }
                if (map.export_image_active)
                {
                    if (map.bingKey[map.bingKey_selected].pulls > 48)
                    {
                        check_free_key();
                    }
                    if (map.export_image_continue)
                    {
                        map.export_image_timeEnd = Time.realtimeSinceStartup;
                    }

                    for (int j = 0; j < map.texExt.Count; j++)
                    {
                        ext_class texExt = map.texExt[j];

                        if (texExt.loaded && !map.export_image.last_tile)
                        {
                            if (map.export_image_continue)
                            {
                                tile_class tile_class2 = new tile_class();
                                tile_class2.x = map.export_image.subtiles.x * map.export_image.tile.x + map.export_image.subtile.x;
                                tile_class2.y = map.export_image.subtiles.y * map.export_image.tile.y + map.export_image.subtile.y;
                                texExt.tile.x = map.export_image.tile.x;
                                texExt.tile.y = map.export_image.tile.y;
                                texExt.subtile.x = map.export_image.subtile.x;
                                texExt.subtile.y = map.export_image.subtile.y;
                                texExt.latlong_center = Mathw.calc_latlong_center_by_tile(map.export_image_area.latlong1, map.export_image.tile, map.export_image.subtile, map.export_image.subtiles, map.export_image_zoom, 512, export_image_area.image_offset_e);
                                texExt.latlong_area = Mathw.calc_latlong_area_by_tile(map.export_image_area.latlong1, tile_class2, map.export_image_zoom, 512, new Vector2(512, 512), export_image_area.image_offset_e);
                                texExt.url = "https://dev.virtualearth.net/REST/v1/Imagery/Map/" + map.type.ToString() + "/" + texExt.latlong_center.latitude + "," + texExt.latlong_center.longitude + "/" + map.export_image_zoom + "?&mapSize=512,544&key=" + map.bingKey[map.bingKey_selected].key;
                                RequestImage(texExt);

                                if (jump_export_image_tile())
                                {
                                    map.export_image.last_tile = true;
                                }
                            }
                        }
                        else if (texExt.pull.HasRequested && !texExt.loaded)
                        {
                            string errorText;

                            if (texExt.pull.IsError(out errorText) || (Time.realtimeSinceStartup > texExt.startTime + map.timeOut))
                            {
                                RequestImage(texExt);
                            }
                            else if (texExt.pull.IsDone)
                            {
                                texConvert(texExt);
                            }
                        }
                    }
                }
            }
        }

        public void pick_done()
        {
            latlong_area = Mathw.calc_latlong_area_rounded(current_area.upper_left, latlong_mouse, current_area.image_zoom, current_area.resolution, key.shift, 8);
            current_area.lower_right = latlong_area.latlong2;
            current_area.center = Mathw.calc_latlong_center(current_area.upper_left, current_area.lower_right, zoom, new Vector2(position.width, position.height));
            requested_area = current_area;
            map.elExt_check_assign = true;
            get_elevation_info(current_area.center);
            current_area.select = 0;
            map.mode = 0;
        }

        public bool check_image_tile_pulls_done(int texExt_index)
        {
            int num = 0;
            for (int i = 0; i < map.texExt.Count; i++)
            {
                if (map.texExt[i].tile.x == map.texExt[texExt_index].tile.x && map.texExt[i].tile.y == map.texExt[texExt_index].tile.y && map.texExt[i].loaded)
                {
                    if (++num == map.export_image.subtiles_total) return true;
                }
            }
            return false;
        }

        public void pull_elevation(int index)
        {
            ext_class elExt = map.elExt[index];

            elExt.zero_error = 0;
            
            elExt.pull.Request(elExt.url, false);
            elExt.startTime = Time.realtimeSinceStartup;
            map.bingKey[map.bingKey_selected].pulls++;
        }

        public void pull_elevation_zerro(int index)
        {
            ext_class elExt = map.elExt[index];

            elExt.pull.Request(elExt.url, false);
            elExt.startTime = Time.realtimeSinceStartup;
            map.bingKey[map.bingKey_selected].pulls++;
        }

        public void RequestImage(ext_class texExt)
        {
            texExt.loaded = false;
            texExt.zero_error = 0;

            texExt.pull.Request(texExt.url, true);
            texExt.startTime = Time.realtimeSinceStartup;
            map.bingKey[map.bingKey_selected].pulls++;
        }

        public bool check_elevation_pulls_done()
        {
            for (int i = 0; i < map.elExt.Count; i++)
            {
                if (!map.elExt[i].loaded) return false;
            }

            return true;
        }

        public bool check_image_pulls_done()
        {
            for (int i = 0; i < map.texExt.Count; i++)
            {
                if (!map.texExt[i].loaded)
                {
                    return false;
                }
            }
            return true;
        }

        public void check_export_heightmap_call()
        {
            for (int i = 0; i < map.region.Count; i++)
            {
                for (int j = 0; j < map.region[i].area.Count; j++)
                {
                    if (map.region[i].area[j].export_heightmap_call)
                    {
                        map.region[i].area[j].export_heightmap_call = false;
                        start_elevation_pull(map.region[i], map.region[i].area[j]);
                        return;
                    }
                }
            }
        }

        public void check_export_image_active()
        {
            for (int i = 0; i < map.region.Count; i++)
            {
                for (int j = 0; j < map.region[i].area.Count; j++)
                {
                    if (map.region[i].area[j].export_image_call)
                    {
                        map.region[i].area[j].export_image_call = false;
                        start_image_pull(map.region[i], map.region[i].area[j]);
                        return;
                    }
                }
            }
        }

        public void stop_all_elevation_pull()
        {
            for (int i = 0; i < map.region.Count; i++)
            {
                stop_elevation_pull_region(map.region[i]);
            }
        }

        public void stop_elevation_pull(map_region_class region1, map_area_class area1)
        {
            map.export_heightmap_active = false;
            area1.export_heightmap_active = false;
            if (!map.export_image_active)
            {
                Application.runInBackground = false;
            }
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
                fs = null;
            }
            AssetDatabase.Refresh();
            check_export_heightmap_call();
            check_export_image_active();
        }

        public void stop_elevation_pull_region(map_region_class region1)
        {
            int i = 0;
            for (i = 0; i < region1.area.Count; i++)
            {
                region1.area[i].export_heightmap_call = false;
            }
            for (i = 0; i < region1.area.Count; i++)
            {
                if (region1.area[i].export_heightmap_active)
                {
                    stop_elevation_pull(region1, region1.area[i]);
                }
            }
        }

        public void StopAllImagePull()
        {
            for (int i = 0; i < map.region.Count; i++)
            {
                stop_image_pull_region(map.region[i]);
            }
        }

        public void stop_image_pull_region(map_region_class region1)
        {
            int i = 0;
            for (i = 0; i < region1.area.Count; i++)
            {
                region1.area[i].export_image_call = false;
            }
            for (i = 0; i < region1.area.Count; i++)
            {
                if (region1.area[i].export_image_active)
                {
                    stop_image_pull(region1, region1.area[i], false);
                }
            }
        }

        public void stop_image_pull(map_region_class region1, map_area_class area1, bool import_settings)
        {
            area1.export_image_active = false;
            map.export_image_active = false;
            if (area1.export_image_import_settings && import_settings)
            {
                start_image_import_settings(area1);
            }
            if (map.file_tex2 != null)
            {
                map.file_tex2.Close();
            }
            if (map.file_tex3 != null)
            {
                map.file_tex3.Close();
            }
            if (!map.export_heightmap_active)
            {
                Application.runInBackground = false;
            }
            AssetDatabase.Refresh();
            check_export_image_active();
            check_export_heightmap_call();
        }

        public void start_image_pull_region(map_region_class region1)
        {
            for (int i = 0; i < region1.area.Count; i++)
            {
                if (!region1.area[i].export_image_active && !region1.area[i].export_image_call)
                {
                    start_image_pull(region1, region1.area[i]);
                }
            }
        }

        public void start_elevation_pull_region(map_region_class region1)
        {
            for (int i = 0; i < region1.area.Count; i++)
            {
                if (!region1.area[i].export_heightmap_active && !region1.area[i].export_heightmap_call)
                {
                    start_elevation_pull(region1, region1.area[i]);
                }
            }
        }

        public void start_elevation_pull(map_region_class region1, map_area_class area1)
        {
            if (area1.export_heightmap_active)
            {
                stop_elevation_pull(region1, area1);
            }
            else if (area1.export_heightmap_call)
            {
                area1.export_heightmap_call = false;
            }
            else if (map.export_heightmap_active || map.export_image_active)
            {
                area1.export_heightmap_call = true;
            }
            else
            {
                Application.runInBackground = true;
                map.export_heightmap.tiles.x = (int)Mathf.Ceil(area1.heightmap_resolution.x / 32);
                map.export_heightmap.tiles.y = (int)Mathf.Ceil(area1.heightmap_resolution.y / 32);
                if (map.export_heightmap.tiles.x != area1.heightmap_resolution.x / 32 || map.export_heightmap.tiles.y != area1.heightmap_resolution.y / 32)
                {
                    area1.export_heightmap_not_fit = true;
                    area1.export_heightmap_bres.x = area1.heightmap_resolution.x - Mathf.Floor(area1.heightmap_resolution.x / 32) * 32;
                    area1.export_heightmap_bres.y = area1.heightmap_resolution.y - Mathf.Floor(area1.heightmap_resolution.y / 32) * 32;
                    if (area1.export_heightmap_bres.x == 0)
                    {
                        area1.export_heightmap_bres.x = 32;
                    }
                    if (area1.export_heightmap_bres.y == 0)
                    {
                        area1.export_heightmap_bres.y = 32;
                    }
                }
                else
                {
                    area1.export_heightmap_not_fit = false;
                }
                create_elExt();
                map.export_heightmap.last_tile = false;
                map.export_heightmap.tile.reset();
                map.export_heightmap_zoom = area1.heightmap_zoom;
                map.export_heightmap_area.latlong1 = area1.upper_left;
                map.export_heightmap_area.latlong2 = area1.lower_right;
                if (bytes == null)
                {
                    bytes = new byte[2048];
                }
                else if (bytes.Length < 2048)
                {
                    bytes = new byte[2048];
                }
                open_stream(area1.export_heightmap_path, area1.export_heightmap_filename + ".Raw");
                map.export_heightmap_timeStart = Time.realtimeSinceStartup;
                map.export_heightmap_timePause = Time.realtimeSinceStartup;
                map.export_heightmap_timeEnd = Time.realtimeSinceStartup;
                map.export_heightmap_active = true;
                area1.export_heightmap_active = true;
                export_heightmap_region = region1;
                export_heightmap_area = area1;
                if (area1.normalizeHeightmap)
                {
                    string text = area1.export_heightmap_path + "/" + area1.export_heightmap_filename + "_N.Raw";
                    if (File.Exists(text))
                    {
                        FileUtil.DeleteFileOrDirectory(text);
                        FileUtil.DeleteFileOrDirectory(text + ".meta");
                        Debug.Log("Deleting the old normalized heightmap: " + text);
                    }
                }
                map.mode = 0;
            }
        }

        public void start_image_pull(map_region_class region1, map_area_class area1)
        {
            if (area1.export_image_active)
            {
                stop_image_pull(region1, area1, false);
            }
            else if (area1.export_image_call)
            {
                area1.export_image_call = false;
            }
            else if (map.export_image_active || map.export_heightmap_active)
            {
                area1.export_image_call = true;
            }
            else
            {
                Application.runInBackground = true;
                map.export_image.tiles = area1.tiles;
                map.export_image.subtiles.x = area1.resolution / 512;
                map.export_image.subtiles.y = area1.resolution / 512;
                map.export_image.subtiles_total = (int)Mathf.Pow(area1.resolution / 512, 2);
                create_texExt();
                map.export_image.tile.x = current_area.start_tile.x;
                map.export_image.tile.y = current_area.start_tile.y;
                map.tex2_tile.x = current_area.start_tile.x;
                map.tex2_tile.y = current_area.start_tile.y;
                if (current_area.start_tile.x == current_area.tiles.x && current_area.start_tile.y == current_area.tiles.y)
                {
                    map.export_image.last_tile = true;
                }
                else
                {
                    map.export_image.last_tile = false;
                }
                map.export_tex2 = false;
                map.export_tex3 = false;
                map.export_image.subtile.reset();
                map.tex3_tile.reset();
                jump_export_tex3_tile();
                map.export_image.subtile_total = 0;
                map.export_image.subtile2_total = 0;
                map.export_tex3 = false;
                map.export_image_area.latlong1 = area1.upper_left;
                map.export_image_area.latlong2 = area1.lower_right;
                export_image_region = region1;
                export_image_area = area1;
                if (!map.export_raw)
                {
                    if (!tex2)
                    {
                        tex2 = new Texture2D(area1.resolution, area1.resolution, TextureFormat.RGBA32, false, false);
                    }
                    else if (tex2.width != area1.resolution)
                    {
                        tex2.Reinitialize(area1.resolution, area1.resolution);
                        tex2.Apply();
                    }
                    if (!tex3)
                    {
                        tex3 = new Texture2D(area1.resolution, area1.resolution, TextureFormat.RGBA32, false, false);
                    }
                    else if (tex3.width != area1.resolution)
                    {
                        tex3.Reinitialize(area1.resolution, area1.resolution);
                        tex3.Apply();
                    }
                }
                else
                {
                    if (tex2)
                    {
                        // this.map.tex2.Resize(0, 0);
                        // this.map.tex2.Apply();
                    }
                    if (tex3)
                    {
                        // this.map.tex3.Resize(0, 0);
                        // this.map.tex3.Apply();
                    }
                    if (map.file_tex2 != null)
                    {
                        map.file_tex2.Close();
                    }
                    if (map.file_tex3 != null)
                    {
                        map.file_tex3.Close();
                    }
                }
                map.export_image_zoom = area1.image_zoom;
                map.export_image_timeStart = Time.realtimeSinceStartup;
                map.export_image_timeEnd = Time.realtimeSinceStartup;
                map.export_image_timePause = Time.realtimeSinceStartup;
                map.export_image_active = true;
                area1.export_image_active = true;
                map.mode = 0;
            }
        }

        public bool jump_export_heightmap_tile()
        {
            map.export_heightmap.tile.x++;

            if (map.export_heightmap.tile.x >= map.export_heightmap.tiles.x)
            {
                map.export_heightmap.tile.x = 0;
                map.export_heightmap.tile.y++;
                if (map.export_heightmap.tile.y >= map.export_heightmap.tiles.y)
                {
                    Repaint();
                    return true;
                }
            }
            return false;
        }

        public bool jump_export_image_tile()
        {
            map.export_image.subtile.x++;

            if (map.export_image.subtile.x >= map.export_image.subtiles.x)
            {
                map.export_image.subtile.x = 0;
                map.export_image.subtile.y++;

                if (map.export_image.subtile.y >= map.export_image.subtiles.y)
                {
                    map.export_image.subtile.y = 0;
                    map.export_image.tile.x++;

                    if (map.export_image.tile.x >= map.export_image.tiles.x)
                    {
                        map.export_image.tile.x = 0;
                        map.export_image.tile.y++;

                        if (map.export_image.tile.y >= map.export_image.tiles.y)
                        {
                            Repaint();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool jump_export_tex_tile()
        {
            map.tex2_tile.x++;

            if (map.tex2_tile.x >= map.export_image.tiles.x)
            {
                map.tex2_tile.x = 0;
                map.tex2_tile.y++;

                if (map.tex2_tile.y >= map.export_image.tiles.y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool jump_export_tex3_tile()
        {
            map.tex3_tile.x++;

            if (map.tex3_tile.x >= map.export_image.tiles.x)
            {
                map.tex3_tile.x = 0;
                map.tex3_tile.y++;

                if (map.tex3_tile.y >= map.export_image.tiles.y)
                {
                    return true;
                }
            }
            return false;
        }

        bool ElevationConvert(int elExt_index)
        {
            if (fs == null) { stop_elevation_pull_region(current_region); return false; }

            string text = map.elExt[elExt_index].pull.GetText();

            int index = text.IndexOf("elevations");
            string substring1 = text.Substring(index + 13);
            int index2 = substring1.IndexOf("]");

            substring1 = substring1.Substring(0, index2);

            String numberS = "";
            char charS;
            int number;
            int number_index = 0;
            int xx;
            int yy;
            int value_int;
            float height;
            byte byte_hi;
            byte byte_lo;
            int byteIndex = 0;
            int zerodata = 0;

            xx = map.elExt[elExt_index].tile.x * 32;
            yy = map.elExt[elExt_index].tile.y * 32;

            for (int i = 0; i < substring1.Length; ++i)
            {
                charS = substring1[i];

                if (charS != ',') numberS += charS;

                if (charS == ',' || i == substring1.Length - 1)
                {
                    number = Int16.Parse(numberS);

                    height = (number + 1000) * (65535.0f / 10000.0f);

                    if (number == 0)
                    {
                        ++zerodata;
                        if (zerodata > 80)
                        {
                            ++map.elExt[elExt_index].zero_error;

                            if (++map.elExt[elExt_index].zero_error < 3)
                            {
                                map.elExt[elExt_index].error = 1;
                                map.elExt[elExt_index].pull.Request(map.elExt[elExt_index].url, false);
                                ++map.bingKey[map.bingKey_selected].pulls;
                                return false;
                            }
                        }
                    }

                    value_int = (int)height;

                    byte_hi = (byte)(value_int >> 8);
                    byte_lo = (byte)(value_int - (byte_hi << 8));

                    bytes[byteIndex++] = byte_lo;
                    bytes[byteIndex++] = byte_hi;

                    // Debug.Log(number);
                    numberS = String.Empty;
                    ++number_index;
                }

            }

            map.elExt[elExt_index].error = 0;

            for (int yb = 0; yb < map.elExt[elExt_index].bres.y; ++yb)
            {
                // fs.Position = ((yy+((map.elExt[elExt_index].bres.y-1)-yb))*(yy*export_heightmap_area.heightmap_resolution.x*2))+(xx*2);
                fs.Position = (int)((yy * export_heightmap_area.heightmap_resolution.x * 2) + (xx * 2) + (((map.elExt[elExt_index].bres.y - 1) - yb) * (export_heightmap_area.heightmap_resolution.x * 2)));
                fs.Write(bytes, (int)(yb * map.elExt[elExt_index].bres.x * 2), (int)(map.elExt[elExt_index].bres.x * 2));
            }

            return true;
        }

        bool ElevationConvertZero(int elExt_index)
        {
            if (fs == null) { stop_elevation_pull_region(current_region); return false; }

            int xx;
            int yy;
            int value_int;
            float height;
            byte byte_hi;
            byte byte_lo;
            int byteIndex = 0;

            xx = map.elExt[elExt_index].tile.x * 32;
            yy = map.elExt[elExt_index].tile.y * 32;

            for (int i = 0; i < 1024; ++i)
            {
                height = 1000 * (65535.0f / 10000.0f);

                value_int = (int)height;

                byte_hi = (byte)(value_int >> 8);
                byte_lo = (byte)(value_int - (byte_hi << 8));

                bytes[byteIndex++] = byte_lo;
                bytes[byteIndex++] = byte_hi;
            }

            map.elExt[elExt_index].error = 0;

            for (int yb = 0; yb < map.elExt[elExt_index].bres.y; ++yb)
            {
                fs.Position = (int)((yy * export_heightmap_area.heightmap_resolution.x * 2) + (xx * 2) + (((map.elExt[elExt_index].bres.y - 1) - yb) * (export_heightmap_area.heightmap_resolution.x * 2)));
                fs.Write(bytes, (int)(yb * map.elExt[elExt_index].bres.x * 2), (int)(map.elExt[elExt_index].bres.x * 2));
            }
            // Debug.Log("numbers in string: "+number_index);
            // Debug.Log(map.elExt[elExt_index].bres.x+","+map.elExt[elExt_index].bres.y);
            return true;
        }

        public void texConvert(ext_class texExt)
        {
            tile_class tile_class = new tile_class();

            if ((texExt.tile.x == map.tex2_tile.x && texExt.tile.y == map.tex2_tile.y) || export_image_area.resolution == 512)
            {
                Texture2D tex = texExt.pull.GetTexture();
                pixels = tex.GetPixels(0, 32, 512, 512);
                Draw.DisposeTexture(ref tex);

                // Debug.Log("TexConvertA " + texExt.index + " " + texExt.url);

                tile_class.x = texExt.tile.x * map.export_image.subtiles.x + texExt.subtile.x;
                tile_class.y = texExt.tile.y * map.export_image.subtiles.y + texExt.subtile.y;

                bool imageError = CheckImageError();

                texExt.error = imageError ? 1 : 0;

                if (imageError)
                {
                    texExt.zero_error = texExt.zero_error + 1;
                    if (texExt.zero_error < 5)
                    {
                        texExt.pull.Request(texExt.url, true);
                        texExt.startTime = Time.realtimeSinceStartup;
                        map.bingKey[map.bingKey_selected].pulls += 1;
                        return;
                    }
                }

                texExt.loaded = true;

                if (!map.export_raw)
                {
                    tex2.SetPixels(texExt.subtile.x * 512, (map.export_image.subtiles.y - 1 - texExt.subtile.y) * 512, 512, 512, pixels);
                    tex2.Apply();
                }
                else
                {
                    if (!map.export_tex2)
                    {
                        if (map.export_raw)
                        {
                            create_raw_files(export_image_area.export_image_path, texExt, 2);
                        }
                        map.export_tex2 = true;
                    }
                    if (!ExportTextureToRaw(map.file_tex2, new Vector2(texExt.subtile.x * 512, (map.export_image.subtiles.y - 1 - texExt.subtile.y) * 512)))
                    {
                        return;
                    }
                }

                map.export_image.subtile_total++;

                // Check end of tile
                if (map.export_image.subtile_total == map.export_image.subtiles_total || export_image_area.resolution == 512)
                {
                    // Debug.LogError(map.export_image.subtile_total);
                    string text = null;
                    if (!map.export_raw)
                    {
                        if (export_image_area.resolution == 512)
                        {
                            text = export_image_area.export_image_filename + "_x" + texExt.tile.x + "_y" + (map.export_image.tiles.y - 1 - texExt.tile.y);
                        }
                        else
                        {
                            text = export_image_area.export_image_filename + "_x" + map.tex2_tile.x + "_y" + (map.export_image.tiles.y - 1 - map.tex2_tile.y);
                        }
                    }
                    if (export_image_area.preimage_export_active && !export_image_area.preimage_save_new)
                    {
                        map.preimage_edit.y1 = 0;
                        map.preimage_edit.x1 = 0;
                        //! map.preimage_edit.convert_texture(map.tex2, map.tex2, map.tex2.width, map.tex2.height, false);
                    }
                    if (export_image_area.export_image_world_file)
                    {
                        latlong_area_class latlong_area_class = Mathw.calc_latlong_area_by_tile2(map.export_image_area.latlong1, texExt.tile, map.export_image_zoom, export_image_area.resolution, new Vector2(export_image_area.resolution, export_image_area.resolution));
                        double num = (latlong_area_class.latlong2.longitude - latlong_area_class.latlong1.longitude) / export_image_area.resolution;
                        double num2 = (latlong_area_class.latlong2.latitude - latlong_area_class.latlong1.latitude) / export_image_area.resolution;
                        StreamWriter streamWriter = new StreamWriter(export_image_area.export_image_path + "/" + text + ".jgw");
                        streamWriter.WriteLine(num.ToString());
                        streamWriter.WriteLine("0");
                        streamWriter.WriteLine("0");
                        streamWriter.WriteLine(num2.ToString());
                        streamWriter.WriteLine(latlong_area_class.latlong1.longitude);
                        streamWriter.WriteLine(latlong_area_class.latlong1.latitude);
                        streamWriter.Close();
                    }
                    jump_export_tex_tile();
                    jump_export_tex3_tile();
                    if (map.track_tile)
                    {
                        export_image_area.start_tile.x = map.tex2_tile.x;
                        export_image_area.start_tile.y = map.tex2_tile.y;
                    }
                    if (map.export_jpg)
                    {
                        export_texture_as_jpg(export_image_area.export_image_path + "/" + text + ".jpg", tex2, map.export_jpg_quality);
                    }
                    if (map.export_png)
                    {
                        export_texture_to_file(export_image_area.export_image_path, text, tex2);
                    }
                    if (export_image_area.image_stop_one)
                    {
                        stop_image_pull(export_image_region, export_image_area, true);
                    }
                    if (map.export_tex3)
                    {
                        map.export_image.subtile_total = map.export_image.subtile2_total;
                        map.export_image.subtile2_total = 0;
                        map.export_tex3 = false;

                        if (!map.export_raw)
                        {
                            Mathw.Swap(ref tex2, ref tex3);
                        }
                        else
                        {
                            map.file_tex2.Close();
                            FileStream file_tex = map.file_tex2;
                            map.file_tex2 = map.file_tex3;
                            map.file_tex3 = file_tex;
                        }
                        map.tex_swapped = true;
                    }
                    else
                    {
                        if (map.export_image.last_tile && check_image_pulls_done())
                        {
                            Debug.Log("Exporting image Region: " + export_image_region.name + " -> Area: " + export_image_area.name + " done.");
                            if (map.track_tile)
                            {
                                export_image_area.start_tile.x = 0;
                                export_image_area.start_tile.y = 0;
                            }
                            stop_image_pull(export_image_region, export_image_area, true);
                        }
                        map.export_image.subtile_total = 0;
                        if (map.export_raw)
                        {
                            map.file_tex2.Close();
                        }
                        map.export_tex2 = false;
                    }
                }
            }
            else if (texExt.tile.x == map.tex3_tile.x && texExt.tile.y == map.tex3_tile.y)
            {
                if (map.export_image.subtile2_total < map.export_image.subtiles_total - 1)
                {
                    if (!map.export_tex3 && map.export_raw)
                    {
                        create_raw_files(export_image_area.export_image_path, texExt, 1);
                    }

                    Texture2D tex = texExt.pull.GetTexture();
                    pixels = tex.GetPixels(0, 32, 512, 512);
                    Draw.DisposeTexture(ref tex);

                    // Debug.Log("TexConvertB " + texExt.index + " " + texExt.url);

                    tile_class.x = texExt.tile.x * map.export_image.subtiles.x + texExt.subtile.x;
                    tile_class.y = texExt.tile.y * map.export_image.subtiles.y + texExt.subtile.y;

                    bool imageError = CheckImageError();
                    texExt.error = imageError ? 1 : 0;

                    if (imageError)
                    {
                        texExt.zero_error = texExt.zero_error + 1;
                        if ((texExt.zero_error = texExt.zero_error + 1) < 5)
                        {
                            texExt.pull.Request(texExt.url, true);
                            texExt.startTime = Time.realtimeSinceStartup;
                            map.bingKey[map.bingKey_selected].pulls += 1;
                            return;
                        }
                    }

                    texExt.loaded = true;

                    if (!map.export_raw)
                    {
                        tex3.SetPixels(texExt.subtile.x * 512, (map.export_image.subtiles.y - 1 - texExt.subtile.y) * 512, 512, 512, pixels);
                        tex3.Apply();
                    }
                    else if (!ExportTextureToRaw(map.file_tex3, new Vector2(texExt.subtile.x * 512, (map.export_image.subtiles.y - 1 - texExt.subtile.y) * 512)))
                    {
                        return;
                    }

                    map.export_image.subtile2_total++;
                    map.export_tex3 = true;
                }
            }
        }

        public bool CheckImageError()
        {
            int num = 0;
            int i = 0;
            int j = 0;
            Color errorColor = map.errorColor;

            for (i = 0; i < 480; i += 4)
            {
                for (j = 0; j < 512; j += 4)
                {
                    if (pixels[i * 512 + j].r == errorColor.r && pixels[i * 512 + j].g == errorColor.g && pixels[i * 512 + j].b == errorColor.b)
                    {
                        if (num++ == 5)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        public void create_elExt()
        {
            List<ext_class> elExt = map.elExt;
            elExt.Clear();

            for (int i = 0; i < map.export_elExt; i++)
            {
                elExt.Add(new ext_class(i) { startTime = 0, loaded = true });
            }
        }

        public void create_texExt()
        {
            List<ext_class> texExt = map.texExt;
            texExt.Clear();

            for (int i = 0; i < map.export_texExt; i++)
            {
                texExt.Add(new ext_class(i) { startTime = 0, loaded = true });
            }
        }

        public void open_stream(string path, string fileName)
        {
            fs = new FileStream(path + "/" + fileName, FileMode.OpenOrCreate);
        }

        public void calc_heightmap_settings(map_area_class area)
        {
            area.heightmap_scale = Mathw.calc_latlong_area_resolution(area.center, area.heightmap_zoom);
            area.heightmap_resolution.x = Mathf.Round((float)(area.size.x / area.heightmap_scale));
            area.heightmap_resolution.y = Mathf.Round((float)(area.size.y / area.heightmap_scale));
        }

        public void calc_terrain_heightmap_resolution()
        {
            current_area.terrain_heightmap_resolution = (int)(current_area.heightmap_resolution.x / current_area.tiles.x);
            if (current_area.terrain_heightmap_resolution < 33)
            {
                current_area.terrain_heightmap_resolution = 33;
                current_area.terrain_heightmap_resolution_select = 0;
            }
            else if (current_area.terrain_heightmap_resolution < 65)
            {
                current_area.terrain_heightmap_resolution = 65;
                current_area.terrain_heightmap_resolution_select = 1;
            }
            else if (current_area.terrain_heightmap_resolution < 129)
            {
                current_area.terrain_heightmap_resolution = 129;
                current_area.terrain_heightmap_resolution_select = 2;
            }
            else if (current_area.terrain_heightmap_resolution < 257)
            {
                current_area.terrain_heightmap_resolution = 257;
                current_area.terrain_heightmap_resolution_select = 3;
            }
            else if (current_area.terrain_heightmap_resolution < 513)
            {
                current_area.terrain_heightmap_resolution = 513;
                current_area.terrain_heightmap_resolution_select = 4;
            }
            else if (current_area.terrain_heightmap_resolution < 1025)
            {
                current_area.terrain_heightmap_resolution = 1025;
                current_area.terrain_heightmap_resolution_select = 5;
            }
            else if (current_area.terrain_heightmap_resolution < 2049)
            {
                current_area.terrain_heightmap_resolution = 2049;
                current_area.terrain_heightmap_resolution_select = 6;
            }
            else if (current_area.terrain_heightmap_resolution < 4097)
            {
                current_area.terrain_heightmap_resolution = 4097;
                current_area.terrain_heightmap_resolution_select = 7;
            }
        }

        public string calc_24_hours()
        {
            int num = 0;
            int num2 = DateTime.Now.Day - map.bingKey[map.bingKey_selected].pulls_startDay;
            if (num2 > 0)
            {
                num = num2 * 24 - map.bingKey[map.bingKey_selected].pulls_startHour + DateTime.Now.Hour;
            }
            else
            {
                num += DateTime.Now.Hour - map.bingKey[map.bingKey_selected].pulls_startHour;
            }
            int num3;
            if (map.bingKey[map.bingKey_selected].pulls_startMinute > DateTime.Now.Minute)
            {
                num--;
                num3 = 60 - (map.bingKey[map.bingKey_selected].pulls_startMinute - DateTime.Now.Minute);
            }
            else
            {
                num3 = DateTime.Now.Minute - map.bingKey[map.bingKey_selected].pulls_startMinute;
            }
            num = 23 - num;
            num3 = 60 - num3;
            if (num3 == 60)
            {
                num++;
                num3 = 0;
            }
            if (num < 0)
            {
                map.bingKey[map.bingKey_selected].reset();
            }
            return num.ToString() + ":" + num3.ToString("D2");
        }

        public void export_texture_as_jpg(string file, Texture2D texture, int quality)
        {
            JPGEncoder_class jPGEncoder_class = new JPGEncoder_class(texture, quality);
            while (!jPGEncoder_class.isDone)
            {
            }
            File.WriteAllBytes(file, jPGEncoder_class.GetBytes());
        }

        public void export_texture_to_file(string path, string file, Texture2D export_texture)
        {
            byte[] array = export_texture.EncodeToPNG();
            File.WriteAllBytes(path + "/" + file + ".png", array);
        }

        public void create_raw_files(string path, ext_class texExt, int mode)
        {
            if (export_image_area.resolution == 512)
            {
                map.file_tex2 = new FileStream(path + "/" + export_image_area.export_image_filename + "_x" + texExt.tile.x + "_y" + texExt.tile.y + ".raw", FileMode.OpenOrCreate);
            }
            else
            {
                if (mode == 0 || mode == 2)
                {
                    map.file_tex2 = new FileStream(path + "/" + export_image_area.export_image_filename + "_x" + map.tex2_tile.x + "_y" + map.tex2_tile.y + ".raw", FileMode.OpenOrCreate);
                }
                if (mode == 0 || mode == 1)
                {
                    map.file_tex3 = new FileStream(path + "/" + export_image_area.export_image_filename + "_x" + map.tex3_tile.x + "_y" + map.tex3_tile.y + ".raw", FileMode.OpenOrCreate);
                }
            }
        }

        public bool ExportTextureToRaw(FileStream texFile, Vector2 offset)
        {
            int i = 0;
            int j = 0;
            int num = export_image_area.resolution * 3;
            byte[] array = new byte[512 * 3];

            if (texFile == null)
            {
                Debug.Log("Image exporting is interupted, please start again.");
                StopAllImagePull();
                return false;
            }

            texFile.Position = (long)(num * (export_image_area.resolution - 512 - offset.y) + offset.x * 3);
            for (i = 511; i >= 0; i--)
            {
                for (j = 0; j < 512; j++)
                {
                    array[j * 3] = (byte)(pixels[i * 512 + j].r * 255);
                    array[j * 3 + 1] = (byte)(pixels[i * 512 + j].g * 255);
                    array[j * 3 + 2] = (byte)(pixels[i * 512 + j].b * 255);
                }
                texFile.Write(array, 0, 512 * 3);
                texFile.Seek(num - (512 * 3), SeekOrigin.Current);
            }

            return true;
        }

        public void combine_textures_begin(map_area_class area1, string path, string file)
        {
            if (combine_export_file != null)
            {
                combine_export_file.Close();
            }
            if (combine_import_file != null)
            {
                combine_import_file.Close();
            }
            combine_area = area1;
            combine_width = (ulong)(area1.resolution * 3 * area1.tiles.x);
            combine_height = (ulong)((long)combine_width * area1.resolution);
            combine_length = (ulong)(area1.resolution * area1.resolution * 3 * (area1.tiles.x * area1.tiles.y));
            combine_import_path = area1.export_image_path + "/";
            if (combine_export_file != null)
            {
                combine_export_file.Close();
            }
            combine_export_file = new FileStream(path + "/" + file + "_combined.raw", FileMode.OpenOrCreate);
            combine_export_file.SetLength((long)combine_length);
            combine_call = false;
            combine_byte = new byte[area1.resolution * 3];
            combine_y = 0;
            combine_x = 0;
            combine_y1 = 0;
            combine_generate = true;
            Application.runInBackground = true;
        }

        public void combine_textures()
        {
            int i = 0;
            int j = 0;
            int k = 0;
            combine_time = Time.realtimeSinceStartup;
            for (i = combine_y; i < combine_area.tiles.y; i++)
            {
                for (j = combine_x; j < combine_area.tiles.x; j++)
                {
                    if (!combine_call)
                    {
                        if (combine_import_file != null)
                        {
                            combine_import_file.Close();
                        }
                        combine_import_filename = combine_area.export_image_filename + "_x" + j + "_y" + i;
                        if (!File.Exists(combine_import_path + combine_import_filename + ".raw"))
                        {
                            Debug.Log(combine_import_path + combine_import_filename + ".raw" + " does not exist");
                            combine_generate = false;
                            combine_export_file.Close();
                            return;
                        }
                        combine_import_file = new FileStream(combine_import_path + combine_import_filename + ".raw", FileMode.Open);
                        combine_export_file.Seek((long)combine_height * i + combine_area.resolution * 3 * j, SeekOrigin.Begin);
                    }
                    for (k = combine_y1; k < combine_area.resolution; k++)
                    {
                        combine_import_file.Read(combine_byte, 0, combine_area.resolution * 3);
                        combine_export_file.Write(combine_byte, 0, combine_area.resolution * 3);
                        combine_export_file.Seek((long)combine_width - combine_area.resolution * 3, SeekOrigin.Current);
                        if (Time.realtimeSinceStartup - combine_time > 1f / global_script.target_frame)
                        {
                            combine_call = true;
                            combine_y1 = k + 1;
                            combine_y = i;
                            combine_x = j;
                            return;
                        }
                    }
                    combine_call = false;
                    combine_y1 = 0;
                }
                combine_x = 0;
            }
            combine_export_file.Close();
            combine_generate = false;
        }

        public void slice_textures_begin(map_area_class area1, string path, string file)
        {
            combine_area = area1;
            combine_export_path = area1.export_image_path + "/";
            combine_byte = new byte[area1.resolution * 3];
            combine_width = (ulong)(area1.resolution * 3 * area1.tiles.x);
            combine_height = (ulong)((long)combine_width * area1.resolution);
            combine_length = (ulong)(area1.resolution * area1.resolution * 3 * (area1.tiles.x * area1.tiles.y));
            if (!current_area.preimage_save_new)
            {
                combine_import_file = new FileStream(path + "/" + file + "_combined2.raw", FileMode.Open);
            }
            else
            {
                combine_import_file = new FileStream(current_area.preimage_path + "/" + current_area.preimage_filename + ".raw", FileMode.Open);
            }
            combine_pixels = new Color[area1.resolution];
            if (!tex2)
            {
                tex2 = new Texture2D(area1.resolution, area1.resolution, TextureFormat.RGBA32, false, false);
            }
            else if (tex2.width != area1.resolution)
            {
                tex2.Reinitialize(area1.resolution, area1.resolution);
                tex2.Apply();
            }
            combine_call = false;
            combine_y = 0;
            combine_x = 0;
            combine_y1 = 0;
            slice_generate = true;
            Application.runInBackground = true;
        }

        public void slice_textures()
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            combine_time = Time.realtimeSinceStartup;
            for (i = combine_y; i < combine_area.tiles.y; i++)
            {
                for (j = combine_x; j < combine_area.tiles.x; j++)
                {
                    if (!combine_call)
                    {
                        combine_export_filename = combine_area.export_image_filename + "_x" + j + "_y" + (combine_area.tiles.y - 1 - i);
                        combine_import_file.Seek((long)combine_height * i + combine_area.resolution * 3 * j, SeekOrigin.Begin);
                    }
                    for (k = combine_y1; k < combine_area.resolution; k++)
                    {
                        combine_import_file.Read(combine_byte, 0, combine_area.resolution * 3);
                        for (l = 0; l < combine_area.resolution; l++)
                        {
                            combine_pixels[l] = new Color(combine_byte[l * 3] * 1f / 255, combine_byte[l * 3 + 1] * 1f / 255, combine_byte[l * 3 + 2] * 1f / 255);
                        }
                        tex2.SetPixels(0, combine_area.resolution - k - 1, combine_area.resolution, 1, combine_pixels);
                        combine_import_file.Seek((long)combine_width - combine_area.resolution * 3, SeekOrigin.Current);
                        if (Time.realtimeSinceStartup - combine_time > 1f / global_script.target_frame)
                        {
                            combine_call = true;
                            combine_y1 = k + 1;
                            combine_y = i;
                            combine_x = j;
                            return;
                        }
                    }
                    combine_y1 = 0;
                    combine_call = false;
                    export_texture_as_jpg(combine_export_path + combine_export_filename + ".jpg", tex2, map.export_jpg_quality);
                }
                combine_x = 0;
            }
            combine_import_file.Close();
            slice_generate = false;
            AssetDatabase.Refresh();
        }

        public void keyHelp()
        {
            if (map.key_edit)
            {
                gui_y2 = (int)(gui_y2 + (80 + map.bingKey.Count * 19.9f));
            }
            GUI.color = map.backgroundColor;
            help_rect = new Rect(359, 62 + gui_y2, 450, 100);
            EditorGUI.DrawPreviewTexture(help_rect, Draw.tex2);
            GUI.color = Color.red;
            EditorGUI.LabelField(new Rect(guiWidth3, 65 + gui_y2, 200, 20), "Refresh Map", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(548, 65 + gui_y2, 250, 20), "Keyboard F5", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(guiWidth3, 85 + gui_y2, 200, 20), "Navigate Around", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(548, 85 + gui_y2, 250, 20), "Hold left mouse button and drag", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(guiWidth3, 104 + gui_y2, 200, 20), "Goto position", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(548, 104 + gui_y2, 250, 20), "Double click left mouse button", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(guiWidth3, 123 + gui_y2, 200, 20), "Zoom", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(548, 123 + gui_y2, 200, 20), "Mouse scroll wheel", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(guiWidth3, 142 + gui_y2, 200, 20), "Elevation Info", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(548, 142 + gui_y2, 200, 20), "Right mouse button", EditorStyles.boldLabel);
            GUI.color = Color.white;
        }

        public void texture_fill(Texture2D texture, Color color, bool apply)
        {
            int width = texture.width;
            int height = texture.height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    texture.SetPixel(j, i, color);
                }
            }
            if (apply)
            {
                texture.Apply();
            }
        }

        public string sec_to_timeMin(float seconds, bool display_seconds)
        {
            int num = (int)(seconds / 60);
            seconds -= num * 60;
            string arg_B2_0;
            if (num == 0)
            {
                arg_B2_0 = seconds.ToString("F2");
            }
            else
            {
                int num2 = (int)seconds;
                seconds -= num2;
                int num3 = (int)(seconds * 100);
                arg_B2_0 = ((!display_seconds) ? (num.ToString() + ":" + num2.ToString("D2")) : (num.ToString() + ":" + num2.ToString("D2") + "." + num3.ToString("D2")));
            }
            return arg_B2_0;
        }

        public void check_free_key()
        {
            for (int i = 0; i < map.bingKey.Count; i++)
            {
                if (map.bingKey[i].pulls < 48000)
                {
                    map.bingKey_selected = i;
                    break;
                }
            }
        }

        public GameObject create_worldcomposer_parent()
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "_WorldComposer";
            gameObject.transform.position = new Vector3(0, 0, 0);
            return gameObject;
        }

        public void parent_terraincomposer_children()
        {
            GameObject gameObject = GameObject.Find("global_settings");
            if (gameObject)
            {
                if (!terraincomposer)
                {
                    GameObject gameObject2 = GameObject.Find("_WorldComposer");
                    if (!gameObject2)
                    {
                        gameObject2 = create_worldcomposer_parent();
                    }
                    gameObject.transform.parent = gameObject2.transform;
                }
                else
                {
                    GameObject gameObject2 = GameObject.Find("_TerrainComposer");
                    gameObject.transform.parent = gameObject2.transform;
                    GameObject gameObject3 = GameObject.Find("_WorldComposer");
                    if (gameObject3)
                    {
                        GameObject.DestroyImmediate(gameObject3);
                    }
                }
            }
        }

        public void load_global_settings()
        {
            string text = install_path + "/Templates/GlobalSettings.asset";

            if (!File.Exists(GetFullInstallPath() + "/Templates/GlobalSettings.asset"))
            {
                FileUtil.CopyFileOrDirectory(install_path + "/Templates/GlobalSettings2.asset", install_path + "/Templates/GlobalSettings.asset");
                AssetDatabase.Refresh();
            }
            else
            {
                global_script = (global_settings_tc)AssetDatabase.LoadAssetAtPath(text, typeof(global_settings_tc));
                map = global_script.map;
                Repaint();
            }
        }

        public void add_terrainArea(int index)
        {
            terrain_region.add_area(index);
            add_terrain(terrain_region.area[index], 0, 0, terrain_region.area[0]);
            auto_search(terrain_region.area[index].auto_search);
            auto_search(terrain_region.area[index].auto_name);
        }

        public void create_terrains_area()
        {
            if (global_script.settings.disableLightmapping)
            {
#if !UNITY_5
                Lightmapping.bakedGI = false;
                Lightmapping.realtimeGI = false;
#endif
            }

            if (terraincomposer && current_area.export_to_terraincomposer)
            {
                EditorWindow window = GetWindow(Type.GetType("TerrainComposer"));
                window.titleContent = new GUIContent("TerrainComp.");
            }
            Application.runInBackground = true;
            create_region = current_region;
            create_area = current_area;
            create_area.terrain_done = false;
            terrain_region.area[0].terrains.Clear();
            terrain_parent = new GameObject();
            terrain_parent.name = create_region.name;
            if (terrain_region.area.Count == 0)
            {
                add_terrainArea(0);
            }
            if (terrain_region.area[0].terrains.Count == 0)
            {
                add_terrain(terrain_region.area[0], 0, 0, terrain_region.area[0]);
            }
            terrain_region.area[0].tiles_select.x = create_area.tiles.x;
            terrain_region.area[0].tiles_select.y = create_area.tiles.y;
            terrain_region.area[0].tiles_select_total = create_area.tiles.x * create_area.tiles.y;
            if (terrain_region.area[0].tiles_select_total == 0)
            {
                notify_text = "The area is not created. Use the 'Pick' button to create an area";
                GameObject.DestroyImmediate(terrain_parent);
            }
            else
            {
                if (!Directory.Exists(current_area.export_terrain_path))
                {
                    string text = current_area.export_terrain_path.Replace(Application.dataPath, "Assets");
                    int num = text.LastIndexOf("/");
                    string text2 = text.Substring(num + 1);
                    text = text.Replace("/" + text2, string.Empty);
                    AssetDatabase.CreateFolder(text, text2);
                }
                if (current_area.normalizeHeightmap)
                {
                    string text3 = current_area.export_heightmap_path + "/" + current_area.export_heightmap_filename;
                    if (!File.Exists(text3 + "_N.Raw"))
                    {
                        current_area.normalizedHeight = NormalizeHeightmap(current_area.heightmap_resolution, text3 + ".Raw");
                    }
                }
                terrain_region.area[0].auto_name.format = "_x%x_y%y";
                terrain_size.x = Mathf.Round((float)(create_area.size.x / create_area.tiles.x));
                if (create_area.normalizeHeightmap)
                {
                    terrain_size.y = create_area.normalizedHeight;
                }
                else
                {
                    terrain_size.y = 10000;
                }
                terrain_size.z = Mathf.Round((float)(create_area.size.y / create_area.tiles.y));
                create_terrain_count = 0;
                create_terrain_loop = true;
                generate_begin();
            }
        }

        public void create_terrain(terrain_area_class terrainArea1, terrain_class2 preterrain, string terrain_path, Transform terrain_parent)
        {
            int num = 0;
            int i = 0;
            int num3 = 0;
            int num4 = terrainArea1.tiles_select_total - num3;
            for (i = create_terrain_count; i < num4; i++)
            {
                if (terrainArea1.terrains.Count <= i || !terrainArea1.terrains[i].terrain)
                {
                    GameObject gameObject = new GameObject();
                    Terrain terrain = (Terrain)gameObject.AddComponent(typeof(Terrain));
                    TerrainCollider terrainCollider = (TerrainCollider)gameObject.AddComponent(typeof(TerrainCollider));
                    num = i + num3;
                    tile_class tile_class = calc_terrain_tile(num, terrainArea1.tiles_select);
                    string text = terrainArea1.auto_name.get_name(tile_class.x, tile_class.y, num);
                    string text2 = create_area.export_terrain_path;
                    text2 = "Assets" + text2.Replace(Application.dataPath, string.Empty);
                    text2 += "/" + create_area.terrain_scene_name + text + ".asset";
                    terrain.terrainData = new TerrainData();
                    gameObject.AddComponent(typeof(TerrainDetail));
                    terrain.terrainData.heightmapResolution = heightmap_resolution;
                    terrain.terrainData.baseMapResolution = preterrain.basemap_resolution;
                    terrain.terrainData.alphamapResolution = preterrain.splatmap_resolution;
                    terrain.terrainData.SetDetailResolution(preterrain.detail_resolution, preterrain.detail_resolution_per_patch);
                    terrain.terrainData.size = terrain_size * create_area.terrain_scale;

#if !UNITY_5 && !UNITY_2017 && !UNITY_2018 && !UNITY_2019_1
                    Shader terrainShader = Shader.Find("HDRP/TerrainLit");
                    if (terrainShader == null) terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                    if (terrainShader == null) terrainShader = Shader.Find("Nature/Terrain/Standard");

                    if (global_script.matTerrain == null)
                    {
                        global_script.matTerrain = new Material(terrainShader);
                    }
                    global_script.matTerrain.shader = terrainShader;
                    terrain.materialTemplate = global_script.matTerrain;
#endif

                    gameObject.isStatic = true;
                    if (terrain_parent)
                    {
                        gameObject.transform.parent = terrain_parent;
                    }
                    terrain.name = create_area.terrain_scene_name + text;
                    AssetDatabase.CreateAsset(terrain.terrainData, text2);
                    terrainCollider.terrainData = terrain.terrainData;

                    if (terrainArea1.terrains.Count - 1 < i + num3)
                    {
                        if (terrainArea1.copy_settings)
                        {
                            // Debug.Log("Copy!!!"+terrain_index);
                            if (num > 0) { add_terrain(terrainArea1, terrainArea1.terrains.Count, terrainArea1.copy_terrain, terrainArea1); } else { add_terrain(terrainArea1, terrainArea1.terrains.Count, -1, terrainArea1); }
                        }
                        else
                        {
                            add_terrain(terrainArea1, terrainArea1.terrains.Count, -1, terrainArea1);
                        }
                    }

                    gameObject.transform.position = new Vector3(-preterrain.size.x / 2, -1000, -preterrain.size.z / 2);
                    terrainArea1.terrains[num].terrain = terrain;
                    terrain_parameters(terrainArea1.terrains[num]);
                    terrainArea1.terrains[num].tile_x = tile_class.x;
                    terrainArea1.terrains[num].tile_z = tile_class.y;
                    terrainArea1.terrains[num].prearea.max();
                    terrainArea1.terrains[num].foldout = false;
                    terrainArea1.terrains[num].terrain.heightmapPixelError = 5;
                    fit_terrain_tile(terrain_region.area[0], terrain_region.area[0].terrains[create_terrain_count], true);
                    if (create_area.do_image)
                    {
                        assign_terrain_splat_alpha(terrainArea1.terrains[num], true);
                        SetTerrainAreaSplatTextures(terrain_region.area[0], num);
                    }
                    create_area.terrain_done = true;
                    if (create_area.do_heightmap && (!terraincomposer || !create_area.export_to_terraincomposer))
                    {
                        heightmap_y = heightmap_resolution - 1;
                        generate = true;
                    }
                    create_terrain_count++;

#if !UNITY_5 && !UNITY_2017 && !UNITY_2018 && !UNITY_2019_1
                    SetAlphaMap(terrain.terrainData);
#endif
                    return;
                }
            }
            terrainArea1.tiles.x = terrainArea1.tiles_select.x;
            terrainArea1.tiles.y = terrainArea1.tiles.y - 1 - terrainArea1.tiles_select.y;
            terrainArea1.tiles_total = terrainArea1.tiles_select_total;
            terrainArea1.size.x = terrainArea1.terrains[0].size.x * terrainArea1.tiles.x;
            terrainArea1.size.z = terrainArea1.terrains[0].size.x * terrainArea1.tiles.y;
            terrainArea1.size.y = terrainArea1.terrains[0].size.y;
            AssetDatabase.Refresh();
            terrains_neighbor(terrainArea1);
            terrains_neighbor_script(terrainArea1, 1);
            create_terrain_loop = false;
            if (create_area.do_heightmap && (!create_area.export_to_terraincomposer || !terraincomposer))
            {
                heights = new float[0, 0];
                generate = false;
            }
            if (!map.export_heightmap_active && !map.export_image_active) { Application.runInBackground = false; }
            create_splat_count = 0;
            if (create_area.do_heightmap && (!create_area.export_to_terraincomposer || !terraincomposer))
            {
                heights = (float[,])Array.CreateInstance(typeof(float), new int[2]);
                generate = false;
            }
            if (create_area.auto_import_settings_apply)
            {
                start_image_import_settings(create_area);
                return;
            }
        }

        void SetAlphaMap(TerrainData terrainData)
        {
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;

            float[,,] map = new float[width, height, 1];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y, 0] = 1;
                }
            }

            terrainData.SetAlphamaps(0, 0, map);
        }

        public void start_image_import_settings(map_area_class area1)
        {
            area1.maxTextureSize = (int)Mathf.Pow(2, area1.maxTextureSize_select + 5);
            create_area = area1;
            terrain_region.area[0].tiles_select.x = create_area.tiles.x;
            terrain_region.area[0].tiles_select.y = create_area.tiles.y;
            import_settings_count = 0;
            apply_import_settings = true;
        }

        public void clear_terrains()
        {
            for (int i = 0; i < terrain_region.area[0].terrains.Count; i++)
            {
                terrain_region.area[0].terrains[i].terrain = null;
            }
            terrain_region.area[0].terrains.Clear();
        }

        public void SetTerrainAreaSplatTextures(terrain_area_class terrainArea1, int terrainIndex)
        {
            string text = null;
            tile_class tile_class = calc_terrain_tile(terrainIndex, terrainArea1.tiles_select);
            if (map.export_jpg)
            {
                text = create_area.export_image_path.Replace(Application.dataPath, "Assets") + "/" + create_area.export_image_filename + "_x" + tile_class.x.ToString() + "_y" + tile_class.y.ToString() + ".jpg";
            }
            else if (map.export_png)
            {
                text = create_area.export_image_path.Replace(Application.dataPath, "Assets") + "/" + create_area.export_image_filename + "_x" + tile_class.x.ToString() + "_y" + tile_class.y.ToString() + ".png";
            }
            if (!File.Exists(text))
            {
                notify_text = text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles";
                Debug.Log(text + " doesn't exist! Make sure the image tiles are the same as the exported image tiles.");
            }
            else
            {
                Type type = TC.GetType(typeof(MonoBehaviour), "ReliefTerrain");
                if (type == null)
                {
                    terrainArea1.terrains[terrainIndex].add_splatprototype(0);
                    terrainArea1.terrains[terrainIndex].splatPrototypes[0].tileSize = new Vector2(terrainArea1.terrains[terrainIndex].terrain.terrainData.size.x, terrainArea1.terrains[terrainIndex].terrain.terrainData.size.z);
                    terrainArea1.terrains[terrainIndex].splatPrototypes[0].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(text, typeof(Texture2D));
                    if (!terrainArea1.terrains[terrainIndex].splatPrototypes[0].texture)
                    {
                        Debug.Log(text);
                    }
                    terrain_splat_textures(terrainArea1.terrains[terrainIndex]);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        terrainArea1.terrains[terrainIndex].add_splatprototype(0);
                    }
                    terrainArea1.terrains[terrainIndex].splatPrototypes[0].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Dirt.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[1].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest3.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[2].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest2.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[3].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Grass.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[4].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest1.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[5].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/GrassRock.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[6].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Cliff.psd", typeof(Texture2D));
                    terrainArea1.terrains[terrainIndex].splatPrototypes[7].texture = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Rock1.psd", typeof(Texture2D));
                    terrain_splat_textures(terrainArea1.terrains[terrainIndex]);
                    if (type != null)
                    {
                        terrainArea1.terrains[terrainIndex].rtp_script = terrainArea1.terrains[terrainIndex].terrain.gameObject.AddComponent(type);
                    }

                    Component rtpScript = terrainArea1.terrains[terrainIndex].rtp_script;

                    if (rtpScript != null)
                    {
                        FieldInfo colorGlobalField = type.GetField("ColorGlobal");
                        colorGlobalField.SetValue(rtpScript, (Texture2D)AssetDatabase.LoadAssetAtPath(text, typeof(Texture2D)));

                        FieldInfo globalSettingsHolderField = type.GetField("globalSettingsHolder");
                        object globalSettingsHolder = globalSettingsHolderField.GetValue(rtpScript);
                        Type globalSettingsHolderType = globalSettingsHolder.GetType();

                        Texture2D[] texBumps = new Texture2D[8];

                        texBumps[0] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Dirt_NRM.png", typeof(Texture2D));
                        texBumps[1] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest3_NRM.png", typeof(Texture2D));
                        texBumps[2] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest2_NRM.png", typeof(Texture2D));
                        texBumps[3] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Grass_NRM.png", typeof(Texture2D));
                        texBumps[4] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest1_NRM.png", typeof(Texture2D));
                        texBumps[5] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/GrassRock_NRM.png", typeof(Texture2D));
                        texBumps[6] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Cliff_NRM.png", typeof(Texture2D));
                        texBumps[7] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Rock1_NRM.png", typeof(Texture2D));

                        globalSettingsHolderType.GetField("Bumps").SetValue(globalSettingsHolder, texBumps);

                        Texture2D[] texHeights = new Texture2D[8];

                        texHeights[0] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Dirt_DISP.png", typeof(Texture2D));
                        texHeights[1] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest3_DISP.png", typeof(Texture2D));
                        texHeights[2] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest2_DISP.png", typeof(Texture2D));
                        texHeights[3] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Grass_DISP.png", typeof(Texture2D));
                        texHeights[4] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Forest1_DISP.png", typeof(Texture2D));
                        texHeights[5] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/GrassRock_DISP.png", typeof(Texture2D));
                        texHeights[6] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Cliff_DISP.png", typeof(Texture2D));
                        texHeights[7] = (Texture2D)AssetDatabase.LoadAssetAtPath(install_path + "/Templates/Textures/Rock1_DISP.png", typeof(Texture2D));

                        globalSettingsHolderType.GetField("Heights").SetValue(globalSettingsHolder, texHeights);

                        globalSettingsHolderType.GetField("GlobalColorMapBlendValues").SetValue(globalSettingsHolder, Vector3.one);
                        globalSettingsHolderType.GetField("_GlobalColorMapNearMIP").SetValue(globalSettingsHolder, 1);
                        globalSettingsHolderType.GetField("ReliefTransform").SetValue(globalSettingsHolder, new Vector4((terrain_size.x * create_area.terrain_scale) / 22, (terrain_size.z * create_area.terrain_scale) / 22, 0, 0));
                        globalSettingsHolderType.GetField("distance_start").SetValue(globalSettingsHolder, 0);
                        globalSettingsHolderType.GetField("distance_start_bumpglobal").SetValue(globalSettingsHolder, 0);
                        globalSettingsHolderType.GetField("rtp_perlin_start_val").SetValue(globalSettingsHolder, 1);
                        globalSettingsHolderType.GetField("distance_transition_bumpglobal").SetValue(globalSettingsHolder, 300);

                        CreateRTPCombinedTextures(terrainArea1.terrains[terrainIndex], globalSettingsHolderType, globalSettingsHolder);

                        globalSettingsHolderType.GetMethod("RefreshAll").Invoke(globalSettingsHolder, null);
                    }
                }
                create_splat_count++;
            }
        }

        public void terrain_splat_textures(terrain_class2 preTerrain)
        {
            if (!preTerrain.terrain) return;

#if UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            List<SplatPrototype> list = new List<SplatPrototype>();
            
            for (int i = 0; i < preTerrain.splatPrototypes.Count; i++)
            {
                if (preTerrain.splatPrototypes[i].texture)
                {
                    var splat = new SplatPrototype();
                    splat.texture = preTerrain.splatPrototypes[i].texture;
                    splat.tileSize = preTerrain.splatPrototypes[i].tileSize;
                    splat.tileOffset = preTerrain.splatPrototypes[i].tileOffset;

                    list.Add(splat);
                }
            }

            preTerrain.terrain.terrainData.splatPrototypes = list.ToArray();
#else

            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

            for (int i = 0; i < preTerrain.splatPrototypes.Count; i++)
            {
                var s = preTerrain.splatPrototypes[i];

                if (s.texture)
                {
                    TerrainLayer d = new TerrainLayer();
                    d.diffuseTexture = s.texture;
                    d.normalMapTexture = s.normalMap;
                    d.normalScale = 1;
                    d.tileOffset = s.tileOffset;
                    d.tileSize = s.tileSize;

                    terrainLayers.Add(d);
                }
            }

            string terrainPath = current_area.export_terrain_path.Replace(Application.dataPath, "Assets");

            for (int i = 0; i < terrainLayers.Count; i++)
            {
                string path = terrainPath + "/" + current_area.terrain_scene_name + "_" + preTerrain.index + "_TerrainLayer_" + i + ".asset";

                TerrainLayer terrainLayer = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TerrainLayer)) as TerrainLayer;

                if (terrainLayer == null)
                {
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                    UnityEditor.AssetDatabase.CreateAsset(terrainLayers[i], path);
                }
                else
                {
                    CopyTerrainLayer(terrainLayers[i], terrainLayer);
                    terrainLayers[i] = terrainLayer;
                }
            }

            preTerrain.terrain.terrainData.terrainLayers = terrainLayers.ToArray();
#endif
        }

#if !UNITY_5 && !UNITY_2017 && !UNITY_2018_1 && !UNITY_2018_2
        public void CopyTerrainLayer(TerrainLayer s, TerrainLayer d)
        {
            d.diffuseRemapMax = s.diffuseRemapMax;
            d.diffuseRemapMin = s.diffuseRemapMin;
            d.diffuseTexture = s.diffuseTexture;
            d.maskMapRemapMax = s.maskMapRemapMax;
            d.maskMapRemapMin = s.maskMapRemapMin;
            d.maskMapTexture = s.maskMapTexture;
            d.metallic = s.metallic;
            d.name = s.name;
            d.normalMapTexture = s.normalMapTexture;
            d.normalScale = s.normalScale;
            d.smoothness = s.smoothness;
            d.specular = s.specular;
            d.tileOffset = s.tileOffset;
            d.tileSize = s.tileSize;
        }
#endif

        public tile_class calc_terrain_tile(int terrain_index, tile_class tiles)
        {
            tile_class arg_5F_0;
            if (tiles.x == 0 || tiles.y == 0)
            {
                apply_import_settings = false;
                create_terrain_loop = false;
                notify_text = "The Area is not created. Use the 'Pick' button to create an area";
                arg_5F_0 = null;
            }
            else
            {
                tile_class tile_class = new tile_class();
                tile_class.y = terrain_index / tiles.x;
                tile_class.x = terrain_index - tile_class.y * tiles.x;
                arg_5F_0 = tile_class;
            }
            return arg_5F_0;
        }

        public tile_class calc_terrain_tile2(int terrain_index, tile_class tiles)
        {
            tile_class tile_class = new tile_class();
            tile_class.y = terrain_index / tiles.x;
            tile_class.x = terrain_index - tile_class.y * tiles.x;
            return tile_class;
        }

        public int calc_terrain_index_old(tile_class tile, tile_class tiles)
        {
            return tile.x * tiles.y + tile.y;
        }

        void assign_terrain_splat_alpha(terrain_class2 preterrain1, bool update_asset)
        {
            if (preterrain1.terrain)
            {
                if (!preterrain1.terrain.terrainData) { return; }
                if (preterrain1.splatPrototypes.Count < 1) { return; }

                if (update_asset) { update_terrain_asset(preterrain1); }

                string path = AssetDatabase.GetAssetPath(preterrain1.terrain.terrainData);

                object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

                preterrain1.splat_alpha = new Texture2D[objects.Length - 1];

                for (int i = 0; i < objects.Length; ++i)
                {
                    if (objects[i].GetType() == typeof(Texture2D))
                    {
                        String numbers_only = Regex.Replace(((Texture)objects[i]).name, "[^0-9]", "");
                        int index = Convert.ToInt32(numbers_only);

                        preterrain1.splat_alpha[index] = (Texture2D)objects[i];
                    }
                }
            }
        }

        public byte[] process_out(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(255 - bytes[i]);
            }
            return bytes;
        }

        public void assign_all_terrain_splat_alpha(terrain_area_class terrainArea1, bool update_asset)
        {
            for (int i = 0; i < terrainArea1.terrains.Count; i++)
            {
                assign_terrain_splat_alpha(terrainArea1.terrains[i], update_asset);
            }
        }

        public void add_terrain(terrain_area_class terrainArea1, int terrain_number, int copy, terrain_area_class terrainArea2)
        {
            terrainArea1.terrains.Insert(terrain_number, new terrain_class2());
            if (copy > -1 && copy < terrainArea2.terrains.Count)
            {
                terrainArea1.terrains[terrain_number].terrain = null;
            }
            terrainArea1.terrains[terrain_number].index = terrain_number;
            terrainArea1.set_terrain_text();
        }

        public void terrain_parameters(terrain_class2 preterrain1)
        {
            preterrain1.terrain.heightmapPixelError = preterrain1.heightmapPixelError;
            preterrain1.terrain.heightmapMaximumLOD = preterrain1.heightmapMaximumLOD;
            preterrain1.terrain.basemapDistance = 200000;// preterrain1.basemapDistance;
#if UNITY_2017 || UNITY_2018
            preterrain1.terrain.castShadows = false;
#else
            preterrain1.terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#endif
#if !UNITY_2017 && !UNITY_2018_1 && !UNITY_2018_2
            preterrain1.terrain.allowAutoConnect = true;
            preterrain1.terrain.drawInstanced = true;
#endif
            preterrain1.terrain.treeDistance = preterrain1.treeDistance;
            preterrain1.terrain.detailObjectDistance = preterrain1.detailObjectDistance;
            preterrain1.terrain.detailObjectDensity = preterrain1.detailObjectDensity;
            preterrain1.terrain.treeBillboardDistance = preterrain1.treeBillboardDistance;
            preterrain1.terrain.treeCrossFadeLength = preterrain1.treeCrossFadeLength;
            preterrain1.terrain.treeMaximumFullLODCount = preterrain1.treeMaximumFullLODCount;
            preterrain1.terrain.terrainData.wavingGrassSpeed = preterrain1.wavingGrassSpeed;
            preterrain1.terrain.terrainData.wavingGrassAmount = preterrain1.wavingGrassAmount;
            preterrain1.terrain.terrainData.wavingGrassStrength = preterrain1.wavingGrassStrength;
            preterrain1.terrain.terrainData.wavingGrassTint = preterrain1.wavingGrassTint;
        }

        public void terrains_neighbor(terrain_area_class terrainArea1)
        {
            int num = 0;
            for (int i = 0; i < terrainArea1.terrains.Count; i++)
            {
                if (terrainArea1.terrains[i].terrain)
                {
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x - 1, terrainArea1.terrains[i].tile_z);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.left = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.left = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x, terrainArea1.terrains[i].tile_z - 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.bottom = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.bottom = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x + 1, terrainArea1.terrains[i].tile_z);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.right = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.right = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x, terrainArea1.terrains[i].tile_z + 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.top = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.top = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x + 1, terrainArea1.terrains[i].tile_z + 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.bottom_right = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.bottom_right = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x - 1, terrainArea1.terrains[i].tile_z + 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.bottom_left = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.bottom_left = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x + 1, terrainArea1.terrains[i].tile_z - 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.top_right = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.top_right = -1;
                    }
                    num = search_tile(terrainArea1, terrainArea1.terrains[i].tile_x - 1, terrainArea1.terrains[i].tile_z - 1);
                    if (num != -1)
                    {
                        terrainArea1.terrains[i].neighbor.top_left = num;
                    }
                    else
                    {
                        terrainArea1.terrains[i].neighbor.top_left = -1;
                    }
                    terrainArea1.terrains[i].neighbor.self = i;
                    terrainArea1.terrains[i].index = i;
                }
            }
        }

        public void center_terrain_position(terrain_area_class terrainArea1, terrain_class2 preterrain1)
        {
            if (preterrain1.terrain)
            {
                if (preterrain1.terrain.terrainData)
                {
                    Vector3 vector = new Vector3(-preterrain1.terrain.terrainData.size.x / 2, 0, -preterrain1.terrain.terrainData.size.z / 2) + terrainArea1.center;
                    if (preterrain1.terrain.transform.position != vector)
                    {
                        preterrain1.terrain.transform.position = vector;
                    }
                }
            }
        }

        public int calc_terrain_index(tile_class tile, tile_class tiles)
        {
            return tile.x + tile.y * (tiles.x - 1);
        }

        Vector3 get_terrainArea_center(terrain_area_class terrainArea1, bool include_position)
        {
            if (!terrainArea1.terrains[0].terrain) { return Vector3.zero; }

            Vector2 pos, size;

            size.x = terrainArea1.tiles_select.x * terrainArea1.terrains[0].terrain.terrainData.size.x;
            size.y = terrainArea1.tiles_select.y * terrainArea1.terrains[0].terrain.terrainData.size.z;

            size /= 2;
            pos.x = size.x;
            pos.y = size.y;

            if (include_position)
            {
                int leftBottom = calc_terrain_index(new tile_class(0, terrainArea1.tiles.y), terrainArea1.tiles);

                // Debug.Log("left: "+leftBottom);

                pos.x = size.x + terrainArea1.terrains[leftBottom].terrain.transform.position.x;
                pos.y = size.y + terrainArea1.terrains[leftBottom].terrain.transform.position.z;
            }

            return new Vector3(pos.x, terrainArea1.terrains[0].terrain.transform.position.y, pos.y);
        }

        public int fit_terrain_tiles(terrain_area_class terrainArea1, terrain_class2 preterrain1, bool refit)
        {
            int arg_1BC_0;
            if (terrainArea1.terrains.Count < 2)
            {
                if (terrainArea1.terrains.Count == 1)
                {
                    center_terrain_position(terrainArea1, terrainArea1.terrains[0]);
                }
                arg_1BC_0 = 1;
            }
            else
            {
                Vector3 position = default(Vector3);
                Vector3 vector = default(Vector3);
                vector = get_terrainArea_center(terrainArea1, false);
                for (int i = 0; i < terrainArea1.terrains.Count; i++)
                {
                    if (terrainArea1.terrains[i].terrain)
                    {
                        position.x = terrainArea1.terrains[i].tile_x * terrainArea1.terrains[i].terrain.terrainData.size.x + terrainArea1.center.x - vector.x;
                        position.y = terrainArea1.center.y;
                        position.z = (terrainArea1.terrains[i].tile_z + 1) * -terrainArea1.terrains[i].terrain.terrainData.size.z + terrainArea1.center.z + vector.z;
                        terrainArea1.terrains[i].rect = new Rect(position.x, position.z, terrain_size.x, terrain_size.z);
                        if (refit)
                        {
                            terrainArea1.terrains[i].terrain.transform.position = position;
                        }
                    }
                }
                terrains_neighbor(terrainArea1);
                terrains_neighbor_script(terrainArea1, 1);
                arg_1BC_0 = 1;
            }
            return arg_1BC_0;
        }

        public int fit_terrain_tile(terrain_area_class terrainArea1, terrain_class2 preterrain1, bool refit)
        {
            Vector3 position = default(Vector3);
            Vector3 vector = default(Vector3);
            vector = get_terrainArea_center(terrainArea1, false);
            position.x = preterrain1.tile_x * preterrain1.terrain.terrainData.size.x + terrainArea1.center.x - vector.x;
            if (current_area.normalizeHeightmap) position.y = 0; else position.y = -1000;
            position.z = preterrain1.tile_z * preterrain1.terrain.terrainData.size.z + terrainArea1.center.z - vector.z;
            preterrain1.rect = new Rect(position.x, position.z, terrain_size.x, terrain_size.z);
            if (refit)
            {
                preterrain1.terrain.transform.position = position;
            }
            return 1;
        }

        public void terrains_neighbor_script(terrain_area_class terrainArea1, int mode)
        {
            for (int i = 0; i < terrainArea1.terrains.Count; ++i)
            {
                if (terrainArea1.terrains[i].terrain)
                {
                    TerrainNeighbors terrainNeighbors = (TerrainNeighbors)terrainArea1.terrains[i].terrain.GetComponent(typeof(TerrainNeighbors));

                    if (mode == 1)
                    {
                        if (terrainNeighbors == null) { terrainNeighbors = (TerrainNeighbors)terrainArea1.terrains[i].terrain.gameObject.AddComponent(typeof(TerrainNeighbors)); }

                        int terrain_number = terrainArea1.terrains[i].neighbor.left;
                        if (terrain_number != -1) { terrainNeighbors.left = terrainArea1.terrains[terrain_number].terrain; } else { terrainNeighbors.left = null; }

                        terrain_number = terrainArea1.terrains[i].neighbor.top;
                        if (terrain_number != -1) { terrainNeighbors.top = terrainArea1.terrains[terrain_number].terrain; } else { terrainNeighbors.top = null; }

                        terrain_number = terrainArea1.terrains[i].neighbor.right;
                        if (terrain_number != -1) { terrainNeighbors.right = terrainArea1.terrains[terrain_number].terrain; } else { terrainNeighbors.right = null; }

                        terrain_number = terrainArea1.terrains[i].neighbor.bottom;
                        if (terrain_number != -1) { terrainNeighbors.bottom = terrainArea1.terrains[terrain_number].terrain; } else { terrainNeighbors.bottom = null; }
                    }
                    if (mode == -1)
                    {
                        if (terrainNeighbors)
                        {
                            DestroyImmediate(terrainNeighbors);
                        }
                    }
                }
            }
        }

        public int search_tile(terrain_area_class terrainArea1, int tile_x, int tile_z)
        {
            int arg_91_0;
            if (tile_x > terrainArea1.tiles_select.x - 1 || tile_x < 0)
            {
                arg_91_0 = -1;
            }
            else if (tile_z > terrainArea1.tiles_select.y - 1 || tile_z < 0)
            {
                arg_91_0 = -1;
            }
            else
            {
                for (int i = 0; i < terrainArea1.terrains.Count; i++)
                {
                    if (terrainArea1.terrains[i].tile_x == tile_x && terrainArea1.terrains[i].tile_z == tile_z)
                    {
                        arg_91_0 = i;
                        return arg_91_0;
                    }
                }
                arg_91_0 = -1;
            }
            return arg_91_0;
        }

        public void update_terrain_asset(terrain_class2 preterrain)
        {
            if (preterrain.terrain)
            {
                string assetPath = AssetDatabase.GetAssetPath(preterrain.terrain.terrainData);
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        public void auto_search(auto_search_class auto_search)
        {
            int select_index = auto_search.select_index;
            if (!global_script)
            {
                load_global_settings();
            }
            auto_search.format = global_script.auto_search_list[select_index].format;
            auto_search.digits = global_script.auto_search_list[select_index].digits;
            auto_search.start_x = global_script.auto_search_list[select_index].start_x;
            auto_search.start_y = global_script.auto_search_list[select_index].start_y;
            auto_search.start_n = global_script.auto_search_list[select_index].start_n;
            auto_search.output_format = global_script.auto_search_list[select_index].output_format;
        }

        public void terrains_heightmap_resolution()
        {
            for (int i = 0; i < terrain_region.area[0].terrains.Count; i++)
            {
            }
        }

        public void generate_begin()
        {
            heightmap_resolution = create_area.terrain_heightmap_resolution;
            if (heightmap_resolution < 33)
            {
                heightmap_resolution = 33;
            }
            else if (heightmap_resolution > 33 && heightmap_resolution < 65)
            {
                heightmap_resolution = 65;
            }
            else if (heightmap_resolution > 65 && heightmap_resolution < 129)
            {
                heightmap_resolution = 129;
            }
            else if (heightmap_resolution > 129 && heightmap_resolution < 257)
            {
                heightmap_resolution = 257;
            }
            else if (heightmap_resolution > 257 && heightmap_resolution < 513)
            {
                heightmap_resolution = 513;
            }
            else if (heightmap_resolution > 513 && heightmap_resolution < 1025)
            {
                heightmap_resolution = 1025;
            }
            else if (heightmap_resolution > 1025 && heightmap_resolution < 2049)
            {
                heightmap_resolution = 2049;
            }
            else if (heightmap_resolution > 2049)
            {
                heightmap_resolution = 4097;
            }
            if ((create_area.do_heightmap && (!terraincomposer || !create_area.export_to_terraincomposer)) || generate_manual)
            {
                heights = new float[heightmap_resolution, heightmap_resolution];

                rawFile.file = create_area.export_heightmap_path + "/" + create_area.export_heightmap_filename;
                if (create_area.normalizeHeightmap)
                {
                    rawFile.file = rawFile.file + "_N";
                }
                rawFile.file = rawFile.file + ".raw";
                if (!load_raw_file())
                {
                    create_terrain_loop = false;
                    if (!map.export_heightmap_active && !map.export_image_active)
                    {
                        Application.runInBackground = false;
                    }
                    notify_text = "Heightmap File: " + rawFile.file + " does not exist.";
                }
                raw_auto_scale();
                heightmap_count_terrain = 0;
                heightmap_break_x_value = 0;
            }
        }

        public int generate_heightmap2()
        {
            frames = 1 / (Time.realtimeSinceStartup - auto_speed_time);
            auto_speed_time = Time.realtimeSinceStartup;
            int arg_31F_0;
            if (terrain_region.area[0].terrains.Count == 0)
            {
                generate = false;
                create_area.terrain_done = false;
                Repaint();
            }
            else
            {
                if (terrain_region.area[0].terrains[heightmap_count_terrain].terrain)
                {
                    heightmap_res_y = heightmap_y;
                    while (heightmap_res_y >= heightmap_res_y - generate_speed)
                    {
                        if (heightmap_res_y < 0)
                        {
                            if (generate)
                            {
                                terrain_apply(terrain_region.area[0].terrains[heightmap_count_terrain]);
                                for (int i = 0; i < heightmap_resolution; i++)
                                {
                                    for (int j = 0; j < heightmap_resolution; j++)
                                    {
                                        heights[i, j] = 0;
                                    }
                                }
                                generate = false;
                            }
                            if (!generate)
                            {
                                heightmap_count_terrain++;
                                if (!generate_manual)
                                {
                                    arg_31F_0 = 2;
                                    return arg_31F_0;
                                }
                                if (heightmap_count_terrain < terrain_region.area[0].terrains.Count)
                                {
                                    generate = true;
                                    heightmap_res_y = heightmap_resolution - 1;
                                    heightmap_y = heightmap_resolution - 1;
                                    heightmap_break_x_value = 0;
                                    Repaint();
                                    arg_31F_0 = 2;
                                    return arg_31F_0;
                                }
                                generate_manual = false;
                                rawFile.loaded = false;
                                Repaint();
                                arg_31F_0 = 2;
                                return arg_31F_0;
                            }
                            else
                            {
                                heightmap_res_y = heightmap_resolution - 1;
                                heightmap_y = heightmap_resolution - 1;
                                heightmap_break_x_value = 0;
                            }
                        }
                        heightmap_res_x = heightmap_break_x_value;
                        while (heightmap_res_x < heightmap_resolution)
                        {
                            heights[(int)heightmap_res_y, (int)heightmap_res_x] = create_area.terrain_curve.Evaluate(calc_raw_value(new Vector2(heightmap_res_x, heightmap_resolution - 1 - heightmap_res_y), create_area.heightmap_offset_e));
                            heightmap_res_x += 1;
                        }
                        heightmap_break_x_value = 0;
                        heightmap_res_y -= 1;
                    }
                    heightmap_y -= generate_speed + 1;
                    arg_31F_0 = 1;
                    return arg_31F_0;
                }
                notify_text = "Terrains are not complete anymore, please recreate the terrains";
                generate = false;
                create_area.terrain_done = false;
                Repaint();
            }
            arg_31F_0 = 0;
            return arg_31F_0;
        }

        public void terrain_apply(terrain_class2 preterrain1)
        {
            preterrain1.terrain.terrainData.SetHeights(0, 0, heights);
        }

        public void raw_auto_scale()
        {
            conversion_step.x = (heightmap_resolution - 1) * terrain_region.area[0].tiles_select.x / (rawFile.resolution.x - 1);
            conversion_step.y = (heightmap_resolution - 1) * terrain_region.area[0].tiles_select.y / (rawFile.resolution.y - 1);
        }

        public float calc_raw_value(Vector2 pos, Vector2 offset)
        {
            float num = 0;
            float num2 = 0;
            float num3 = 0f;
            float num4 = 0f;
            num3 = terrain_region.area[0].terrains[heightmap_count_terrain].tile_x * (heightmap_resolution - 1);
            num4 = (terrain_region.area[0].tiles_select.y - terrain_region.area[0].terrains[heightmap_count_terrain].tile_z - 1) * (heightmap_resolution - 1);
            pos.x = (pos.x + num3) / conversion_step.x + num;
            pos.y = (pos.y + num4) / conversion_step.y + num2;
            h_local_x = (int)pos.x;
            h_local_y = (int)pos.y;
            float arg_1F0_0;
            if (h_local_x > rawFile.resolution.x - 1 || h_local_x < 0)
            {
                arg_1F0_0 = 0;
            }
            else if (h_local_y > rawFile.resolution.y - 1 || h_local_y < 0)
            {
                arg_1F0_0 = 0;
            }
            else
            {
                uint index = (uint)(h_local_y * (rawFile.resolution.x * 2) + h_local_x * 2);

                byte b = rawFile.bytes[index];
                byte b2 = rawFile.bytes[index + 1];
                arg_1F0_0 = (b * rawFile.product1 + b2 * rawFile.product2) / 65535f;
            }
            return arg_1F0_0;
        }

        public bool load_raw_file()
        {
            bool fileExists = File.Exists(rawFile.file);

            if (fileExists)
            {
                rawFile.LoadRawFile(rawFile.file);
                rawFile.resolution.x = create_region.area[create_region.area_select].heightmap_resolution.x;
                rawFile.resolution.y = create_region.area[create_region.area_select].heightmap_resolution.y;

                if (rawFile.mode == raw_mode_enum.Mac)
                {
                    rawFile.product1 = 256f;
                    rawFile.product2 = 1f;
                }
                else
                {
                    rawFile.product1 = 1f;
                    rawFile.product2 = 256f;
                }
                rawFile.loaded = true;
                fileExists = true;
            }
            return fileExists;
        }

        public void add_area(map_region_class region1, int index, string name, bool duplicate = false)
        {
            if (duplicate)
            {
                var area = current_area.GetShallowCopy();
                area.name = current_area.name + "_Copy";
                region1.area.Add(area);
            }
            else
            {
                region1.area.Add(new map_area_class(name, index));
                index = region1.area.Count - 1;
                map_area_class map_area_class = region1.area[index];
                map_area_class.center = global_script.map_latlong_center;
                
                map_area_class.export_heightmap_path = Application.dataPath;
                map_area_class.export_image_path = map_area_class.export_heightmap_path;
                map_area_class.export_terrain_path = map_area_class.export_heightmap_path + "/Terrains";
                map_area_class.export_heightmap_filename = map_area_class.name;
                map_area_class.export_image_filename = map_area_class.name;
                map_area_class.terrain_scene_name = "_" + map_area_class.name;
                map_area_class.terrain_scene_name = map_area_class.name;
            }

            region1.area_select = region1.area.Count - 1;
            region1.make_area_popup();
        }

        public void set_terrain_default(map_area_class area1)
        {
            area1.export_image_import_settings = true;
            area1.export_terrain_path = area1.export_heightmap_path + "/Terrains";
            area1.export_to_terraincomposer = true;
            area1.terrain_scene_name = "_" + area1.name;
            area1.terrain_scene_name = area1.name;
            area1.do_heightmap = true;
            area1.do_image = true;
            area1.mipmapEnabled = true;
            area1.filterMode = FilterMode.Trilinear;
            area1.anisoLevel = 9;
            area1.maxTextureSize_select = 6;
            area1.auto_import_settings_apply = true;

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
        area1.textureFormat = TextureImporterFormat.AutomaticCompressed;
#endif
        }

        public void init_paths()
        {
            if (global_script)
            {
                map_area_class map_area_class = map.region[map.region_select].area[map.region[map.region_select].area_select];
                if (map_area_class.export_heightmap_path.Length == 0)
                {
                    map_area_class.export_heightmap_path = Application.dataPath;
                }
                if (map_area_class.export_image_path.Length == 0)
                {
                    map_area_class.export_image_path = Application.dataPath;
                }
                if (map_area_class.export_terrain_path.Length == 0)
                {
                    map_area_class.export_terrain_path = Application.dataPath + "/Terrains";
                }
                if (!string.IsNullOrEmpty(map_area_class.preimage_path))
                {
                    if (map_area_class.preimage_path.Length == 0)
                    {
                        if (!map_area_class.preimage_path_changed)
                        {
                            map_area_class.preimage_path = map_area_class.export_image_path;
                        }
                        else
                        {
                            map_area_class.preimage_path = Application.dataPath;
                        }
                    }
                }
                else
                {
                    map_area_class.preimage_path = Application.dataPath;
                }
            }
        }

        public void copy_texture_to_buffer(buffer_class buffer, Texture2D texture, int x, int y, int width, int height)
        {
            pixels = texture.GetPixels(x, y, width, height);
            if (buffer.bytes != null)
            {
                if (buffer.bytes.Length != pixels.Length * 3)
                {
                    buffer.bytes = new byte[pixels.Length * 3];
                }
            }
            else
            {
                buffer.bytes = new byte[pixels.Length * 3];
            }
            for (int i = 0; i < pixels.Length; i++)
            {
                buffer.bytes[i * 3] = (byte)(pixels[i][0] * 255);
                buffer.bytes[i * 3 + 1] = (byte)(pixels[i][1] * 255);
                buffer.bytes[i * 3 + 2] = (byte)(pixels[i][2] * 255);
            }
        }

        public void copy_buffer_to_texture(buffer_class buffer)
        {
            if (pixels == null)
            {
                pixels = new Color[buffer.bytes.Length / 3];
            }
            else if (pixels.Length != buffer.bytes.Length / 3)
            {
                pixels = new Color[buffer.bytes.Length / 3];
            }
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i][0] = buffer.bytes[i * 3] * 1f / 255;
                pixels[i][1] = buffer.bytes[i * 3 + 1] * 1f / 255;
                pixels[i][2] = buffer.bytes[i * 3 + 2] * 1f / 255;
            }
        }

        public void image_generate_begin()
        {
            if (!map.preimage_edit.generate || map.preimage_edit.mode != 2)
            {
                if (!map.preimage_edit.generate)
                {
                    map.preimage_edit.y1 = 0;
                    map.preimage_edit.x1 = 0;
                    map.preimage_edit.mode = 1;
                    map.preimage_edit.generate = true;
                    map.preimage_edit.repeat = 0;
                    int num = map.preimage_edit.radiusSelect;
                    if (num > 740)
                    {
                        num = 740;
                    }
                    map.preimage_edit.radius = num;
                    map.preimage_edit.first = true;
                    map.preimage_edit.resolution = new Vector2(800, 768);
                    map.preimage_edit.inputBuffer.resolution = new Vector2(800, 768);
                    map.preimage_edit.inputBuffer.size = new Vector2(800, 768);
                    map.preimage_edit.inputBuffer.radius = -20;
                    map.preimage_edit.inputBuffer.init();
                    map.preimage_edit.tile.x = 0;
                    map.preimage_edit.tile.y = 0;
                    map.preimage_edit.inputBuffer.getRects(map.preimage_edit.tile);
                    if (map.preimage_edit.inputBuffer.file != null)
                    {
                        map.preimage_edit.inputBuffer.file.Close();
                    }
                    map.preimage_edit.outputBuffer.resolution = map.preimage_edit.inputBuffer.resolution;
                    map.preimage_edit.outputBuffer.size = map.preimage_edit.inputBuffer.size;
                    map.preimage_edit.outputBuffer.radius = map.preimage_edit.inputBuffer.radius;
                    map.preimage_edit.outputBuffer.init();
                    map.preimage_edit.tile.x = 0;
                    map.preimage_edit.tile.y = 0;
                    map.preimage_edit.outputBuffer.getRects(map.preimage_edit.tile);
                    copy_texture_to_buffer(map.preimage_edit.inputBuffer, map0, 0, 0, 800, 768);
                    map.preimage_edit.outputBuffer.clear_bytes();
                }
                else
                {
                    map.preimage_edit.generate_call = true;
                }
            }
        }

        public void image_edit_apply()
        {
            if (!map0)
            {
                map0 = new Texture2D(1600, 768, TextureFormat.RGBA32, false, false);
            }
            map.preimage_edit.convert_texture_raw(true);
            if (!map.preimage_edit.generate)
            {
                copy_buffer_to_texture(map.preimage_edit.outputBuffer);
                map0.SetPixels(800, 0, 800, 768, pixels);
                map0.Apply();
                if (map.preimage_edit.generate_call)
                {
                    map.preimage_edit.regen = false;
                    map.preimage_edit.border = false;
                    image_generate_begin();
                    map.preimage_edit.generate_call = false;
                }
                else if (map.preimage_edit.regen)
                {
                    map.preimage_edit.inputBuffer.copy_bytes(map.preimage_edit.outputBuffer.bytes, map.preimage_edit.inputBuffer.bytes);
                    map.preimage_edit.y1 = 0;
                    map.preimage_edit.x1 = 0;
                    map.preimage_edit.generate = true;
                    map.preimage_edit.first = false;
                    map.preimage_edit.regen = false;
                    map.preimage_edit.radius = map.preimage_edit.radius - 25;
                    map.preimage_edit.repeat = map.preimage_edit.repeat + 1;
                }
            }
            Repaint();
        }

        public bool convert_textures_begin(map_area_class area1)
        {
            String path1 = current_area.export_image_path + "/" + current_area.export_image_filename + "_combined.raw";

            if (!File.Exists(path1))
            {
                notify_text = path1 + " doesn't exist! Export images as Raw in 'Image Export', after that use the 'Combine' button to combine them.";
                Debug.Log(path1 + " doesn't exist! Export images as Raw in 'Image Export', after that use the 'Combine' button to combine them.");
                return false;
            }

            map.preimage_edit.time_start = Time.realtimeSinceStartup;

            String path2;

            if (!current_area.preimage_save_new)
            {
                path2 = current_area.export_image_path + "/" + current_area.export_image_filename + "_combined2.raw";
            }
            else
            {
                path2 = current_area.preimage_path + "/" + current_area.preimage_filename + "_combined2.raw";
            }

            map.preimage_edit.first = true;
            map.preimage_edit.resolution = new Vector2(2048, 2048);

            //map.preimage_edit.outputRaw = new FileStream(current_area.export_image_path+"/"+current_area.export_image_filename+"_combined2.raw",FileMode.Create);

            map.preimage_edit.radius = map.preimage_edit.radiusSelect;

            map.preimage_edit.inputBuffer.resolution = new Vector2(current_area.resolution * current_area.tiles.x, current_area.resolution * current_area.tiles.y);
            //map.preimage_edit.inputBuffer.resolution = Vector2(8192,8192);
            map.preimage_edit.inputBuffer.size = new Vector2(2048, 2048);
            map.preimage_edit.inputBuffer.radius = map.preimage_edit.radius;

            map.preimage_edit.inputBuffer.init();
            map.preimage_edit.tile.x = 0;
            map.preimage_edit.tile.y = 0;
            map.preimage_edit.inputBuffer.getRects(map.preimage_edit.tile);

            if (map.preimage_edit.inputBuffer.file != null) { map.preimage_edit.inputBuffer.file.Close(); }
            map.preimage_edit.inputBuffer.file = new FileStream(path1, FileMode.Open);
            map.preimage_edit.inputBuffer.read();

            // map.preimage_edit.outputBuffer.resolution = Vector2(current_area.resolution*current_area.tiles.x,current_area.resolution*current_area.tiles.y);

            map.preimage_edit.outputBuffer.resolution = map.preimage_edit.inputBuffer.resolution;
            map.preimage_edit.outputBuffer.size = map.preimage_edit.inputBuffer.size;
            map.preimage_edit.outputBuffer.radius = map.preimage_edit.radius;

            map.preimage_edit.outputBuffer.init();
            map.preimage_edit.tile.x = 0;
            map.preimage_edit.tile.y = 0;
            map.preimage_edit.outputBuffer.getRects(map.preimage_edit.tile);
            map.preimage_edit.outputBuffer.clear_bytes();
            if (map.preimage_edit.outputBuffer.file != null) { map.preimage_edit.outputBuffer.file.Close(); }
            map.preimage_edit.outputBuffer.file = new FileStream(path2, FileMode.OpenOrCreate);

            // map.preimage_edit.outputBuffer.copy_bytes(map.preimage_edit.inputBuffer.bytes,map.preimage_edit.outputBuffer.bytes);
            // map.preimage_edit.outputBuffer.write();

            // if (map.preimage_edit.inputBuffer.file) {map.preimage_edit.inputBuffer.file.Close();}
            // if (map.preimage_edit.outputBuffer.file) {map.preimage_edit.outputBuffer.file.Close();}
            //}
            //else {
            //map.preimage_edit.outputRaw = new FileStream(current_area.export_image_path+"/"+current_area.export_image_filename+"_combined2.raw",FileMode.Open);
            //}
            map.preimage_edit.generate = true;
            map.preimage_edit.y1 = 0;
            map.preimage_edit.x1 = 0;
            map.preimage_edit.repeat = 0;
            map.preimage_edit.mode = 2;
            return true;
        }

        public void convert_textures_raw(map_area_class area1)
        {
            map.preimage_edit.convert_texture_raw(true);
            if (!map.preimage_edit.generate)
            {
                map.preimage_edit.outputBuffer.write();
                map.preimage_edit.tile.x = map.preimage_edit.tile.x + 1;
                if (map.preimage_edit.tile.x > map.preimage_edit.inputBuffer.tiles.x - 1)
                {
                    map.preimage_edit.tile.x = 0;
                    map.preimage_edit.tile.y = map.preimage_edit.tile.y + 1;
                    if (map.preimage_edit.tile.y > map.preimage_edit.inputBuffer.tiles.y - 1)
                    {
                        if (!map.preimage_edit.regen)
                        {
                            if (map.preimage_edit.inputBuffer.file != null)
                            {
                                map.preimage_edit.inputBuffer.file.Close();
                            }
                            if (map.preimage_edit.outputBuffer.file != null)
                            {
                                map.preimage_edit.outputBuffer.file.Close();
                            }
                            return;
                        }
                        map.preimage_edit.inputBuffer.file = map.preimage_edit.outputBuffer.file;
                        map.preimage_edit.tile.x = 0;
                        map.preimage_edit.tile.y = 0;
                        map.preimage_edit.first = false;
                        map.preimage_edit.regen = false;
                        map.preimage_edit.radius = map.preimage_edit.radius - 25;
                        map.preimage_edit.repeat = map.preimage_edit.repeat + 1;
                    }
                }
                map.preimage_edit.inputBuffer.getRects(map.preimage_edit.tile);
                map.preimage_edit.outputBuffer.getRects(map.preimage_edit.tile);
                map.preimage_edit.outputBuffer.clear_bytes();
                map.preimage_edit.inputBuffer.read();
                map.preimage_edit.generate = true;
                map.preimage_edit.y1 = (int)map.preimage_edit.inputBuffer.offset.y;
            }
            Repaint();
        }

        public void load_convert_texture(map_area_class area1)
        {
            convert_tile.y = area1.preimage_count / area1.tiles.x;
            convert_tile.x = area1.preimage_count - convert_tile.y * area1.tiles.x;
            string text = area1.export_image_filename + "_x" + convert_tile.x + "_y" + convert_tile.y;
            string text2 = area1.export_image_path + "/";
            if (map.export_jpg)
            {
                if (File.Exists(text2 + text + ".jpg"))
                {
                    Draw.set_image_import_settings(text2.Replace(Application.dataPath, "Assets") + text + ".jpg", true, TextureImporterFormat.RGB24, TextureWrapMode.Clamp, convert_area.resolution, convert_area.mipmapEnabled, convert_area.filterMode, convert_area.anisoLevel, 3);
                    convert_texture = (Texture2D)AssetDatabase.LoadAssetAtPath(text2.Replace(Application.dataPath, "Assets") + text + ".jpg", typeof(Texture2D));
                }
                else
                {
                    notify_text = text2 + text + ".jpg doesn't exist! Make sure the image tiles are the same as the exported image tiles";
                    Debug.Log(text2 + text + ".jpg doesn't exist! Make sure the image tiles are the same as the exported image tiles.");
                }
            }
            else if (map.export_png)
            {
                if (File.Exists(text2 + text + ".png"))
                {
                    Draw.set_image_import_settings(text2.Replace(Application.dataPath, "Assets") + text + ".png", true, TextureImporterFormat.RGB24, TextureWrapMode.Clamp, convert_area.resolution, convert_area.mipmapEnabled, convert_area.filterMode, convert_area.anisoLevel, 3);
                    convert_texture = (Texture2D)AssetDatabase.LoadAssetAtPath(text2.Replace(Application.dataPath, "Assets") + text + ".png", typeof(Texture2D));
                }
                else
                {
                    notify_text = text2 + text + ".png doesn't exist! Make sure the image tiles are the same as the exported image tiles";
                    Debug.Log(text2 + text + ".png doesn't exist! Make sure the image tiles are the same as the exported image tiles.");
                }
            }
        }

        public void save_convert_texture(map_area_class area1)
        {
            string text;
            string text2;
            if (area1.preimage_save_new)
            {
                text = area1.preimage_filename + "_x" + convert_tile.x + "_y" + convert_tile.y;
                text2 = area1.preimage_path;
            }
            else
            {
                text = area1.export_image_filename + "_x" + convert_tile.x + "_y" + convert_tile.y;
                text2 = area1.export_image_path;
            }
            if (map.export_jpg)
            {
                export_texture_as_jpg(text2 + "/" + text + ".jpg", convert_texture, map.export_jpg_quality);
                string text3 = area1.export_image_filename + "_x" + convert_tile.x + "_y" + convert_tile.y;
                string export_image_path = area1.export_image_path;
                Draw.set_image_import_settings(export_image_path.Replace(Application.dataPath, "Assets") + "/" + text3 + ".jpg", false, area1.textureFormat, TextureWrapMode.Clamp, area1.resolution, area1.mipmapEnabled, area1.filterMode, area1.anisoLevel, 127);
                if (map.preimage_edit.import_settings)
                {
                    import_image_area = area1;
                    import_jpg_path = text2.Replace(Application.dataPath, "Assets") + "/" + text + ".jpg";
                    import_settings_call = true;
                    import_jpg_call = true;
                }
            }
            if (map.export_png)
            {
                export_texture_to_file(text2, text, convert_texture);
                string text3 = area1.export_image_filename + "_x" + convert_tile.x + "_y" + convert_tile.y;
                string export_image_path = area1.export_image_path;
                Draw.set_image_import_settings(export_image_path.Replace(Application.dataPath, "Assets") + "/" + text3 + ".png", false, area1.textureFormat, TextureWrapMode.Clamp, area1.resolution, area1.mipmapEnabled, area1.filterMode, area1.anisoLevel, 127);
                if (map.preimage_edit.import_settings)
                {
                    import_image_area = area1;
                    import_png_path = text2.Replace(Application.dataPath, "Assets") + "/" + text + ".png";
                    import_settings_call = true;
                    import_png_call = true;
                }
            }
        }

        public void check_content_done()
        {
            if (global_script)
            {
                if (global_script.settings.wc_loading > 0)
                {
                    if (!global_script.settings.wc_contents.HasRequested)
                    {
                        global_script.settings.wc_loading = 0;
                    }
                    else
                    {
                        int num = read_check();
                        if (global_script.settings.wc_loading == 1)
                        {
                            if (global_script.settings.wc_contents.IsDone)
                            {
                                global_script.settings.wc_loading = 0;
                                float num2 = 0f;
                                float num3 = 0f;
                                num3 = read_version();
                                write_checked(DateTime.Now.Day.ToString());

                                string errorText;
                                if (!global_script.settings.wc_contents.IsError(out errorText))
                                {
                                    string text = global_script.settings.wc_contents.GetText();
                                    if (float.TryParse(text, out num2))
                                    {
                                        global_script.settings.new_version = num2;
                                        global_script.settings.old_version = num3;
                                        if (num2 > num3)
                                        {
                                            map.button_update = true;
                                            notify_text = "A new WorldComposer update is available";
                                            if (num == 0)
                                            {
                                                global_script.settings.update_version = true;
                                            }
                                            else if (num == 1)
                                            {
                                                global_script.settings.update_display = true;
                                                global_script.settings.update_version = true;
                                            }
                                            else if (num > 1)
                                            {
                                                global_script.settings.update_version = true;
                                                content_version();
                                            }
                                        } 
                                        else
                                        {
                                            global_script.settings.update_version = false;
                                        }
                                    }
                                }
                                else
                                {
                                    UnityWebRequest www = global_script.settings.wc_contents.www;
                                    Debug.LogError(errorText + " response code " + www.responseCode + " isNetworkError " + www.isNetworkError + " isHttpError " + www.isHttpError);
                                }
                            }
                        }
                        else if (global_script.settings.wc_loading == 2)
                        {
                            if (global_script.settings.wc_contents.IsDone)
                            {
                                string errorText;
                                if (!global_script.settings.wc_contents.IsError(out errorText))
                                {
                                    global_script.settings.wc_loading = 0;
                                    global_script.settings.update_version2 = true;
                                    global_script.settings.update_version = false;
                                    File.WriteAllBytes(GetFullInstallPath() + "/Update/WorldComposer/WorldComposer.unitypackage", global_script.settings.wc_contents.GetBytes());
                                    if (num < 3)
                                    {
                                        global_script.settings.update_display = true;
                                    }
                                    else if (num == 3)
                                    {
                                        global_script.settings.update_display = true;
                                        import_contents(GetFullInstallPath() + "/Update/WorldComposer/WorldComposer.unitypackage", false);
                                    }
                                    else if (num == 4)
                                    {
                                        import_contents(GetFullInstallPath() + "/Update/WorldComposer/WorldComposer.unitypackage", false);
                                    }
                                }
                                else
                                {
                                    Debug.LogError(errorText);
                                }
                            }
                        }
                        else if (global_script.settings.wc_loading == 3)
                        {
                            if (global_script.settings.new_version == read_version())
                            {
                                AssetDatabase.Refresh();
                                global_script.settings.wc_loading = 4;
                            }
                            else if (EditorApplication.timeSinceStartup > global_script.settings.time_out + 60)
                            {
                                global_script.settings.wc_loading = 0;
                                Debug.Log("Time out with importing WorldComposer update...");
                            }
                        }
                        else if (global_script.settings.wc_loading == 4)
                        {
                            Debug.Log("Updated WorldComposer version " + global_script.settings.old_version + " to version " + read_version().ToString("F3"));
                            notify_text = "Updated WorldComposer version " + global_script.settings.old_version + " to version " + read_version().ToString("F3");
                            global_script.settings.wc_loading = 0;
                        }
                    }
                }
            }
        }

        public float read_version()
        {
            StreamReader streamReader = File.OpenText(GetFullInstallPath() + "/Update/WorldComposer/version.txt");
            string s = streamReader.ReadLine();
            streamReader.Close();
            float result = 0f;
            float.TryParse(s, out result);
            return result;
        }

        public void write_check(string text)
        {
            StreamWriter streamWriter = new StreamWriter(GetFullInstallPath() + "/Update/WorldComposer/check.txt");
            streamWriter.WriteLine(text);
            streamWriter.Close();
        }

        string GetFullInstallPath()
        {
            return Application.dataPath + install_path.Substring(6, install_path.Length - 6);
        }

        public int read_check()
        {
            if (!File.Exists(GetFullInstallPath() + "/Update/WorldComposer/check.txt"))
            {
                write_check("1");
            }
            StreamReader streamReader = File.OpenText(GetFullInstallPath() + "/Update/WorldComposer/check.txt");
            string s = streamReader.ReadLine();
            streamReader.Close();
            int result = 0;
            int.TryParse(s, out result);
            return result;
        }

        public void write_checked(string text)
        {
            StreamWriter streamWriter = new StreamWriter(GetFullInstallPath() + "/Update/WorldComposer/last_checked.txt");
            streamWriter.WriteLine(text);
            streamWriter.Close();
        }

        public float read_checked()
        {
            if (!File.Exists(GetFullInstallPath() + "/Update/WorldComposer/last_checked.txt"))
            {
                write_checked("-1");
            }
            StreamReader streamReader = File.OpenText(GetFullInstallPath() + "/Update/WorldComposer/last_checked.txt");
            string s = streamReader.ReadLine();
            streamReader.Close();
            float result = 0f;
            float.TryParse(s, out result);
            return result;
        }

        public void content_startup()
        {
            if (read_checked() != DateTime.Now.Day && read_check() > 0)
            {
                check_content_version();
            }
        }

        public void content_version()
        {
            Encoding unicode = Encoding.Unicode;
            global_script.settings.wc_contents.Request(unicode.GetString(process_out(File.ReadAllBytes(GetFullInstallPath() + "/templates/content4.dat"))), false);
            global_script.settings.wc_loading = 2;
        }

        public void check_content_version()
        {
            Encoding unicode = Encoding.Unicode;
            if (global_script)
            {
                global_script.settings.wc_contents.Request(unicode.GetString(process_out(File.ReadAllBytes(GetFullInstallPath() + "/templates/content3.dat"))), false);
                global_script.settings.wc_loading = 1;
            }
        }

        public void import_contents(string path, bool window)
        {
            FileInfo fileInfo = new FileInfo(Application.dataPath + "/tc_build/build.txt");
            if (fileInfo.Exists)
            {
                Debug.Log("Updating canceled because of development version");
            }
            else
            {
                AssetDatabase.Refresh();
                AssetDatabase.ImportPackage(path, window);
                Repaint();
            }
            global_script.settings.update_version2 = false;
            global_script.settings.time_out = (float)EditorApplication.timeSinceStartup;
            global_script.settings.wc_loading = 3;
        }

        public void asc_convert_to_raw(string import_path, string export_path)
        {
            // var ttt1: float = Time.realtimeSinceStartup;
            Vector2 minMax = GetAscMinMax(import_path, export_path);
            float deltaHeight = minMax.y - minMax.x;
            Debug.Log("Height range = " + deltaHeight);
            // return;
            // if (current_area.converter_height == 0) {current_area.converter_height = 9000;}

            String text;
            StreamReader file;
            FileStream export_file;
            int no_data;
            float height;
            char s;
            byte byte_hi;
            byte byte_lo;
            int value_int;
            Vector2 resolution;

            file = new StreamReader(import_path);
            export_file = new FileStream(export_path, FileMode.OpenOrCreate);

            text = file.ReadLine();
            text = text.Replace("ncols", String.Empty);
            resolution.x = Int16.Parse(text);
            // Debug.Log(resolution.x);

            text = file.ReadLine();
            text = text.Replace("nrows", String.Empty);
            resolution.y = Int16.Parse(text);

            // Debug.Log(resolution.y);
            Debug.Log("Heightmap resolution X:" + resolution.x + " Y:" + resolution.y);

            current_area.converter_resolution = resolution;

            text = file.ReadLine();
            // Debug.Log(text);
            text = file.ReadLine();
            // Debug.Log(text);
            text = file.ReadLine();
            // Debug.Log(text);

            text = file.ReadLine();
            text = text.Replace("nodata_value", String.Empty);
            text = text.Replace("NODATA_value", String.Empty);
            no_data = Int16.Parse(text);
            // Debug.Log(text);

            text = String.Empty;

            do
            {
                s = (char)file.Read();
                if (s == 32)
                {
                    text = text.Replace(",", ".");
                    Single.TryParse(text, out height);
                    if (height == no_data) { height = 0; }
                    else
                    {
                        height = (height - minMax.x) * (65535.0f / deltaHeight);
                    }
                    value_int = (int)height;

                    byte_hi = (byte)(value_int >> 8);
                    byte_lo = (byte)(value_int - (byte_hi << 8));

                    export_file.WriteByte(byte_lo);
                    export_file.WriteByte(byte_hi);

                    text = String.Empty;
                }
                text += s;
            }
            while (!file.EndOfStream);

            file.Close();
            export_file.Close();

            Debug.Log("Converted " + import_path + " -> " + export_path);
            notify_text = "Converted " + import_path + " -> " + export_path;
        }

        public Vector2 GetAscMinMax(string import_path, string export_path)
        {
            String text;
            StreamReader file;
            int no_data;
            float height;
            char s;
            Vector2 resolution;
            float min = 99999999999;
            float max = -99999999999;

            file = new StreamReader(import_path);

            text = file.ReadLine();
            text = text.Replace("ncols", String.Empty);
            resolution.x = Int16.Parse(text);

            text = file.ReadLine();
            text = text.Replace("nrows", String.Empty);
            resolution.y = Int16.Parse(text);

            current_area.converter_resolution = resolution;

            text = file.ReadLine();
            text = file.ReadLine();
            text = file.ReadLine();

            text = file.ReadLine();
            text = text.Replace("nodata_value", String.Empty);
            text = text.Replace("NODATA_value", String.Empty);
            no_data = Int16.Parse(text);

            text = String.Empty;

            do
            {
                s = (char)file.Read();
                if (s == 32)
                {
                    text = text.Replace(",", ".");
                    Single.TryParse(text, out height);
                    if (height == no_data) { height = 0; }
                    else
                    {
                        if (height > max) max = height;
                        else if (height < min) min = height;
                    }
                    text = String.Empty;
                }
                text += s;
            }
            while (!file.EndOfStream);

            file.Close();
            Debug.Log("Minimum height = " + min + " Maximum height = " + max);

            return new Vector2(min, max);
        }

        public bool check_in_rect()
        {
            return (map_parameters_rect.Contains(key.mousePosition) && map.button_parameters) || (regions_rect.Contains(key.mousePosition) && map.button_region) || (areas_rect.Contains(key.mousePosition) && map.button_region) || (heightmap_export_rect.Contains(key.mousePosition) && map.button_heightmap_export) || (image_export_rect.Contains(key.mousePosition) && map.button_image_export) || (image_editor_rect.Contains(key.mousePosition) && map.button_image_editor) || (converter_rect.Contains(key.mousePosition) && map.button_converter) || (settings_rect.Contains(key.mousePosition) && map.button_settings) || (rectWindow.Contains(key.mousePosition) && map.button_create_terrain) || (update_rect.Contains(key.mousePosition) && map.button_update);
        }

        public void create_info_window()
        {
            if (info_window) { info_window.Close(); return; }
            info_window = (Info_tc)EditorWindow.GetWindow(typeof(Info_tc));
            info_window.global_script = global_script;

            info_window.backgroundColor = new Color(0, 0, 0, 0.5f);

            info_window.text = String.Empty;
            StreamReader sr = File.OpenText(GetFullInstallPath() + "/WorldComposer Release Notes.txt");

            info_window.update_height = 0;
            sr.ReadLine();
            sr.ReadLine();
            sr.ReadLine();

            do
            {
                info_window.text += sr.ReadLine() + "\n";
                info_window.update_height += 13;
            }
            while (!sr.EndOfStream);
            sr.Close();

            info_window.update_height += 13;
            info_window.minSize = new Vector2(1024, 512);
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_01 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		info_window.title = "Release Notes";
#else
            info_window.titleContent = new GUIContent("Release Notes");
#endif
            info_window.parent = this;
        }

        public void smooth_terrain(terrain_class2 preterrain1, float strength)
        {
            if (preterrain1.terrain)
            {
                int heightmapResolution = preterrain1.terrain.terrainData.heightmapResolution;
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = 0f;
                float num6 = 1;
                float num7 = 1;
                heights = preterrain1.terrain.terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
                for (int i = 0; i < 1; i++)
                {
                    for (int j = 0; j < heightmapResolution; j++)
                    {
                        for (int k = 1; k < heightmapResolution - 1; k++)
                        {
                            num3 = heights[k - 1, j];
                            num = heights[k, j];
                            num4 = heights[k + 1, j];
                            num2 = num - (num3 + num4) / 2;
                            num2 *= 1 - strength * num6 * num7;
                            num5 = num2 + (num3 + num4) / 2;
                            heights[k, j] = num5;
                        }
                    }
                    for (int j = 1; j < heightmapResolution - 1; j++)
                    {
                        for (int k = 0; k < heightmapResolution; k++)
                        {
                            num3 = heights[k, j - 1];
                            num = heights[k, j];
                            num4 = heights[k, j + 1];
                            num2 = num - (num3 + num4) / 2;
                            num2 *= 1 - strength * num6 * num7;
                            num5 = num2 + (num3 + num4) / 2;
                            heights[k, j] = num5;
                        }
                    }
                }
                preterrain1.terrain.terrainData.SetHeights(0, 0, heights);
                if (preterrain1.color_terrain[0] < 1.5f)
                {
                    preterrain1.color_terrain += new Color(0.5f, 0.5f, 1, 0.5f);
                }
            }
        }

        public void smooth_all_terrain(float strength)
        {
            for (int i = 0; i < terrain_region.area[0].terrains.Count; i++)
            {
                smooth_terrain(terrain_region.area[0].terrains[i], strength);
            }
            heights = (float[,])Array.CreateInstance(typeof(float), new int[2]);
        }

        public void save_global_settings()
        {
            if (global_script)
            {
                EditorUtility.SetDirty(global_script);
                AssetDatabase.SaveAssets();
            }
        }

        public void CreateRTPCombinedTextures(terrain_class2 preterrain1, Type globalSettingsHolderType, object globalSettingsHolder)
        {
            globalSettingsHolderType.GetMethod("RefreshAll").Invoke(globalSettingsHolder, null);

            bool heightTexturesCreated = true;

            bool normalTexturesCreated = (bool)globalSettingsHolderType.GetMethod("PrepareNormals").Invoke(globalSettingsHolder, null);

            if (!normalTexturesCreated)
            {
                notify_text = "RTP needs the Normal Textures to be readable and they have to be the same size, pleace adjust this in the image import settings";
                return;
            }

            int layerCount = (int)globalSettingsHolderType.GetField("numLayers").GetValue(globalSettingsHolder);
            Texture2D[] texHeights = (Texture2D[])globalSettingsHolderType.GetField("Heights").GetValue(globalSettingsHolder);

            Texture2D texHeightmap0 = RTPMethods.PrepareHeights(0, layerCount, texHeights);
            if (texHeightmap0 != null)
            {
                globalSettingsHolderType.GetField("HeightMap").SetValue(globalSettingsHolder, texHeightmap0);

                if (layerCount > 4)
                {
                    Texture2D texHeightmap2 = RTPMethods.PrepareHeights(4, layerCount, texHeights);
                    if (texHeightmap2 != null)
                    {
                        globalSettingsHolderType.GetField("HeightMap2").SetValue(globalSettingsHolder, texHeightmap2);
                        if (layerCount > 8)
                        {
                            Texture2D texHeightmap3 = RTPMethods.PrepareHeights(8, layerCount, texHeights);
                            if (texHeightmap3 != null) globalSettingsHolderType.GetField("HeightMap3").SetValue(globalSettingsHolder, texHeightmap3);
                            else heightTexturesCreated = false;
                        }
                    }
                    else heightTexturesCreated = false;
                }
            }
            else heightTexturesCreated = false;

            if (!heightTexturesCreated)
            {
                notify_text = "RTP needs the Height Textures to be readable and they have to be the same size, pleace adjust this in the image import settings";
                return;
            }
        }

        public void load_button_textures()
        {
            button_settings = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_settings.png", typeof(Texture));
            button_help = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_help.png", typeof(Texture));
            button_discord = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_discord.png", typeof(Texture));
            button_heightmap = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_heightmap.png", typeof(Texture));
            button_update = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_update.png", typeof(Texture));
            button_terrain = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_terrain.png", typeof(Texture));
            button_map = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_map.png", typeof(Texture));
            button_region = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_region.png", typeof(Texture));
            button_edit = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_edit.png", typeof(Texture));
            button_image = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_splatmap.png", typeof(Texture));
            button_converter = (Texture)AssetDatabase.LoadAssetAtPath(install_path + "/Buttons/button_convert.png", typeof(Texture));
        }

        public void reexports()
        {
            stop_all_elevation_pull();
            StopAllImagePull();
        }

        public bool check_area_resize()
        {
            bool arg_5F_0;
            if (!area_rounded)
            {
                if (notify_text.Length != 0)
                {
                    notify_text += "\n\n";
                }
                notify_text += "The tiles are not fitting in the Area. Please resize the area";
                map.mode = 2;
                arg_5F_0 = true;
            }
            else
            {
                arg_5F_0 = false;
            }
            return arg_5F_0;
        }

        public void SnapArea(latlong_class latlong1, latlong_class latlong2, float snapValue)
        {
            double latitude = latlong1.latitude;
            double longitude = latlong1.longitude;
            double num = (latlong2.latitude - latlong1.latitude) / 3;
            double num2 = (latlong2.longitude - latlong1.longitude) / 3;
            latlong1.latitude = Mathf.Round((float)(latlong1.latitude / num)) * num;
            latlong1.longitude = Mathf.Round((float)(latlong1.longitude / num2)) * num2;
            latlong2.latitude += latlong1.latitude - latitude;
            latlong2.longitude += latlong1.longitude - longitude;
        }

        float NormalizeHeightmap(Vector2 resolution, String file)
        {
            int bufferSize = 2048;
            byte[] bytes = new byte[bufferSize];
            byte[] bytes2 = new byte[bufferSize];
            FileStream fr;
            FileStream fw;
            float minHeight = 99999999;
            float maxHeight = -99999999;
            int length;
            float height;
            float heightRange;
            int i;
            int value_int;
            byte byte_hi;
            byte byte_lo;

            if (!File.Exists(file))
            {
                notify_text = "The heightmap file doesn't exist, please export it by clicking the 'Export Heightmap' button in the 'Heightmap Export' tab.";
                Debug.Log("The heightmap file doesn't exist, please export it by clicking the 'Export Heightmap' button in the 'Heightmap Export' tab.");
            }

            fr = new FileStream(file, FileMode.Open);

            do
            {
                length = fr.Read(bytes, 0, bufferSize);
                for (i = 0; i < length; i += 2)
                {
                    height = (bytes[i + 1] * 255) + bytes[i];
                    if (height > maxHeight) maxHeight = height;
                    else if (height < minHeight) minHeight = height;
                }
            }
            while (length != 0);

            fr.Position = 0;
            fw = new FileStream(file.Replace(".Raw", "_N.Raw"), FileMode.OpenOrCreate);
            heightRange = maxHeight - minHeight;

            do
            {
                length = fr.Read(bytes, 0, bufferSize);
                for (i = 0; i < length; i += 2)
                {
                    height = (bytes[i + 1] * 255) + bytes[i];
                    height = ((height - minHeight) / heightRange) * 65535;

                    value_int = (int)height;

                    byte_hi = (byte)(value_int >> 8);
                    byte_lo = (byte)(value_int - (byte_hi << 8));

                    bytes2[i] = byte_lo;
                    bytes2[i + 1] = byte_hi;
                }
                fw.Write(bytes2, 0, length);
            }
            while (length != 0);

            fr.Close();
            fw.Close();

            Debug.Log("Exported normalized heightmap " + file.Replace(".Raw", "_N.Raw"));
            Debug.Log("Minimum height: " + GetRawHeight(minHeight) + " Maximum height: " + GetRawHeight(maxHeight));
            Debug.Log("Height of the terrain with real world scale: " + (GetRawHeight(heightRange) + 1000));
            return (GetRawHeight(heightRange) + 1000);
        }

        public float GetRawHeight(float height)
        {
            return height / 6.5535f - 1000;
        }
    }
}