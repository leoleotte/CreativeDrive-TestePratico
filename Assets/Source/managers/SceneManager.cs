using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour {
	public static string sceneSaveNamePath;
	public enum InputState { Translation, Rotation, Scale };
	private InputState currentInputState;

	private HttpManager httpManager;
	private ResourcesManager resourcesManager;

	public List<Model> models;
	private GameObject modelsHolder;
	private GameObject currentSelectedModel;

	public float mouseMovementSpeed = 0.2f; 
	public float cameraMovementSpeed = 0.1f; 

	//*UI vars//////
	public InputField UIModelNameText;
	public GameObject UISaveScenePanel;
	public GameObject UILoadScenePanel; 
	public GameObject UIModelInfoPanel;
	public GameObject UIModelsList;
	public ModelTextureOptions[] modelsTextureOptions;
	public RawImage UIModelTextureOption1, UIModelTextureOption2;

	private Ray cameraRay;
	private RaycastHit hitInfo;

	void Start() {
		sceneSaveNamePath = Application.persistentDataPath + "/" + "scene.json";
		httpManager = gameObject.AddComponent<HttpManager>();
		resourcesManager = gameObject.AddComponent<ResourcesManager>();
		models = new List<Model>();

		modelsTextureOptions = new ModelTextureOptions[5]; //Hardcoded via requisito da tarefa
		for (int i = 0; i < 5; i++) {
			int textureIndex = i + 1;
			modelsTextureOptions[i] = new ModelTextureOptions("modelo0" + textureIndex, 
				"modelo0" + textureIndex, 
				"modelo0" + textureIndex + "_A");
		}

		InitScene();
	}

	private void InitScene() {
		UILoadScenePanelToggle(true);
		resourcesManager.LoadTextures();
		//Carrega os modelos baseado no JSON do servidor
		StartCoroutine(
			httpManager.GetModelsInfo((string json) => {
				LoadScene(resourcesManager.ParseModelsInfo(json));
			})
		);
	}

	void Update() {
		HandleMouse();
		HandleCameraMovement();
	}

	private void SpawnModels(ModelInfo[] modelsInfo) {
		modelsHolder = modelsHolder == null ? new GameObject("ModelHolder") : modelsHolder;
		foreach(ModelInfo info in modelsInfo) {
			SpawnModel(info);
		}
	}

	private GameObject SpawnModel(ModelInfo info) {
		GameObject modelObject = new GameObject(info.displayName);
		modelObject.transform.parent = modelsHolder.transform;

		Model model = modelObject.AddComponent<Model>();
		model.SetModelInfo(info);
		models.Add(model);
		AddModelListItem(model.modelListItem);

		return modelObject;
	}

	private void AddModelListItem(ModelListItem item) {
		item.transform.SetParent(UIModelsList.transform, false);
		//item.rectTransform.Translate((Vector3.up * models.Count() * -40.0f) + Vector3.up * 200.0f); //Péssimos números hardcoded
	}


	private void HandleMouse() {
		if (Input.GetButtonDown("Select")) {
			cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(cameraRay, out hitInfo)) { //Verifica se o mouse está em cima de um modelo, se estiver pegamos o GameObject dele para edição
				if (hitInfo.collider.gameObject.CompareTag("Model")) {
					SetCurrentSelectedModel(hitInfo.collider.gameObject);
				}
			}
		}

		if (Input.GetButton("Select") && currentSelectedModel != null) { //Move, rotaciona ou muda a escala do modelo selecionado
			switch (currentInputState) {
				case InputState.Translation: //Muda os eixos para permitir todos os 3 possíveis com o movimento do mouse se for pressionada a tecla 'Alt. input' (Alt)
					Vector3 translation = Input.GetButton("Alt. Input") ? new Vector3(0.0f, Input.GetAxis("Mouse Y"), 0.0f) :
						new Vector3(Input.GetAxis("Mouse X"), 0.0f, Input.GetAxis("Mouse Y"));

					currentSelectedModel.transform.Translate(translation * mouseMovementSpeed, Space.World);
					break;
				case InputState.Rotation: //Muda os eixos para permitir todos os 3 possíveis com o movimento do mouse se for pressionada a tecla 'Alt. input' (Alt)
					Vector3 rotation = Input.GetButton("Alt. Input") ? new Vector3(0.0f, 0.0f, -Input.GetAxis("Mouse X")) :
						new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0.0f);

					currentSelectedModel.transform.Rotate(rotation * mouseMovementSpeed, Space.World);
					break;
				case InputState.Scale:
					currentSelectedModel.transform.localScale += (new Vector3(Input.GetAxis("Mouse Y"), 
						Input.GetAxis("Mouse Y"), 
						Input.GetAxis("Mouse Y")) 
					* mouseMovementSpeed);
					break;
			}
		}
	}

	private void HandleCameraMovement() {
		Camera.main.transform.Translate(new Vector3(
				Input.GetAxis("Horizontal"), 
				0.0f, 
				Input.GetAxis("Vertical")) 
			* cameraMovementSpeed, Space.World);
	}

	private void SetCurrentSelectedModel(GameObject model) {
		currentSelectedModel = model;
		if (model == null) {
			UIModelNameText.text = "";
			UIModelInfoPanelToggle(false);
		} else {
			UIModelNameText.text =  model.name;
			UIModelInfoPanelToggle(true);
		}
	}

	private void SaveScene() {
		SceneSaveContainer saveContainer = new SceneSaveContainer(models.Count);
		FileStream fileStream = File.Create(sceneSaveNamePath);

		foreach (Model model in models) {
			saveContainer.Add(model.UpdateModelInfo());
		}
		string json = JsonUtility.ToJson(saveContainer);

		try {
			using (StreamWriter writer = new StreamWriter(fileStream)) {
				writer.Write(json);
			}
		} catch (Exception e) {
			Debug.Log("Não foi possível salvar a cena");
			Debug.Log(e.StackTrace);
		}
	}

	///<param name="modelInfos">Usar lista de modelos pré carregada, se for nulo, carrega os modelos do arquivo local</param> 
	private void LoadScene(ModelInfo[] modelInfos = null) {
		UILoadScenePanelToggle(true);

		ModelInfo[] infos = modelInfos;
		//Carrega os modelos do JSON local
		if (modelInfos == null) {
			string json;
			if (!File.Exists(sceneSaveNamePath)) { //Interrompe o loading caso o arquivo não exista
				UILoadScenePanelToggle(false);
				return;
			}
			FileStream fileStream = File.OpenRead(sceneSaveNamePath);
			using (StreamReader reader = new StreamReader(fileStream)) {
				json = reader.ReadToEnd();
			}
			SceneSaveContainer save = JsonUtility.FromJson<SceneSaveContainer>(json);
			infos = save.models.ToArray();
		}

		//Limpa a cena atual
		foreach (Model model in models) {
			Destroy(model.gameObject);
		}
		models.Clear();
		SetCurrentSelectedModel(null);

		resourcesManager.LoadModels(infos);
		SpawnModels(infos);
		UILoadScenePanelToggle(false);
	}

	//*UI Functions///

	public void UIDuplicateModel() {
		if (currentSelectedModel != null) {
			ModelInfo info = currentSelectedModel.GetComponent<Model>().UpdateModelInfo(true);
			info.displayName = info.displayName + "(Copia)";
			SetCurrentSelectedModel(SpawnModel(info));
		}
	}

	public void UIRenameModel(Text newName) {
		if (currentSelectedModel != null) {
			currentSelectedModel.gameObject.name = newName.text;
			currentSelectedModel.GetComponent<Model>().UpdateModelInfo();
		}
	}

	public void UIChangeModelColor(string colorName) {
		if (currentSelectedModel == null) {
			return;
		}

		Color color;
		switch (colorName) {
			case "red":
				color = Color.red;
				break;
			case "green":
				color = Color.green;
				break;
			case "blue":
				color = Color.blue;
				break;
			default:
				color = Color.white;
				break;
		}

		currentSelectedModel.GetComponent<Model>().SetColor(color);
	}

	public void UICancelSelection() {
		SetCurrentSelectedModel(null);
	}

	public void UISetInputState(int stateIndex) {
		currentInputState = (InputState)stateIndex;
	}

	public void UISaveScenePanelToggle(bool open) {
		UISaveScenePanel.SetActive(open);
	}

	public void UIloadScene() {		
		LoadScene();
	}

	public void UILoadScenePanelToggle(bool open) {
		UILoadScenePanel.SetActive(open);
	}

	public void UISaveScene() {
			SaveScene();
			UISaveScenePanelToggle(true);		
	}

	public void UIModelInfoPanelToggle(bool open) {
		UIModelInfoPanel.SetActive(open);
		if (currentSelectedModel != null && open) {
			string[] textureNames = GetModelTextureOptions(currentSelectedModel.GetComponent<Model>().modelName);
			if (textureNames != null && textureNames.Length > 1) {
				UIModelTextureOption1.texture = resourcesManager.GetTexture(textureNames[0]);
				UIModelTextureOption2.texture = resourcesManager.GetTexture(textureNames[1]);
			}
		}
	}

	public void UIChangeModelTexture(int option) {
		if (currentSelectedModel == null) {
			return;
		}

		Model model = currentSelectedModel.GetComponent<Model>();
		string[] texturesName = GetModelTextureOptions(model.modelName);

		if (texturesName != null && texturesName.Length >= option) {
			currentSelectedModel.GetComponent<Model>().SetTexture(resourcesManager.GetTexture(texturesName[option]));
		}
	}

	public string[] GetModelTextureOptions(string modelName) {
		foreach (ModelTextureOptions options in modelsTextureOptions) {
			if (options.modelName == modelName) {
				return options.textureOptions;
			}
		}
		return null;
	}
}

public struct ModelTextureOptions {
	public string modelName;
	public string[] textureOptions;

	public ModelTextureOptions(string modelName, params string[] textures) {
		this.modelName = modelName;
		textureOptions = textures;
	}
}
