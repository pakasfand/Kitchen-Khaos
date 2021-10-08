using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckIfVideoDone : MonoBehaviour
{
	[SerializeField] private int _loadingSceneIndex;
	public VideoPlayer video;

	private void Start()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			SceneManager.LoadScene(_loadingSceneIndex);
		}
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene(_loadingSceneIndex);
		}
		
		if(video.frame == (long)video.frameCount - 1)
		{
			SceneManager.LoadScene(_loadingSceneIndex);
		}
	}
}
