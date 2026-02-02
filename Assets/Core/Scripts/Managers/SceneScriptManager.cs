using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Game.Config.Config;


public class SceneScriptManager : MonoBehaviour
{
    [Header("Scene Load Options")]
    [SerializeField] private bool useAdditiveLoad = true;     // target scene load additive
    [SerializeField] private bool useLoadingScene = false;     // pakai loading scene atau tidak
    [SerializeField] private bool unloadPreviousScene = true; // unload scene lama setelah selesai

    public static SceneScriptManager _instance { get; private set; }

    [Header("Enhanced Animation Options")]
    [SerializeField] private LoadingAnimationMode animationMode = LoadingAnimationMode.Cinematic;
    [SerializeField] private float progressSpeedScale = 1.0f;

    private Coroutine _finalLoopRoutine;

    [Header("Loading Simulation Settings")]
    [SerializeField] private bool simulateDetailedLoad = true;
    [SerializeField] private bool verboseLog = true;
    [SerializeField] private float minStepDelay = 0.05f;
    [SerializeField] private float maxStepDelay = 0.20f;

    public SceneState _currentState;
    public SceneState _lastCurrentState;
    public List<SceneState> _stateCollections;
    private string _currentStateString;
    private GameEventBus _eventBus;

    private float _smooth;
    private float _target;

    private bool _isDone;
    private bool _doneEventFired;

    // NEW: angle loop vars
    private float _angle;
    private float _angleSpeed = 180f; // degrees per second


    private void Awake()
    {

        _instance = this;
        _eventBus = GameEventBus.Instance; //Deprecated old Function
        var scene = SceneManager.GetActiveScene();
        _currentState = SceneConverter(_currentState);


        DontDestroyOnLoad(gameObject);
    }

    private void Reset()
    {
        _currentState = SceneConverter(_currentState);
    }

    private SceneState SceneConverter(SceneState sceneState)
    {
        var scene = SceneManager.GetActiveScene();
        var sceneString = SwitchStringToSceneState(scene.name);
        return sceneString;
    }

    public static SceneScriptManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SceneScriptManager");
                _instance = go.AddComponent<SceneScriptManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[SceneScriptManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    private readonly string[] LoadingSteps =
    {
            "Scanning assets...",
            "Checking bundle dependencies...",
            "Resolving memory allocations...",
            "Preparing GPU texture uploads...",
            "Constructing shaders...",
            "Building scene object map...",
            "Syncing gameplay systems...",
            "Prewarming effects...",
            "Final validations...",
            "Finishing load..."
        };

    private readonly float[] StepWeights =
    {
            0.04f, 0.06f, 0.08f, 0.10f, 0.07f,
            0.12f, 0.10f, 0.10f, 0.08f, 0.15f
        };

    // Fitur utama satu untuk langsung mengganti berdasarkan fungsi additive atau single load
    public void ChangeState(SceneState newState)
    {
        // preserve previous state (keep prior logic but resilient to nulls)
        _lastCurrentState = SceneManager.GetActiveScene().name != null ? SceneConverter(_currentState) : SceneState.None;
        _currentState = newState;

        // Ensure collection exists
        if (_stateCollections == null)
            _stateCollections = new List<SceneState>();

        // Resolve target scene name once
        string targetSceneName = SwitchStateScene(newState);

        // Default: load the scene (adds or single load determined by LoadSceneAsync)
        LoadSceneAsync(targetSceneName);
    }

    private void SetActiveSceneBzbzbzbzbzbHeee()
    {
        // If we've seen this state before, try to avoid reloading:
        if (_stateCollections.Contains(_currentState))
        {
            var scene = SceneManager.GetSceneByName(SwitchStateScene(_currentState));
            if (scene.IsValid() && scene.isLoaded)
            {
                //While loaded a menu scene on exit(return to main menu) 
                if (scene.name == SwitchStateScene(SceneState.Menu))
                {
                    var sceneCorePlay = SceneManager.GetSceneByName(SwitchStateScene(SceneState.CoreLoader));
                    if (sceneCorePlay.isLoaded)
                    {
                        SceneManager.UnloadSceneAsync(SwitchStateScene(SceneState.CoreLoader));
                        _stateCollections.Clear();
                    }

                    SceneManager.LoadScene(SwitchStateScene(SceneState.Menu));
                }
                else
                {
                    // 1. core
                    // 2. intro
                    // 3. first stage
                    // 4. second stage
                    // Handle Intro scene bug


                    if (SwitchStateScene(_lastCurrentState) == SwitchStateScene(SceneState.Intro))
                    {
                        SceneManager.UnloadSceneAsync(SwitchStateScene(SceneState.Intro));
                    }

                    // Scene is already loaded: make it active and return
                    SceneManager.SetActiveScene(scene);

                }
                if (verboseLog)
                    Debug.Log($"[SceneScriptManager] State Set active scene to '{_currentState}'.");
                return;
            }
            else
            {
                if (verboseLog)
                    Debug.Log($"[SceneScriptManager] State found in collection but scene '{_currentState}' is not loaded. Proceeding to load.");
            }
        }

    }

    private void LoadSceneAsync(string sceneName)
    {
        _currentStateString = sceneName;

        StartCoroutine(FullLoadRoutine(sceneName != "" || sceneName != "None" ? sceneName : _currentStateString));
    }

    private IEnumerator FullLoadRoutine(string targetScene)
    {
        _isDone = false;
        _doneEventFired = false;
        _smooth = 0;
        _target = 0;
        _angle = 0;


        // NEW — force UI angle = 0
        _eventBus.Publish(new GameLoading
        {
            _forceAngleReset = true,
            _angle = 0f,
            _loop = true
        });

        // Start angle loop
        StartCoroutine(AngleLoop());


        if (useLoadingScene)
        {
            if (SceneManager.GetActiveScene().name != loadingScene)
            {
                yield return SceneManager.LoadSceneAsync(
                    loadingScene
                );
            }

            yield return new WaitForSeconds(0.2f);

            if (simulateDetailedLoad)
                yield return SimulateDetailedLoad();

            yield return ActualSceneLoadRoutine(targetScene);

            EnsureDone();
        }
        else
        {

            if (!_stateCollections.Contains(SwitchStringToSceneState(targetScene)) && SceneManager.GetActiveScene().name != targetScene)
            {

                _stateCollections.Add(SwitchStringToSceneState(targetScene));

                yield return SceneManager.LoadSceneAsync(
                    targetScene,
                    useAdditiveLoad ? LoadSceneMode.Additive : LoadSceneMode.Single
                );

            }

            EnsureDone();
        }


        SetActiveSceneBzbzbzbzbzbHeee();
    }

    private IEnumerator AngleLoop()
    {
        while (!_isDone)
        {
            _angle += Time.deltaTime * _angleSpeed;
            if (_angle >= 360f) _angle -= 360f;

            _eventBus.Publish(new GameLoading
            {
                _angle = _angle,
                _loop = true
            });

            yield return null;
        }
    }

    private IEnumerator FinalProgressLoop()
    {
        // "new AsyncOperation()" is invalid and causes a managed AsyncOperation
        // without a native handle which throws when accessing .isDone/.progress.
        // Use a nullable AsyncOperation and only poll if the unload actually started.
        AsyncOperation asyncUnload = null;

        // unload scene if applicable
        if (_lastCurrentState == SceneState.Menu)
        {
            asyncUnload = SceneManager.UnloadSceneAsync(SwitchStateScene(_lastCurrentState));
        }

        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
            {
                // defensive: if for any reason asyncUnload becomes invalid, break out
                if (asyncUnload == null)
                    break;

                if (asyncUnload.progress >= 0.9f)
                {
                    UnityEngine.Debug.Log("Unloading...");
                    break;
                }

                yield return null;
            }
        }

        float pulseTimer = 0f;

        while (true)
        {
            pulseTimer += Time.deltaTime * progressSpeedScale;

            _eventBus.Publish(new GameLoading
            {
                _targetProgress = 1f,
                _message = "Finalizing system...",
                _angle = _angle,
                _loop = true
            });

            yield return null;
        }
    }

    private IEnumerator SimulateDetailedLoad()
    {
        for (int i = 0; i < LoadingSteps.Length; i++)
        {
            if (_isDone) yield break;

            string msg = LoadingSteps[i];
            float w = StepWeights[i];

            if (verboseLog)
                Debug.Log($"[LoadSim] {msg}");

            _eventBus.Publish(new GameLoading { _message = msg });

            float stepEnd = Mathf.Clamp01(_target + w);

            while (_smooth < stepEnd)
            {
                if (_isDone) yield break;

                switch (animationMode)
                {
                    case LoadingAnimationMode.Tween:
                        _target = Mathf.MoveTowards(_target, stepEnd, Time.deltaTime * w * 3f * progressSpeedScale);
                        _smooth = Mathf.Lerp(_smooth, _target, Time.deltaTime * 8f * progressSpeedScale);
                        break;

                    case LoadingAnimationMode.Cinematic:
                        _target += Time.deltaTime * w * 1.4f * progressSpeedScale;
                        _smooth = Mathf.Lerp(_smooth, _target, Time.deltaTime * 2.5f * progressSpeedScale);
                        break;
                }

                _smooth = Mathf.Clamp01(_smooth);
                float eased = EaseSmooth(_smooth);

                _eventBus.Publish(new GameLoading
                {
                    _targetProgress = eased,
                    _loop = true,
                    _message = msg
                });

                yield return null;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(minStepDelay, maxStepDelay) / progressSpeedScale);
        }
    }

    private IEnumerator ActualSceneLoadRoutine(string sceneName)
    {
        AsyncOperation op =
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (_isDone) yield break;

            float mapped = Mathf.Lerp(0.90f, 0.99f, op.progress / 0.9f);

            _eventBus.Publish(new GameLoading
            {
                _targetProgress = mapped,
                _message = "Finalizing scene..."
            });

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        op.allowSceneActivation = true;
    }

    private void EnsureDone()
    {
        // Never fire DONE or final loop for splash screen
        if (_currentState == SceneState.SplashScreen)
            return;

        if (_doneEventFired) return;

        _doneEventFired = true;
        _isDone = true;

        _eventBus.Publish(new GameLoading
        {
            _targetProgress = 1f,
            _isDoneLoad = true,
            _message = "Loading complete!",
            _angle = 0f,
            _loop = true,
            _forceAngleReset = true
        });


        if (_finalLoopRoutine != null)
            StopCoroutine(_finalLoopRoutine);

        // 🔥 Start infinite final UI loop
        _finalLoopRoutine = StartCoroutine(FinalProgressLoop());

        if (verboseLog)
            Debug.Log("[SceneManager] DONE is DONE (final loop active).");
    }

    private float EaseSmooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

}

