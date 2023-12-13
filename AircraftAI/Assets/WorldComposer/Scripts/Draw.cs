#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WorldComposer
{
    static public class Draw
    {
        static public Texture2D tex2;
        static public Texture2D tex3;
        static public Texture2D tex4;

        static public void InitTextures(global_settings_tc globalScript)
        {
            if (tex2 == null) CreateTexture(ref tex2, globalScript.map.backgroundColor);
            if (tex3 == null) CreateTexture(ref tex3, globalScript.map.titleColor);
            if (tex4 == null) CreateTexture(ref tex4, Color.yellow);
        }

        static public void DisposeTextures()
        {
            DisposeTexture(ref tex2);
            DisposeTexture(ref tex3);
            DisposeTexture(ref tex4);
        }

        static public void DisposeTexture(ref Texture2D tex)
        {
            if (tex == null) return;

            Object.DestroyImmediate(tex);
            tex = null;
        }

        static public void CreateTexture(ref Texture2D tex, Color color)
        {
            if (tex != null) return;

            tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
        }

        static public void drawGUIBox(Rect rect, string text, Color backgroundColor, Color highlightColor, Color textColor, Texture tex2, Texture tex3)
        {
            GUI.color = new Color(1, 1, 1, backgroundColor.a);
            EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y + 19, rect.width, rect.height - 19), tex2);
            // GUI.color = highlightColor;
            GUI.color = new Color(1, 1, 1, highlightColor.a);
            EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y, rect.width, 19), tex3);

            GUI.color = textColor;
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, rect.width, 19), text, EditorStyles.boldLabel);
            GUI.color = Color.white;
        }

        static public Vector2 drawText(string text, Vector2 pos, bool background, Color color, Color backgroundColor, float rotation, float fontSize, bool bold, int mode)
        {
            Matrix4x4 identity = Matrix4x4.identity;
            Matrix4x4 identity2 = Matrix4x4.identity;
            int fontSize2 = GUI.skin.label.fontSize;
            FontStyle fontStyle = GUI.skin.label.fontStyle;
            Color color2 = GUI.color;
            GUI.skin.label.fontSize = (int)fontSize;
            if (bold)
            {
                GUI.skin.label.fontStyle = FontStyle.Bold;
            }
            else
            {
                GUI.skin.label.fontStyle = FontStyle.Normal;
            }
            Vector2 result = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(text));
            identity2.SetTRS(new Vector3(pos.x, pos.y, 0), Quaternion.Euler(0, 0, rotation), Vector3.one);
            if (mode == 1)
            {
                GUI.matrix = identity2;
            }
            else if (mode == 2)
            {
                identity.SetTRS(new Vector3(-result.x / 2, -result.y, 0), Quaternion.identity, Vector3.one);
                GUI.matrix = identity2 * identity;
            }
            else if (mode == 3)
            {
                identity.SetTRS(new Vector3(0, -result.y, 0), Quaternion.identity, Vector3.one);
                GUI.matrix = identity2 * identity;
            }
            else if (mode == 4)
            {
                identity.SetTRS(new Vector3(-result.x / 2, -result.y / 2, 0), Quaternion.identity, Vector3.one);
                GUI.matrix = identity2 * identity;
            }
            else if (mode == 5)
            {
                identity.SetTRS(new Vector3(-result.x, 0, 0), Quaternion.identity, Vector3.one);
                GUI.matrix = identity2 * identity;
            }
            else if (mode == 6)
            {
                identity.SetTRS(new Vector3(-result.x, -result.y, 0), Quaternion.identity, Vector3.one);
                GUI.matrix = identity2 * identity;
            }
            if (background)
            {
                GUI.color = backgroundColor;
                if (!tex4)
                {
                    tex4 = new Texture2D(1, 1);
                }
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, result.x, result.y), tex4);
            }
            GUI.color = color;
            GUI.Label(new Rect(0, 0, result.x, result.y), text);
            GUI.skin.label.fontSize = fontSize2;
            GUI.skin.label.fontStyle = fontStyle;
            GUI.color = color2;
            GUI.matrix = Matrix4x4.identity;
            return result;
        }

        static public bool drawText(Rect rect, edit_class edit, bool background, Color color, Color backgroundColor, float fontSize, bool bold, int mode)
        {
            Vector2 vector = new Vector2();
            int fontSize2;
            FontStyle fontStyle;
            Color color2 = GUI.color;
            Vector2 size = new Vector2();
            if (background)
            {
                GUI.color = backgroundColor;
                EditorGUI.DrawPreviewTexture(new Rect(vector.x, vector.y, size.x, size.y), tex2);
            }
            GUI.color = color;
            if (!edit.edit)
            {
                fontSize2 = GUI.skin.label.fontSize;
                fontStyle = GUI.skin.label.fontStyle;
                GUI.skin.label.fontSize = (int)fontSize;
                if (bold)
                {
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                }
                else
                {
                    GUI.skin.label.fontStyle = FontStyle.Normal;
                }
                size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(edit.default_text));
                vector = Mathw.calc_rect_allign(rect, size, mode);
                GUI.Label(new Rect(vector.x, vector.y, size.x, size.y), edit.default_text);
                GUI.skin.label.fontSize = fontSize2;
                GUI.skin.label.fontStyle = fontStyle;
            }
            else
            {
                fontSize2 = GUI.skin.textField.fontSize;
                fontStyle = GUI.skin.textField.fontStyle;
                GUI.skin.textField.fontSize = (int)fontSize;
                if (bold)
                {
                    GUI.skin.textField.fontStyle = FontStyle.Bold;
                }
                else
                {
                    GUI.skin.textField.fontStyle = FontStyle.Normal;
                }
                size = GUI.skin.GetStyle("TextField").CalcSize(new GUIContent(edit.text));
                if (size.x < rect.width)
                {
                    size.x = rect.width;
                }
                size.x += 10;
                vector = Mathw.calc_rect_allign(rect, size, mode);
                edit.text = GUI.TextField(new Rect(vector.x, vector.y, size.x, size.y), edit.text);
                GUI.skin.textField.fontSize = fontSize2;
                GUI.skin.textField.fontStyle = fontStyle;
            }
            if (Event.current.button == 0 && Event.current.clickCount == 2 && Event.current.type == 0 && new Rect(vector.x, vector.y, size.x, size.y).Contains(Event.current.mousePosition))
            {
                edit.edit = !edit.edit;
            }
            bool arg_330_0;
            if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape)
            {
                edit.edit = false;
                GUI.color = color2;
                arg_330_0 = true;
            }
            else
            {
                GUI.color = color2;
                arg_330_0 = false;
            }
            return arg_330_0;
        }

        static public void drawText(Rect rect, string text, bool background, Color color, Color backgroundColor, float fontSize, bool bold, int mode)
        {
            Vector2 vector = new Vector2();
            int fontSize2 = GUI.skin.label.fontSize;
            FontStyle fontStyle = GUI.skin.label.fontStyle;
            Color color2 = GUI.color;
            Vector2 size = new Vector2();
            if (background)
            {
                GUI.color = backgroundColor;
                EditorGUI.DrawPreviewTexture(new Rect(vector.x, vector.y, size.x, size.y), tex2);
            }
            GUI.color = color;
            GUI.skin.label.fontSize = (int)fontSize;
            if (bold)
            {
                GUI.skin.label.fontStyle = FontStyle.Bold;
            }
            else
            {
                GUI.skin.label.fontStyle = FontStyle.Normal;
            }
            size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(text));
            vector = Mathw.calc_rect_allign(rect, size, mode);
            GUI.Label(new Rect(vector.x, vector.y, size.x, size.y), text);
            GUI.skin.label.fontSize = fontSize2;
            GUI.skin.label.fontStyle = fontStyle;
            GUI.color = color2;
        }

        static public bool drawGUIBox(Rect rect, edit_class edit, float fontSize, bool label2, float labelHeight, Color backgroundColor, Color highlightColor, Color highlightColor2, Color textColor, bool border, int width, Rect screen, bool select, Color select_color, bool active)
        {
            if (!select)
            {
                highlightColor += new Color(-0.3f, -0.3f, -0.3f);
                highlightColor2 += new Color(-0.3f, -0.3f, -0.3f);
            }
            GUI.color = highlightColor;
            EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y, rect.width, labelHeight), tex2);
            bool result = drawText(rect, edit, false, textColor, new Color(0.1f, 0.1f, 0.1f, 1), fontSize, true, 6);
            if (label2)
            {
                GUI.color = highlightColor2;
                EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.yMax - labelHeight, rect.width, labelHeight), tex2);
                GUI.color = Color.white;
                if (!active)
                {
                    Drawing_tc1.DrawLine(new Vector2(rect.x + 1, rect.y + labelHeight + 1), new Vector2(rect.xMax - 1, rect.yMax - labelHeight - 1), new Color(1, 0, 0, 0.7f), 3, false, screen);
                    Drawing_tc1.DrawLine(new Vector2(rect.x + 1, rect.yMax - labelHeight - 1), new Vector2(rect.xMax - 1, rect.y + labelHeight + 1), new Color(1, 0, 0, 0.7f), 3, false, screen);
                }
            }
            else if (!active)
            {
                Drawing_tc1.DrawLine(new Vector2(rect.x + 1, rect.y + labelHeight + 1), new Vector2(rect.xMax - 1, rect.yMax - 1), new Color(1, 0, 0, 0.7f), 3, false, screen);
                Drawing_tc1.DrawLine(new Vector2(rect.x + 1, rect.yMax - 1), new Vector2(rect.xMax - 1, rect.y + labelHeight + 1), new Color(1, 0, 0, 0.7f), 3, false, screen);
            }
            if (border)
            {
                DrawRect(rect, highlightColor, width, screen);
                Drawing_tc1.DrawLine(new Vector2(rect.x, rect.y + labelHeight), new Vector2(rect.xMax, rect.y + labelHeight), highlightColor, width, false, screen);
                if (label2)
                {
                    Drawing_tc1.DrawLine(new Vector2(rect.x, rect.yMax - labelHeight), new Vector2(rect.xMax, rect.yMax - labelHeight), highlightColor, width, false, screen);
                }
            }
            GUI.color = Color.white;
            return result;
        }

        static public void drawJoinNode(Rect rect, int length, string text, float fontSize, bool label2, float labelHeight, Color backgroundColor, Color highlightColor, Color highlightColor2, Color textColor, bool border, int width, Rect screen, bool select, Color select_color, bool active, float zoom)
        {
            if (!select)
            {
                highlightColor += new Color(-0.3f, -0.3f, -0.3f);
                highlightColor2 += new Color(-0.3f, -0.3f, -0.3f);
            }
            GUI.color = highlightColor;
            for (int i = 0; i < length; i++)
            {
                EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y + i * zoom, rect.width, labelHeight), tex2);
            }
            for (int i = 0; i < length; i++)
            {
                if (i < length - 1)
                {
                    Drawing_tc1.DrawLine(new Vector2(rect.x, rect.y + (i + 1) * zoom), new Vector2(rect.xMax, rect.y + (i + 1) * zoom), highlightColor, width, false, screen);
                }
            }
            drawText(rect, text, false, textColor, new Color(0.1f, 0.1f, 0.1f, 1), fontSize, true, 6);
            if (border)
            {
                DrawRect(new Rect(rect.x, rect.y, rect.width, (float)length * zoom), highlightColor, width, screen);
            }
            GUI.color = Color.white;
        }

        static public int label_width(string text, bool bold)
        {
            Vector2 vector = new Vector2();
            if (bold)
            {
                FontStyle fontStyle = GUI.skin.label.fontStyle;
                GUI.skin.label.fontStyle = FontStyle.Bold;
                vector = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(text));
                GUI.skin.label.fontStyle = fontStyle;
            }
            else
            {
                vector = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(text));
            }
            return (int)vector.x;
        }

        static public void DrawRect(Rect rect, Color color, float width, Rect screen)
        {
            Drawing_tc1.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), color, width, false, screen);
            Drawing_tc1.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), color, width, false, screen);
            Drawing_tc1.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), color, width, false, screen);
            Drawing_tc1.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), color, width, false, screen);
        }

        static public void draw_arrow(Vector2 point1, int length, int length_arrow, float rotation, Color color, int width, Rect screen)
        {
            length_arrow = (int)(Mathf.Sqrt(2f) * length_arrow);
            Vector2 vector = Mathw.calc_rotation_pixel(point1.x, point1.y - length, point1.x, point1.y, rotation);
            Vector2 pointB = Mathw.calc_rotation_pixel(vector.x - length_arrow, vector.y - length_arrow, vector.x, vector.y, -180 + rotation);
            Vector2 pointB2 = Mathw.calc_rotation_pixel(vector.x + length_arrow, vector.y - length_arrow, vector.x, vector.y, 180 + rotation);
            Drawing_tc1.DrawLine(point1, vector, color, width, false, screen);
            Drawing_tc1.DrawLine(vector, pointB, color, width, false, screen);
            Drawing_tc1.DrawLine(vector, pointB2, color, width, false, screen);
        }

        static public bool draw_latlong_raster(latlong_class center, latlong_class latlong1, latlong_class latlong2, Vector2 offset, double zoom, double current_zoom, int resolution, Rect screen, Color color, int width)
        {
            // map_latlong_center
            bool result = true;
            Vector2 vector = Mathw.latlong_to_pixel(latlong1, center, current_zoom, new Vector2(screen.width, screen.height));
            Vector2 vector2 = Mathw.latlong_to_pixel(latlong2, center, current_zoom, new Vector2(screen.width, screen.height));
            Vector2 vector3 = vector2 - vector;
            vector += new Vector2(-offset.x, offset.y);
            vector2 += new Vector2(-offset.x, offset.y);
            double num = Mathf.Pow(2, (float)(zoom - current_zoom));
            float num2 = (float)(resolution / num);
            if (Mathf.Abs(Mathf.Round(vector3.x / num2) - vector3.x / num2) > 0.01f || Mathf.Abs(Mathf.Round(vector3.y / num2) - vector3.y / num2) > 0.01f)
            {
                result = false;
                color = Color.red;
            }
            for (float num3 = vector.x; num3 < vector.x + vector3.x; num3 += num2)
            {
                Drawing_tc1.DrawLine(new Vector2(num3, vector.y), new Vector2(num3, vector2.y), color, width, false, screen);
            }
            for (float num4 = vector.y; num4 < vector.y + vector3.y; num4 += num2)
            {
                Drawing_tc1.DrawLine(new Vector2(vector.x, num4), new Vector2(vector2.x, num4), color, width, false, screen);
            }
            return result;
        }

        static public void draw_grid(Rect rect, int tile_x, int tile_y, Color color, int width, Rect screen)
        {
            Vector2 vector = new Vector2();
            vector.x = rect.width / tile_x;
            vector.y = rect.height / tile_y;
            for (float num = rect.x; num <= rect.xMax + vector.x / 2; num += vector.x)
            {
                Drawing_tc1.DrawLine(new Vector2(num, rect.y), new Vector2(num, rect.yMax), color, width, false, screen);
            }
            for (float num2 = rect.y; num2 <= rect.yMax + vector.y / 2; num2 += vector.y)
            {
                Drawing_tc1.DrawLine(new Vector2(rect.x, num2), new Vector2(rect.xMax, num2), color, width, false, screen);
            }
        }

        static public void draw_scale_grid(Rect rect, Vector2 offset, float zoom, float scale, Color color, int width, bool draw_center, Rect screen)
        {
            Vector2 vector = new Vector2(screen.width, screen.height) / 2 + offset;
            float num2 = vector.x - rect.x;
            float num3 = vector.y - rect.y;
            int num4 = (int)(num2 / zoom);
            num4 = (int)(num2 - num4 * zoom);
            num4 = (int)(num4 + rect.x);
            int num5 = Mathw.calc_rest_value((vector.x - num4) / zoom, 10);
            if (num5 < 0)
            {
                num5 = -9 - num5;
            }
            else
            {
                num5 = 9 - num5;
            }
            int num7 = (int)(num3 / zoom);
            num7 = (int)(num3 - num7 * zoom);
            num7 = (int)(num7 + rect.y);
            for (float num8 = num4; num8 <= rect.xMax; num8 += zoom)
            {
                Drawing_tc1.DrawLine(new Vector2(num8, rect.y), new Vector2(num8, rect.yMax), color, width, false, screen);
                if (num5 > 9)
                {
                    num5 = 0;
                }
                num5++;
            }
            for (float num9 = num7; num9 <= rect.yMax; num9 += zoom)
            {
                Drawing_tc1.DrawLine(new Vector2(rect.x, num9), new Vector2(rect.xMax, num9), color, width, false, screen);
            }
            if (draw_center)
            {
                Drawing_tc1.DrawLine(new Vector2(vector.x, rect.y), new Vector2(vector.x, rect.yMax), color, width + 2, false, screen);
                Drawing_tc1.DrawLine(new Vector2(rect.x, vector.y), new Vector2(rect.xMax, vector.y), color, width + 2, false, screen);
            }
        }

        static public void set_image_import_settings(string path, bool read, TextureImporterFormat format, TextureWrapMode wrapmode, int maxsize, bool mipmapEnabled, FilterMode filterMode, int anisoLevel, int mode)
        {
            if (path.Length != 0)
            {
                path = path.Replace(Application.dataPath, "Assets");
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                bool flag = false;
                if (textureImporter)
                {
                    if ((mode & 1) != 0 && textureImporter.isReadable != read)
                    {
                        textureImporter.isReadable = read;
                        flag = true;
                    }
                    #if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
                    if ((mode & 2) != 0 && textureImporter.textureFormat != format)
                    {
                        textureImporter.textureFormat = format;
                        flag = true;
                    }
                    #endif
                    if ((mode & 4) != 0 && textureImporter.wrapMode != wrapmode)
                    {
                        textureImporter.wrapMode = wrapmode;
                        flag = true;
                    }
                    if ((mode & 8) != 0 && textureImporter.maxTextureSize != maxsize)
                    {
                        textureImporter.maxTextureSize = maxsize;
                        flag = true;
                    }
                    if ((mode & 16) != 0 && textureImporter.mipmapEnabled != mipmapEnabled)
                    {
                        textureImporter.mipmapEnabled = mipmapEnabled;
                        flag = true;
                    }
                    if ((mode & 32) != 0 && textureImporter.filterMode != filterMode)
                    {
                        textureImporter.filterMode = filterMode;
                        flag = true;
                    }
                    if ((mode & 64) != 0 && textureImporter.anisoLevel != anisoLevel)
                    {
                        textureImporter.anisoLevel = anisoLevel;
                        flag = true;
                    }
                    if (flag)
                    {
                        AssetDatabase.ImportAsset(path);
                    }
                }
                else
                {
                    Debug.Log("Texture Importer can't find " + path);
                }
            }
        }
    }
}
#endif