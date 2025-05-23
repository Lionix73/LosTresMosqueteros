using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections;

public class LobbyVisualizer : MonoBehaviour
{
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform rightPosition;
    [SerializeField] private Transform charactersInGame;
    [SerializeField] private CharacterLibrary characterLibrary;
    [Space]
    [SerializeField] private GameObject leftLabel;
    [SerializeField] private GameObject rightLabel;

    [SerializeField] private List<GameObject> addButtons = new List<GameObject>();
    [SerializeField] private List<GameObject> userLabels = new List<GameObject>();
    public List<string> ids = new List<string>();
    public List<string> names = new List<string>();
    [SerializeField] private List<string> models = new List<string>();

    private Dictionary<string, GameObject> spawnedCharacters = new();

    public void VisualizeLobby(string currentLobbyId)
    {
        if (string.IsNullOrEmpty(currentLobbyId))
        {
            Debug.LogError("No lobby ID found.");
            return;
        }

        StartCoroutine(LoadCharacters(currentLobbyId));
    }

    private IEnumerator LoadCharacters(string currentLobbyId)
    {
        // Limpiar personajes anteriores
        foreach (var obj in spawnedCharacters.Values)
            Destroy(obj);

        DestroyCurrentModels();
        yield return new WaitForSeconds(1);

        spawnedCharacters.Clear();
        ids.Clear();
        models.Clear();
        names.Clear();

        DatabaseReference membersRef = FirebaseDatabase.DefaultInstance
            .GetReference("lobbies")
            .Child(currentLobbyId)
            .Child("members");

        var membersTask = membersRef.GetValueAsync();
        yield return new WaitUntil(() => membersTask.IsCompleted);

        if (membersTask.IsFaulted || !membersTask.Result.Exists)
        {
            Debug.LogError("Error retrieving lobby members or lobby is empty.");
            yield break;
        }

        List<string> memberUids = new List<string>();
        foreach (var member in membersTask.Result.Children)
            memberUids.Add(member.Key);

        memberUids.Sort(); // Mantener orden consistente
        string localUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // Cargar datos de los usuarios
        List<string> loadedNames = new List<string>();
        List<string> loadedModels = new List<string>();
        List<string> orderedUids = new List<string>();

        foreach (var uid in memberUids)
        {
            var userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(uid);
            var userTask = userRef.GetValueAsync();
            yield return new WaitUntil(() => userTask.IsCompleted);

            if (!userTask.IsFaulted && userTask.Result.Exists)
            {
                string modelId = userTask.Result.Child("characterId").Value?.ToString();
                string username = userTask.Result.Child("username").Value?.ToString();

                loadedModels.Add(modelId);
                loadedNames.Add(username);
                orderedUids.Add(uid);
            }
        }

        // Mostrar personajes en escena
        int count = Mathf.Min(3, orderedUids.Count);
        for (int i = 0; i < count; i++)
        {
            string uid = orderedUids[i];
            string modelId = loadedModels[i];
            string username = loadedNames[i];

            GameObject prefab = characterLibrary.GetCharacterPrefab(modelId);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found for character ID: {modelId}");
                continue;
            }

            Transform position = centerPosition;
            GameObject label = userLabels[0];
            GameObject button = addButtons[0];

            GameObject instance = null;

            if (uid != localUid)
            {
                // Invitado: izquierda o derecha
                bool isLeft = !spawnedCharacters.ContainsKey("left");
                position = isLeft ? leftPosition : rightPosition;
                int index = isLeft ? 1 : 2;
                label = userLabels[index - 1];
                button = addButtons[index - 1];

                spawnedCharacters[isLeft ? "left" : "right"] = null;

                // Activar etiqueta correspondiente
                GameObject tagLabel = isLeft ? leftLabel : rightLabel;
                if (tagLabel != null)
                {
                    tagLabel.SetActive(true);
                    TextMeshProUGUI tmp = tagLabel.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        yield return new WaitForSeconds(0.5f);
                        tmp.text = username;
                    }
                }
            }
            else
            {
                // Host al centro
                spawnedCharacters["center"] = null;
            }

            Quaternion rot = new Quaternion(0, -0.39f, 0, 0.91f);
            instance = Instantiate(prefab, position.position, rot, charactersInGame);
            spawnedCharacters[uid] = instance;

            if (label != null) label.SetActive(true);
            if (button != null) button.SetActive(false);
        }
    }


    private void DestroyCurrentModels()
    {
        GameObject[] models = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject m in models)
        {
            Destroy(m);
        }
    }
}
