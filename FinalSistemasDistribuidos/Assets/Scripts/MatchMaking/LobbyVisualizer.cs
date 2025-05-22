using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;

public class LobbyVisualizer : MonoBehaviour
{
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform rightPosition;
    [SerializeField] private CharacterLibrary characterLibrary;
    [SerializeField] private Transform charactersInGame;

    [SerializeField] private List<GameObject> addButtons = new List<GameObject>();
    [SerializeField] private List<GameObject> userLabels = new List<GameObject>();
    public List<string> ids = new List<string>();
    public List<string> names = new List<string>();
    public List<string> models = new List<string>();

    private Dictionary<string, GameObject> spawnedCharacters = new();

    public void VisualizeLobby(string currentLobbyId)
    {
        if (string.IsNullOrEmpty(currentLobbyId))
        {
            Debug.LogError("No lobby ID found.");
            return;
        }

        // Limpia personajes anteriores
        foreach (var obj in spawnedCharacters.Values)
        {
            Destroy(obj);
        }
        spawnedCharacters.Clear();
        DestroyCurrentModels();

        DatabaseReference membersRef = FirebaseDatabase.DefaultInstance
            .GetReference("lobbies")
            .Child(currentLobbyId)
            .Child("members");

        membersRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("Error retrieving lobby members or lobby is empty.");
                return;
            }

            List<string> memberUids = new List<string>();
            foreach (var member in task.Result.Children)
            {
                memberUids.Add(member.Key);
            }

            // Ordena para mantener consistencia visual
            memberUids.Sort();
            ids = memberUids;
            
            string localUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

            for (int i = 0; i < memberUids.Count && i < 3; i++)
            {
                string uid = memberUids[i];
                string currentUid = uid; // Captura para evitar problemas de closures en bucles

                DatabaseReference userRef = FirebaseDatabase.DefaultInstance
                    .GetReference("users")
                    .Child(currentUid);

                userRef.GetValueAsync().ContinueWithOnMainThread(userTask =>
                {
                    if (userTask.IsFaulted || !userTask.Result.Exists)
                    {
                        Debug.LogWarning($"User data not found for UID: {currentUid}");
                        return;
                    }

                    DataSnapshot userSnapshot = userTask.Result;
                    string characterId = userSnapshot.Child("characterId").Value?.ToString();
                    string username = userSnapshot.Child("username").Value?.ToString();

                    names.Add(username);
                    models.Add(characterId);

                    if(task.IsCompleted)
                    {
                        for (int j = 0; j < ids.Count; j++)
                        {
                            GameObject prefab = characterLibrary.GetCharacterPrefab(models[j]);

                            if (prefab == null)
                            {
                                Debug.LogWarning($"Character prefab not found for ID: {models[j]}");
                                return;
                            }

                            // Decidir posición basada en index
                            Transform spawnPosition;
                            if (ids[j] == localUid)
                            {
                                spawnPosition = centerPosition;
                            }
                            else if (j - 1 < ids.IndexOf(ids.Find(x => x == localUid)))
                            {
                                spawnPosition = leftPosition;
                                addButtons[0].SetActive(false);
                                userLabels[0].SetActive(true);
                            }
                            else
                            {
                                spawnPosition = rightPosition;
                                addButtons[1].SetActive(false);
                                userLabels[1].SetActive(true);
                            }

                            GameObject instance = Instantiate(prefab, spawnPosition.position, new Quaternion(0, -0.39f, 0, 0.91f), charactersInGame);
                            instance.name = $"Character_{names[j]}";
                            spawnedCharacters[ids[j]] = instance;
                        }
                    }
                });
            }
        });
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
