using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Uniconn
{
    public class Request
    {
        public delegate void ResponseListener(string responseText);
        public delegate void ErrorListener(string errorMsg);

        public static IEnumerator PostData(string url, object data, ErrorListener errorListener, ResponseListener responseListener)
        {
            WWWForm form = new WWWForm();
            form.AddField("__DATA", JsonUtility.ToJson(data));

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.Send();

            if (www.isError == true)
            {
                errorListener(www.error);
            }
            else
            {
                responseListener(www.downloadHandler.text);
            }
        }

        public static IEnumerator Get(string url, ErrorListener errorListener, ResponseListener responseListener)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.Send();

            if (www.isError == true)
            {
                errorListener(www.error);
            }
            else
            {
                responseListener(www.downloadHandler.text);
            }
        }
    }
}