using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Подключаем библиотеку для нового текста (TextMeshPro)

// === СТРУКТУРА ДЛЯ ВИЗУАЛЬНОЙ НОВЕЛЛЫ ===
[System.Serializable]
public struct DialogLine
{
    public Sprite characterSprite; // Спрайт персонажа (эмоция) для фразы
    public Sprite dialogBoxSprite; // Спрайт подложки текста для фразы
    [TextArea(3, 5)]
    public string dialogText;      // Сам текст фразы
}

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject characterSelectPanel;
    public GameObject prologuePanel;
    public GameObject dialogPanel; // Панель визуальной новеллы
    public GameObject gamePanel;
    public GameObject locationSelectPanel;

    [Header("UI Customization Layers")]
    public Image bodyLayerDisplay;
    public Image clothingLayerDisplay;
    public Image hornsLayerDisplay;

    [Header("Customization Sprites Data")]
    public Sprite[] bodySprites;
    public Sprite[] clothingSprites;
    public Sprite[] hornsSprites;

    [Header("Prologue Settings (Transitions)")]
    public RectTransform currentImageRect; // Для эффекта сдвига
    public RectTransform nextImageRect;    // Для эффекта сдвига
    public Sprite[] prologueSprites;
    public float timePerImage = 3f;
    public float transitionDuration = 0.8f;
    public float slideDistance = 1920f;
    public float distortionAmount = 2.5f;

    [Header("Prologue Text Settings")]
    public CanvasGroup textCanvasGroup;
    public Image textImageComponent;
    public Sprite[] textSprites;
    public RectTransform[] textTemplates; // Шаблоны позиций текста
    public float textFlyDuration = 0.5f;

    [Header("Visual Novel (Dialog) Settings")]
    public Image dialogCharacterImage;
    public Image dialogBoxImage;
    public TextMeshProUGUI dialogTextComponent;
    public float textTypingSpeed = 0.03f;
    public DialogLine[] dialogLines;

    [Header("Gameplay Elements")]
    public GameObject mainPlayerPrefab;
    public GameObject characterBasePrefab;
    public Transform spawnPointPlayer;
    public Transform spawnPointCustomNPC;

    private int currentBodyIndex = 0;
    private int currentClothingIndex = 0;
    private int currentHornsIndex = 0;

    private Coroutine prologueCoroutine;
    private Coroutine typingCoroutine;

    private Image currentImg, nextImg;
    private CanvasGroup currentCanvasGroup, nextCanvasGroup;
    private RectTransform textRect;

    private int currentDialogIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        // Инициализация стартовых панелей
        characterSelectPanel.SetActive(true);
        prologuePanel.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        gamePanel.SetActive(false);
        if (locationSelectPanel != null) locationSelectPanel.SetActive(false);

        UpdateCustomizerUI();

        // Инициализация компонентов пролога
        if (currentImageRect != null && nextImageRect != null)
        {
            currentImg = currentImageRect.GetComponent<Image>();
            nextImg = nextImageRect.GetComponent<Image>();
            currentCanvasGroup = currentImageRect.GetComponent<CanvasGroup>();
            nextCanvasGroup = nextImageRect.GetComponent<CanvasGroup>();
        }

        if (textCanvasGroup != null)
        {
            textRect = textCanvasGroup.GetComponent<RectTransform>();
            textCanvasGroup.gameObject.SetActive(false);
            textCanvasGroup.alpha = 0f;
        }

        // Прячем шаблоны текста при старте игры
        if (textTemplates != null)
        {
            foreach (var template in textTemplates)
                if (template != null) template.gameObject.SetActive(false);
        }
    }

    // ================= КАСТОМИЗАЦИЯ =================
    public void NextBody() { currentBodyIndex = (currentBodyIndex + 1) % bodySprites.Length; UpdateCustomizerUI(); }
    public void PreviousBody() { currentBodyIndex = (currentBodyIndex - 1 + bodySprites.Length) % bodySprites.Length; UpdateCustomizerUI(); }
    public void NextClothing() { currentClothingIndex = (currentClothingIndex + 1) % clothingSprites.Length; UpdateCustomizerUI(); }
    public void PreviousClothing() { currentClothingIndex = (currentClothingIndex - 1 + clothingSprites.Length) % clothingSprites.Length; UpdateCustomizerUI(); }
    public void NextHorns() { currentHornsIndex = (currentHornsIndex + 1) % hornsSprites.Length; UpdateCustomizerUI(); }
    public void PreviousHorns() { currentHornsIndex = (currentHornsIndex - 1 + hornsSprites.Length) % hornsSprites.Length; UpdateCustomizerUI(); }

    void UpdateCustomizerUI()
    {
        if (bodySprites.Length > 0) bodyLayerDisplay.sprite = bodySprites[currentBodyIndex];
        if (clothingSprites.Length > 0) clothingLayerDisplay.sprite = clothingSprites[currentClothingIndex];
        if (hornsSprites.Length > 0) hornsLayerDisplay.sprite = hornsSprites[currentHornsIndex];
    }

    public void ConfirmSelection()
    {
        characterSelectPanel.SetActive(false);
        prologuePanel.SetActive(true);
        prologueCoroutine = StartCoroutine(PlayPrologue());
    }

    // ================= ПРОЛОГ (КОМИКС) =================
    private IEnumerator PlayPrologue()
    {
        if (prologueSprites.Length == 0) { StartVisualNovel(); yield break; }

        currentImg.sprite = prologueSprites[0];
        ResetImageTransform(currentImageRect);

        currentCanvasGroup.alpha = 0f;
        nextCanvasGroup.alpha = 0f;

        float startElapsed = 0f;
        while (startElapsed < transitionDuration)
        {
            startElapsed += Time.deltaTime;
            currentCanvasGroup.alpha = startElapsed / transitionDuration;
            yield return null;
        }
        currentCanvasGroup.alpha = 1f;

        yield return StartCoroutine(AnimateTextIn(0));
        yield return new WaitForSeconds(timePerImage);

        for (int i = 1; i < prologueSprites.Length; i++)
        {
            if (textCanvasGroup != null)
            {
                textCanvasGroup.gameObject.SetActive(false);
                textCanvasGroup.alpha = 0f;
            }

            nextImg.sprite = prologueSprites[i];
            float elapsed = 0f;

            // Теперь ВСЕ картинки (включая 5-ю) стартуют справа
            nextImageRect.anchoredPosition = new Vector2(slideDistance, 0);
            nextCanvasGroup.alpha = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

                // Горизонтальное движение и искажение только по оси X (как у всех)
                currentImageRect.anchoredPosition = new Vector2(-slideDistance * smoothT, 0);
                nextImageRect.anchoredPosition = new Vector2(slideDistance * (1f - smoothT), 0);

                currentImageRect.localScale = new Vector3(1f + (distortionAmount * smoothT), 1f, 1f);
                nextImageRect.localScale = new Vector3(1f + (distortionAmount * (1f - smoothT)), 1f, 1f);

                currentCanvasGroup.alpha = 1f - smoothT;
                nextCanvasGroup.alpha = smoothT;
                yield return null;
            }

            currentImg.sprite = nextImg.sprite;
            ResetImageTransform(currentImageRect);
            currentCanvasGroup.alpha = 1f;
            nextCanvasGroup.alpha = 0f;

            yield return StartCoroutine(AnimateTextIn(i));
            yield return new WaitForSeconds(timePerImage);
        }

        StartVisualNovel();
    }

    private void ResetImageTransform(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private IEnumerator AnimateTextIn(int index)
    {
        if (textSprites != null && index < textSprites.Length && textSprites[index] != null && textCanvasGroup != null)
        {
            textImageComponent.sprite = textSprites[index];
            Vector3 finalScale = Vector3.one;

            if (textTemplates != null && index < textTemplates.Length && textTemplates[index] != null)
            {
                RectTransform target = textTemplates[index];
                textRect.anchorMin = target.anchorMin;
                textRect.anchorMax = target.anchorMax;
                textRect.pivot = target.pivot;
                textRect.sizeDelta = target.sizeDelta;
                textRect.anchoredPosition = target.anchoredPosition;
                textRect.localRotation = target.localRotation;
                finalScale = target.localScale;
            }

            textRect.localScale = Vector3.zero;
            textCanvasGroup.alpha = 0f;
            textCanvasGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < textFlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / textFlyDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                textRect.localScale = Vector3.Lerp(Vector3.zero, finalScale, smoothT);
                textCanvasGroup.alpha = smoothT;
                yield return null;
            }

            textRect.localScale = finalScale;
            textCanvasGroup.alpha = 1f;
        }
    }

    public void SkipPrologue()
    {
        if (prologueCoroutine != null) StopCoroutine(prologueCoroutine);
        // Пропуск пролога кидает сразу в новеллу
        StartVisualNovel();
    }

    // ================= ВИЗУАЛЬНАЯ НОВЕЛЛА =================
    private void StartVisualNovel()
    {
        // Закомментируйте строку ниже (добавьте //), если хотите чтобы последний слайд комикса остался затемненным фоном для диалога
        prologuePanel.SetActive(false); 

        if (dialogLines == null || dialogLines.Length == 0 || dialogPanel == null)
        {
            StartGame();
            return;
        }

        dialogPanel.SetActive(true);
        currentDialogIndex = 0;
        ShowDialogLine();
    }

    private void ShowDialogLine()
    {
        DialogLine currentLine = dialogLines[currentDialogIndex];

        // Меняем персонажа
        if (currentLine.characterSprite != null && dialogCharacterImage != null)
        {
            dialogCharacterImage.gameObject.SetActive(true);
            dialogCharacterImage.sprite = currentLine.characterSprite;
        }
        else if (dialogCharacterImage != null)
        {
            dialogCharacterImage.gameObject.SetActive(false);
        }

        // Меняем подложку
        if (dialogBoxImage != null && currentLine.dialogBoxSprite != null)
        {
            dialogBoxImage.sprite = currentLine.dialogBoxSprite;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (dialogTextComponent != null)
            typingCoroutine = StartCoroutine(TypeText(currentLine.dialogText));
    }

    private IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        dialogTextComponent.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogTextComponent.text += letter;
            yield return new WaitForSeconds(textTypingSpeed);
        }

        isTyping = false;
    }

    // Эта функция вызывается прозрачной кнопкой "NextButton" во время диалога
    public void OnDialogNextButtonClicked()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogTextComponent.text = dialogLines[currentDialogIndex].dialogText;
            isTyping = false;
        }
        else
        {
            currentDialogIndex++;
            if (currentDialogIndex < dialogLines.Length)
            {
                ShowDialogLine();
            }
            else
            {
                StartGame();
            }
        }
    }

    // ================= СТАРТ ИГРЫ И СПАВН ПЕРСОНАЖЕЙ =================
    private void StartGame()
    {
        prologuePanel.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        gamePanel.SetActive(true);

        if (mainPlayerPrefab != null && spawnPointPlayer != null)
        {
            GameObject player = Instantiate(mainPlayerPrefab, spawnPointPlayer.position, Quaternion.identity);
            FindObjectOfType<DynamicJoystickMovement>()?.SetTarget(player.transform);
            FindObjectOfType<CameraFollow>()?.SetTarget(player.transform);
        }

        if (characterBasePrefab != null && spawnPointCustomNPC != null)
        {
            GameObject customNPC = Instantiate(characterBasePrefab, spawnPointCustomNPC.position, Quaternion.identity);
            SetupCharacterVisuals(customNPC, currentBodyIndex, currentClothingIndex, currentHornsIndex);
        }
    }

    void SetupCharacterVisuals(GameObject targetObject, int bodyIdx, int clothIdx, int hornIdx)
    {
        SpriteRenderer[] renderers = targetObject.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length >= 3)
        {
            if (bodyIdx >= 0 && bodyIdx < bodySprites.Length) renderers[0].sprite = bodySprites[bodyIdx];
            if (clothIdx >= 0 && clothIdx < clothingSprites.Length) renderers[1].sprite = clothingSprites[clothIdx];
            else renderers[1].sprite = null;
            if (hornIdx >= 0 && hornIdx < hornsSprites.Length) renderers[2].sprite = hornsSprites[hornIdx];
            else renderers[2].sprite = null;
        }
    }

    // ================= НАВИГАЦИЯ ПО ЛОКАЦИЯМ =================
    public void OpenLocationPanel()
    {
        if (locationSelectPanel != null) locationSelectPanel.SetActive(true);
    }

    public void CloseLocationPanel()
    {
        if (locationSelectPanel != null) locationSelectPanel.SetActive(false);
    }

    public void GoToSupermarket()
    {
        Debug.Log("Загрузка: Супермаркет");
        CloseLocationPanel();
        SceneManager.LoadScene("SupermarketScene");
    }

    public void GoToSchool()
    {
        Debug.Log("Загрузка: Школа");
        CloseLocationPanel();
        SceneManager.LoadScene("SchoolScene");
    }

    public void GoToYard()
    {
        Debug.Log("Загрузка: Двор");
        CloseLocationPanel();
        SceneManager.LoadScene("YardScene");
    }

    public void GoToDevRoom()
    {
        Debug.Log("Загрузка: Комната разработчика (Все мини-игры)");
        CloseLocationPanel();
        SceneManager.LoadScene("DevRoomScene");
    }
}