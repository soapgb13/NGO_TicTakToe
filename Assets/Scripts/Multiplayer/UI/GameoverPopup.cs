using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverPopup : MonoBehaviour
{
   [SerializeField] private GameObject gameOverPopupHolder;
   
   [SerializeField] private TMP_Text titleText;
   [SerializeField] private TMP_Text winnerNameText;

   private const string AnyOneWinString = "Game Over !\n Winner is :";
   private const string DrawString = "Game Over!\n Try Again!";
   
   public void OnEnable()
   {
      GameEvents.OnGameOver += OnGameOverAction;
   }

   private void OnDestroy()
   {
      GameEvents.OnGameOver -= OnGameOverAction;
   }


   private void OnGameOverAction(TileOwnerType ownerType)
   {
      if (ownerType == TileOwnerType.Empty)
      {
         titleText.text = DrawString;
         winnerNameText.text = "Draw!";
      }
      else
      {
         titleText.text = AnyOneWinString;
         winnerNameText.text = ownerType.ToString();
      }
      
      gameOverPopupHolder.SetActive(true);
   }

   public void GoBackToMainMenu()
   {
      StartCoroutine(WaitAndCloseGameOverPopup());
   }

   private IEnumerator WaitAndCloseGameOverPopup()
   {
      YieldInstruction wait = new WaitForSeconds(0.1f);
      
      NetworkManager.Singleton.Shutdown();
      
      while (NetworkManager.Singleton.ShutdownInProgress)
      {
         yield return wait;
      }
      
      gameOverPopupHolder.SetActive(false);
      
      SceneManager.LoadScene("MainMenu");
   }

   public void QuitBtnClicked()
   {
      BoardManager.Instance.isQuitGame = true;
      StartCoroutine(WaitAndCloseGameOverPopup());
   }
}
