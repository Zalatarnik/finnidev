using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Подключаем библиотеку для загрузки новых сцен и локаций 

public class GameFlowManager : MonoBehaviour
{
[Header("UI Panels")]
public GameObject characterSelectPanel;
public GameObject prologuePanel;
public GameObject gamePanel;
public GameObject locationSelectPanel; // Сюда перетащить новую панель (НОВОЕ) 

[Header("UI Customization Layers")]
public Image bodyLayerDisplay;
public Image clothingLayerDisplay;
public Image hornsLayerDisplay;

[Header("Customization Sprites Data")]
public Sprite[] bodySprites;
public Sprite[] clothingSprites;
public Sprite[] hornsSprites;

[Header("Prologue Settings")]
public Image prologueImageDisplay;
public Sprite[] prologueSprites;
public float timePerImage = 5f;

[Header("Gameplay Elements")]
public GameObject mainPlayerPrefab;
public GameObject characterBasePrefab;
public Transform spawnPointPlayer;
public Transform spawnPointCustomNPC;

private int currentBodyIndex = 0;
private int currentClothingIndex = 0;
private int currentHornsIndex = 0;

private Coroutine prologueCoroutine;

void Start()
{
characterSelectPanel.SetActive(true);
prologuePanel.SetActive(false);
gamePanel.SetActive(false);
if (locationSelectPanel != null) locationSelectPanel.SetActive(false); // Прячем панель локаций
UpdateCustomizerUI();
}

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

private IEnumerator PlayPrologue()
{
for (int i = 0; i < prologueSprites.Length; i++)
{
prologueImageDisplay.sprite = prologueSprites[i];
yield return new WaitForSeconds(timePerImage);
}
EndPrologue();
}

public void SkipPrologue()
{
if (prologueCoroutine != null) StopCoroutine(prologueCoroutine);
EndPrologue();
}

private void EndPrologue()
{
prologuePanel.SetActive(false);
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

// Открыть панель локаций (вызывается зоной-триггером RoomExitTrigger)
public void OpenLocationPanel()
{
if (locationSelectPanel != null) locationSelectPanel.SetActive(true);
}

// Закрыть панель локаций (для кнопки-крестика)
public void CloseLocationPanel()
{
if (locationSelectPanel != null) locationSelectPanel.SetActive(false);
}

// Функции для 4 кнопок загружают соответствующие сцены (имена сцен должны в точности совпадать!)
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