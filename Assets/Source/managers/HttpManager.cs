using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HttpManager : MonoBehaviour {
	private UnityWebRequest webRequest;

	public IEnumerator GetModelsInfo(System.Action<string> callback) 
	{
		Debug.Log("Requesting models info...");
		webRequest = UnityWebRequest.Get("https://s3-sa-east-1.amazonaws.com/static-files-prod/unity3d/models.json");
		yield return webRequest.SendWebRequest();

		if (webRequest.isNetworkError || webRequest.isHttpError) {
			Debug.Log("Ocorreu um erro no download das informações de modelos: " + webRequest.error);
		} else {
			callback(webRequest.downloadHandler.text);
		}
	}
}
