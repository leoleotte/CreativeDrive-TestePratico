using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour {
	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	public ModelListItem modelListItem;

	public string modelName;

	///<summary>
	///Atualiza as informações do modelo: Coordenadas, rotação, escala, mesh, textura e cor
	///</summary>
	public void SetModelInfo(ModelInfo info) {
		AddModelComponents();
		transform.position = info.position;
		transform.rotation = info.quaternionRotation;
		transform.localScale = info.scale;
		SetMesh(info.mesh);
		SetTexture(info.texture);
		SetColor(info.color);
		gameObject.tag = "Model";
		modelName = info.name;
		if (info.displayName != null && info.displayName.Length > 0) {
			gameObject.name = info.displayName;
		}

		UpdateListItem(info);
	}

	private void UpdateListItem(ModelInfo info) {
		if (modelListItem == null) {
			GameObject obj = Instantiate(Resources.Load<GameObject>("UIModelListItem"));
			modelListItem = obj.GetComponent<ModelListItem>();
		}
		modelListItem.gameObject.name = ("ModelListItem-" + info.displayName);
		modelListItem.SetInfo(info);
	}

	public void SetMesh(Mesh mesh) {
		if (mesh != null) {
			meshFilter.mesh = mesh;
			meshCollider.sharedMesh = mesh;
		}
	}

	public void SetTexture(Texture2D texture) {
		if (texture != null) {
			meshRenderer.material.mainTexture = texture;
		}
	}

	public void SetColor(Color color) {
		if (color != null && !color.Equals(Color.clear)) {
			meshRenderer.material.color = color;
		}
	}

	void AddModelComponents() {
		meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshFilter = gameObject.AddComponent<MeshFilter>();
		meshCollider = gameObject.AddComponent<MeshCollider>();
	}

	///<param name="withAssets">Deve carregar os assets junto (mesh e textura)</param> 
	public ModelInfo UpdateModelInfo(bool withAssets = false) {
		ModelInfo modelInfo = new ModelInfo(modelName, transform.position, transform.rotation.eulerAngles, transform.localScale);

		if (meshRenderer == null) {
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
		}
		if (meshFilter == null) {
			meshFilter = gameObject.GetComponent<MeshFilter>();
		}

		if (withAssets) {
			modelInfo.mesh = meshFilter.mesh;
			modelInfo.texture = (Texture2D)meshRenderer.material.mainTexture;
		}

		modelInfo.textureName = meshRenderer.material.mainTexture.name;
		modelInfo.displayName = gameObject.name;
		modelInfo.color = meshRenderer.material.color;

		UpdateListItem(modelInfo);
		return modelInfo;
	}

	void OnDestroy() {
		if (modelListItem != null) {
			Destroy(modelListItem.gameObject);
		}
	}
}
