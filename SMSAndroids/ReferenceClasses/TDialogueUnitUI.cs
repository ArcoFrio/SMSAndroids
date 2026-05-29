using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue.UnityUI;

public abstract class TDialogueUnitUI : MonoBehaviour
{
    [NonSerialized]
    protected DialogueUI m_DialogueUI;

    public virtual void OnAwake(DialogueUI dialogueUI)
    {
        m_DialogueUI = dialogueUI;
    }

    public abstract void OnReset(bool isNew);

    public abstract void OnStartNext(Story story, int nodeId, Args args);

    public abstract void OnFinishNext(Story story, int nodeId, Args args);
}