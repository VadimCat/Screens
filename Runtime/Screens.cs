using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Ji2.Camera;
using Ji2.Screens;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Ji2Core.Core.ScreenNavigation
{
    public class Screens : MonoBehaviour, IScreenSize
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private List<BaseScreen> screens;
        [SerializeField] private RectTransform transform;
        [SerializeField] private CanvasScaler scaler;

        private Dictionary<Type, BaseScreen> _screenOrigins;

        private BaseScreen _currentScreen;
        private CameraSource _cameraSource;
        private IObjectResolver _resolver;

        public BaseScreen CurrentScreen => _currentScreen;

        public Vector2 ScreenSize => new(transform.rect.width, transform.rect.height);

        public float ScaleFactor => transform.rect.height / scaler.referenceResolution.y;

        [Inject]
        public void Construct(CameraSource cameraSource, IObjectResolver resolver)
        {
            _resolver = resolver;
            _cameraSource = cameraSource;
            _cameraSource.CameraChanged += SetCamera;
            SetCamera(cameraSource.MainCamera);
                
            _screenOrigins = new Dictionary<Type, BaseScreen>();
            foreach (var screen in screens)
            {
                _screenOrigins[screen.GetType()] = screen;
            }
        }

        private void SetCamera(Camera camera)
        {
            canvas.worldCamera = camera;
        }

        public async Task<BaseScreen> PushScreen(Type type)
        {
            if (_currentScreen != null)
            {
                await CloseCurrent();
            }

            _currentScreen = Instantiate(_screenOrigins[type], transform);
            await _currentScreen.Show();
            return _currentScreen;
        }

        public async UniTask<TScreen> PushScreen<TScreen>() where TScreen : BaseScreen
        {
            if (_currentScreen != null)
            {
                await CloseCurrent();
            }

            if (_currentScreen is TScreen screen)
            {
                return screen;
            }
            
            _currentScreen = _resolver.Instantiate(_screenOrigins[typeof(TScreen)], transform);
            await _currentScreen.Show();
            return (TScreen)_currentScreen;
        }

        public async UniTask CloseScreen<TScreen>() where TScreen : BaseScreen
        {
            if (_currentScreen is TScreen)
            {
                await _currentScreen.Close();
                Destroy(_currentScreen.gameObject);
                _currentScreen = null;
            }
        }

        private async UniTask CloseCurrent()
        {
            await _currentScreen.Close();
            Destroy(_currentScreen.gameObject);
            _currentScreen = null;
        }
    }

    public interface IScreenSize
    {
        Vector2 ScreenSize { get; }
    }
}