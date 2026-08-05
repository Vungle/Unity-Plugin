using System;
using UnityEngine;

namespace VungleAds
{
    public enum VungleBannerSize
    {
        Banner = 0,            // 320x50
        BannerShort = 1,       // 300x50
        BannerLeaderboard = 2, // 728x90
        Mrec = 3,              // 300x250
        FlexibleHeight = 4,    // Publisher-defined width, creative-determined height
        FixedSize = 5          // Custom width and height
    }

    public interface IVungleBannerView
    {
        void Load();
        void Attach(int x, int y);
        void Attach(int x, int y, int width, int height);
        void Attach(RectTransform slot);
        void Detach();
        void Destroy();
    }

    public partial class VungleBannerView : IVungleBannerView
    {
        public string placementId { get; private set; }
        public VungleBannerSize adSize { get; private set; }
        public int customWidth { get; private set; }
        public int customHeight { get; private set; }
        public Action onLoadSuccess { get; set; }
        public Action<string> onLoadFailed { get; set; }
        public Action onWillPresent { get; set; }
        public Action onDidPresent { get; set; }
        public Action<string> onPresentFailed { get; set; }
        public Action onWillClose { get; set; }
        public Action onDidClose { get; set; }
        public Action onImpression { get; set; }
        public Action onClick { get; set; }
        public Action onWillLeaveApplication { get; set; }

        internal bool isDestroyed;

        public void Attach(int x, int y)
        {
            if (isDestroyed) return;
            AttachNative(x, y, 0, 0);
        }

        public void Attach(int x, int y, int width, int height)
        {
            if (isDestroyed) return;
            AttachNative(x, y, width, height);
        }

        /// <summary>
        /// Attaches the banner over <paramref name="slot"/>'s current screen
        /// position. The position is captured once at the time of the call;
        /// call Attach again if the layout moves.
        /// </summary>
        public void Attach(RectTransform slot)
        {
            if (isDestroyed || slot == null) return;

            Canvas canvas = slot.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Vector3[] corners = new Vector3[4];
            slot.GetWorldCorners(corners);

            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

            AttachNative((int)min.x, (int)(Screen.height - max.y),
                (int)(max.x - min.x), (int)(max.y - min.y));
        }

        public void Detach()
        {
            if (isDestroyed) return;
            DetachNative();
        }

        public void Destroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            DestroyNative();
        }
    }
}
