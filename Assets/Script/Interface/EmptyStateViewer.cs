using UnityEngine;

public class EmptyStateViewer : MonoBehaviour
{
    [SerializeField] private Transform _contentParent;
    [SerializeField] private GameObject _listView;
    [SerializeField] private GameObject _emptyView;

    private int _lastActiveCount = -1;

    private void OnEnable()
    {
        _lastActiveCount = -1;
        UpdateView();
    }

    private void Update()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        int activeCount = CountActiveChildren();
        if (activeCount == _lastActiveCount) return;

        _lastActiveCount = activeCount;
        bool isEmpty = activeCount == 0;
        _listView.SetActive(!isEmpty);
        _emptyView.SetActive(isEmpty);
    }

    private int CountActiveChildren()
    {
        int count = 0;
        for (int i = 0; i < _contentParent.childCount; i++)
        {
            if (_contentParent.GetChild(i).gameObject.activeSelf)
                count++;
        }
        return count;
    }
}
