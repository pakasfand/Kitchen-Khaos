using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckIfVideoDone : MonoBehaviour
{

	public VideoPlayer video;


	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Return))
		{
			video.frame = (long)video.frameCount - 5;
		}
		
		if(video.frame == (long)video.frameCount - 1)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

			
		}
	}
}
