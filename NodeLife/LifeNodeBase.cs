using System;
using System.Collections.Generic;
using System.Text;

namespace NodeLife
{
    public abstract class LifeNodeBase
    {
        public abstract LifeNodeBase Next();
        public abstract bool IsAlive(int x, int y);
        public abstract int Dimension
        {
            get;
        }
        public abstract bool IsLeaf
        {
            get;
        }
        public abstract bool IsOneStepLargerThanLeaf
        {
            get;
        }
        public abstract int Population
        {
            get;
        }
        public abstract LifeNode ExpandUniverse();
        public abstract LifeNodeBase NW
        {
            get;
        }
        public abstract LifeNodeBase NE
        {
            get;
        }
        public abstract LifeNodeBase SW
        {
            get;
        }
        public abstract LifeNodeBase SE
        {
            get;
        }

        public override string ToString()
        {
            if (this.Dimension > 16)
                return "LifeNode Dimension " + this.Dimension.ToString();
            StringBuilder sb = new StringBuilder((this.Dimension + 1) * this.Dimension);
            for (int j = 0; j < this.Dimension; j++)
            {
                for (int i = 0; i < this.Dimension; i++)
                {
                    if (this.IsAlive(i, j))
                        sb.Append('X');
                    else
                        sb.Append('.');
                }
                sb.Append('|');
            }
            return sb.ToString();

        }
    }
}
