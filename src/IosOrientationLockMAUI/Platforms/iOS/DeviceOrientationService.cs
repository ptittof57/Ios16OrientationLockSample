using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
namespace IosOrientationLockMAUI.Platforms.iOS
{
    public class DeviceOrientationService : IDeviceOrientationService
    {
        private readonly AppDelegateEx _applicationDelegate;
        private readonly ILogger<IDeviceOrientationService> _logger;

        public DeviceOrientationService(ILogger<IDeviceOrientationService> logger)
        {
            if (MauiUIApplicationDelegate.Current is AppDelegateEx orientationServiceDelegate)
                _applicationDelegate = orientationServiceDelegate;
            else
                throw new NotImplementedException($"AppDelegate must be derived from {nameof(AppDelegateEx)} to use this implementation!");

            _logger = logger;
        }

        public DisplayOrientation CurrentOrientation => DeviceDisplay.Current.MainDisplayInfo.Orientation;


        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        private void SetOrientation(UIInterfaceOrientationMask uiInterfaceOrientationMask)
        {
            var rootWindowScene = (UIApplication.SharedApplication.ConnectedScenes.ToArray()?.FirstOrDefault()) as UIWindowScene;

            if (rootWindowScene == null)
                return;

#pragma warning disable CA1422
            var rootViewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
#pragma warning restore CA1422

            if (rootViewController == null)
                return;

            rootWindowScene.RequestGeometryUpdate(new UIWindowSceneGeometryPreferencesIOS(uiInterfaceOrientationMask),
            error =>
            {
                _logger.LogError("Error while attempting to lock orientation: {Error}", error.LocalizedDescription);
            });

            rootViewController.SetNeedsUpdateOfSupportedInterfaceOrientations();
            rootViewController.NavigationController?.SetNeedsUpdateOfSupportedInterfaceOrientations();
        }

        public void LockPortrait()
        {
            _applicationDelegate.CurrentLockedOrientation = UIInterfaceOrientationMask.Portrait;
            if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            {
                SetOrientation(UIInterfaceOrientationMask.Portrait);
            }
            else
            {
                UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));
            }
        }

        public void LockLandscape()
        {
            _applicationDelegate.CurrentLockedOrientation = UIInterfaceOrientationMask.LandscapeRight;
            if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            {
                SetOrientation(UIInterfaceOrientationMask.LandscapeRight);
            }
            else
            {
                UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.LandscapeRight), new NSString("orientation"));
            }
        }

        public void UnlockOrientation()
        {
            _applicationDelegate.CurrentLockedOrientation = UIInterfaceOrientationMask.AllButUpsideDown;
            if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            {
                SetOrientation(UIInterfaceOrientationMask.AllButUpsideDown);
            }
        }
    }
}
