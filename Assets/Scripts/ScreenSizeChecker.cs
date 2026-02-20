//Thanks stackoverflow <3
using UnityEngine;

public class ScreenSizeChecker : MonoBehaviour
{
    public static ScreenSizeChecker instance;

    private float _previousWidth;
    private float _previousHeight;

    public delegate void OnScreenSizeChanged();
    public OnScreenSizeChanged onScreenSizeChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
            return;
        }
        instance.onScreenSizeChanged = null;
        Destroy(gameObject);
    }

    private void Init()
    {
        _previousWidth = Screen.width;
        _previousHeight = Screen.height;
    }

    private void Update()
    {
        if (onScreenSizeChanged == null) { return; }
        if (_previousWidth != Screen.width || _previousHeight != Screen.height)
        {
            _previousWidth = Screen.width;
            _previousHeight = Screen.height;
            onScreenSizeChanged.Invoke();
        }
    }
}