    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningCubeAnimController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Animator _animator;
    [SerializeField] private string _boolParameterName = "isSpinning";


    //Monos
    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }


    //Utils
    public void StartSpinning()
    {
        if (_animator.GetBool(_boolParameterName) == false)
            _animator.SetBool(_boolParameterName, true);
    }

    public void StopSpinning()
    {
        if (_animator.GetBool(_boolParameterName) == true)
            _animator.SetBool(_boolParameterName, false);
    }



}
