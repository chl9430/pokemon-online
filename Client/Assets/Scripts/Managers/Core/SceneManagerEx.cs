using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    object _data;
    BaseScene _curScene;

    public BaseScene CurrentScene 
    { 
        get 
        {
            if (_curScene == null)
                return GameObject.FindFirstObjectByType<BaseScene>();
            else
                return _curScene;
        }
        set
        {
            _curScene = value;
        }
    }

    public object Data { get { return _data; } set { _data = value; } }

	public void LoadScene(Define.Scene type, LoadSceneMode loadeSceneMode = LoadSceneMode.Single)
    {
        Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type), loadeSceneMode);
    }

    public void AsyncLoadScene(Define.Scene type, Action action, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GetSceneName(type), loadSceneMode);

        if (asyncLoad == null) return;

        Action<AsyncOperation> onCompletedHandler = null;

        onCompletedHandler += (operation) =>
        {
            action.Invoke();

            operation.completed -= onCompletedHandler;
        };

        asyncLoad.completed += onCompletedHandler;
    }

    public void AsyncUnLoadScene(Define.Scene type, Action action)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(GetSceneName(type));

        if (asyncUnload != null)
        {
            Action<AsyncOperation> onCompletedHandler = null;

            onCompletedHandler = (operation) =>
            {
                action.Invoke();

                operation.completed -= onCompletedHandler;
            };

            asyncUnload.completed += onCompletedHandler;
        }
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
