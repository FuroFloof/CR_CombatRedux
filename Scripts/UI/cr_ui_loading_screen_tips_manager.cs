using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class cr_ui_loading_screen_tips_manager : cr_MonoBehavior
{
    public TextMeshProUGUI PrevDrawingLabel;
    public RectTransform PrevDrawingLabelTransform;
    
    public TextMeshProUGUI NextDrawingLabel;
    public RectTransform NextDrawingLabelTransform;

    [NonSerialized] public int currentTipIndex = 0;
    public string[] tips;
    public void Start()
    {
        FetchTips();
        TipProcess();
    }

    void OnEnable()
    {
        FetchTips();
        TipProcess();
    }

    public void FetchTips()
    {
        string filePath = Path.Combine(cr_game.TrueGameConfigPath, "UI", "misc", "tips.json");
        var parser = cr_game_file_parser.Instance;
        
        string[] _tips = parser.GetFileVar<string[]>(
            filePath, 
            nameof(cr_game_defaults_tips.cr_loading_tips), 
            cr_game_defaults_tips.cr_loading_tips
        );
        
        tips = _tips;
        currentTipIndex = 0;
    }


    Coroutine CP;
    public void TipProcess()
    {
        if(CP != null)
        {
            StopCoroutine(CP);
        }

        int riggedTip = -1; // change to tip index for testing!
        
        int randomIndex = Random.Range(0, tips.Length);
        currentTipIndex = randomIndex;
        
        if (riggedTip != -1) currentTipIndex = riggedTip;
        
        DrawTip(currentTipIndex, true, NextDrawingLabel);
        
        CP = StartCoroutine(CTipProcess());
    }
    
    public IEnumerator CTipProcess() 
    {
        var wait = new WaitForSeconds(4.5f);
        while (true)
        {
            yield return wait;
            NextTip();
        }
    }

    [ContextMenu("Next Tip")]
    public void NextTip()
    {
        currentTipIndex++;
        if(currentTipIndex > tips.Length - 1) currentTipIndex = 0;

        currentTipIndex = CheckNextTip(currentTipIndex, 1);
        
        DrawTip(currentTipIndex, true, NextDrawingLabel);
    }
    
    [ContextMenu("Previous Tip")]
    public void PrevTip()
    {
        currentTipIndex--;
        if(currentTipIndex < 0) currentTipIndex = tips.Length - 1;
        
        currentTipIndex = CheckNextTip(currentTipIndex, -1);
        
        DrawTip(currentTipIndex, true, PrevDrawingLabel);
    }

    Coroutine c;
    public void DrawTip(int tipIndex, bool showTransition, TextMeshProUGUI targetLabel)
    {
        if(c != null)
        {
            StopCoroutine(c);
        }
    
        c = StartCoroutine(CDrawTip(tipIndex, showTransition, targetLabel));
    }

    public float transitionSpeed = 5f;
    TextMeshProUGUI _lastUsedLabel;
    public IEnumerator CDrawTip(int tipIndex, bool showTransition, TextMeshProUGUI _targetLabel)
    {
        var lastUsedLabel = _lastUsedLabel;
    
        var targetLabel = _targetLabel;
        var targetLabelTransform = targetLabel == NextDrawingLabel ? NextDrawingLabelTransform : PrevDrawingLabelTransform;
        
        var oppositeLabel = targetLabel != NextDrawingLabel ? NextDrawingLabel : PrevDrawingLabel;
        var oppositeLabelTransform = targetLabel != NextDrawingLabel ? NextDrawingLabelTransform : PrevDrawingLabelTransform;
        
        
        string nextTip = tips[tipIndex];

        string[] tipSplit = nextTip.Split('~');
        if (tipSplit.Length != 1) 
        {
            nextTip = tipSplit[1];
        }
        
        
        if (!showTransition)
        {
            lastUsedLabel.text = nextTip;
            yield break;
        }
    
        Vector3 verySmall = Vector3.zero;

        
        if(oppositeLabel != lastUsedLabel) 
        {
            oppositeLabel.text = targetLabel.text;
        
            targetLabelTransform.localScale = Vector3.zero;
            oppositeLabelTransform.localScale = Vector3.one;
        }
        
        
        {
            Vector3 start = Vector3.one;
            Vector3 target = verySmall;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                oppositeLabelTransform.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }
            oppositeLabelTransform.localScale = target;
        }
        
        targetLabel.text = nextTip;
        
        
        {
            Vector3 start = verySmall;
            Vector3 target = Vector3.one;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                targetLabelTransform.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }
            targetLabelTransform.localScale = target;
        }

        _lastUsedLabel = targetLabel;
        
        c = null;
    }
    
    
    
    public int CheckNextTip(int nextTipIndex, int direction)
    {
        string wantedTip = tips[nextTipIndex];
        int skippedTipIndex = nextTipIndex + direction;

        string[] tipSplit = wantedTip.Split('~');
        if (tipSplit.Length == 1) return nextTipIndex;
        
        string tipCode = tipSplit[0];
        string actualTip = tipSplit[1];
    
        switch(tipCode)
        {
            case "0": // dont look behind you joke tip
                var boo = FindObjectOfType<cr_ui_fools_player_turn_around_tip>();
                boo.EnableDontTurnAround();
            break;
                
            
            
            case "tf_i": // must have tf2 installed to view
                bool isTFInstalled = cr_Steamworks.Instance.GetIsTF2Installed(false);
                if (!isTFInstalled){ Debug.Log("Does not have tf2 installed");  return skippedTipIndex; } 
            break;
            
            
                
            case "r_1k": // random 1 : 1000 chance to see
                int rand = Random.Range(1, 1000);
                if (rand != 1) { Debug.Log($"{rand} is not 1"); return skippedTipIndex; } 
            break;
            
            
            
            case "name": // fetch pc name
                string fullTip = $"{actualTip}{cr_game.GetPCUsername()}";
                tips[nextTipIndex] = fullTip;
            break;
            
            
            
            case "cred": // last credit card number generated randomly
                int lastCreditCardNumber = Random.Range(0, 10);
                string creditCardTip = $"{actualTip}{lastCreditCardNumber}";
                tips[nextTipIndex] = creditCardTip;
            break;
            
            
            
            case "alt": // alt f4 gag!
                cr_player_gag_alt_f4.Instance.IsInGagLoading = true;
            break;
            
            
            
            default: // reset flags
                cr_player_gag_alt_f4.Instance.IsInGagLoading = false;
            break;
        }

        return nextTipIndex;
    }
}
