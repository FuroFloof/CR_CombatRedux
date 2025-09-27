using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class cr_ui_main_menu_play_matchmake : cr_MonoBehavior
{
    public string d_MatchmakingSceneName = "00_dev_catch";
    public string d_MatchmakingRoomName = "dev_match_catch";

    public GameObject Modal;

    public TextMeshProUGUI MatchmakeStateHeader;
    public TextMeshProUGUI MatchmakeStateFooter;

    public GameObject closeModalButton;

    void Start()
    {
        CloseModal();
    }


    public void CloseModal()
    {
        Modal.SetActive(false);
    }


    bool isMatchmaking;
    [ContextMenu("MM/Start Matchmaking")]
    public void StartMatchmake()
    {
        if (isMatchmaking)
        {
            ShowMessage("Matchmaking Already in progress", "", true);
            return;
        }
        
        isMatchmaking = true;

        StartCoroutine(CStartMatchmake());
    }
    
    public void ShowMessage(string header, string footer, bool showCloseModal)
    {
        if (!Modal.activeInHierarchy) Modal.SetActive(true);
        closeModalButton.SetActive(showCloseModal);
                
        MatchmakeStateHeader.text = header;
        MatchmakeStateFooter.text = footer;
    }

    float dotWaitTime = .5f;
    public IEnumerator CStartMatchmake()
    {
        int loops = 4;
        
        MatchmakeStateFooter.text = "";
        
        for(int i = 0; i < loops; i++)
        {
            yield return new WaitForSeconds(dotWaitTime);
            ShowMessage("Matchmaking", MatchmakeStateFooter.text += ".", false);
        }

        ShowMessage("Matchmaking", "Match Found!\nConnecting...", false);
        
        yield return new WaitForSeconds(1);

        cr_game_loader.Instance.AutoHostOrJoin(d_MatchmakingRoomName, d_MatchmakingSceneName);
    }
}
