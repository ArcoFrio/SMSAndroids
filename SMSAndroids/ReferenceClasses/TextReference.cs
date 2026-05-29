using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Common.UnityUI;

[Serializable]
public class TextReference
{
    private enum Type
    {
        Text,
        TMP
    }

    private const int MAX_CHARACTER_COUNT = 99999;

    [SerializeField]
    private Type m_Type;

    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private TMP_Text m_TMP;

    [NonSerialized]
    private string m_Value;

    [NonSerialized]
    private int m_CharactersVisible = 99999;

    public string Text
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
            switch (m_Type)
            {
                case Type.Text:
                    RefreshLegacyText();
                    break;
                case Type.TMP:
                    RefreshTMPText();
                    RefreshTMPCharactersVisible();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public int CharactersVisible
    {
        get
        {
            return m_CharactersVisible;
        }
        set
        {
            m_CharactersVisible = value;
            switch (m_Type)
            {
                case Type.Text:
                    RefreshLegacyText();
                    break;
                case Type.TMP:
                    RefreshTMPCharactersVisible();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public Color Color
    {
        get
        {
            return m_Type switch
            {
                Type.Text => m_Text.color,
                Type.TMP => m_TMP.color,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        set
        {
            switch (m_Type)
            {
                case Type.Text:
                    m_Text.color = value;
                    break;
                case Type.TMP:
                    m_TMP.color = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public bool AreAllCharactersVisible => m_Value.Length <= CharactersVisible;

    public TextReference()
    {
        m_Value = Text;
    }

    public TextReference(Text text)
        : this()
    {
        m_Type = Type.Text;
        m_Text = text;
    }

    public TextReference(TMP_Text text)
    {
        m_Type = Type.TMP;
        m_TMP = text;
    }

    public override string ToString()
    {
        return m_Type switch
        {
            Type.Text => (m_Text != null) ? m_Text.gameObject.name : "(none)",
            Type.TMP => (m_TMP != null) ? m_TMP.gameObject.name : "(none)",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void RefreshLegacyText()
    {
        if (!(m_Text == null))
        {
            int length = Math.Min(m_Value.Length, CharactersVisible);
            string text = m_Value.Substring(0, length);
            if (text != m_Text.text)
            {
                m_Text.text = text;
            }
        }
    }

    private void RefreshTMPText()
    {
        if (!(m_TMP == null) && m_TMP.text != m_Value)
        {
            m_TMP.text = m_Value;
        }
    }

    private void RefreshTMPCharactersVisible()
    {
        int charactersVisible = CharactersVisible;
        if (m_TMP.maxVisibleCharacters != charactersVisible)
        {
            m_TMP.maxVisibleCharacters = charactersVisible;
        }
    }
}