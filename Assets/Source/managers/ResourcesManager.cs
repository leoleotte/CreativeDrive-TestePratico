using UnityEngine;
using SimpleJSON;

public class ResourcesManager : MonoBehaviour {
	public static string MODELS_PATH = "Models/"; 
	public static string TEXTURES_PATH = "Textures/"; 

	private int modelsLoaded = 0;
	Texture2D[] textures;

	public ModelInfo[] ParseModelsInfo(string modelsJson) {
		JSONNode json = JSON.Parse(modelsJson); //O JsonUtility da Unity não consegue dar parse no JSON que está no servidor...

		int modelsNumber = json["models"].Count;
		ModelInfo[] modelsInfo = new ModelInfo[modelsNumber];

		for (int i = 0; i < modelsNumber; i++) {
			JSONNode model = json["models"][i];

			modelsInfo[i] = new ModelInfo(model["name"].Value,
				model["position"].ReadVector3(),
				model["rotation"].ReadVector3(),
				model["scale"].ReadVector3());
			modelsInfo[i].textureName = model["name"].Value;
		}
		return modelsInfo;
	}

	public void LoadTextures() {
		try {
			textures = Resources.LoadAll<Texture2D>(TEXTURES_PATH);
			Debug.Log("Loaded " + textures.Length + " textures!");
		} catch (System.Exception e) {
			Debug.Log("Não foi possível carregar as texturas");
			Debug.Log(e.StackTrace);
		}
	}

	public Texture2D GetTexture(string textureName) {
		if (textures != null && textures.Length > 0) {
			foreach (Texture2D texture in textures) {
				if (texture.name == textureName) {
					return texture;
				}
			}
		}
		Debug.Log("Não foi possível encontrar a textura: " + textureName);
		return null;
	}

	public void LoadModels(ModelInfo[] modelsInfo) {
		modelsLoaded = 0;
		foreach (ModelInfo info in modelsInfo) {
			try {
				info.mesh = Resources.Load<Mesh>(MODELS_PATH + info.name);
				info.texture = GetTexture(info.textureName);
				modelsLoaded++;
			} catch (System.Exception e) {
				Debug.Log("Não foi possível carregar o modelo: " + info.name);
				Debug.Log(e.StackTrace);
			}
		}
		Debug.Log("Loaded " + modelsLoaded + " models!");
	}
}
