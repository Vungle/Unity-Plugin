using System;
using UnityEngine;

namespace VungleAds
{
    public interface IVungleNative
    {
        bool CanPlay();
        void Load();
        void Attach(int x, int y, int width, int height);
        void Attach(int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            RectInt[] clickableRects);
        void Attach(RectTransform slot, RectTransform mediaSlot = null,
            RectTransform[] clickableViews = null);
        void Detach();
        void Destroy();
    }

    public partial class VungleNative : IVungleNative
    {
        public string placementId { get; private set; }
        public Action onLoadSuccess { get; set; }
        public Action<string> onLoadFailed { get; set; }
        public Action onDidPresent { get; set; }
        public Action<string> onPresentFailed { get; set; }
        public Action onDidClose { get; set; }
        public Action onImpression { get; set; }
        public Action onClick { get; set; }
        public Action onWillLeaveApplication { get; set; }
        public Action<string, string, string, double, string> onAdDataReceived { get; set; }

        public string AdTitle { get; private set; } = "";
        public string AdBody { get; private set; } = "";
        public string AdCallToAction { get; private set; } = "";
        public double AdStarRating { get; private set; }
        public string AdIconUrl { get; private set; } = "";

        internal bool isDestroyed;

        private readonly Vector3[] cornersBuffer = new Vector3[4];

        public void Attach(int x, int y, int width, int height)
        {
            if (isDestroyed) return;
            if (!CanPlay())
            {
                onPresentFailed?.Invoke("Native ad is not ready to attach — wait for onLoadSuccess");
                return;
            }
            AttachNative(x, y, width, height, x, y, width, height, null);
        }

        /// <summary>
        /// Attaches the native ad container over the given screen rect, with the
        /// media view laid out (aspect-fit) inside the separate media rect. Each
        /// entry in <paramref name="clickableRects"/> (screen coordinates, y down
        /// from the top) becomes a clickable region registered with the SDK, so
        /// taps on app-rendered elements there (title, icon, CTA, ...) count as
        /// ad clicks. When <paramref name="clickableRects"/> is null, the SDK
        /// defaults to making the media view clickable; when rects are provided,
        /// ONLY those regions are clickable — include a rect covering the media
        /// view to keep it clickable.
        /// </summary>
        public void Attach(int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            RectInt[] clickableRects)
        {
            if (isDestroyed) return;
            if (!CanPlay())
            {
                onPresentFailed?.Invoke("Native ad is not ready to attach — wait for onLoadSuccess");
                return;
            }
            AttachNative(x, y, width, height, mediaX, mediaY, mediaWidth, mediaHeight,
                Flatten(clickableRects));
        }

        /// <summary>
        /// Attaches the native ad container over <paramref name="slot"/>'s current
        /// screen position, with the media view laid out inside
        /// <paramref name="mediaSlot"/> (or filling the slot when null). Each
        /// RectTransform in <paramref name="clickableViews"/> becomes a clickable
        /// region (pass the slot itself to make everything clickable). When
        /// <paramref name="clickableViews"/> is null, the SDK defaults to making
        /// the media view clickable; when provided, ONLY the given regions are
        /// clickable — include <paramref name="mediaSlot"/> to keep the media view
        /// clickable. The position is captured once at the time of the call; call
        /// Attach again if the layout moves.
        /// </summary>
        public void Attach(RectTransform slot, RectTransform mediaSlot = null,
            RectTransform[] clickableViews = null)
        {
            if (isDestroyed || slot == null) return;
            if (!CanPlay())
            {
                onPresentFailed?.Invoke("Native ad is not ready to attach — wait for onLoadSuccess");
                return;
            }

            Canvas canvas = slot.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            var (slotMin, slotMax) = ScreenRect(slot, cam);

            int clickableCount = clickableViews != null ? clickableViews.Length : 0;
            int[] rects = new int[8 + clickableCount * 4];
            rects[0] = (int)slotMin.x;
            rects[1] = (int)(Screen.height - slotMax.y);
            rects[2] = (int)(slotMax.x - slotMin.x);
            rects[3] = (int)(slotMax.y - slotMin.y);

            // Sub-rects are measured relative to the slot in float space and
            // re-anchored to the slot's integer position so the geometry the
            // bridge receives is internally consistent
            if (mediaSlot == null || mediaSlot == slot)
                Array.Copy(rects, 0, rects, 4, 4);
            else
                WriteRelativeRect(mediaSlot, cam, slotMin, slotMax, rects, 4);
            for (int i = 0; i < clickableCount; i++)
            {
                if (clickableViews[i] == slot)
                    Array.Copy(rects, 0, rects, 8 + i * 4, 4);
                else
                    WriteRelativeRect(clickableViews[i], cam, slotMin, slotMax, rects, 8 + i * 4);
            }

            int[] clickableRects = null;
            if (clickableCount > 0)
            {
                clickableRects = new int[clickableCount * 4];
                Array.Copy(rects, 8, clickableRects, 0, clickableCount * 4);
            }
            AttachNative(rects[0], rects[1], rects[2], rects[3],
                rects[4], rects[5], rects[6], rects[7], clickableRects);
        }

        /// <summary>
        /// Hides the ad but keeps the native views and SDK registration alive,
        /// so a later Attach shows it again cheaply.
        /// </summary>
        public void Detach()
        {
            if (isDestroyed) return;
            DetachNative();
        }

        /// <summary>
        /// Releases the ad entirely: unregisters from the SDK (freeing the
        /// rendered media content, ad options view, and click handlers) and
        /// tears down the native views and bridge references. Call when the
        /// ad is no longer needed — e.g. leaving the scene or replacing the
        /// ad with a newly loaded one. The instance cannot be reused after.
        /// </summary>
        public void Destroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            DestroyNative();
        }

        private (Vector2 min, Vector2 max) ScreenRect(RectTransform rt, Camera cam)
        {
            rt.GetWorldCorners(cornersBuffer);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, cornersBuffer[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, cornersBuffer[2]);
            return (min, max);
        }

        // Writes rt's rect anchored to the slot's already-written integer rect
        // (dest[0..3]), using slot-relative offsets rounded in float space
        private void WriteRelativeRect(RectTransform rt, Camera cam,
            Vector2 slotMin, Vector2 slotMax, int[] dest, int offset)
        {
            var (emin, emax) = ScreenRect(rt, cam);
            dest[offset] = dest[0] + Mathf.RoundToInt(emin.x - slotMin.x);
            dest[offset + 1] = dest[1] + Mathf.RoundToInt(slotMax.y - emax.y);
            dest[offset + 2] = Mathf.RoundToInt(emax.x - emin.x);
            dest[offset + 3] = Mathf.RoundToInt(emax.y - emin.y);
        }

        private static int[] Flatten(RectInt[] rects)
        {
            if (rects == null || rects.Length == 0) return null;
            int[] flat = new int[rects.Length * 4];
            for (int i = 0; i < rects.Length; i++)
            {
                flat[i * 4] = rects[i].x;
                flat[i * 4 + 1] = rects[i].y;
                flat[i * 4 + 2] = rects[i].width;
                flat[i * 4 + 3] = rects[i].height;
            }
            return flat;
        }

        internal void SetAdData(string title, string body, string ctaText, double rating, string iconUrl)
        {
            AdTitle = title ?? "";
            AdBody = body ?? "";
            AdCallToAction = ctaText ?? "";
            AdStarRating = rating;
            AdIconUrl = iconUrl ?? "";
            onAdDataReceived?.Invoke(AdTitle, AdBody, AdCallToAction, AdStarRating, AdIconUrl);
        }
    }
}
