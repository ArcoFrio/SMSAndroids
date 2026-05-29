using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Actant
{
    [SerializeField]
    private PropertyGetString m_Name = new PropertyGetString();

    [SerializeField]
    private PropertyGetString m_Description = new PropertyGetString();

    public string GetName(Args args)
    {
        return m_Name.Get(args);
    }

    public string GetDescription(Args args)
    {
        return m_Description.Get(args);
    }
}