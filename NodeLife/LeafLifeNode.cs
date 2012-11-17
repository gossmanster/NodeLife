using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace NodeLife
{
    public class LeafLifeNode : LifeNodeBase
    {
        private int bits; // 4 least significant bits
        private int population; // Could be stored in upper bits of this.bits
        private static LeafLifeNode[] allNodes = new LeafLifeNode[16];
        private static int[,] masks = { { 1, 4 }, { 2, 8 } };

        static LeafLifeNode()
        {
            for (int i = 0; i < 16; i++)
            {
                allNodes[i] = new LeafLifeNode(i);
            }
        }

        private LeafLifeNode(int bits)
        {
            this.bits = bits;
            int mask = 1;
            // Will only count if 4x4 or smaller
            for(int i = 0; i < 17; i++)
            {
                if ((this.bits & mask) == mask)
                    this.population++;
                mask *= 2;
            }
        }

        public static LeafLifeNode CreateNode(int i)
        {
            return allNodes[i];
        }

        public static LeafLifeNode CreateNode(bool nw, bool ne, bool sw, bool se)
        {
            int bits = 0;
            if (nw)
                bits++;
            if (ne)
                bits += 0x02;
            if (sw)
                bits += 0x04;
            if (se)
                bits += 0x08;
            return LeafLifeNode.CreateNode(bits);
        }

        // Internal because its an implementation detail
        internal static bool GetBit(int bits, int x, int y)
        {
            int mask = masks[x,y];
            return ((bits & mask) == mask);
        }

        // Internal because it's an implementation detail
        internal static int SetBit(int bits, int x, int y)
        {
            int mask = masks[x, y];
            return bits | mask;
        }

        public sealed override bool IsAlive(int x, int y)
        {
            int mask = masks[x, y];
            return ((this.bits & mask) == mask);
        }
        public override int Dimension
        {
            get { return 2; }
        }
        public override int Population
        {
            get { return this.population; }
        }
        public override bool IsLeaf
        {
            get { return true; }
        }
        public override bool IsOneStepLargerThanLeaf
        {
            get { return false; }
        }
        public override LifeNodeBase Next()
        {
            throw new NotImplementedException("Leafs cannot calculate next node");
        }

        public override LifeNode ExpandUniverse()
        {
            LeafLifeNode emptySpace = LeafLifeNode.CreateNode(0);
            // Subnodes for nw, ne, sw, se
            LeafLifeNode[] subNodes = new LeafLifeNode[] { emptySpace, emptySpace, emptySpace, emptySpace };

            // New nw will have old nw value in its se corner
            if ((this.bits & 0x01) == 0x01)
                subNodes[0] = LeafLifeNode.CreateNode(0x08);
            // New ne will have old ne in its sw corner
            if ((this.bits & 0x02) == 0x02)
                subNodes[1] = LeafLifeNode.CreateNode(0x04);
            if ((this.bits & 0x04) == 0x04)
                subNodes[2] = LeafLifeNode.CreateNode(0x02);
            if ((this.bits & 0x08) == 0x08)
                subNodes[3] = LeafLifeNode.CreateNode(0x01);
            return LifeNode.CreateNode(subNodes[0], subNodes[1], subNodes[2], subNodes[3]);       
        }
        public override bool Equals(object obj)
        {
            LeafLifeNode other = obj as LeafLifeNode;
            if ((other != null) && (other.bits == this.bits))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (this.bits + 3) * 53;
        }

        public override LifeNodeBase NE
        {
            get { throw new NotImplementedException(); }
        }

        public override LifeNodeBase NW
        {
            get { throw new NotImplementedException(); }
        }

        public override LifeNodeBase SE
        {
            get { throw new NotImplementedException(); }
        }

        public override LifeNodeBase SW
        {
            get { throw new NotImplementedException(); }
        }
    }
}
