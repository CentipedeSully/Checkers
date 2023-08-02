using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Delcarations
    [SerializeField] private AudioSource _buttonClick;
    [SerializeField] private AudioSource _mousePositionMove;
    [SerializeField] private AudioSource _unitSelection;
    [SerializeField] private AudioSource _unitPositionMove;

 


    //Monobehaviours





    //Internal Utils





    //External Utils
    public void PlayButtonClick()
    {
        _buttonClick.Play();
    }

    public void PlayMousePositionMove()
    {
        _mousePositionMove.Play();
    }

    public void PlayUnitSelection()
    {
        _unitSelection.Play();
    }

    public void PlayUnitPositionMove()
    {
        _unitPositionMove.Play();
    }




}
