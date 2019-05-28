using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ModelListItem : MonoBehaviour
{
	public RectTransform rectTransform;
	public Text itemText;
	public RawImage itemImage;

    public void SetInfo(ModelInfo info) {
		rectTransform = gameObject.GetComponent<RectTransform>();
		Texture2D preview = null;
#if UNITY_EDITOR
		preview = AssetPreview.GetAssetPreview(Resources.Load<Mesh>(ResourcesManager.MODELS_PATH + info.name)); //Não disponível fora do editor
#endif
		itemText.text = info.displayName;
		itemImage.texture = preview;
	}
}
