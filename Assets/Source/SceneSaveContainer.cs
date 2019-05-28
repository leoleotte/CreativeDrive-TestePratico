using System;
using System.Collections.Generic;

[Serializable]
public class SceneSaveContainer
{
	public List<ModelInfo> models;

	public SceneSaveContainer(int size) {
		models = new List<ModelInfo>(size);
	}

	public void Add(ModelInfo info) {
		models.Add(info);
	}
}
