using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AmbientSoundManagerTests
{
    private GameObject go;
    private AmbientSoundManager ambientSoundManager;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        ambientSoundManager = go.AddComponent<AmbientSoundManager>();
        ambientSoundManager.ambientAudioSource = go.AddComponent<AudioSource>();
        ambientSoundManager.ambientSounds = new AudioClip[] { AudioClip.Create("TestClip", 44100, 1, 44100, false) };
        ambientSoundManager.minInterval = 0.1f;
        ambientSoundManager.maxInterval = 0.2f;

        if (Object.FindObjectOfType<AudioListener>() == null)
        {
            go.AddComponent<AudioListener>();
        }
    }

    [Test]
    public void Test_AudioSourceNull()
    {
        ambientSoundManager.ambientAudioSource = null;
        ambientSoundManager.Start();
        Assert.IsNotNull(ambientSoundManager.ambientAudioSource);
    }

    [UnityTest]
    public IEnumerator Test_RandomAmbience()    // INTEGRATION TEST
    {
        Coroutine coroutine = ambientSoundManager.StartCoroutine(ambientSoundManager.PlayRandomAmbientSounds());

        yield return new WaitForSeconds(0.3f);

        Assert.IsTrue(ambientSoundManager.ambientAudioSource.isPlaying);

        ambientSoundManager.StopCoroutine(coroutine);
    }
}

public class AudioManagerTests
{
    private GameObject go;
    private AudioManager mgr;
    private AudioSource src;

    [SetUp]
    public void SetUp()
    {
        AudioManager.Instance = null;

        go = new GameObject("AudioManagerTest");
        mgr = go.AddComponent<AudioManager>();
        src = go.AddComponent<AudioSource>();
        mgr.audioSource = src;
    }

    [TearDown]
    public void TearDown()
    {
        AudioManager.Instance = null;
        Object.DestroyImmediate(go);
    }

    private AudioClip MakeClip(string name)
    {
        return AudioClip.Create(name, 44100, 1, 44100, false);
    }

    [Test]
    public void Test_CursedSoundFlags()
    {
        AudioClip clip = MakeClip("cursed");

        mgr.PlayCursedSound(clip);

        Assert.AreEqual(clip, src.clip, "Should replace the AudioSource.clip");
        Assert.IsTrue(mgr.isCursedSoundPlaying, "Should set isCursedSoundPlaying to true");
    }

    [Test]
    public void Test_MissedQuestionSound()
    {
        AudioClip clip = MakeClip("missed");
        mgr.isCursedSoundPlaying = false;

        mgr.PlayMissedQuestionSound(clip);

        Assert.AreEqual(clip, src.clip, "Should assign the clip when no cursed sound is playing");
        Assert.IsFalse(mgr.isCursedSoundPlaying, "Should not touch isCursedSoundPlaying");
    }

    [Test]
    public void Test_CorrectSound()
    {
        AudioClip clip = MakeClip("correct");
        src.Stop();

        mgr.PlayCorrectSound(clip);

        Assert.AreEqual(clip, src.clip, "Should assign the clip when nothing is playing");
    }

    [Test]
    public void Test_QuestionSound()
    {
        AudioClip clip = MakeClip("question");
        src.Stop();

        mgr.PlayQuestionSound(clip);

        Assert.AreEqual(clip, src.clip, "Should assign the clip when nothing is playing");
    }

    [Test]
    public void Test_KnockSound()
    {
        AudioClip original = MakeClip("orig");
        src.clip = original;
        AudioClip knock = MakeClip("knock");

        mgr.PlayKnockSound(knock);

        Assert.AreEqual(original, src.clip, "OneShot must not replace the main clip");
    }

    [Test]
    public void Test_StopCursedSound()
    {
        mgr.isCursedSoundPlaying = true;

        mgr.StopCursedSound();

        Assert.IsFalse(mgr.isCursedSoundPlaying, "Should clear the cursed-sound flag");
    }
}

public class CursedQuestionsTests
{
    [Test]
    public void Test_CursedPoolPopulated()
    {
        Assert.AreEqual(10, CursedQuestions.CursedQuestionPool.Count);
        Assert.Contains("Is this number the meaning to life?", CursedQuestions.CursedQuestionPool);
        Assert.Contains("Have you been here before?", CursedQuestions.CursedQuestionPool);
    }

    [Test]
    public void Test_WarningResponsesPopulated()
    {
        Assert.AreEqual(10, CursedQuestions.WarningResponses.Count);
        Assert.AreEqual(10, CursedQuestions.WarningResponses.Distinct().Count());
    }
}

public class CutsceneManagerTests  // INTEGRATION TEST (1)
{
    private GameObject go;
    private CutsceneManager mgr;
    private TextMeshProUGUI tmp;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("CutsceneManagerTests");
        mgr = go.AddComponent<CutsceneManager>();
        var textGO = new GameObject("Text");
        tmp = textGO.AddComponent<TextMeshProUGUI>();
        mgr.cutsceneText = tmp;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(tmp.gameObject);
    }

    [Test]
    public void Test_ShowNextBlock()
    {
        var idxField = typeof(CutsceneManager).GetField("currentBlockIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        var coroField = typeof(CutsceneManager).GetField("typingCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.AreEqual(0, (int)idxField.GetValue(mgr));
        Assert.IsNull(coroField.GetValue(mgr));
        var showMi = typeof(CutsceneManager).GetMethod("ShowNextBlock", BindingFlags.Instance | BindingFlags.NonPublic);
        showMi.Invoke(mgr, null);
        Assert.AreEqual(1, (int)idxField.GetValue(mgr));
        Assert.IsNotNull(coroField.GetValue(mgr));
    }

    [Test]
    public void Test_ColorChanging()
    {
        var blocksField = typeof(CutsceneManager).GetField("cutsceneBlocks", BindingFlags.Instance | BindingFlags.NonPublic);
        var blocks = (string[])blocksField.GetValue(mgr);
        var expected = blocks[0];
        var showMi = typeof(CutsceneManager).GetMethod("ShowNextBlock", BindingFlags.Instance | BindingFlags.NonPublic);
        showMi.Invoke(mgr, null);
        var skipMi = typeof(CutsceneManager).GetMethod("SkipTyping", BindingFlags.Instance | BindingFlags.NonPublic);
        skipMi.Invoke(mgr, null);
        Assert.AreEqual(expected, tmp.text);
        Assert.AreEqual(Color.red, tmp.color);
    }
}

public class FadeManagerTests //Explain Test_SetAlpha
{
    [Test]
    public void Test_AssigningCanvasGroup()
    {
        var go = new GameObject("FadeManagerTest");
        var child = new GameObject("Child");
        var cgChild = child.AddComponent<CanvasGroup>();
        child.transform.SetParent(go.transform, false);

        var mgr = go.AddComponent<FadeManager>();

        Assert.AreEqual(mgr, FadeManager.Instance);
        Assert.AreEqual(cgChild, mgr.fadeCanvasGroup);

        Object.DestroyImmediate(go);
    }


    [Test]
    public void Test_SetAlphaAndToggleRaycasting()
    {
        var go = new GameObject("FadeManagerTest2");
        var mgr = go.AddComponent<FadeManager>();
        var cg = go.AddComponent<CanvasGroup>();
        mgr.fadeCanvasGroup = cg;
        mgr.fadeDuration = 0f;
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        var fadeMethod = typeof(FadeManager)
            .GetMethod("Fade", BindingFlags.NonPublic | BindingFlags.Instance);
        var enumerator = (IEnumerator)fadeMethod.Invoke(mgr, new object[] { 0f });
        while (enumerator.MoveNext()) { }

        Assert.AreEqual(0f, cg.alpha);
        Assert.IsFalse(cg.blocksRaycasts);

        Object.DestroyImmediate(go);
    }
}

public class HeartbeatManagerTests // Explain Test_VolumeAndPitchViaIntensity
{
    [SetUp]
    public void Setup()
    {
        typeof(HeartbeatManager)
            .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
            .SetValue(null, null);
    }


    [Test]
    public void Test_VolumeAndPitchViaIntensity()
    {
        var go = new GameObject("HeartBeatManagerTest");
        var mgr = go.AddComponent<HeartbeatManager>();
        var src = go.AddComponent<AudioSource>();

        typeof(HeartbeatManager)
            .GetField("heartbeatAudioSource", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(mgr, src);

        mgr.minVolume = 0.3f;
        mgr.maxVolume = 1.0f;
        mgr.minPitch = 0.8f;
        mgr.maxPitch = 1.5f;

        mgr.UpdateHeartbeat(0.5f);

        Assert.AreEqual(Mathf.Lerp(mgr.minVolume, mgr.maxVolume, 0.5f), src.volume);
        Assert.AreEqual(Mathf.Lerp(mgr.minPitch, mgr.maxPitch, 0.5f), src.pitch);

        Object.DestroyImmediate(go);
    }
}

public class HeartManagerTests
{
    GameObject go;
    HeartManager mgr;
    Image[] hearts;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("HeartManagerTest");
        mgr = go.AddComponent<HeartManager>();
        hearts = new Image[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            var hgo = new GameObject("heart");
            hgo.transform.SetParent(go.transform, false);
            hearts[i] = hgo.AddComponent<Image>();
        }
        mgr.heartImages = hearts;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_LoseHeart()
    {
        mgr.ResetHearts();
        Assert.IsFalse(mgr.LoseHeart());
        Assert.IsFalse(hearts[2].enabled);
        Assert.IsFalse(mgr.IsGameOver());
        mgr.LoseHeart();
        mgr.LoseHeart();
        Assert.IsTrue(mgr.IsGameOver());
    }

    [Test]
    public void Test_ResetHearts()
    {
        mgr.ResetHearts();
        hearts[1].enabled = false;
        mgr.ResetHearts();
        foreach (var h in hearts)
            Assert.IsTrue(h.enabled);
    }
}

public class LosingScreenManagerTests // INTEGRATION TEST (1)
{
    class FakeFadeManager : FadeManager
    {
        public string lastScene;
        public override void FadeToScene(string sceneName)
        {
            lastScene = sceneName;
        }
    }

    [Test]
    public void Test_FadeToHome()
    {
        var fakeGO = new GameObject();
        var fake = fakeGO.AddComponent<FakeFadeManager>();
        FadeManager.Instance = fake;

        var go = new GameObject();
        var mgr = go.AddComponent<LosingScreenManager>();

        mgr.ReturnHome();

        Assert.AreEqual("StartScreen", fake.lastScene);

        Object.DestroyImmediate(fakeGO);
        Object.DestroyImmediate(go);
    }
}

public class ClipboardButtonTests
{
    private GameObject go;
    private ClipboardButton btn;
    private Image bg;
    private Button uiButton;
    private TextMeshProUGUI tmp;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("ClipboardButtonTest");
        bg = go.AddComponent<Image>();
        uiButton = go.AddComponent<Button>();
        btn = go.AddComponent<ClipboardButton>();
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        tmp = textGO.AddComponent<TextMeshProUGUI>();
        btn.buttonText = tmp;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_ButtonY()
    {
        btn.SetupNumberButton(42, 3);
        Assert.False(btn.isRowX);
        Assert.AreEqual(3, btn.rowIndex);
        Assert.AreEqual("42", tmp.text);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual(Color.black, tmp.color);
    }

    [Test]
    public void Test_ButtonX()
    {
        btn.SetupRowXButton(2);
        Assert.True(btn.isRowX);
        Assert.AreEqual(2, btn.rowIndex);
        Assert.AreEqual("X", tmp.text);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual(Color.red, tmp.color);
    }

    [Test]
    public void Test_ToggleColors()
    {
        btn.SetupNumberButton(1, 0);
        btn.ToggleRed();
        Assert.True(btn.IsMarkedRed);
        Assert.AreEqual(Color.red, bg.color);
        Assert.AreEqual(Color.red, tmp.color);
        btn.ToggleRed();
        Assert.False(btn.IsMarkedRed);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual(Color.black, tmp.color);
    }

    [Test]
    public void Test_ToggleColorRed()
    {
        btn.SetRed(true);
        Assert.True(btn.IsMarkedRed);
        Assert.AreEqual(Color.red, bg.color);
        Assert.AreEqual(Color.red, tmp.color);
        btn.SetRed(false);
        Assert.False(btn.IsMarkedRed);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual(Color.black, tmp.color);
    }

    [Test]
    public void Test_ClearRedState()
    {
        btn.SetRed(true);
        btn.ResetVisual();
        Assert.False(btn.IsMarkedRed);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual(Color.black, tmp.color);
    }
}

public class NumberScrollerTests
{
    private GameObject go;
    private NumberScroller scroller;
    private TextMeshProUGUI tmp;
    private Image img;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("NumberScrollerTests");
        scroller = go.AddComponent<NumberScroller>();
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        tmp = textGO.AddComponent<TextMeshProUGUI>();
        scroller.numberText = tmp;
        var imgGO = new GameObject("Image");
        imgGO.transform.SetParent(go.transform, false);
        img = imgGO.AddComponent<Image>();
        scroller.numberImage = img;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_SetXToTranslucent()
    {
        scroller.Start();
        Assert.AreEqual("X", tmp.text);
        Assert.AreEqual(new Color(1f, 1f, 1f, 0.4f), img.color);
    }

    [Test]
    public void Test_Dragging()
    {
        var fi = typeof(NumberScroller)
            .GetField("isDragging", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.False((bool)fi.GetValue(scroller));
        scroller.OnPointerDown(null);
        Assert.True((bool)fi.GetValue(scroller));
    }

    [Test]
    public void Test_NotDragging()
    {
        var fi = typeof(NumberScroller)
            .GetField("isDragging", BindingFlags.Instance | BindingFlags.NonPublic);
        fi.SetValue(scroller, true);
        scroller.OnPointerUp(null);
        Assert.False((bool)fi.GetValue(scroller));
    }

    [Test]
    public void Test_IncrementsByDragging()
    {
        var fi = typeof(NumberScroller)
            .GetField("isDragging", BindingFlags.Instance | BindingFlags.NonPublic);
        fi.SetValue(scroller, true);
        var data = new PointerEventData(null)
        {
            delta = new Vector2(0f, scroller.scrollThreshold)
        };
        scroller.OnDrag(data);
        int expected = Mathf.Clamp(8 + 1, 10, 99);
        Assert.AreEqual(expected, scroller.selectedNumber);
        Assert.AreEqual(expected.ToString(), tmp.text);
    }

    [Test]
    public void Test_SetImageOpaque()
    {
        scroller.LockNumberBox();
        Assert.AreEqual(new Color(1f, 1f, 1f, 1f), img.color);
    }

    [Test]
    public void Test_Unlock()
    {
        scroller.EnableNumberBox();
        Assert.AreEqual(Color.white, tmp.color);
        Assert.AreEqual(new Color(1f, 1f, 1f, 1f), img.color);
        var fi = typeof(NumberScroller)
            .GetField("isLocked", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.False((bool)fi.GetValue(scroller));
    }

    [Test]
    public void Test_DisableBox()
    {
        scroller.DisableNumberBox();
        Assert.AreEqual(Color.gray, tmp.color);
        Assert.AreEqual(new Color(1f, 1f, 1f, 0.4f), img.color);
        var fi = typeof(NumberScroller)
            .GetField("isLocked", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True((bool)fi.GetValue(scroller));
    }

    [Test]
    public void Test_ResetScroller()
    {
        scroller.selectedNumber = 50;
        tmp.color = Color.black;
        img.color = Color.black;
        scroller.ResetScroller();
        Assert.AreEqual(10, scroller.selectedNumber);
        Assert.AreEqual("X", tmp.text);
        Assert.AreEqual(Color.white, tmp.color);
        Assert.AreEqual(new Color(1f, 1f, 1f, 1f), img.color);
    }
}

public class SafeReactionTests
{
    private GameObject go;
    private SafeReaction mgr;
    private TextMeshProUGUI responseText;
    private CanvasGroup flickerCanvas;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("SafeReactionTests");
        mgr = go.AddComponent<SafeReaction>();
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        responseText = txtGO.AddComponent<TextMeshProUGUI>();
        mgr.responseText = responseText;
        var cgGO = new GameObject("Canvas");
        cgGO.transform.SetParent(go.transform, false);
        flickerCanvas = cgGO.AddComponent<CanvasGroup>();
        mgr.flickerCanvas = flickerCanvas;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_InitializeReponses()
    {
        typeof(SafeReaction)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(mgr, null);

        var availField = typeof(SafeReaction)
            .GetField("availableResponses", BindingFlags.Instance | BindingFlags.NonPublic);
        var availList = (List<string>)availField.GetValue(mgr);

        var masterField = typeof(SafeReaction)
            .GetField("mistakeResponses", BindingFlags.Instance | BindingFlags.NonPublic);
        var masterList = (List<string>)masterField.GetValue(mgr);

        Assert.AreEqual(masterList.Count, availList.Count);
        Assert.AreEqual(0f, flickerCanvas.alpha);
    }

    [Test]
    public void Test_ResetResponses()
    {
        var availField = typeof(SafeReaction)
            .GetField("availableResponses", BindingFlags.Instance | BindingFlags.NonPublic);
        availField.SetValue(mgr, new List<string>());

        mgr.ResetResponses();

        var masterField = typeof(SafeReaction)
            .GetField("mistakeResponses", BindingFlags.Instance | BindingFlags.NonPublic);
        var masterList = (List<string>)masterField.GetValue(mgr);

        var availList = (List<string>)availField.GetValue(mgr);
        Assert.AreEqual(masterList.Count, availList.Count);
    }

    [Test]
    public void Test_DisplayReaction()
    {
        var coroField = typeof(SafeReaction)
            .GetField("typingCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNull(coroField.GetValue(mgr));

        mgr.DisplayReaction("Hello");

        Assert.IsNotNull(coroField.GetValue(mgr));
    }

    [Test]
    public void Test_CursedReaction()
    {
        var coroField = typeof(SafeReaction)
            .GetField("typingCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNull(coroField.GetValue(mgr));

        mgr.DisplayCursedReaction("World");

        Assert.IsNotNull(coroField.GetValue(mgr));
    }

    [Test]
    public void Test_MistakeReaction()
    {
        var availField = typeof(SafeReaction)
            .GetField("availableResponses", BindingFlags.Instance | BindingFlags.NonPublic);
        var initial = new List<string> { "A", "B", "C" };
        availField.SetValue(mgr, new List<string>(initial));

        mgr.DisplayRandomMistakeReaction();

        var availList = (List<string>)availField.GetValue(mgr);
        Assert.AreEqual(initial.Count - 1, availList.Count);
    }
}

public class SafeShakerTests
{
    GameObject go;
    SafeShaker shaker;
    Image img;
    Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        shaker = go.AddComponent<SafeShaker>();

        var imgGO = new GameObject();
        imgGO.transform.SetParent(go.transform, false);
        img = imgGO.AddComponent<Image>();
        typeof(SafeShaker)
            .GetField("safeImage", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(shaker, img);

        testSprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.zero);
        var list = new List<Sprite> { testSprite };
        typeof(SafeShaker)
            .GetField("safeSprites", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(shaker, list);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_AssignRandomSprite()
    {
        typeof(SafeShaker)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(shaker, null);

        Assert.AreEqual(testSprite, img.sprite);
    }

    [Test]
    public void Test_SetShaking()
    {
        var fi = typeof(SafeShaker)
            .GetField("isShaking", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.False((bool)fi.GetValue(shaker));

        shaker.ShakeSafe();

        Assert.True((bool)fi.GetValue(shaker));
    }
}

public class StartMenuTests // INTEGRATION TEST (1)
{
    private GameObject go;
    private StartMenu menu;
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button startButton;
    private TextMeshProUGUI responseText;

    private class FakeFade : FadeManager
    {
        public string lastScene;
        public override void FadeToScene(string sceneName)
        {
            lastScene = sceneName;
        }
    }

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        menu = go.AddComponent<StartMenu>();

        easyButton = new GameObject().AddComponent<Button>();
        mediumButton = new GameObject().AddComponent<Button>();
        hardButton = new GameObject().AddComponent<Button>();
        startButton = new GameObject().AddComponent<Button>();
        responseText = new GameObject().AddComponent<TextMeshProUGUI>();

        menu.easyButton = easyButton;
        menu.mediumButton = mediumButton;
        menu.hardButton = hardButton;
        menu.startButton = startButton;
        menu.responseText = responseText;
    }

    [TearDown]
    public void TearDown()
    {
        FadeManager.Instance = null;
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(easyButton.gameObject);
        Object.DestroyImmediate(mediumButton.gameObject);
        Object.DestroyImmediate(hardButton.gameObject);
        Object.DestroyImmediate(startButton.gameObject);
        Object.DestroyImmediate(responseText.gameObject);
    }

    [Test]
    public void Test_ToggleDifficulty() // INTEGRATION TESTS
    {
        var method = typeof(StartMenu).GetMethod("SetDifficulty", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(menu, new object[] { 123f, "TestMode" });
        Assert.AreEqual(123f, StartMenu.selectedTime);
        var field = typeof(StartMenu).GetField("selectedMode", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.AreEqual("TestMode", field.GetValue(menu));
    }

    [Test]
    public void Test_Fade()
    {
        var fakeGo = new GameObject();
        var fake = fakeGo.AddComponent<FakeFade>();
        FadeManager.Instance = fake;

        var method = typeof(StartMenu).GetMethod("StartGame", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(menu, null);

        Assert.AreEqual("GameScene", fake.lastScene);

        Object.DestroyImmediate(fakeGo);
    }

    [Test]
    public void Test_ButtonsAssociatedWithDifficulty()
    {
        menu.Start();
        easyButton.onClick.Invoke();
        Assert.AreEqual(600f, StartMenu.selectedTime);
        mediumButton.onClick.Invoke();
        Assert.AreEqual(300f, StartMenu.selectedTime);
        hardButton.onClick.Invoke();
        Assert.AreEqual(60f, StartMenu.selectedTime);
    }
}

public class WinScreenManagerTests
{
    private GameObject go;
    private WinScreenManager mgr;
    private TextMeshProUGUI winText;
    private TextMeshProUGUI endingText;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("WinScreenTests");
        mgr = go.AddComponent<WinScreenManager>();

        var wtGO = new GameObject("WinText");
        wtGO.transform.SetParent(go.transform, false);
        winText = wtGO.AddComponent<TextMeshProUGUI>();
        mgr.winText = winText;

        var etGO = new GameObject("EndingText");
        etGO.transform.SetParent(go.transform, false);
        endingText = etGO.AddComponent<TextMeshProUGUI>();
        endingText.enabled = false;
        mgr.endingText = endingText;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Test_DisplayFastMessage()
    {
        var method = typeof(WinScreenManager)
            .GetMethod("SelectMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        string result = (string)method.Invoke(mgr, new object[] { 100f, 100f });

        string expected = "Speed has always been your priority, oftentimes over your own safety. You wish you had taken life a little slower as you peek into the cavern of the safe. Hundreds of eyes look back at you.";
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Test_DisplaySlowMessage()
    {
        var method = typeof(WinScreenManager)
            .GetMethod("SelectMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        string result = (string)method.Invoke(mgr, new object[] { 1f, 100f });

        string expected = "You wish you had taken the repeated warnings more seriously as you slowly open the door to the safe. You always hoped of one day seizing riches because of your job. But this, this wasn't what you wanted to see. Pandora's Box stayed closed for a reason.";
        Assert.AreEqual(expected, result);
        Assert.AreEqual("<color=#FF0000>BAD ENDING</color>", endingText.text);
        Assert.IsTrue(endingText.enabled);
    }

    [Test]
    public void Test_DisplayBadMessage()
    {
        var method = typeof(WinScreenManager)
            .GetMethod("DisplayBadEnding", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(mgr, null);

        Assert.AreEqual("<color=#FF0000>BAD ENDING</color>", endingText.text);
        Assert.IsTrue(endingText.enabled);
    }
}

public class QuestionManagerTests
{
    private GameObject go;
    private QuestionManager mgr;
    private TextMeshProUGUI responseText;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("QuestionManagerTests");
        mgr = go.AddComponent<QuestionManager>();

        var rtGO = new GameObject("ResponseText");
        rtGO.transform.SetParent(go.transform, false);
        responseText = rtGO.AddComponent<TextMeshProUGUI>();
        mgr.responseText = responseText;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        QuestionManager.AllQuestions.Clear();
    }

    [Test]
    public void Test_PopulationQuestions()
    {
        QuestionManager.AllQuestions.Clear();
        mgr.PopulateQuestions();
        Assert.IsTrue(QuestionManager.AllQuestions.Count > 0);
        Assert.IsTrue(QuestionManager.AllQuestions.ContainsKey("Is this number divisible by 2?"));
        Assert.IsTrue(QuestionManager.AllQuestions.ContainsKey("Is this number greater than the number of stripes on the U.S. flag?"));
    }

    [Test]
    public void Test_PrimeWorks()
    {
        Assert.IsTrue(QuestionManager.IsPrime(7));
        Assert.IsFalse(QuestionManager.IsPrime(9));
        Assert.IsFalse(QuestionManager.IsPrime(1));
    }

    [Test]
    public void Test_LockingLocksAndGeneratingNumbers()
    {
        mgr.correctNumbers = new int[3];
        mgr.locks = new bool[3] { true, true, true };
        mgr.GenerateNewNumbers();
        for (int i = 0; i < 3; i++)
        {
            Assert.GreaterOrEqual(mgr.correctNumbers[i], 10);
            Assert.Less(mgr.correctNumbers[i], 100);
            Assert.IsFalse(mgr.locks[i]);
        }
    }

    [Test]
    public void Test_CoroutineValid()
    {
        var coroField = typeof(QuestionManager)
            .GetField("yesNoCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNull(coroField.GetValue(mgr));
        mgr.DisplayYesNoResponse(true);
        Assert.AreEqual("The Safe Says: \"Yes\"", mgr.responseMessage);
        Assert.IsNotNull(coroField.GetValue(mgr));
    }

    [Test]
    public void Test_LockScrollers()
    {
        var scrollerGO1 = new GameObject("S1");
        var scrollerGO2 = new GameObject("S2");
        var s1 = scrollerGO1.AddComponent<NumberScroller>();
        var s2 = scrollerGO2.AddComponent<NumberScroller>();
        mgr.numberScrollers = new NumberScroller[] { s1, s2 };
        var isLockedField = typeof(NumberScroller)
            .GetField("isLocked", BindingFlags.Instance | BindingFlags.NonPublic);

        mgr.LockAllBoxes();

        Assert.IsTrue((bool)isLockedField.GetValue(s1));
        Assert.IsTrue((bool)isLockedField.GetValue(s2));

        Object.DestroyImmediate(scrollerGO1);
        Object.DestroyImmediate(scrollerGO2);
    }

    [Test]
    public void Test_ColorChanging()
    {
        var qt = new GameObject("Q1").AddComponent<TextMeshProUGUI>();
        mgr.questionTexts = new TextMeshProUGUI[] { qt };
        qt.color = Color.white;
        mgr.OnHoverEnter(0);
        Assert.AreEqual(Color.black, qt.color);
        Object.DestroyImmediate(qt.gameObject);
    }

    [Test]
    public void Test_ColorChangingOFF()
    {
        var qt = new GameObject("Q2").AddComponent<TextMeshProUGUI>();
        mgr.questionTexts = new TextMeshProUGUI[] { qt };
        qt.color = Color.white;

        // not cursed
        typeof(QuestionManager)
            .GetField("cursedQuestionIndices", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(mgr, new List<int>());
        mgr.OnHoverExit(0);
        Assert.AreEqual(Color.white, qt.color);

        // cursed
        var cursedList = new List<int> { 0 };
        typeof(QuestionManager)
            .GetField("cursedQuestionIndices", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(mgr, cursedList);
        mgr.OnHoverExit(0);
        Assert.AreEqual(Color.red, qt.color);

        Object.DestroyImmediate(qt.gameObject);
    }
}

public class QuestionManagerIntegrationTests
{
    class FakeAudioManager : AudioManager
    {
        public int questionCount;
        public int cursedCount;

        public override void PlayQuestionSound(AudioClip clip) { questionCount++; }
        public override void PlayCursedSound(AudioClip clip) { cursedCount++; }
        public override void StopCursedSound() { }
    }

    class FakeSafeReaction : SafeReaction
    {
        public string lastMessage;
        public override void DisplayReaction(string msg) { lastMessage = msg; }
    }

    private GameObject go;
    private QuestionManager mgr;
    private GameObject audioGO;
    private FakeAudioManager fakeAudio;
    private GameObject reactGO;
    private FakeSafeReaction fakeReaction;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("QuestionManagerIntegration");
        mgr = go.AddComponent<QuestionManager>();

        var rtGO = new GameObject("RT");
        rtGO.transform.SetParent(go.transform, false);
        mgr.responseText = rtGO.AddComponent<TextMeshProUGUI>();

        audioGO = new GameObject("Audio");
        fakeAudio = audioGO.AddComponent<FakeAudioManager>();
        AudioManager.Instance = fakeAudio;

        reactGO = new GameObject("React");
        fakeReaction = reactGO.AddComponent<FakeSafeReaction>();
        mgr.safeReaction = fakeReaction;

        mgr.questionSound = AudioClip.Create("qs", 44100, 1, 44100, false);
        mgr.cursedSound = AudioClip.Create("cs", 44100, 1, 44100, false);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(audioGO);
        Object.DestroyImmediate(reactGO);
    }

    [Test]
    public void Test_QuestionButtonStartsCoroutine()
    {
        mgr.QuestionButtonClicked(true, "ignored");

        Assert.AreEqual(1, fakeAudio.questionCount);
        Assert.AreEqual("The Safe Says: \"Yes\"", mgr.responseMessage);

        var field = typeof(QuestionManager)
            .GetField("yesNoCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field.GetValue(mgr));
    }

    [Test]
    public void Test_QuestionButtonInvokesCursedSound()
    {
        mgr.CursedQuestionClicked("Random question");

        Assert.AreEqual(1, fakeAudio.cursedCount);
        Assert.IsFalse(string.IsNullOrEmpty(fakeReaction.lastMessage));

        var field = typeof(QuestionManager)
            .GetField("yesNoCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field.GetValue(mgr));
    }
} // INTEGRATION TESTS (2)

































