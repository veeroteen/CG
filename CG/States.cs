using CG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG
{
    public enum OSTATE
    {
        MESH,
        POLYGON,
    }
    public enum ISTATE
    {
        FREE,
        DRAW,
        CONTINUOUSDRAW,
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
        public IState(Action callback,ISTATE state,int pid = -1, int did = -1) 
        {
            this.callback = callback;
            Istate = state;
            pID = pid;
            DotID = did;
            type = TYPE.NONE;
        }
        public void toggleDraw(int pid, int did) 
        {
            pID = pid;
            DotID = did;
            Istate = ISTATE.DRAW;
            type = TYPE.DOT;
            callback();
        }
        public void ToggleContiniousDraw(bool flag) 
        {
            if (Istate == ISTATE.DRAW || Istate == ISTATE.CONTINUOUSDRAW)
            {
                if (flag)
                {
                    Istate = ISTATE.CONTINUOUSDRAW;
                }
                else
                {
                    Istate = ISTATE.DRAW;
                }
            }
            callback();
        }

        public void ToggleFreeState() 
        {
            Istate = ISTATE.FREE;
            pID = -1;
            DotID = -1;
            type = TYPE.NONE;
            callback();
        }
        public void ToggleMoveState()
        {
            Istate = ISTATE.MOVE;
            pID = -1;
            DotID = -1;
            callback();
        }
    }


}



