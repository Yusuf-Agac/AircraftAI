using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace WorldComposer
{
    public class WebRequest
    {
        static List<WebRequest> requests = new List<WebRequest>();
        static float tStamp;
        const float delayRequest = 0.02f;

        #if UNITY_5
        public WWW www;
        #else
        public UnityWebRequest www;
        #endif

        string url;
        bool isTextureRequest;
        bool isAddedToList;
        int redoCount = 0;

        public static void ResetStampTime()
        {
            tStamp = -1000;
        }

        static public void ProcessRequests()
        {
            if (requests.Count == 0) return;

            if (Time.realtimeSinceStartup - tStamp < delayRequest) return;
            tStamp = Time.realtimeSinceStartup;

            try
            {
                SendWebRequest(requests[0]);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            requests[0].isAddedToList = false;
            requests.RemoveAt(0);
        }

        static public void SendWebRequest(WebRequest webRequest)
        {
            #if UNITY_5
            webRequest.www = new WWW(webRequest.url);
            #else
            #if UNITY_2017_1
            webRequest.www.Send();
            #else
            webRequest.www.SendWebRequest();
            #endif
            #endif
        }

        public void Request(string url, bool isTextureRequest)
        {
            url = url.ToString(CultureInfo.InvariantCulture);

            if (www != null)
            {
                if (isAddedToList)
                {
                    requests.Remove(this);
                    isAddedToList = false;
                }

                www.Dispose();
                www = null;
            }

            this.url = url;
            this.isTextureRequest = isTextureRequest;

            #if !UNITY_5
            if (isTextureRequest)
            {
                www = UnityWebRequestTexture.GetTexture(url);
                // Debug.Log(url);
            }
            else www = UnityWebRequest.Get(url);
            #endif

            if (Time.realtimeSinceStartup - tStamp < delayRequest)
            {
                isAddedToList = true;
                requests.Add(this);
                return;
            }
            tStamp = Time.realtimeSinceStartup;

            SendWebRequest(this);
        }

        public bool HasRequested
        {
            get
            {
                return www != null;
            }
        }

        public bool IsDone
        {
            get
            {
                if (www == null) return false;
                return www.isDone;
            }
        }

        public bool IsDoneIfErrorRedo
        {
            get
            {
                if (www != null && www.isDone)
                {
                    #if UNITY_5
                    if (!string.IsNullOrEmpty(www.error))
                    #else
                    if (www.isNetworkError || www.isHttpError)
                    #endif
                    {
                        if (redoCount++ <= 3)
                        {
                            if (redoCount == 4)
                            {
                                Debug.LogError("Check if your Bing key is corrent, see the WorldComposer window how to solve it.");
                            }
                            else Debug.LogError(www.error + " response code " + www.responseCode + " isNetworkError " + www.isNetworkError + " isHttpError " + www.isHttpError);
                            RedoRequest(); 
                        }
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public bool IsError(out string text)
        {
            #if UNITY_5
            bool isError = !string.IsNullOrEmpty(www.error);
            #else
            bool isError = www.isNetworkError || www.isHttpError;
            #endif

            if (isError)
            { 
                text = www.error;
                www = null;
            }
            else text = string.Empty;

            return isError;
        }

        public void RedoRequest()
        {
            Request(url, isTextureRequest);
        }

        public void Abort()
        {
            if (www != null && !www.isDone)
            {
                #if UNITY_5
                www.Dispose();
                #else
                www.Abort();
                #endif
                
                www = null;
            }
        }

        public string GetText()
        {
            redoCount = 0;

            if (www == null)
            {
                Debug.LogError("www = null");
                return string.Empty;
            }
            #if UNITY_5
            string text = www.text;
            #else
            if (www.downloadHandler == null)
            {
                Debug.LogError("downloadHandeler = null");
                return string.Empty;
            }
            string text = www.downloadHandler.text;
            #endif

            www = null;

            return text;
        }

        public Texture2D GetTexture()
        {
            redoCount = 0;

            #if UNITY_5
            Texture2D texture = www.texture;
            #else
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            #endif

            www = null;

            return texture;
        }

        public byte[] GetBytes()
        {
            redoCount = 0;

            #if UNITY_5
            byte[] bytes = www.bytes;
            #else
            byte[] bytes = www.downloadHandler.data;
            #endif
            www = null;

            return bytes;
        }
    }
}
