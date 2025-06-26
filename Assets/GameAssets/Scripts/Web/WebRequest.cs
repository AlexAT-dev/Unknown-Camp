using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public delegate void RequestDelegate(UnityWebRequest request);

public static class WebRequest
{
    private static string server = "https://unknowncampserver.onrender.com/api/";

	private static string bearerToken = null;

    public static void SetBearerToken(string token)
    {
        bearerToken = token;
    }

    public static void SetBaseUrl(string baseUrl)
    {
        server = baseUrl;
    }

    public static void Get(MonoBehaviour behaviour, string url, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        behaviour.StartCoroutine(GetRequest(server + url, action, headers));
    }

    public static void Post(MonoBehaviour behaviour, string url, string jsonData, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        behaviour.StartCoroutine(SendJsonRequest(server + url, jsonData, "POST", action, headers));
    }

    public static void Put(MonoBehaviour behaviour, string url, string jsonData, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        behaviour.StartCoroutine(SendJsonRequest(server + url, jsonData, "PUT", action, headers));
    }

    public static void Delete(MonoBehaviour behaviour, string url, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        behaviour.StartCoroutine(DeleteRequest(server + url, action, headers));
    }

    public static void PostFile(MonoBehaviour behaviour, string url, string filePath, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        behaviour.StartCoroutine(PostFileRequest(server + url, filePath, action, headers));
    }

     private static IEnumerator GetRequest(string url, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, headers);
            request.timeout = 10;

            yield return request.SendWebRequest();

            action(request);
        }
    }

    private static IEnumerator SendJsonRequest(string url, string jsonData, string method, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, headers);
            request.timeout = 10;

            yield return request.SendWebRequest();

            action(request);
        }
    }

    private static IEnumerator DeleteRequest(string url, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, headers);
            request.timeout = 10;

            yield return request.SendWebRequest();

            action(request);
        }
    }

    private static IEnumerator PostFileRequest(string url, string filePath, RequestDelegate action, Dictionary<string, string> headers = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerFile(filePath);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "multipart/form-data");
            ApplyHeaders(request, headers);
            request.timeout = 10;

            yield return request.SendWebRequest();

            action(request);
        }
    }

    private static void ApplyHeaders(UnityWebRequest request, Dictionary<string, string> headers)
    {
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + bearerToken);
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
    }
}
