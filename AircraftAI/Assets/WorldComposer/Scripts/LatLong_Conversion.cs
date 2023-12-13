using UnityEngine;
using System.Collections;
using System;

namespace WorldComposer
{
    [Serializable]
    public class LatLong_Conversion : MonoBehaviour
    {
        public latlong_class latlong_center = new latlong_class();
        public latlong_class[] latlong;
        public Vector2 offset = new Vector2(0, -27);

        const double minLatitude = -85.05112878;
        const double maxLatitude = 85.05112878;
        const double minLongitude = -180;
        const double maxLongitude = 180;

        void Start()
        {
            int counter = 0;

            latlong[2].latitude = 49.34544372558594;
            latlong[2].longitude = -119.579584441234;

            for (int i = 0; i < latlong.Length; i++)
            {
                latlong_class p = latlong[i];

                Vector2 pos = GetPosition(p.latitude, p.longitude);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Transform newT = go.transform;
                newT.position = new Vector3(pos.x, 0, pos.y);

                double latitude, longitude;

                GetLatLong(newT.position, out latitude, out longitude);

                Debug.Log(latitude + " : " + longitude);

                go.name = "Test point " + counter;
                counter++;
            }
        }

        void GetLatLong(Vector3 pos, out double latitude, out double longitude)
        {
            map_pixel_class map_pixel = new map_pixel_class();
            map_pixel_class map_pixel_center = latlong_to_pixel2(latlong_center, 19);

            double map_resolution = calc_latlong_area_resolution(latlong_center, 19);

            map_pixel.x = ((pos.x - offset.x) / map_resolution) + map_pixel_center.x;
            map_pixel.y = (-(pos.z - offset.y) / map_resolution) + map_pixel_center.y;

            latlong_class returnVal = pixel_to_latlong2(map_pixel, 19);

            latitude = returnVal.latitude;
            longitude = returnVal.longitude;
        }

        Vector2 GetPosition(double lat, double lon)
        {
            var latlong = new latlong_class(lat, lon);

            Vector2 returnVal;

            map_pixel_class map_pixel = latlong_to_pixel2(latlong, 19);
            map_pixel_class map_pixel_center = latlong_to_pixel2(latlong_center, 19);

            double map_resolution = calc_latlong_area_resolution(latlong_center, 19);

            returnVal.x = (float)((map_pixel.x - map_pixel_center.x) * map_resolution);
            returnVal.y = (float)((-map_pixel.y + map_pixel_center.y) * map_resolution);

            returnVal += offset;

            return returnVal;
        }

        latlong_class clip_latlong(latlong_class latlong)
        {
            if (latlong.latitude > maxLatitude) { latlong.latitude -= (maxLatitude * 2); }
            else if (latlong.latitude < minLatitude) { latlong.latitude += (maxLatitude * 2); }
            if (latlong.longitude > 180) { latlong.longitude -= 360; }
            else if (latlong.longitude < -180) { latlong.longitude += 360; }

            return latlong;
        }

        map_pixel_class clip_pixel(map_pixel_class map_pixel, double zoom)
        {
            double mapSize = 256 * Mathf.Pow(2, (float)zoom);

            if (map_pixel.x > mapSize - 1) { map_pixel.x -= mapSize - 1; }
            else if (map_pixel.x < 0) { map_pixel.x = mapSize - 1 - map_pixel.x; }

            if (map_pixel.y > mapSize - 1) { map_pixel.y -= mapSize - 1; }
            else if (map_pixel.y < 0) { map_pixel.y = mapSize - 1 - map_pixel.y; }

            return map_pixel;
        }

        public map_pixel_class latlong_to_pixel2(latlong_class latlong, double zoom)
        {
            latlong = clip_latlong(latlong);

            double pi = 3.14159265358979323846264338327950288419716939937510;

            double x = (latlong.longitude + 180.0) / 360.0;
            double sinLatitude = Mathf.Sin((float)latlong.latitude * (float)pi / 180.0f);
            double y = 0.5 - Mathf.Log((float)((1.0f + sinLatitude) / (1.0f - sinLatitude))) / (4.0f * pi);

            x *= 256.0 * Mathf.Pow(2.0f, (float)zoom);
            y *= 256.0 * Mathf.Pow(2.0f, (float)zoom);

            map_pixel_class map_pixel = new map_pixel_class();

            map_pixel.x = x;
            map_pixel.y = y;

            return map_pixel;
        }

        public latlong_class pixel_to_latlong2(map_pixel_class map_pixel, double zoom)
        {
            map_pixel = clip_pixel(map_pixel, zoom);

            double pi = 3.14159265358979323846264338327950288419716939937510;

            double mapSize = 256.0f * Mathf.Pow(2.0f, (float)zoom);

            double x = (map_pixel.x / mapSize) - 0.5;
            double y = 0.5 - (map_pixel.y / mapSize);

            latlong_class latlong = new latlong_class();

            latlong.latitude = 90.0 - 360.0 * Mathf.Atan(Mathf.Exp((float)(-y * 2.0 * pi))) / pi;
            latlong.longitude = 360.0 * x;

            return latlong;
        }

        public double calc_latlong_area_resolution(latlong_class latlong, double zoom)
        {
            double pi = 3.14159265358979323846264338327950288419716939937510;

            double map_resolution = 156543.04 * Mathf.Cos((float)(latlong.latitude * (pi / 180))) / (Mathf.Pow(2, (float)zoom));
            return map_resolution;
        }
    }
}