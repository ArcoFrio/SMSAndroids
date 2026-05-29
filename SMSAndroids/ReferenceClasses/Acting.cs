using System;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Acting
{
    [SerializeField]
    private PortraitMode m_Portrait = PortraitMode.ActorDefault;

    [SerializeField]
    private Actor m_Actor;

    [SerializeField]
    private int m_Expression;

    public PortraitMode Portrait => m_Portrait;

    public Actor Actor => m_Actor;

    public int Expression => m_Expression;
}