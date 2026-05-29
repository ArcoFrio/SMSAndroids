using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Expressions : TPolymorphicList<Expression>
{
    public const string NAME_EXPRESSIONS = "m_Expressions";

    [SerializeReference]
    private Expression[] m_Expressions = new Expression[1]
    {
        new Expression()
    };

    public override int Length => m_Expressions.Length;

    public Expression FromId(IdString id)
    {
        if (m_Expressions.Length == 0)
        {
            return null;
        }

        Expression[] expressions = m_Expressions;
        foreach (Expression expression in expressions)
        {
            if (expression.Id.Hash == id.Hash)
            {
                return expression;
            }
        }

        return m_Expressions[0];
    }

    public Expression FromIndex(int index)
    {
        if (m_Expressions.Length == 0)
        {
            return null;
        }

        index = Mathf.Clamp(index, 0, m_Expressions.Length - 1);
        return m_Expressions[index];
    }
}