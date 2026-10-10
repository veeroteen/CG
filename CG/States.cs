using System;

namespace CG
{
    public enum OSTATE
    {
        MESH,
        POLYGON,
    }

    public enum ISTATE
    {
        DRAW,
        MOVE,
        SELECTION
    }

    public class IState
    {
        public int pID;
        public int DotID;
        public TYPE type;
        public ISTATE Istate { get; protected set; }
        private readonly Action callback;

        public IState(Action callback, ISTATE state, int pid = -1, int did = -1)
        {
            this.callback = callback;
            Istate = state;
            pID = pid;
            DotID = did;
            type = TYPE.NONE;
        }

        public void SetState(ISTATE newState)
        {
            Istate = newState;
            pID = -1;
            DotID = -1;
            type = TYPE.NONE;
            callback();
        }

        public void SetActiveDrawPrimitive(int pid, int did)
        {
            pID = pid;
            DotID = did;
            type = TYPE.DOT;
            callback();
        }
    }
}