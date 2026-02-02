using Game.Model.Player;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RetryController : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void LoadRetryToSpawnPlayer()
    {
        // Attempt to acquire save manager (via ServiceLocator or static Instance)
        object saveMgrObj = null;
        try
        {
            // Try ServiceLocator first (some projects register singletons there)
            saveMgrObj = ServiceLocator.Get<SaveManager>(); // try a generic get to avoid compile error if SaveManager type is unknown to this file
        }
        catch
        {
            saveMgrObj = null;
        }


        // Try to get a player entity via ServiceLocator or FindObjectOfType
        PlayerEntity playerEntity = null;

        try
        {

            if (playerEntity == null)
            {
                playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
            }

        }
        catch (Exception)
        {
            playerEntity = null;
        }

        // Handling First scenario not find checkpoint after death
        var effectiveSaveManager = (SaveManager)saveMgrObj;

        if (effectiveSaveManager != null && playerEntity != null)
        {
            try
            {
                // Call LoadInto(playerEntity) using reflection to avoid hard compile dependency
                var loadIntoMethod = effectiveSaveManager.GetType().GetMethod("LoadInto", new Type[] { playerEntity.GetType() })
                                     ?? effectiveSaveManager.GetType().GetMethod("LoadInto", new Type[] { typeof(object) });

                if (loadIntoMethod != null)
                {
                    loadIntoMethod.Invoke(effectiveSaveManager, new object[] { playerEntity });
                    Debug.Log("[LoaderStageController] Loaded player data via SaveManager.");
                }
                else
                {
                    Debug.LogWarning("[LoaderStageController] SaveManager found but no suitable LoadInto method signature.");
                }


            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LoaderStageController] Failed to load player save data: " + ex.Message);
            }
        }
        else
        {
            if (effectiveSaveManager == null)
                Debug.Log("[LoaderStageController] No SaveManager.Instance available at Awake; skipping load.");
            if (playerEntity == null)
                Debug.Log("[LoaderStageController] No PlayerEntity found; skipping load.");
        }

        effectiveSaveManager.LoadInto(playerEntity);

        playerEntity.transform.position = playerEntity.Stats.lastCheckpointPosition;

        GameObject gameoverPanel = GameObject.FindWithTag("PanelGameOver");
        GameObject gameoverPanelButton = GameObject.FindWithTag("PanelGameOverButton");
        //GameObject mainButtonPanel = GameObject.FindWithTag("MainButton");
        //GameObject statusUiPanel = GameObject.FindWithTag("StatusUiPanel");
        GameObject[] joystick = GameObject.FindGameObjectsWithTag("FloatingJoystick");

        if (gameoverPanel.gameObject.activeSelf && gameoverPanelButton.gameObject.activeSelf)
        {
            gameoverPanel.gameObject.SetActive(false);
            gameoverPanelButton.gameObject.SetActive(false);

            //mainButtonPanel.gameObject.GetComponent<Renderer>().enabled = true;
            //statusUiPanel.gameObject.GetComponent<Renderer>().enabled = true;
            foreach (var item in joystick)
            {
                item.gameObject.GetComponent<Image>().enabled = true;
            }
        }
    }
}
