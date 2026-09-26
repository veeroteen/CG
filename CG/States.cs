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
        MOVE
    }



    public class IState
    {
        public int pID;
        public int DotID;
        public ISTATE Istate { get; protected set; }
        public IState(ISTATE state,int pid = -1, int did = -1) 
        {
            Istate = state;
            pID = pid;
            DotID = did;
            
        }
        public void toggleDraw(int pid, int did) 
        {
            pID = pid;
            DotID = did;
            Istate = ISTATE.DRAW;
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
        
        }

        public void ToggleFreeState() 
        {
            Istate = ISTATE.FREE;
            pID = -1;
            DotID = -1;
        }

    }


}



