using System;
using UnityEngine;

public class NPCInteraction : InteractionController
{
	private float lastInteractionTime;

	private string _npcName;
	[SerializeField] DialogueData _dialogueData;


	protected override void OnInteract()
	{
		if(_dialogueData == null)
		{
			return;
		}

		var lines = _dialogueData.GetLines();
		bool hasAnyLine = false;
		foreach(var line in lines)
		{
			hasAnyLine = true;
			break;
		}
		
		if(hasAnyLine== false)
		{
			return;
		}

		
		ShowDialogue();
	}

	private void ShowDialogue()
	{
		lastInteractionTime = Time.time;
		GameManager.Instance?.StartDialogue(_dialogueData);
	}
}
