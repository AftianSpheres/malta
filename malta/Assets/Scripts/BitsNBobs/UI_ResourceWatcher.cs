using UnityEngine;
using System.Collections;

/// <summary>
/// There's a really simple, elegant way to do this, but it involves pointers
/// and is thus probably not worth it.
/// </summary>
public class UI_ResourceWatcher : MonoBehaviour
{
    new public UnityEngine.UI.Text guiText;
    public ResourceType resource;
    private int resourceCountCache;
    private int resourceMaxCache;
    private bool dirty;

    // Update is called once per frame
    void Update ()
    {
        if (GameDataManager.Instance != null) // don't try to access managers before they exist and crash shit, dumbass
        {
            RefreshCachedValues();
            if (dirty) guiText.text = resourceCountCache + " / " + resourceMaxCache;
        }
	}

    void RefreshCachedValues ()
    {
        switch (resource)
        {
            case ResourceType.Bricks:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resBricks, GameDataManager.Instance.dataStore.resBricks_max);
                break;
            case ResourceType.Clay:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resClay, GameDataManager.Instance.dataStore.resClay_max);
                break;
            case ResourceType.Metal:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resMetal, GameDataManager.Instance.dataStore.resMetal_max);
                break;
            case ResourceType.Ore:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resOre, GameDataManager.Instance.dataStore.resOre_max);
                break;
            case ResourceType.Planks:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resPlanks, GameDataManager.Instance.dataStore.resPlanks_max);
                break;
            case ResourceType.Lumber:
                _in_RefreshCachedValues(GameDataManager.Instance.dataStore.resLumber, GameDataManager.Instance.dataStore.resLumber_max);
                break;
        }
    }

    void _in_RefreshCachedValues (int res, int max)
    {
        if (resourceCountCache != res || resourceMaxCache != max) dirty = true;
        resourceCountCache = res;
        resourceMaxCache = max;
    }
}
