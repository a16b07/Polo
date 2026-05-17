using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{
    // F8 — play bad ending cutscene
    // F9 — force wave clear (unlock door)

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            if (CutscenePlayer.Instance != null)
                CutscenePlayer.Instance.Play(false,
                    () => WaveManager.Instance?.StartNextWave());
            else
                Debug.LogWarning("CutscenePlayer not found");
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.ForceWaveClear();
            else
                Debug.LogWarning("WaveManager not found");
        }
    }

}
