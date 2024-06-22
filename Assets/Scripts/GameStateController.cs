using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Chunk;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    [SerializeField] private Backdrop backdrop;
    [SerializeField] private ChunkGenerator chunkGenerator;

    public delegate void OnGameLoadFinishAction();

    public event OnGameLoadFinishAction OnGameLoadFinish;
    
    public static GameStateController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Init().Forget();
    }

    private async UniTaskVoid Init()
    {
        Cursor.lockState = CursorLockMode.Confined;
        backdrop.Activate("Loading Data ...");
        RuntimePreviewGenerator.PreviewDirection = -Vector3.forward;
        //RuntimePreviewGenerator.Padding = 0.4f;
        await UniTask.WaitUntil(AddressableHelper.Instance.IsFullyLoaded);
        backdrop.Activate("Loading Chunks ...");
        await chunkGenerator.InitChunkGenerator();
        OnGameLoadFinish?.Invoke();
        backdrop.Deactivate();
    }
}
