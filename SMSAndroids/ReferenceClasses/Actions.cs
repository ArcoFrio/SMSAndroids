using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[HelpURL("https://docs.gamecreator.io/gamecreator/visual-scripting/actions")]
[AddComponentMenu("Game Creator/Visual Scripting/Actions")]
[DefaultExecutionOrder(1)]
public class Actions : BaseActions
{
    public override void Invoke(GameObject self = null)
    {
        Args args = new Args((self != null) ? self : base.gameObject, base.gameObject);
        Run(args);
    }

    public async Task Run()
    {
        try
        {
            await ExecInstructions();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString(), this);
        }
    }

    public async Task Run(Args args)
    {
        try
        {
            await ExecInstructions(args);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString(), this);
        }
    }

    public void Cancel()
    {
        StopExecInstructions();
    }
}