using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class TabGroup<T, K> : MonoBehaviour where T : Button where K : MonoBehaviour 
{
    [SerializeField] protected Tab<T, K>[] _tabs;

    public event Action<K> OnSelectTab;

    protected virtual void OnEnable()
    {
        foreach (var item in _tabs)
        {
            item.tabButton.onClick.AddListener(() => HandleSelectTab(item.tabPanel));
        }
    }

    protected virtual void OnDisable()
    {
        foreach (var item in _tabs)
        {
            item.tabButton.onClick.RemoveListener(() => HandleSelectTab(item.tabPanel));
        }
    }

    protected virtual void HandleSelectTab(K tabPanel)
    {
        OnSelectTab?.Invoke(tabPanel);
    }
}