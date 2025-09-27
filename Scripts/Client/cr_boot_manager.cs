using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cr_boot_manager : cr_MonoBehavior
{
    public string devMainScene = "";
    public int waitTime = 2;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        StartCoroutine(BootProcess());
    }
    
    
    public IEnumerator BootProcess()
    {
        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene(cr_game_loader.FindBuildIndexByName(cr_game.CR_MAIN_MENU_SCENE));
        cr_game_loader.Instance.StartLocalSingle(devMainScene);
        //cr_game_loader.Instance.AutoHostOrJoin("dev", devMainScene);
    }
}
