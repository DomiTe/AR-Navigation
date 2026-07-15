using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace UniNav.Core {
    public class PathController : MonoBehaviour {

        [Header("References")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform xrCamera;
        [SerializeField] private Transform buildingRoot;

        [Header("Path Settings")]
        [SerializeField] private float dotSpacing = 0.65f;
        [SerializeField] private float arrivalDistance = 1.5f;

        private static readonly Color GREEN_MAIN  = new Color(0.46f, 0.72f, 0.17f);
        private static readonly Color GREEN_DARK  = new Color(0.28f, 0.50f, 0.08f);
        private static readonly Color GREEN_LIGHT = new Color(0.68f, 0.88f, 0.35f);

        private NavMeshPath _path;
        private Vector3 _targetPos;
        private bool _hasTarget = false;
        private bool _arrived = false;
        private string _targetName = "";

        private float _lastRecalcTime = 0f;
        private const float RecalcInterval = 0.5f;

        private List<GameObject> _dots = new List<GameObject>();
        private List<float> _dotBaseSizes = new List<float>();
        private Material _dotMat;
        private Material _whiteMat;

        private GameObject _arrowObj;
        private GameObject _arrowDisc;
        private GameObject _arrowChevron;
        private float _arrowAngle = 0f;
        private float _arrowAngleSmooth = 0f;
        private Vector3[] _cachedCorners = new Vector3[0];

        private Canvas _hudCanvas;
        private GameObject _bottomPanel;
        private TextMeshProUGUI _destText;
        private TextMeshProUGUI _distText;
        private GameObject _arrivalPanel;
        private GameObject _edgeIndicator;
        private RectTransform _edgeIndicatorRT;
        private TextMeshProUGUI _edgeIndicatorTxt;

        private void Start() {
            _path = new NavMeshPath();
            if (xrCamera == null) Debug.LogError("XR CAMERA NOT ASSIGNED");
            if (buildingRoot == null) Debug.LogError("BUILDING ROOT NOT ASSIGNED");

            NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
            Debug.Log($"NavMesh: {tri.vertices.Length} vertices");

            BuildMaterials();
            BuildArrow();
            BuildHUD();
        }

        private void Update() {
            if (!_hasTarget || _arrived) return;

            float dist = Vector3.Distance(
                new Vector3(xrCamera.position.x, 0f, xrCamera.position.z),
                new Vector3(_targetPos.x, 0f, _targetPos.z));

            if (dist < arrivalDistance) { TriggerArrival(); return; }

            UpdateDistanceText(dist);
            UpdateArrowTransform();
            AnimateDotsUpdate();

            if (Time.time - _lastRecalcTime >= RecalcInterval) {
                _lastRecalcTime = Time.time;
                CalculateAndDrawPath();
            }
        }


        public void SetTarget(Vector3 localCoordinates, string targetName = "Ziel") {
            if (buildingRoot == null) { Debug.LogError("Building Root missing!"); return; }
            ClearDots();
            _targetPos = buildingRoot.TransformPoint(localCoordinates);
            _targetName = targetName;
            _hasTarget = true;
            _arrived = false;

            if (_destText != null) _destText.text = targetName;
            if (_distText != null) _distText.text = "";
            if (_bottomPanel != null) _bottomPanel.SetActive(true);
            if (_arrivalPanel != null) _arrivalPanel.SetActive(false);
            if (_arrowObj != null) _arrowObj.SetActive(true);

            Debug.Log($"Target world: {_targetPos}");
        }

        private void CalculateAndDrawPath() {
            if (xrCamera == null) return;

            Vector3 startPos = xrCamera.position;

            NavMeshHit sh;
            if (!NavMesh.SamplePosition(startPos, out sh, 3f, NavMesh.AllAreas)) {
                Debug.LogWarning($"Start {startPos} not on NavMesh."); return;
            }
            NavMeshHit th;
            if (!NavMesh.SamplePosition(_targetPos, out th, 2f, NavMesh.AllAreas)) {
                Debug.LogError($"Target {_targetPos} not on NavMesh."); _hasTarget = false; return;
            }

            if (NavMesh.CalculatePath(sh.position, th.position, NavMesh.AllAreas, _path)) {
                if (_path.status == NavMeshPathStatus.PathComplete) {
                    SpawnChevrons(_path.corners);
                    _cachedCorners = _path.corners;
                    UpdateArrowDirection(_path.corners);
                    Debug.Log($"Path: {_path.corners.Length} corners.");
                } else {
                    Debug.LogWarning($"Path status: {_path.status}"); _hasTarget = false;
                }
            } else {
                Debug.LogError("CalculatePath false."); _hasTarget = false;
            }
        }

        private void UpdateArrowDirection(Vector3[] corners) {
            if (corners.Length < 2) return;

            Vector3 navDir = corners[1] - xrCamera.position;
            navDir.y = 0f;
            if (navDir == Vector3.zero) return;
            navDir.Normalize();

            Vector3 camFwd = xrCamera.forward; camFwd.y = 0f;
            if (camFwd == Vector3.zero) return;
            camFwd.Normalize();
            Vector3 camRight = xrCamera.right; camRight.y = 0f; camRight.Normalize();

            _arrowAngle = Mathf.Atan2(Vector3.Dot(navDir, camRight), Vector3.Dot(navDir, camFwd)) * Mathf.Rad2Deg;
        }

        private void UpdateArrowTransform() {
            if (_arrowObj == null || !_arrowObj.activeSelf) return;

            float t = Time.time;
            float bob = Mathf.Sin(t * 1.8f) * 0.04f;

            Vector3 camFwdH = xrCamera.forward;
            camFwdH.y = 0f;
            if (camFwdH == Vector3.zero) camFwdH = Vector3.forward;
            camFwdH.Normalize();

            Vector3 pos = xrCamera.position + camFwdH * 4f;
            pos.y = xrCamera.position.y - 0.15f + bob;
            _arrowObj.transform.position = pos;
            _arrowObj.transform.LookAt(xrCamera.position);

            if (_arrowDisc != null) {
                float pulse = 1f + Mathf.Sin(t * 2.5f) * 0.06f;
                _arrowDisc.transform.localScale = new Vector3(0.42f * pulse, 0.42f * pulse, 0.06f);
            }

            // Winkel jeden Frame aktualisieren (Kamera dreht sich, Corners bleiben cached)
            if (_cachedCorners.Length >= 2)
                UpdateArrowDirection(_cachedCorners);

            _arrowAngleSmooth = Mathf.LerpAngle(_arrowAngleSmooth, _arrowAngle, Time.deltaTime * 8f);

            if (_arrowChevron != null)
                _arrowChevron.transform.localRotation = Quaternion.Euler(0f, 0f, -_arrowAngleSmooth);

            UpdateEdgeIndicator();
        }

        // am Bildschirmrand wenn der Pfeil außerhalb des Sichtfeldes ist
        private void UpdateEdgeIndicator() {
            if (_edgeIndicator == null || _arrowObj == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 vp = cam.WorldToViewportPoint(_arrowObj.transform.position);
            bool inView = vp.z > 0f && vp.x > 0.08f && vp.x < 0.92f && vp.y > 0.08f && vp.y < 0.92f;

            _edgeIndicator.SetActive(!inView && _arrowObj.activeSelf);
            if (inView || !_arrowObj.activeSelf) return;

            Vector2 dir = (vp.z < 0f)
                ? new Vector2(0.5f - vp.x, 0.5f - vp.y).normalized
                : new Vector2(vp.x - 0.5f, vp.y - 0.5f).normalized;

            float refW = 1080f;
            float refH = refW * ((float)Screen.height / Screen.width);
            float halfW = refW * 0.5f - refW * 0.10f;
            float halfH = refH * 0.5f - refH * 0.10f;
            float tx = (Mathf.Abs(dir.x) > 0.001f) ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
            float ty = (Mathf.Abs(dir.y) > 0.001f) ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
            _edgeIndicatorRT.anchoredPosition = dir * Mathf.Min(tx, ty);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            _edgeIndicatorRT.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void SpawnChevrons(Vector3[] corners) {
            ClearDots();
            const float skipNearDist = 1.5f;
            float accDist = 0f;

            for (int i = 0; i < corners.Length - 1; i++) {
                Vector3 a = corners[i];
                Vector3 b = corners[i + 1];
                Vector3 segDir = b - a; segDir.y = 0f;
                float segLen = segDir.magnitude;
                if (segLen < 0.01f) continue;
                segDir.Normalize();

                // Chevron an Kurven: Bisector zwischen einkommendem und ausgehendem Segment
                if (i > 0) {
                    Vector3 prevDir = (a - corners[i - 1]); prevDir.y = 0f;
                    if (prevDir.magnitude > 0.01f) {
                        prevDir.Normalize();
                        Vector3 bisector = (prevDir + segDir).normalized;
                        if (bisector.magnitude < 0.01f) bisector = segDir;
                        SpawnSingleChevron(new Vector3(a.x, a.y + 0.015f, a.z), bisector);
                    }
                }

                int count = Mathf.Max(1, Mathf.FloorToInt(segLen / dotSpacing));
                for (int j = 0; j < count; j++) {
                    float t = (float)j / count;
                    if (accDist + t * segLen < skipNearDist) continue; // erste 1.5m frei lassen
                    Vector3 p = Vector3.Lerp(a, b, t);
                    p.y = Mathf.Lerp(a.y, b.y, t) + 0.015f;
                    SpawnSingleChevron(p, segDir);
                }

                accDist += segLen;
            }
        }

        private void SpawnSingleChevron(Vector3 position, Vector3 direction) {
            GameObject chevron = new GameObject("NavChevron");
            chevron.transform.position = position;
            chevron.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            BuildChevronArms(chevron.transform);
            _dots.Add(chevron);
            _dotBaseSizes.Add(1f);
        }

        // ∧ Form aus zwei flachen Würfeln auf dem Boden
        private void BuildChevronArms(Transform parent) {
            float armLen = 0.26f;
            float armW   = 0.075f;
            float armH   = 0.022f;
            float angle  = 38.7f;

            GameObject armL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armL.transform.SetParent(parent);
            armL.transform.localPosition = new Vector3(-0.08f, 0f, 0f);
            armL.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            armL.transform.localScale    = new Vector3(armW, armH, armLen);
            armL.GetComponent<Renderer>().material = _dotMat;
            Destroy(armL.GetComponent<Collider>());

            GameObject armR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armR.transform.SetParent(parent);
            armR.transform.localPosition = new Vector3(0.08f, 0f, 0f);
            armR.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);
            armR.transform.localScale    = new Vector3(armW, armH, armLen);
            armR.GetComponent<Renderer>().material = _dotMat;
            Destroy(armR.GetComponent<Collider>());
        }

        // Wellen-Animation, Farbe nach Distanz, passierte Chevrons ausblenden
        private void AnimateDotsUpdate() {
            float t = Time.time;
            int idx = 0;

            Vector3 camPosFlat = new Vector3(xrCamera.position.x, 0f, xrCamera.position.z);
            Vector3 camFwdFlat = new Vector3(xrCamera.forward.x, 0f, xrCamera.forward.z).normalized;

            for (int i = 0; i < _dots.Count; i++) {
                if (_dots[i] == null || _dots[i].name != "NavChevron") continue;

                Vector3 dotPosFlat = new Vector3(_dots[i].transform.position.x, 0f, _dots[i].transform.position.z);
                Vector3 toDot = dotPosFlat - camPosFlat;
                float dist = toDot.magnitude;

                // Chevron hinter dem Nutzer ausblenden
                bool passed = dist < 1.2f && Vector3.Dot(toDot.normalized, camFwdFlat) < 0f;
                if (_dots[i].activeSelf == passed) _dots[i].SetActive(!passed);
                if (passed) { idx++; continue; }

                float wave = 0.5f + 0.5f * Mathf.Sin(t * 3.5f - idx * 0.6f);
                float s = 0.80f + wave * 0.20f;
                _dots[i].transform.localScale = new Vector3(s, 1f, s);

                // Nah = hellgrün, weit = dunkelgrün
                float fade = Mathf.Clamp01(dist / 7f);
                Color chevronColor = Color.Lerp(GREEN_LIGHT, GREEN_DARK, fade);

                Renderer[] rends = _dots[i].GetComponentsInChildren<Renderer>();
                foreach (var r in rends) {
                    r.material.color = chevronColor;
                    if (r.material.HasProperty("_EmissionColor"))
                        r.material.SetColor("_EmissionColor", chevronColor * (0.6f - fade * 0.4f));
                }

                idx++;
            }
        }

        private void ClearDots() {
            foreach (var d in _dots) if (d != null) Destroy(d);
            _dots.Clear();
            _dotBaseSizes.Clear();
        }

        private void BuildArrow() {
            _arrowObj = new GameObject("NavArrow");

            _arrowDisc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _arrowDisc.transform.SetParent(_arrowObj.transform);
            _arrowDisc.transform.localPosition = Vector3.zero;
            _arrowDisc.transform.localScale    = new Vector3(0.42f, 0.42f, 0.06f);
            _arrowDisc.GetComponent<Renderer>().material = _dotMat;
            Destroy(_arrowDisc.GetComponent<Collider>());
            _arrowDisc.name = "ArrowDisc";

            _arrowChevron = new GameObject("Chevron");
            _arrowChevron.transform.SetParent(_arrowObj.transform);
            _arrowChevron.transform.localPosition = new Vector3(0f, 0f, 0.04f);
            _arrowChevron.transform.localRotation = Quaternion.identity;

            float thick = 0.038f;

            GameObject armL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armL.transform.SetParent(_arrowChevron.transform);
            armL.transform.localPosition = new Vector3(-0.065f, 0.01f, 0f);
            armL.transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
            armL.transform.localScale    = new Vector3(thick, 0.19f, thick);
            armL.GetComponent<Renderer>().material = _whiteMat;
            Destroy(armL.GetComponent<Collider>());

            GameObject armR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armR.transform.SetParent(_arrowChevron.transform);
            armR.transform.localPosition = new Vector3(0.065f, 0.01f, 0f);
            armR.transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
            armR.transform.localScale    = new Vector3(thick, 0.19f, thick);
            armR.GetComponent<Renderer>().material = _whiteMat;
            Destroy(armR.GetComponent<Collider>());

            _arrowObj.SetActive(false);
        }

        private void TriggerArrival() {
            _hasTarget = false;
            _arrived   = true;
            ClearDots();
            if (_arrowObj    != null) _arrowObj.SetActive(false);
            if (_bottomPanel != null) _bottomPanel.SetActive(false);
            if (_arrivalPanel != null) {
                _arrivalPanel.SetActive(true);
                StartCoroutine(HideAfter(_arrivalPanel, 4f));
            }
            Debug.Log("Arrived.");
        }

        private IEnumerator HideAfter(GameObject go, float delay) {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(false);
        }

        private void BuildHUD() {
            float refW = 1080f;
            float refH = refW * ((float)Screen.height / Screen.width);
            float hudH = refH * 0.096f;
            float pad  = refW * 0.018f;

            GameObject canvasGO = new GameObject("NavHUD");
            _hudCanvas = canvasGO.AddComponent<Canvas>();
            _hudCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 10;

            CanvasScaler sc = canvasGO.AddComponent<CanvasScaler>();
            sc.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(refW, refH);
            sc.matchWidthOrHeight  = 0f;

            canvasGO.AddComponent<GraphicRaycaster>();

            _bottomPanel = MakePanel(canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(pad, pad), new Vector2(-pad, hudH),
                new Color(0.08f, 0.12f, 0.08f, 0.88f));

            GameObject accent = MakePanel(_bottomPanel.transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(8f, 0f), GREEN_MAIN);
            accent.name = "Accent";

            float fsBig = refH * 0.023f;
            float fsSm  = refH * 0.016f;

            _destText = MakeText(_bottomPanel.transform,
                new Vector2(0f, 0.48f), new Vector2(1f, 1f),
                new Vector2(28f, 0f), new Vector2(-16f, -8f),
                "", fsBig, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);

            _distText = MakeText(_bottomPanel.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0.48f),
                new Vector2(28f, 8f), new Vector2(-16f, 0f),
                "", fsSm, FontStyles.Normal, GREEN_LIGHT, TextAlignmentOptions.Left);

            _bottomPanel.SetActive(false);

            float arrW = refW * 0.88f;
            float arrH = refH * 0.15f;
            _arrivalPanel = MakePanel(canvasGO.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-arrW * 0.5f, -arrH * 0.5f), new Vector2(arrW * 0.5f, arrH * 0.5f),
                new Color(GREEN_MAIN.r, GREEN_MAIN.g, GREEN_MAIN.b, 0.93f));

            MakeText(_arrivalPanel.transform,
                Vector2.zero, Vector2.one,
                new Vector2(16f, 16f), new Vector2(-16f, -16f),
                "Angekommen!", refH * 0.029f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);

            _arrivalPanel.SetActive(false);

            // Indikator am Bildschirmrand
            _edgeIndicator = new GameObject("EdgeIndicator");
            _edgeIndicator.transform.SetParent(canvasGO.transform, false);
            _edgeIndicatorRT = _edgeIndicator.AddComponent<RectTransform>();
            _edgeIndicatorRT.anchorMin = new Vector2(0.5f, 0.5f);
            _edgeIndicatorRT.anchorMax = new Vector2(0.5f, 0.5f);
            _edgeIndicatorRT.sizeDelta = new Vector2(refW * 0.07f, refW * 0.07f);
            _edgeIndicatorTxt = _edgeIndicator.AddComponent<TextMeshProUGUI>();
            _edgeIndicatorTxt.text      = "▲";
            _edgeIndicatorTxt.fontSize  = refW * 0.055f;
            _edgeIndicatorTxt.color     = GREEN_MAIN;
            _edgeIndicatorTxt.alignment = TextAlignmentOptions.Center;
            _edgeIndicator.SetActive(false);
        }

        private void UpdateDistanceText(float dist) {
            if (_distText == null) return;
            _distText.text = dist >= 10f
                ? $"ca. {Mathf.RoundToInt(dist)} m entfernt"
                : $"ca. {dist:F1} m entfernt";
        }

        private void BuildMaterials() {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _dotMat = new Material(s);
            _dotMat.color = GREEN_MAIN;
            if (_dotMat.HasProperty("_EmissionColor")) {
                _dotMat.EnableKeyword("_EMISSION");
                _dotMat.SetColor("_EmissionColor", GREEN_MAIN * 0.55f);
            }
            _whiteMat = new Material(s) { color = Color.white };
        }

        private GameObject MakePanel(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 offsetMin, Vector2 offsetMax, Color color) {
            GameObject go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private TextMeshProUGUI MakeText(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax,
            string text, float size, FontStyles style, Color color, TextAlignmentOptions align) {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
            tmp.color = color; tmp.alignment = align;
            return tmp;
        }

        private void OnDestroy() {
            ClearDots();
            if (_arrowObj  != null) Destroy(_arrowObj);
            if (_hudCanvas != null) Destroy(_hudCanvas.gameObject);
        }
    }
}
