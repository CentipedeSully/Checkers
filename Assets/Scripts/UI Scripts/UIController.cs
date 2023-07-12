using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    //Declarations
    [SerializeField] private GameObject _darkTurnObjects;
    [SerializeField] private GameObject _lightTurnObjects;
    [SerializeField] private GameObject _darkPieceCountUI;
    [SerializeField] private GameObject _lightPieceCountUI;
    [SerializeField] private TextMeshProUGUI _darkPieceCountTxt;
    [SerializeField] private TextMeshProUGUI _lightPieceCountTxt;
    [SerializeField] private GameObject _turnCounterUI;
    [SerializeField] private GameObject _drawCounterUI;
    [SerializeField] private TextMeshProUGUI _turnCountTxt;
    [SerializeField] private TextMeshProUGUI _drawCountTxt;

    private int _drawCounter = -1;
    private int _turnCounter = -1;

    //Monobehavoiurs
    private void Awake()
    {
        //HideDarkTurnUI();
        //HideLightTurnUI();
    }



    //Internal Utils




    //Getters Setters, & Commands

    public void SetDarkPieceCount(int newCount)
    {
        _darkPieceCountTxt.text = newCount.ToString();
    }

    public void SetLightPieceCount(int newCount)
    {
        _lightPieceCountTxt.text = newCount.ToString();
    }

    public void IncrementTurnCount()
    {
        _turnCounter++;
        _turnCountTxt.text = _turnCounter.ToString();
    }

    public void IncrementDrawCounter()
    {
        _drawCounter++;
        _drawCountTxt.text = _drawCounter.ToString();
    }

    public int GetDrawCounter()
    {
        return _drawCounter;
    }

    public void ResetDrawCounter()
    {
        _drawCounter = -1;
    }

    public void ShowDarkTurnUI()
    {
        _darkTurnObjects.SetActive(true);
    }

    public void ShowLightTurnUI()
    {
        _lightTurnObjects.SetActive(true);
    }

    public void HideDarkTurnUI()
    {
        _darkTurnObjects.SetActive(false);
    }

    public void HideLightTurnUI()
    {
        _lightTurnObjects.SetActive(false);
    }

    public void HidePieceCounters()
    {
        _darkPieceCountUI.SetActive(false);
        _lightPieceCountUI.SetActive(false);
    }

    public void ShowPieceCounters()
    {
        _darkPieceCountUI.SetActive(true);
        _lightPieceCountUI.SetActive(true);
    }

    public void HideTurnCounterUI()
    {
        _turnCounterUI.SetActive(false);
    }

    public void ShowTurnCounterUI()
    {
        _turnCounterUI.SetActive(true);
    }

    public void HideDrawCounterUI()
    {
        _drawCounterUI.SetActive(false);
    }

    public void ShowDrawCounterUI()
    {
        _drawCounterUI.SetActive(true);
    }

}
