using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    [SerializeField] private GameObject _loadingBar;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private int _kitchenSceneIndex;

    private AsyncOperation _loadingStatus; 
        
    void Start()
    {
        _loadingStatus = SceneManager.LoadSceneAsync(_kitchenSceneIndex, LoadSceneMode.Single);
    }

    void Update()
    {
        if (_loadingStatus.isDone)
        {
            Destroy(_loadingBar);
        }
        
        transform.Rotate(0, 0,
        _rotationSpeed * Time.deltaTime, Space.Self);
    }
}
