using UnityEngine.Networking;
using UnityEngine;

public static class API
{
    public static void AccountLogin(MonoBehaviour behaviour, AccountLoginDTO accountLoginDTO)
    {
        WebRequest.Post(behaviour, "Account/Login", JsonUtility.ToJson(accountLoginDTO), (UnityWebRequest request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
            }
            else
            {

            }
        });
    }
}
