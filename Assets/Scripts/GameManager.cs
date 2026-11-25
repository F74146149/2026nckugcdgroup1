using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Transform player;

    public void StartGame()
    {
        if (roomManager == null || waveManager == null || player == null)
        {
            Debug.LogError("Assign the RoomManager, WaveManager, and player in the Inspector.", this);
            return;
        }

        roomManager.GenerateMap();
        player.position = Vector3.zero;
        waveManager.countwaves();
    }
}
