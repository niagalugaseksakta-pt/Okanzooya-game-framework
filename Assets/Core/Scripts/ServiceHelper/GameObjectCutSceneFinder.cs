using UnityEngine;

public class GameObjectCutSceneFinder : MonoBehaviour
{
    public GameObject FindForCutSceneObjects(string tagObject)
    {
        return FinderTagHelper.FindTagged(tagObject);
    }
}

