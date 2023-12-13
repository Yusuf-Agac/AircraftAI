#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;

namespace WorldComposer
{
    public static class Drawing_tc1
    {
        [Serializable]
        public class clip_class
        {
            public float u1;
            public float u2;

            public clip_class(float start, float end)
            {
                u1 = start;
                u2 = end;
            }
        }

        [Serializable]
        public class point_class
        {
            public Vector2 p1;
            public Vector2 p2;

            public point_class(Vector2 start, Vector2 end)
            {
                p1 = start;
                p2 = end;
            }
        }

        public static Texture2D aaLineTex;
        public static Texture2D lineTex;
        public static bool clippingEnabled = true;
        public static Rect clippingBounds;
        public static Material lineMaterial;

        static Drawing_tc1()
        {
            aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
            aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
            aaLineTex.SetPixel(0, 1, Color.white);
            aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
            aaLineTex.Apply();
            lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
            lineTex.SetPixel(0, 1, Color.white);
            lineTex.Apply();
        }

        public static void DrawLineMac(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            Color color2 = GUI.color;
            Matrix4x4 matrix = GUI.matrix;
            float num = width;
            if (antiAlias)
            {
                width *= 3;
            }
            float num2 = Vector3.Angle(pointB - pointA, Vector2.right) * ((pointA.y > pointB.y) ? -1 : 1);
            float magnitude = (pointB - pointA).magnitude;
            if (magnitude > 0.01f)
            {
                Vector3 vector = new Vector3(pointA.x, pointA.y, 0);
                Vector3 vector2 = new Vector3((pointB.x - pointA.x) * 0.5f, (pointB.y - pointA.y) * 0.5f, 0);
                Vector3 vector3 = Vector3.zero;
                if (antiAlias)
                {
                    vector3 = new Vector3(-num * 1.5f * Mathf.Sin(num2 * 0.0174532924f), num * 1.5f * Mathf.Cos(num2 * 0.0174532924f));
                }
                else
                {
                    vector3 = new Vector3(-num * 0.5f * Mathf.Sin(num2 * 0.0174532924f), num * 0.5f * Mathf.Cos(num2 * 0.0174532924f));
                }
                GUI.color = color;
                GUI.matrix = translationMatrix(vector) * GUI.matrix;
                GUIUtility.ScaleAroundPivot(new Vector2(magnitude, width), new Vector2(-0.5f, 0));
                GUI.matrix = translationMatrix(-vector) * GUI.matrix;
                GUIUtility.RotateAroundPivot(num2, Vector2.zero);
                GUI.matrix = translationMatrix(vector - vector3 - vector2) * GUI.matrix;
                if (antiAlias)
                {
                    GUI.DrawTexture(new Rect(0, 0, 1, 1), Drawing_tc1.aaLineTex);
                }
                else
                {
                    GUI.DrawTexture(new Rect(0, 0, 1, 1), Drawing_tc1.lineTex);
                }
            }
            GUI.matrix = matrix;
            GUI.color = color2;
        }

        public static void DrawLineWindows(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            Color color2 = GUI.color;
            Matrix4x4 matrix = GUI.matrix;
            if (antiAlias)
            {
                width *= 3;
            }
            float num = Vector3.Angle(pointB - pointA, Vector2.right) * ((pointA.y > pointB.y) ? -1 : 1);
            float magnitude = (pointB - pointA).magnitude;
            Vector3 vector = new Vector3(pointA.x, pointA.y, 0);
            GUI.color = color;
            GUI.matrix = translationMatrix(vector) * GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(magnitude, width), new Vector2(-0.5f, 0));
            GUI.matrix = translationMatrix(-vector) * GUI.matrix;
            GUIUtility.RotateAroundPivot(num, new Vector2(0, 0));
            GUI.matrix = translationMatrix(vector + new Vector3(width / 2, -magnitude / 2) * Mathf.Sin(num * 0.0174532924f)) * GUI.matrix;
            if (!antiAlias)
            {
                GUI.DrawTexture(new Rect(0, 0, 1, 1), lineTex);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, 1, 1), aaLineTex);
            }
            GUI.matrix = matrix;
            GUI.color = color2;
        }

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias, Rect screen)
        {
            clippingBounds = screen;
            DrawLine(pointA, pointB, color, width);
        }

        public static void curveOutIn(Rect wr, Rect wr2, Color color, Color shadow, int width, Rect screen)
        {
            BezierLine(new Vector2(wr.x + wr.width, wr.y + width + wr.height / 2), new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + width + wr.height / 2), new Vector2(wr2.x, wr2.y + width + wr2.height / 2), new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + width + wr2.height / 2), shadow, width, true, 20, screen);
            BezierLine(new Vector2(wr.x + wr.width, wr.y + wr.height / 2), new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2), new Vector2(wr2.x, wr2.y + wr2.height / 2), new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + wr2.height / 2), color, width, true, 20, screen);
        }

        public static void BezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments, Rect screen)
        {
            Vector2 pointA = cubeBezier(start, startTangent, end, endTangent, 0);
            for (int i = 1; i <= segments; i++)
            {
                Vector2 vector = cubeBezier(start, startTangent, end, endTangent, (i / segments));
                DrawLine(pointA, vector, color, width, antiAlias, screen);
                pointA = vector;
            }
        }

        public static Vector2 cubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float num = 1 - t;
            return s * num * num * num + 3 * st * num * num * t + 3 * et * num * t * t + e * t * t * t;
        }

        public static Matrix4x4 translationMatrix(Vector3 v)
        {
            return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
        }

        public static bool clip_test(float p, float q, clip_class u)
        {
            float num;
            bool result = true;
            if (p < 0)
            {
                num = q / p;
                if (num > u.u2)
                {
                    result = false;
                }
                else if (num > u.u1)
                {
                    u.u1 = num;
                }
            }
            else if (p > 0)
            {
                num = q / p;
                if (num < u.u1)
                {
                    result = false;
                }
                else if (num < u.u2)
                {
                    u.u2 = num;
                }
            }
            else if (q < 0)
            {
                result = false;
            }
            return result;
        }

        public static bool segment_rect_intersection(Rect bounds, point_class p)
        {
            float num = p.p2.x - p.p1.x;
            float num2;
            clip_class clip_class = new clip_class(0, 1f);
            int arg_173_0;
            if (clip_test(-num, p.p1.x - bounds.xMin, clip_class) && clip_test(num, bounds.xMax - p.p1.x, clip_class))
            {
                num2 = p.p2.y - p.p1.y;
                if (clip_test(-num2, p.p1.y - bounds.yMin, clip_class) && clip_test(num2, bounds.yMax - p.p1.y, clip_class))
                {
                    if (clip_class.u2 < 1f)
                    {
                        p.p2.x = p.p1.x + clip_class.u2 * num;
                        p.p2.y = p.p1.y + clip_class.u2 * num2;
                    }
                    if (clip_class.u1 > 0)
                    {
                        p.p1.x = p.p1.x + clip_class.u1 * num;
                        p.p1.y = p.p1.y + clip_class.u1 * num2;
                    }
                    arg_173_0 = 1;
                    return arg_173_0 != 0;
                }
            }
            arg_173_0 = 0;
            return arg_173_0 != 0;
        }

        public static void BeginGroup(Rect position)
        {
            clippingEnabled = true;
            clippingBounds = new Rect(0, 0, position.width, position.height);
            GUI.BeginGroup(position);
        }

        public static void EndGroup()
        {
            GUI.EndGroup();
            clippingBounds = new Rect(0, 0, Screen.width, Screen.height);
            clippingEnabled = false;
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;

            // point_class point_class = new point_class(start, end);
            lineMaterial.SetPass(0);
            Vector3 vector, vector2;

            if (width == 1)
            {
                GL.Begin(1);
                GL.Color(color);
                vector = new Vector3(start.x, start.y, 0);
                vector2 = new Vector3(end.x, end.y, 0);
                GL.Vertex(vector);
                GL.Vertex(vector2);
            }
            else
            {
                GL.Begin(7);
                GL.Color(color);
                vector = new Vector3(end.y, start.x, 0);
                vector2 = new Vector3(start.y, end.x, 0);
                Vector3 vector3 = (vector - vector2).normalized * width / 2f;
                Vector3 vector4 = new Vector3(start.x, start.y, 0);
                Vector3 vector5 = new Vector3(end.x, end.y, 0);
                GL.Vertex(vector4 - vector3);
                GL.Vertex(vector4 + vector3);
                GL.Vertex(vector5 + vector3);
                GL.Vertex(vector5 - vector3);
            }
            GL.End();
        }

        public static void DrawBox(Rect box, Color color, float width)
        {
            Vector2 vector = new Vector2(box.xMin, box.yMin);
            Vector2 vector2 = new Vector2(box.xMax, box.yMin);
            Vector2 vector3 = new Vector2(box.xMax, box.yMax);
            Vector2 vector4 = new Vector2(box.xMin, box.yMax);
            DrawLine(vector, vector2, color, width);
            DrawLine(vector2, vector3, color, width);
            DrawLine(vector3, vector4, color, width);
            DrawLine(vector4, vector, color, width);
        }

        public static void DrawBox(Vector2 topLeftCorner, Vector2 bottomRightCorner, Color color, float width)
        {
            Rect box = new Rect(topLeftCorner.x, topLeftCorner.y, bottomRightCorner.x - topLeftCorner.x, bottomRightCorner.y - topLeftCorner.y);
            DrawBox(box, color, width);
        }

        public static void DrawRoundedBox(Rect box, float radius, Color color, float width)
        {
            Vector2 vector, vector2, vector3, vector4, vector5, vector6, vector7, vector8;
            vector = new Vector2(box.xMin + radius, box.yMin);
            vector2 = new Vector2(box.xMax - radius, box.yMin);
            vector3 = new Vector2(box.xMax, box.yMin + radius);
            vector4 = new Vector2(box.xMax, box.yMax - radius);
            vector5 = new Vector2(box.xMax - radius, box.yMax);
            vector6 = new Vector2(box.xMin + radius, box.yMax);
            vector7 = new Vector2(box.xMin, box.yMax - radius);
            vector8 = new Vector2(box.xMin, box.yMin + radius);
            DrawLine(vector, vector2, color, width);
            DrawLine(vector3, vector4, color, width);
            DrawLine(vector5, vector6, color, width);
            DrawLine(vector7, vector8, color, width);
            Vector2 startTangent;
            Vector2 endTangent;
            float num = radius / 2;
            startTangent = new Vector2(vector8.x, vector8.y + num);
            endTangent = new Vector2(vector.x - num, vector.y);
            DrawBezier(vector8, startTangent, vector, endTangent, color, width);
            startTangent = new Vector2(vector2.x + num, vector2.y);
            endTangent = new Vector2(vector3.x, vector3.y - num);
            DrawBezier(vector2, startTangent, vector3, endTangent, color, width);
            startTangent = new Vector2(vector4.x, vector4.y + num);
            endTangent = new Vector2(vector5.x + num, vector5.y);
            DrawBezier(vector4, startTangent, vector5, endTangent, color, width);
            startTangent = new Vector2(vector6.x - num, vector6.y);
            endTangent = new Vector2(vector7.x, vector7.y + num);
            DrawBezier(vector6, startTangent, vector7, endTangent, color, width);
        }

        public static void DrawConnectingCurve(Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 vector = start - end;
            Vector2 startTangent = start;
            startTangent.x -= (vector / 2).x;
            Vector2 endTangent = end;
            endTangent.x += (vector / 2).x;
            int segments = Mathf.FloorToInt((vector.magnitude / 20) * 3);
            DrawBezier(start, startTangent, end, endTangent, color, width, segments);
        }

        public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width)
        {
            int segments = Mathf.FloorToInt((start - end).magnitude / 20) * 3;
            DrawBezier(start, startTangent, end, endTangent, color, width, segments);
        }

        public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, int segments)
        {
            Vector2 start2 = CubeBezier(start, startTangent, end, endTangent, 0);
            for (int i = 1; i <= segments; i++)
            {
                Vector2 vector = CubeBezier(start, startTangent, end, endTangent, (i / segments));
                DrawLine(start2, vector, color, width);
                start2 = vector;
            }
        }

        public static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float num = 1 - t;
            float num2 = num * t;
            return num * num * num * s + 3 * num * num2 * st + 3 * num2 * t * et + t * t * t * e;
        }
    }
}
#endif
