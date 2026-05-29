using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Dialogue.UnityUI;

[AddComponentMenu("Game Creator/UI/Dialogue/Speech UI")]
public class SpeechUI : TDialogueUnitUI
{
    private const float TIME_SAFEGUARD = 0.25f;

    [SerializeField]
    private GameObject m_Active;

    [SerializeField]
    private GameObject m_ActiveActor;

    [SerializeField]
    private TextReference m_ActorName = new TextReference();

    [SerializeField]
    private TextReference m_ActorDescription = new TextReference();

    [SerializeField]
    private GameObject m_ActivePortrait;

    [SerializeField]
    private Image m_Portrait;

    [SerializeField]
    private TextReference m_Text = new TextReference();

    [SerializeField]
    private GameObject m_Skip;

    [NonSerialized]
    private Story m_Story;

    [NonSerialized]
    private int m_NodeId;

    [NonSerialized]
    private Args m_Args;

    [NonSerialized]
    private bool m_IsActive;

    [NonSerialized]
    private float m_StartTime;

    [NonSerialized]
    private int m_PreviousActorHash;

    [NonSerialized]
    private bool m_RunningChoice;

    [field: NonSerialized]
    public static SpeechUI Current { get; private set; }

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        Current = null;
    }

    public override void OnReset(bool isNew)
    {
        if (isNew)
        {
            m_PreviousActorHash = -1;
            m_RunningChoice = false;
        }
    }

    public override void OnStartNext(Story story, int nodeId, Args args)
    {
        if (!ApplicationManager.IsExiting)
        {
            m_Story = story;
            m_NodeId = nodeId;
            m_Args = args;
            m_IsActive = false;
            if (m_Active != null)
            {
                m_Active.SetActive(value: false);
            }

            Node node = story.Content.Get(nodeId);
            if (node != null)
            {
                node.EventStartText -= OnStartText;
                node.EventStartChoice -= OnStartChoice;
                node.EventFinishType -= OnFinishText;
                node.EventStartText += OnStartText;
                node.EventStartChoice += OnStartChoice;
                node.EventFinishType += OnFinishText;
            }
        }
    }

    public override void OnFinishNext(Story story, int nodeId, Args args)
    {
        if (!ApplicationManager.IsExiting)
        {
            Node node = story.Content.Get(nodeId);
            if (node != null)
            {
                node.EventStartText -= OnStartText;
                node.EventStartChoice -= OnStartChoice;
                node.EventFinishType -= OnFinishText;
            }
        }
    }

    private void Update()
    {
        if (!m_IsActive)
        {
            return;
        }

        Node node = m_Story?.Content.Get(m_NodeId);
        if (node != null)
        {
            int charactersVisible = ((node.Actor != null) ? node.Actor.Typewriter.GetCharactersVisible(m_StartTime, m_Story.Time) : int.MaxValue);
            m_Text.CharactersVisible = charactersVisible;
            if (m_Skip != null)
            {
                bool active = node.Duration == NodeDuration.UntilInteraction && m_Text.AreAllCharactersVisible && m_StartTime + 0.25f < m_Story.Time.Time && !m_RunningChoice;
                m_Skip.SetActive(active);
            }
        }
    }

    private void OnStartText(int nodeId)
    {
        if (ApplicationManager.IsExiting)
        {
            return;
        }

        Node node = m_Story.Content.Get(nodeId);
        if (node == null)
        {
            return;
        }

        if (m_DialogueUI.SpeechSkin != null)
        {
            m_DialogueUI.SpeechSkin.PlayClipStart();
        }

        if (m_ActiveActor != null)
        {
            m_ActiveActor.gameObject.SetActive(node.Actor != null);
            if (node.Actor != null)
            {
                m_ActorName.Text = node.Actor.GetName(m_Args);
                m_ActorDescription.Text = node.Actor.GetDescription(m_Args);
            }
        }

        Sprite sprite = ((node.Actor != null) ? node.Actor.GetExpressionFromIndex(node.Expression) : null)?.GetSprite(m_Args);
        if (m_ActivePortrait != null)
        {
            Portrait portrait = ((node.Portrait != PortraitMode.ActorDefault) ? ((Portrait)node.Portrait) : ((node.Actor != null) ? node.Actor.Portrait : Portrait.None));
            bool active = sprite != null && portrait != Portrait.None;
            m_ActivePortrait.SetActive(active);
        }

        if (m_Portrait != null)
        {
            m_Portrait.overrideSprite = sprite;
        }

        m_Text.Text = node.Text;
        m_Text.CharactersVisible = 0;
        if (m_Skip != null)
        {
            m_Skip.SetActive(value: false);
        }

        if (m_Active != null)
        {
            m_Active.SetActive(value: true);
        }

        m_StartTime = m_Story.Time.Time;
        m_IsActive = true;
        Animator animator = this.Get<Animator>();
        bool num = animator != null && animator.runtimeAnimatorController != null;
        int num2 = ((node.Actor != null) ? node.Actor.GetHashCode() : 0);
        if (num && m_DialogueUI.SpeechSkin.AnimateWhen switch
        {
            SpeechSkin.AnimationWhen.NewSpeaker => m_PreviousActorHash != num2,
            SpeechSkin.AnimationWhen.Always => true,
            _ => throw new ArgumentOutOfRangeException(),
        })
        {
            animator.SetTrigger(SpeechSkin.ANIMATOR_OPEN);
        }

        m_PreviousActorHash = num2;
        m_RunningChoice = false;
    }

    private void OnStartChoice(int nodeId)
    {
        m_RunningChoice = true;
    }

    private void OnFinishText(int nodeId)
    {
        if (!ApplicationManager.IsExiting)
        {
            if (m_DialogueUI.SpeechSkin != null)
            {
                m_DialogueUI.SpeechSkin.PlayClipFinish();
            }

            if (m_Active != null)
            {
                m_Active.SetActive(value: false);
            }

            m_IsActive = false;
        }
    }

    public void Skip()
    {
        if (m_StartTime + 0.25f > m_Story.Time.Time)
        {
            return;
        }

        Node node = m_Story?.Content.Get(m_NodeId);
        if (node == null)
        {
            return;
        }

        if (node.Actor != null)
        {
            float duration = node.Actor.Typewriter.GetDuration(node.Text);
            if (m_Story.Time.Time < m_StartTime + duration)
            {
                m_StartTime = -9999f;
                m_Text.CharactersVisible = node.Text.Length;
                m_Story.StopTypewriter();
                return;
            }
        }

        m_Story.Continue();
    }
}