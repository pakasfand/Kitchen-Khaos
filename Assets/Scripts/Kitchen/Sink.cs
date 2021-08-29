using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CleanDish
{
    public DishType dishtype;
    public GameObject prefab;
    public Transform stack;
    public Vector3 offset = Vector3.zero;
}

public class Sink : MonoBehaviour
{
    [SerializeField] private GameObject _progressBarGO;
    [SerializeField] private Image _progressBar;
    [SerializeField] private float _cleaningTime;
    [SerializeField] private List<CleanDish> _cleanDishes;

    private bool _active;
    private float _activeTimer;
    private List<DishType> _dishesBeingCleaned;

    public static Action OnDishesCleaned;

    private void OnEnable()
    {
        PlayerInteraction.OnPlayerStartedCleaning += OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning += OnPlayerStoppedCleaning;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnPlayerStartedCleaning -= OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning -= OnPlayerStoppedCleaning;
    }

    private void OnPlayerStartedCleaning(List<DishType> dishesBeingCleaned)
    {
        _progressBarGO.SetActive(true);
        _active = true;
        _dishesBeingCleaned = dishesBeingCleaned;
    }

    private void Update()
    {
        if(_active)
        {
            _activeTimer += Time.deltaTime;

            float width = (_activeTimer/_cleaningTime) * (3.0f);

            _progressBar.rectTransform.sizeDelta =
                new Vector2(width, _progressBar.rectTransform.rect.height);
        }

        if(_activeTimer >= _cleaningTime)
        {
            _active = false;

            _progressBarGO.SetActive(false);

            PopulateStack();

            OnDishesCleaned?.Invoke();
        }
    }

    private void PopulateStack()
    {
        foreach (var dishType in _dishesBeingCleaned)
        {
            foreach (var cleanDish in _cleanDishes)
            {
                if(dishType == cleanDish.dishtype)
                {
                    var dish = Instantiate(cleanDish.prefab, cleanDish.stack);
                    dish.transform.localPosition += cleanDish.offset;
                    cleanDish.offset += new Vector3(0f, cleanDish.prefab.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.size.y, 0f);
                }
            }
        }
    }

    private void OnPlayerStoppedCleaning()
    {
        _progressBarGO.SetActive(false);
        _active = false;
        _activeTimer = 0f;
    }
}
