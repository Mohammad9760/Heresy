using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder (-1000)]
public class GameManager : MonoBehaviour
{
	private static bool paused;
	
	public static bool IsGamePaused
	{
		get
		{
			return paused;
		}
		set
		{
			paused = value;
			OnPauseToggled();
		}
	}
	
	public delegate void PauseToggle();
	public static event PauseToggle OnPauseToggled;
	
	public static Controls InputMaps;
	
	
	private void Awake()
	{
		InputMaps = new Controls();
		InputMaps.Enable();
		
		InputMaps.UI.Back.performed += (_) => IsGamePaused = !IsGamePaused;
		OnPauseToggled += PauseToggled;
	}
	
	private void Start()
    {
	    IsGamePaused = false;
    }
    
	private void PauseToggled()
	{
		if(IsGamePaused)
			InputMaps.Player.Disable();
		else
			InputMaps.Player.Enable();
			
		Time.timeScale = IsGamePaused ? 0: 1;
		Cursor.lockState = IsGamePaused ? CursorLockMode.Confined: CursorLockMode.Locked;
	}
}

public static class Settings
{
	public static float MusicVolume
	{
		get
		{
			if(!PlayerPrefs.HasKey("MusicVolume"))
				PlayerPrefs.SetFloat("MusicVolume", 1);
			return PlayerPrefs.GetFloat("MusicVolume");
		}
		set
		{
			PlayerPrefs.SetFloat("MusicVolume", value);
		}
	}
	public static float SFXVolume
	{
		get
		{
			if(!PlayerPrefs.HasKey("SFXVolume"))
				PlayerPrefs.SetFloat("SFXVolume", 1);
			return PlayerPrefs.GetFloat("SFXVolume");
		}
		set
		{
			PlayerPrefs.SetFloat("SFXVolume", value);
		}
	}
}
