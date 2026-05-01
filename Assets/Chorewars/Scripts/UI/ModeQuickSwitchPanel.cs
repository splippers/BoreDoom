using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Chorewars.Core;
using UnityEngine.XR;

namespace Chorewars.UI
{
    /// <summary>
    /// Large world-space launch UI for Quest: IMGUI is not raycastable in VR.
    /// Uses uGUI + Meta OVRRaycaster / OVRInputModule when available so controller lasers work.
    /// </summary>
    public sealed class ModeQuickSwitchPanel : MonoBehaviour
    {
        private static ModeQuickSwitchPanel _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null) return;

            var go = new GameObject(nameof(ModeQuickSwitchPanel));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ModeQuickSwitchPanel>();
        }

        private GameObject _canvasRoot;
        private bool _built;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_built) BuildWorldUi();
        }

        private void Start()
        {
            if (!_built) BuildWorldUi();
        }

        private void LateUpdate()
        {
            if (_canvasRoot == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            var t = _canvasRoot.transform;
            var pos = cam.transform.position + cam.transform.forward * 1.65f + cam.transform.up * 0.08f;
            t.position = pos;
            t.rotation = Quaternion.LookRotation(cam.transform.position - t.position, Vector3.up);
        }

        private void BuildWorldUi()
        {
            if (_built) return;
            _built = true;

            EnsureVrEventSystem();

            _canvasRoot = new GameObject("ModeQuickSwitchCanvas");
            _canvasRoot.transform.SetParent(transform, false);

            var canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var rt = _canvasRoot.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1280, 860);

            var scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            TryInstallVrRaycaster(_canvasRoot);

            var panel = CreateUiChild(_canvasRoot.transform, "Panel", stretch: true);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.07f, 0.09f, 0.14f, 0.94f);

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 40, 40);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            AddTitle(panel, "BoreDOOM");
            AddSubtitle(panel, "Pick a chore mode — point your laser and pull the trigger.");
            AddBigButton(panel, "HooverMode", "Hoover", new Color(0.12f, 0.65f, 0.95f, 1f));
            AddBigButton(panel, "MowingMode", "Mowing", new Color(0.35f, 0.85f, 0.45f, 1f));
            AddBigButton(panel, "__RELOAD__", "Reload current scene", new Color(0.55f, 0.55f, 0.62f, 1f));

            var hint = CreateUiChild(panel.transform, "Hint", stretch: false);
            var hintRt = hint.GetComponent<RectTransform>();
            hintRt.sizeDelta = new Vector2(0, 110);
            var hintText = hint.gameObject.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (hintText.font == null)
                hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            hintText.fontSize = 28;
            hintText.color = new Color(0.85f, 0.88f, 0.95f, 0.85f);
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText.verticalOverflow = VerticalWrapMode.Overflow;
            hintText.text =
                "Tip: use the right controller laser on a button, then trigger.\n" +
                "Hands-only UI rays need Meta Interaction (next iteration).";

            var closeRow = CreateUiChild(panel.transform, "CloseRow", stretch: false);
            var closeRt = closeRow.GetComponent<RectTransform>();
            closeRt.sizeDelta = new Vector2(0, 70);
            var closeBtn = closeRow.gameObject.AddComponent<Button>();
            var closeImg = closeRow.gameObject.AddComponent<Image>();
            closeImg.color = new Color(0.25f, 0.27f, 0.32f, 1f);
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(() => _canvasRoot.SetActive(false));

            var closeTextGo = new GameObject("Label");
            closeTextGo.transform.SetParent(closeRow, false);
            var closeTextRt = closeTextGo.AddComponent<RectTransform>();
            closeTextRt.anchorMin = Vector2.zero;
            closeTextRt.anchorMax = Vector2.one;
            closeTextRt.offsetMin = Vector2.zero;
            closeTextRt.offsetMax = Vector2.zero;
            var closeText = closeTextGo.AddComponent<Text>();
            closeText.font = hintText.font;
            closeText.fontSize = 30;
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.text = "Hide panel";

            _canvasRoot.transform.localScale = Vector3.one * 0.00135f;
        }

        private static RectTransform CreateUiChild(Transform parent, string name, bool stretch)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            return rect;
        }

        private static void AddTitle(RectTransform parent, string title)
        {
            var rt = CreateUiChild(parent, "Title", stretch: false);
            rt.sizeDelta = new Vector2(0, 120);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 78;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = title;
        }

        private static void AddSubtitle(RectTransform parent, string text)
        {
            var rt = CreateUiChild(parent, "Subtitle", stretch: false);
            rt.sizeDelta = new Vector2(0, 70);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 30;
            t.color = new Color(0.75f, 0.8f, 0.9f, 0.95f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.text = text;
        }

        private void AddBigButton(RectTransform parent, string sceneKey, string label, Color bg)
        {
            var row = CreateUiChild(parent, $"Btn_{label}", stretch: false);
            row.sizeDelta = new Vector2(0, 108);

            var img = row.gameObject.AddComponent<Image>();
            img.color = bg;

            var btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(bg.r * 1.08f, bg.g * 1.08f, bg.b * 1.08f, 1f);
            colors.pressedColor = new Color(bg.r * 0.85f, bg.g * 0.85f, bg.b * 0.85f, 1f);
            btn.colors = colors;

            string sceneName = sceneKey;
            if (sceneKey == "__RELOAD__")
            {
                btn.onClick.AddListener(() =>
                {
                    var active = SceneManager.GetActiveScene().name;
                    SwitchTo(active);
                });
            }
            else
            {
                btn.onClick.AddListener(() => SwitchTo(sceneName));
            }

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(row, false);
            var tr = textGo.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var txt = textGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 44;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label.ToUpperInvariant();
        }

        private static void SwitchTo(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;

            BootstrapSceneRouter.SetLastModeSceneName(sceneName);
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
        }

        private static void EnsureVrEventSystem()
        {
            var systems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            EventSystem es = systems.Length > 0 ? systems[0] : null;
            for (int i = 1; i < systems.Length; i++)
            {
                if (systems[i] != null)
                    Destroy(systems[i].gameObject);
            }

            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
                DontDestroyOnLoad(esGo);
            }

            foreach (var m in es.GetComponents<BaseInputModule>())
                Destroy(m);

            if (!TryAddVrInputModule(es.gameObject))
                es.gameObject.AddComponent<StandaloneInputModule>();
        }

        private static void TryInstallVrRaycaster(GameObject canvasGo)
        {
            foreach (var gr in canvasGo.GetComponents<GraphicRaycaster>())
                Destroy(gr);

            if (!TryAddOvrRaycaster(canvasGo))
                canvasGo.AddComponent<GraphicRaycaster>();
        }

        private static bool TryAddVrInputModule(GameObject go)
        {
            var t = Type.GetType("UnityEngine.EventSystems.OVRInputModule, Oculus.VR", throwOnError: false)
                    ?? Type.GetType("UnityEngine.EventSystems.OVRInputModule, Meta.XR", throwOnError: false)
                    ?? FindTypeInLoadedAssemblies("OVRInputModule");
            if (t == null) return false;
            if (go.GetComponent(t) != null) return true;
            go.AddComponent(t);
            return true;
        }

        private static bool TryAddOvrRaycaster(GameObject go)
        {
            var t = Type.GetType("UnityEngine.EventSystems.OVRRaycaster, Oculus.VR", throwOnError: false)
                    ?? Type.GetType("UnityEngine.EventSystems.OVRRaycaster, Meta.XR", throwOnError: false)
                    ?? FindTypeInLoadedAssemblies("OVRRaycaster");
            if (t == null) return false;
            if (go.GetComponent(t) != null) return true;
            go.AddComponent(t);
            return true;
        }

        private static Type FindTypeInLoadedAssemblies(string simpleName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var ty in asm.GetTypes())
                    {
                        if (ty.Name == simpleName || ty.FullName == simpleName)
                            return ty;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // ignore noisy assemblies
                }
                catch
                {
                    // ignore
                }
            }

            return null;
        }
    }
}
