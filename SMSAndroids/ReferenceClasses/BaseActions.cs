using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

public abstract class BaseActions : MonoBehaviour
{
    [SerializeField]
    protected InstructionList m_Instructions = new InstructionList();

    private Args m_Args;

    public bool IsRunning => m_Instructions.IsRunning;

    public int RunningIndex => m_Instructions.RunningIndex;

    public event Action EventInstructionStartRunning;

    public event Action EventInstructionEndRunning;

    public event Action<int> EventInstructionRun;

    protected BaseActions()
    {
        m_Instructions.EventRunInstruction += OnEventInstructionRun;
        m_Instructions.EventStartRunning += OnEventInstructionStartRunning;
        m_Instructions.EventEndRunning += OnEventInstructionStopRunning;
    }

    public abstract void Invoke(GameObject self = null);

    protected async Task ExecInstructions()
    {
        if (m_Args == null)
        {
            m_Args = new Args(base.gameObject);
        }

        await ExecInstructions(m_Args);
    }

    protected async Task ExecInstructions(Args args)
    {
        await m_Instructions.Run(args);
    }

    protected void StopExecInstructions()
    {
        m_Instructions.Cancel();
    }

    protected virtual void OnDisable()
    {
        StopExecInstructions();
    }

    protected virtual void OnDestroy()
    {
        this.EventInstructionStartRunning = null;
        this.EventInstructionEndRunning = null;
        this.EventInstructionRun = null;
    }

    private void OnEventInstructionRun(int i)
    {
        this.EventInstructionRun?.Invoke(i);
    }

    private void OnEventInstructionStartRunning()
    {
        this.EventInstructionStartRunning?.Invoke();
    }

    private void OnEventInstructionStopRunning()
    {
        this.EventInstructionEndRunning?.Invoke();
    }
}