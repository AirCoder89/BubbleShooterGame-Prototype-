using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;

public enum SoundList
{
	Shoot,Connect,Merge,Combo,Achieve2028,Perfect,UIButton,StartGame,UIPopUp
}

[System.Serializable]
public class Sound{
	[BoxGroup("Initialize")] public SoundList name;
	[BoxGroup("Initialize")] public AudioClip clip;

	[BoxGroup("Settings")] [Range(0f,1f)] public float volume;
	[BoxGroup("Settings")] [Range(0f, 2f)] public float pitch;
	[BoxGroup("Settings")] public bool loop;
	[BoxGroup("Settings")] [ReadOnly] public bool play = true;

	[HideInInspector] public AudioSource source;
}


public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public Sound[] sounds;
	
	[FoldoutGroup("Test Sounds")] [SerializeField]
	private SoundList selectedSound;
	[FoldoutGroup("Test Sounds")] [Range(0f, 2f)] public float pitch;
	
	[FoldoutGroup("Test Sounds")][Button("Play Sound",ButtonSizes.Medium)]
	private void PlaySound()
	{
		Play(selectedSound,pitch);
	}
	void Awake () 
	{
		
		if(Instance != null) return;
		Instance = this;
		
		foreach (var s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
		}
	}

	public void UpdateVolume(float volume)
	{
		foreach (var s in sounds)
		{
			s.source.volume = s.volume = volume;
		}
	}
	
	public void Play(SoundList sName,float pitch = 0f)
	{
		try
		{
			var s = Array.Find(sounds, sound => sound.name == sName);
			if (s != null && pitch > 0f)
			{
				s.pitch = pitch;
				s.source.pitch = pitch;
			}
			if(s.play) s.source.Play();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
	
	
	public void Stop(SoundList sName)
	{
		try
		{
			var s = Array.Find(sounds, sound => sound.name == sName);
			s.source.Stop();
		}
		catch
		{
			throw new Exception("Sound not Found");
		}
	}
}
