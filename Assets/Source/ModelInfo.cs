using System;
using UnityEngine;

[Serializable]
public class ModelInfo {
	public string name;
	public Vector3 position, scale, rotation;
	public Quaternion quaternionRotation;

	[System.NonSerialized]
	public Mesh mesh;
	[System.NonSerialized]
	public Texture2D texture;

	public Color color;
	public string displayName;
	public string textureName;

	public ModelInfo(string name, Vector3 position, Vector3 rotation, Vector3 scale) {
		this.name = name;
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
		quaternionRotation = Quaternion.Euler(rotation);
		displayName = name;
	}
}