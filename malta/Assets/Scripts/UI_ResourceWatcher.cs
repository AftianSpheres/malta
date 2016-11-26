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
                _in_RefreshCachedValues(GameDataManager.Instance.resBricks, GameDataManager.Instance.resBricks_max);
                break;
            case ResourceType.Clay:
                _in_RefreshCachedValues(GameDataManager.Instance.resClay, GameDataManager.Instance.resClay_max);
                break;
            case ResourceType.Metal:
                _in_RefreshCachedValues(GameDataManager.Instance.resMetal, GameDataManager.Instance.resMetal_max);
                break;
            case ResourceType.Ore:
                _in_RefreshCachedValues(GameDataManager.Instance.resOre, GameDataManager.Instance.resOre_max);
                break;
            case ResourceType.Planks:
                _in_RefreshCachedValues(GameDataManager.Instance.resPlanks, GameDataManager.Instance.resPlanks_max);
                break;
            case ResourceType.Lumber:
                _in_RefreshCachedValues(GameDataManager.Instance.resLumber, GameDataManager.Instance.resLumber_max);
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
