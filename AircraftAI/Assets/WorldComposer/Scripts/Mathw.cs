using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldComposer
{
    static public class Mathw
    {
        const double minLatitude = -85.05113f;
        const double maxLatitude = 85.05113f;
        const double minLongitude = -180;
        const double maxLongitude = 180;

        static public void Swap<T>(ref T item1, ref T item2)
        {
            T temp = item1;
            item1 = item2;
            item2 = temp;
        }

        static public Vector2 calc_rect_allign(Rect rect, Vector2 size, int mode)
        {
            Vector2 result = new Vector2();
            if (mode == 1)
            {
                result.x = rect.x;
                result.y = rect.y;
            }
            else if (mode == 2)
            {
                result.x = rect.x + rect.width / 2 - size.x / 2;
                result.y = rect.yMax;
            }
            else if (mode == 3)
            {
                result.x = rect.x;
                result.y = rect.yMax;
            }
            else if (mode == 4)
            {
                result.x = rect.x + rect.width / 2 - size.x / 2;
                result.y = rect.y + rect.height / 2 - size.y / 2;
            }
            else if (mode == 5)
            {
                result.x = rect.xMax - size.x;
                result.y = rect.y;
            }
            else if (mode == 6)
            {
                result.x = rect.x + rect.width / 2 - size.x / 2;
                result.y = rect.y - size.y;
            }
            return result;
        }

        static public Vector2 calc_rotation_pixel(float x, float y, float xx, float yy, float rotation)
        {
            Vector2 result = new Vector2(x - xx, y - yy);
            float magnitude = result.magnitude;
            if (magnitude != 0)
            {
                result.x /= magnitude;
                result.y /= magnitude;
            }
            float num = Mathf.Acos(result.x);
            if (result.y < 0)
            {
                num = Mathf.PI * 2 - num;
            }
            num -= rotation * 0.0174532924f;
            result.x = Mathf.Cos(num) * magnitude + xx;
            result.y = Mathf.Sin(num) * magnitude + yy;
            return result;
        }

        static public double clip(double n, double minValue, double maxValue)
        {
            return calcMin(calcMax(n, minValue), maxValue);
        }

        static public latlong_class clip_latlong(latlong_class latlong)
        {
            if (latlong.latitude > maxLatitude)
            {
                latlong.latitude -= maxLatitude * 2;
            }
            else if (latlong.latitude < minLatitude)
            {
                latlong.latitude += maxLatitude * 2;
            }
            if (latlong.longitude > maxLongitude)
            {
                latlong.longitude -= 360;
            }
            else if (latlong.longitude < minLongitude)
            {
                latlong.longitude += 360;
            }
            return latlong;
        }

        static public map_pixel_class clip_pixel(map_pixel_class map_pixel, double zoom)
        {
            double num = 256 * Mathf.Pow(2, (float)zoom);
            if (map_pixel.x > num - 1)
            {
                map_pixel.x -= num - 1;
            }
            else if (map_pixel.x < 0)
            {
                map_pixel.x = num - 1 - map_pixel.x;
            }
            if (map_pixel.y > num - 1)
            {
                map_pixel.y -= num - 1;
            }
            else if (map_pixel.y < 0)
            {
                map_pixel.y = num - 1 - map_pixel.y;
            }
            return map_pixel;
        }

        static public double calcMin(double a, double b)
        {
            return (a >= b) ? b : a;
        }

        static public double calcMax(double a, double b)
        {
            return (a <= b) ? b : a;
        }

        static public int mapSize(int zoom)
        {
            return (int)(Mathf.Pow(2, zoom) * 256);
        }

        static public Vector2 latlong_to_pixel(latlong_class latlong, latlong_class latlong_center, double zoom, Vector2 screen_resolution)
        {
            latlong = clip_latlong(latlong);
            latlong_center = clip_latlong(latlong_center);
            double num = 3.14159274f;
            double num2 = (latlong.longitude + 180) / 360;
            double num3 = Mathf.Sin((float)(latlong.latitude * num / 180));
            double num4 = 0.5f - Mathf.Log((float)((1 + num3) / (1 - num3))) / (4 * num);
            Vector2 vector = new Vector2((float)num2, (float)num4);
            num2 = (latlong_center.longitude + 180) / 360;
            num3 = Mathf.Sin((float)(latlong_center.latitude * num / 180));
            num4 = 0.5f - Mathf.Log((float)((1 + num3) / (1 - num3))) / (4 * num);
            Vector2 vector2 = new Vector2((float)num2, (float)num4);
            Vector2 vector3 = vector - vector2;
            vector3 *= 256 * Mathf.Pow(2, (float)zoom);
            return vector3 + screen_resolution / 2;
        }

        static public map_pixel_class latlong_to_pixel2(latlong_class latlong, double zoom)
        {
            latlong = clip_latlong(latlong);
            double num = 3.14159274f;
            double num2 = (latlong.longitude + 180f) / 360f;
            double num3 = Mathf.Sin((float)(latlong.latitude * num / 180f));
            double num4 = 0.5f - Mathf.Log((float)((1f + num3) / (1f - num3))) / (4f * num);
            num2 *= 256f * Mathf.Pow(2f, (float)zoom);
            num4 *= 256f * Mathf.Pow(2f, (float)zoom);
            return new map_pixel_class
            {
                x = num2,
                y = num4
            };
        }

        static public latlong_class pixel_to_latlong2(map_pixel_class map_pixel, double zoom)
        {
            map_pixel = clip_pixel(map_pixel, zoom);
            double num = 3.14159274f;
            double num2 = 256f * Mathf.Pow(2f, (float)zoom);
            double num3 = map_pixel.x / num2 - 0.5f;
            double num4 = 0.5f - map_pixel.y / num2;
            return new latlong_class
            {
                latitude = 90f - 360f * Mathf.Atan(Mathf.Exp((float)(-(float)num4 * (double)2f * num))) / num,
                longitude = 360f * num3
            };
        }

        static public latlong_class pixel_to_latlong(Vector2 offset, latlong_class latlong_center, double zoom)
        {
            double num = 3.14159274f;
            double num2 = 256 * Mathf.Pow(2, (float)zoom);
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong_center, zoom);
            map_pixel_class map_pixel_class2 = new map_pixel_class();
            map_pixel_class2.x = map_pixel_class.x + offset.x;
            map_pixel_class2.y = map_pixel_class.y + offset.y;
            double num3 = map_pixel_class2.x / num2 - 0.5f;
            double num4 = 0.5f - map_pixel_class2.y / num2;
            return clip_latlong(new latlong_class
            {
                latitude = 90 - 360 * Mathf.Atan(Mathf.Exp((float)(-(float)num4 * (double)2 * num))) / num,
                longitude = 360 * num3
            });
        }

        static public map_pixel_class calc_latlong_area_size(latlong_class latlong1, latlong_class latlong2, latlong_class latlong_center)
        {
            double num = 3.14159274f;
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong1, 19);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(latlong2, 19);
            double num2 = 156543.047f * Mathf.Cos((float)(latlong_center.latitude * (num / 180))) / Mathf.Pow(2, 19);
            return new map_pixel_class
            {
                x = (map_pixel_class2.x - map_pixel_class.x) * num2,
                y = (map_pixel_class2.y - map_pixel_class.y) * num2
            };
        }

        static public double calc_latlong_area_resolution(latlong_class latlong, double zoom)
        {
            double num = 3.14159274f;
            return 156543.047f * Mathf.Cos((float)(latlong.latitude * (num / 180))) / Mathf.Pow(2, (float)zoom);
        }

        static public latlong_area_class calc_latlong_area_rounded(latlong_class latlong1, latlong_class latlong2, double zoom, int resolution, bool square, int mode)
        {
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong1, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(latlong2, zoom);
            map_pixel_class map_pixel_class3 = new map_pixel_class();
            map_pixel_class3.x = Mathf.Round((float)((map_pixel_class2.x - map_pixel_class.x) / resolution)) * resolution;
            if (square)
            {
                map_pixel_class3.y = map_pixel_class3.x;
            }
            else
            {
                map_pixel_class3.y = Mathf.Round((float)((map_pixel_class2.y - map_pixel_class.y) / resolution)) * resolution;
            }
            if (mode == 1)
            {
                if (map_pixel_class.x > map_pixel_class2.x - resolution)
                {
                    map_pixel_class.x = map_pixel_class2.x - resolution;
                }
                else
                {
                    map_pixel_class.x = map_pixel_class2.x - map_pixel_class3.x;
                }
            }
            else if (mode == 2)
            {
                if (map_pixel_class2.x < map_pixel_class.x + resolution)
                {
                    map_pixel_class2.x = map_pixel_class.x + resolution;
                }
                else
                {
                    map_pixel_class2.x = map_pixel_class.x + map_pixel_class3.x;
                }
            }
            else if (mode == 3)
            {
                if (map_pixel_class.y > map_pixel_class2.y - resolution)
                {
                    map_pixel_class.y = map_pixel_class2.y - resolution;
                }
                else
                {
                    map_pixel_class.y = map_pixel_class2.y - map_pixel_class3.y;
                }
            }
            else if (mode == 4)
            {
                if (map_pixel_class2.y < map_pixel_class.y + resolution)
                {
                    map_pixel_class2.y = map_pixel_class.y + resolution;
                }
                else
                {
                    map_pixel_class2.y = map_pixel_class.y + map_pixel_class3.y;
                }
            }
            else if (mode == 5)
            {
                if (map_pixel_class.x > map_pixel_class2.x - resolution)
                {
                    map_pixel_class.x = map_pixel_class2.x - resolution;
                }
                else
                {
                    map_pixel_class.x = map_pixel_class2.x - map_pixel_class3.x;
                }
                if (map_pixel_class.y > map_pixel_class2.y - resolution)
                {
                    map_pixel_class.y = map_pixel_class2.y - resolution;
                }
                else
                {
                    map_pixel_class.y = map_pixel_class2.y - map_pixel_class3.y;
                }
            }
            else if (mode == 6)
            {
                if (map_pixel_class2.x < map_pixel_class.x + resolution)
                {
                    map_pixel_class2.x = map_pixel_class.x + resolution;
                }
                else
                {
                    map_pixel_class2.x = map_pixel_class.x + map_pixel_class3.x;
                }
                if (map_pixel_class.y > map_pixel_class2.y - resolution)
                {
                    map_pixel_class.y = map_pixel_class2.y - resolution;
                }
                else
                {
                    map_pixel_class.y = map_pixel_class2.y - map_pixel_class3.y;
                }
            }
            else if (mode == 7)
            {
                if (map_pixel_class.x > map_pixel_class2.x - resolution)
                {
                    map_pixel_class.x = map_pixel_class2.x - resolution;
                }
                else
                {
                    map_pixel_class.x = map_pixel_class2.x - map_pixel_class3.x;
                }
                if (map_pixel_class2.y < map_pixel_class.y + resolution)
                {
                    map_pixel_class2.y = map_pixel_class.y + resolution;
                }
                else
                {
                    map_pixel_class2.y = map_pixel_class.y + map_pixel_class3.y;
                }
            }
            else if (mode == 8)
            {
                if (map_pixel_class2.x - resolution < map_pixel_class.x)
                {
                    map_pixel_class2.x = map_pixel_class.x + resolution;
                }
                else
                {
                    map_pixel_class2.x = map_pixel_class.x + map_pixel_class3.x;
                }
                if (map_pixel_class2.y - resolution < map_pixel_class.y)
                {
                    map_pixel_class2.y = map_pixel_class.y + resolution;
                }
                else
                {
                    map_pixel_class2.y = map_pixel_class.y + map_pixel_class3.y;
                }
            }
            return new latlong_area_class
            {
                latlong1 = pixel_to_latlong2(map_pixel_class, zoom),
                latlong2 = pixel_to_latlong2(map_pixel_class2, zoom)
            };
        }

        static public tile_class calc_latlong_area_tiles(latlong_class latlong1, latlong_class latlong2, double zoom, int resolution)
        {
            tile_class tile_class = new tile_class();
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong1, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(latlong2, zoom);
            tile_class.x = (int)Mathf.Round((float)((map_pixel_class2.x - map_pixel_class.x) / resolution));
            tile_class.y = (int)Mathf.Round((float)((map_pixel_class2.y - map_pixel_class.y) / resolution));
            return tile_class;
        }

        static public latlong_class calc_latlong_center(latlong_class latlong1, latlong_class latlong2, double zoom, Vector2 screen_resolution)
        {
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong1, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(latlong2, zoom);
            return pixel_to_latlong2(new map_pixel_class
            {
                x = (map_pixel_class.x + map_pixel_class2.x) / 2,
                y = (map_pixel_class.y + map_pixel_class2.y) / 2
            }, zoom);
        }

        #if UNITY_EDITOR
        static public void calc_latlong_area_from_center(map_area_class area, latlong_class center, double zoom, Vector2 resolution)
        {
            map_pixel_class map_pixel_class = latlong_to_pixel2(area.center, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(center, zoom);
            map_pixel_class map_pixel_class3 = latlong_to_pixel2(area.upper_left, zoom);
            map_pixel_class map_pixel_class4 = latlong_to_pixel2(area.lower_right, zoom);
            map_pixel_class map_pixel_class5 = new map_pixel_class();
            map_pixel_class5.x = map_pixel_class2.x - map_pixel_class.x;
            map_pixel_class5.y = map_pixel_class2.y - map_pixel_class.y;
            map_pixel_class3.x += map_pixel_class5.x;
            map_pixel_class3.y += map_pixel_class5.y;
            map_pixel_class4.x = map_pixel_class3.x + resolution.x;
            map_pixel_class4.y = map_pixel_class3.y + resolution.y;
            area.upper_left = pixel_to_latlong2(map_pixel_class3, zoom);
            area.lower_right = pixel_to_latlong2(map_pixel_class4, zoom);
            area.center = center;
        }

        static public void calc_latlong1_area_from_center(map_area_class area, latlong_class center, double zoom)
        {
            map_pixel_class map_pixel_class = latlong_to_pixel2(area.upper_left, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(center, zoom);
            map_pixel_class map_pixel_class3 = latlong_to_pixel2(area.center, zoom);
            map_pixel_class map_pixel_class4 = latlong_to_pixel2(area.lower_right, zoom);
            map_pixel_class map_pixel_class5 = new map_pixel_class();
            map_pixel_class5.x = map_pixel_class2.x - map_pixel_class.x;
            map_pixel_class5.y = map_pixel_class2.y - map_pixel_class.y;
            map_pixel_class3.x += map_pixel_class5.x;
            map_pixel_class3.y += map_pixel_class5.y;
            map_pixel_class4.x += map_pixel_class5.x;
            map_pixel_class4.y += map_pixel_class5.y;
            area.upper_left = center;
            area.center = pixel_to_latlong2(map_pixel_class3, zoom);
            area.lower_right = pixel_to_latlong2(map_pixel_class4, zoom);
        }

        static public void calc_latlong2_area_from_center(map_area_class area, latlong_class center, double zoom)
        {
            map_pixel_class map_pixel_class = latlong_to_pixel2(area.lower_right, zoom);
            map_pixel_class map_pixel_class2 = latlong_to_pixel2(center, zoom);
            map_pixel_class map_pixel_class3 = latlong_to_pixel2(area.center, zoom);
            map_pixel_class map_pixel_class4 = latlong_to_pixel2(area.upper_left, zoom);
            map_pixel_class map_pixel_class5 = new map_pixel_class();
            map_pixel_class5.x = map_pixel_class2.x - map_pixel_class.x;
            map_pixel_class5.y = map_pixel_class2.y - map_pixel_class.y;
            map_pixel_class3.x += map_pixel_class5.x;
            map_pixel_class3.y += map_pixel_class5.y;
            map_pixel_class4.x += map_pixel_class5.x;
            map_pixel_class4.y += map_pixel_class5.y;
            area.lower_right = center;
            area.center = pixel_to_latlong2(map_pixel_class3, zoom);
            area.upper_left = pixel_to_latlong2(map_pixel_class4, zoom);
        }
        #endif

        static public Vector2 calc_pixel_zoom(Vector2 pixel, double zoom, double current_zoom, Vector2 screen_resolution)
        {
            double num = Mathf.Pow(2, (float)(zoom - current_zoom));
            Vector2 vector = pixel - screen_resolution;
            vector *= (float)num;
            return vector + screen_resolution;
        }

        static public latlong_area_class calc_latlong_area_by_tile(latlong_class latlong, tile_class tile, double zoom, int resolution, Vector2 bresolution, Vector2 offset)
        {
            float num = Mathf.Pow(2, (float)(19 - zoom));
            zoom = 19;
            resolution = (int)(resolution * num);
            bresolution *= num;
            latlong_area_class latlong_area_class = new latlong_area_class();
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong, zoom);
            Vector2 vector = new Vector2(0, 0);
            map_pixel_class.x += tile.x * resolution + offset.x;
            map_pixel_class.y += tile.y * resolution + offset.y;
            if (tile.x > 0)
            {
                map_pixel_class.x += num;
                vector.x = num;
            }
            if (tile.y > 0)
            {
                map_pixel_class.y += num;
                vector.y = num;
            }
            latlong_class latlong_class = pixel_to_latlong2(map_pixel_class, zoom);
            latlong_area_class.latlong1 = latlong_class;
            map_pixel_class.x += bresolution.x - vector.x;
            map_pixel_class.y += bresolution.y - vector.y;
            latlong_class = pixel_to_latlong2(map_pixel_class, zoom);
            latlong_area_class.latlong2 = latlong_class;
            return latlong_area_class;
        }

        static public latlong_area_class calc_latlong_area_by_tile2(latlong_class latlong, tile_class tile, double zoom, int resolution, Vector2 bresolution)
        {
            latlong_area_class latlong_area_class = new latlong_area_class();
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong, zoom);
            map_pixel_class.x += tile.x * resolution;
            map_pixel_class.y += tile.y * resolution;
            latlong_class latlong_class = pixel_to_latlong2(map_pixel_class, zoom);
            latlong_area_class.latlong1 = latlong_class;
            map_pixel_class.x += bresolution.x;
            map_pixel_class.y += bresolution.y;
            latlong_class = pixel_to_latlong2(map_pixel_class, zoom);
            latlong_area_class.latlong2 = latlong_class;
            return latlong_area_class;
        }

        static public latlong_class calc_latlong_center_by_tile(latlong_class latlong, tile_class tile, tile_class subtile, tile_class subtiles, double zoom, int resolution, Vector2 offset)
        {
            float num = Mathf.Pow(2, (float)(19 - zoom));
            zoom = 19;
            resolution = (int)(resolution * num);
            map_pixel_class map_pixel_class = latlong_to_pixel2(latlong, zoom);
            map_pixel_class.x += tile.x * subtiles.x * resolution + subtile.x * resolution;
            map_pixel_class.y += tile.y * subtiles.y * resolution + subtile.y * resolution;
            map_pixel_class.x += resolution / 2 + offset.x;
            map_pixel_class.y += resolution / 2 + offset.y;
            return pixel_to_latlong2(map_pixel_class, zoom);
        }

        static public int calc_rest_value(float value1, float divide)
        {
            int num = (int)(value1 / divide);
            return (int)(value1 - num * divide);
        }

        static public map_pixel_class calc_latlong_to_mercator(latlong_class latlong)
        {
            map_pixel_class map_pixel_class = new map_pixel_class();
            map_pixel_class.x = latlong.latitude * 20037508f / 180;
            map_pixel_class.y = Mathf.Log(Mathf.Tan((float)((90 + latlong.longitude) * 3.14159274f / 360))) / 0.0174532924f;
            map_pixel_class.y = map_pixel_class.y * 20037508f / 180;
            return map_pixel_class;
        }

        static public latlong_class calc_mercator_to_latlong(map_pixel_class pixel)
        {
            latlong_class latlong_class = new latlong_class();
            latlong_class.longitude = pixel.x / 20037508f * 180;
            latlong_class.latitude = pixel.y / 20037508f * 180;
            latlong_class.latitude = 57.2957764f * (2 * Mathf.Atan(Mathf.Exp((float)(latlong_class.latitude * 3.14159274f / 180))) - 1.57079637f);
            return latlong_class;
        }

        static public tile_class calc_terrain_tile(int terrain_index, tile_class tiles)
        {
            tile_class tile_class = new tile_class();
            tile_class.y = terrain_index / tiles.x;
            tile_class.x = terrain_index - tile_class.y * tiles.x;
            return tile_class;
        }
    }
}
