using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;

namespace NodeLife
{
    public class LifeNode : LifeNodeBase
    {
        private LifeNodeBase nw, ne, sw, se;
        private int dimension;
        private int population = -1;
        private LifeNodeBase next = null;

        private static Random randomGenerator = new Random((int)System.DateTime.Now.Ticks);


        // ctor is private because we always want to call the factory method CreateNode,
        // which is the one that caches (hashes) nodes
        private LifeNode(LifeNodeBase nw, LifeNodeBase ne, LifeNodeBase sw, LifeNodeBase se)
        {
            // ASSERT nw, ne, sw, se are all the same dimension
            Debug.Assert(nw.Dimension == ne.Dimension);
            Debug.Assert(ne.Dimension == sw.Dimension);
            Debug.Assert(sw.Dimension == se.Dimension);
            this.dimension = nw.Dimension * 2;
            this.nw = nw;
            this.ne = ne;
            this.sw = sw;
            this.se = se;
        }

        public sealed override LifeNodeBase NE
        {
            get { return this.ne; }
        }

        public sealed override LifeNodeBase NW
        {
            get { return this.nw; }
        }

        public sealed override LifeNodeBase SE
        {
            get { return this.se; }
        }

        public sealed override LifeNodeBase SW
        {
            get { return this.sw; }
        }

        private static Dictionary<LifeNode, LifeNode> cache = new Dictionary<LifeNode, LifeNode>();
        private static long hits = 0;
        private static long misses = 0;

        public static void ClearStats()
        {
            hits = 0;
            misses = 0;
            nextCacheMiss = 0;
            nextCalls = 0;
            newLeafs = 0;
        }
        public static LifeNode CreateNode(LifeNodeBase nw, LifeNodeBase ne, LifeNodeBase sw, LifeNodeBase se)
        {
            LifeNode newNode = new LifeNode(nw, ne, sw, se);
            LifeNode cached;
            if (!cache.TryGetValue(newNode, out cached))
            {
                cache.Add(newNode, newNode);
                misses++;
            }
            else
            {
                newNode = cached;
                hits++;
            }
            return newNode;
        }

        public static void GarbageCollect(LifeNode universe)
        {
            cache = new Dictionary<LifeNode, LifeNode>();
            foreach (KeyValuePair<LifeNode,LifeNode> pair in cache)
            {
                LifeNode node = pair.Key;
                CreateNode(node.nw, node.ne, node.sw, node.se);
            }
        }

        public delegate LeafLifeNode InitLeaf();

        public static LifeNodeBase InitSpace(int dimension, InitLeaf initFunc)
        {
            if (!Utils.IsPowerOfTwo(dimension))
                throw new InvalidOperationException("dimension must be a power of 2");
            if (dimension == 2)
                return initFunc();
            LifeNodeBase subNode1 = LifeNode.InitSpace(dimension / 2, initFunc);
            LifeNodeBase subNode2 = LifeNode.InitSpace(dimension / 2, initFunc);
            LifeNodeBase subNode3 = LifeNode.InitSpace(dimension / 2, initFunc);
            LifeNodeBase subNode4 = LifeNode.InitSpace(dimension / 2, initFunc);
            return LifeNode.CreateNode(subNode1, subNode2, subNode3, subNode4);
        }

        public static LifeNodeBase EmptySpace(int dimension)
        {
            Debug.Assert(dimension > 1);
            Debug.Assert(Utils.IsPowerOfTwo(dimension));
            if (dimension == 2)
                return LeafLifeNode.CreateNode(0);
            LifeNodeBase next = EmptySpace(dimension / 2);
            return LifeNode.CreateNode(next, next, next, next);
        }

        public static LifeNodeBase RandomSpace(int dimension)
        {
            return InitSpace(dimension,
                            () => LeafLifeNode.CreateNode(randomGenerator.Next(15)));
        }

        // Return true if their are live cells around the border.  We then need to 
        // call ExpandUniverse or the next generation will not be calculated properly
        // because of boundary conditions
        public bool NeedsExpansion()
        {
            // Just always expand when we're down near leaves
            if (this.Dimension < 16)
                return true;
            if (this.nw.Population != this.nw.SE.SE.Population)
                return true;
            if (this.ne.Population != this.ne.SW.SW.Population)
                return true;
            if (this.sw.Population != this.sw.NE.NE.Population)
                return true;
            if (this.se.Population != this.se.NW.NW.Population)
                return true;
            return false;

        }

        public override LifeNode ExpandUniverse()
        {
            LifeNodeBase emptySpace = LifeNode.EmptySpace(this.dimension / 2);
            LifeNode newNW = LifeNode.CreateNode(emptySpace, emptySpace, emptySpace, this.nw);
            Debug.Assert(newNW.Dimension == this.dimension);
            LifeNode newNE = LifeNode.CreateNode(emptySpace, emptySpace, this.ne, emptySpace);
            LifeNode newSW = LifeNode.CreateNode(emptySpace, this.sw, emptySpace, emptySpace);
            LifeNode newSE = LifeNode.CreateNode(this.se, emptySpace, emptySpace, emptySpace);
            LifeNode retval = LifeNode.CreateNode(newNW, newNE, newSW, newSE);
            Debug.Assert(retval.Dimension == dimension * 2);
            Debug.Assert(retval.Population == this.Population);
            return retval;
        }

        private int countNeighbors(int x, int y)
        {
            int count = 0;
            if (this.IsAlive(x -1,  y - 1))
                count++;
            if (this.IsAlive(x, y - 1))
                count++;
            if (this.IsAlive(x + 1, y - 1))
                count++;
            if (this.IsAlive(x - 1, y))
                count++;
            if (this.IsAlive(x + 1, y))
                count++;
            if (this.IsAlive(x - 1, y + 1))
                count++;
            if (this.IsAlive(x, y + 1))
                count++;
            if (this.IsAlive(x + 1, y + 1))
                count++;
            return count;
        }

        bool calcNewCell(int x, int y)
        {
            int count = this.countNeighbors(x, y);
            if (this.IsAlive(x, y))
            {
                if (count == 2)
                    return true;
                if (count == 3)
                    return true;
            }
            else
            {
                if (count == 3)
                    return true;
            }
            return false;
        }

        // Given a 4x4 Node, calculate the new leaf that represents the 
        // 2x2 center of this node.
        private static long newLeafs = 0;
        private LeafLifeNode calcNewLeaf()
        {
            Debug.Assert(this.IsOneStepLargerThanLeaf);
            newLeafs++;
            int newBits = 0;
            if (this.calcNewCell(1, 1))
                newBits += 1;
            if (this.calcNewCell(2, 1))
                newBits += 2;
            if (this.calcNewCell(1, 2))
                newBits += 4;
            if (this.calcNewCell(2, 2))
                newBits += 8;
            return LeafLifeNode.CreateNode(newBits);
        }

        // Create new LifeNode from central bits of nw, ne, sw, se
        public static LifeNodeBase CreateCenterNode(LifeNodeBase nw, LifeNodeBase ne, LifeNodeBase sw, LifeNodeBase se)
        {
            LifeNodeBase retval = null;
            if (nw.IsLeaf)
            {
                bool nwsebit = nw.IsAlive(1, 1);
                bool neswbit = ne.IsAlive(0, 1);
                bool swnebit = sw.IsAlive(1, 0);
                bool senwbit = se.IsAlive(0, 0);
                retval = LeafLifeNode.CreateNode(nwsebit, neswbit, swnebit, senwbit);
            }
            else
            {
                retval = LifeNode.CreateNode(nw.SE, ne.SW, sw.NE, se.NW);
            }
            return retval;
        }

        // Next cell is created using "helper" temporary cells that surround
        // the center.
        //
        // XX|XX|XX|XX
        // XX|NW|NE|XX
        // XX|SW|SE|XX
        // XX|XX|XX|XX
        public LifeNodeBase TemporaryCenterNode()
        {
            Debug.Assert(this.IsLeaf == false);
            return LifeNode.CreateCenterNode(this.nw, this.ne, this.sw, this.se);
        }

        public LifeNodeBase TemporaryCenterCenterNode()
        {
            LifeNodeBase retval = null;
            if (this.NW.IsOneStepLargerThanLeaf)
            {
                bool nwbit = this.nw.SE.IsAlive(1, 1);
                bool nebit = this.ne.SW.IsAlive(0, 1);
                bool swbit = this.sw.NE.IsAlive(1, 0);
                bool sebit = this.se.NW.IsAlive(0, 0);
                retval = LeafLifeNode.CreateNode(nwbit, nebit, swbit, sebit);
            }
            else
            {
                retval = LifeNode.CreateNode(this.nw.SE.SE, this.ne.SW.SW, this.sw.NE.NE, this.se.NW.NW);
            }
            return retval;
        }


        // Given two nodes, w and e, pull out these bits
        // XX|XX|XX|XX||XX|XX|XX|XX
        // XX|XX|XX|NW||NE|XX|XX|XX
        // XX|XX|XX|SW||SE|XX|XX|XX
        // XX|XX|XX|XX||XX|XX|XX|XX
        public static LifeNodeBase TemporaryCenterXNode(LifeNodeBase w, LifeNodeBase e)
        {
            Debug.Assert(w.Dimension == e.Dimension);
            LifeNodeBase retval = null;
            if (w.IsOneStepLargerThanLeaf)
            {
                bool nwbit = w.NE.IsAlive(1, 1);
                bool nebit = e.NW.IsAlive(0, 1);
                bool swbit = w.SE.IsAlive(1, 0);
                bool sebit = e.SW.IsAlive(0, 0);
                retval = LeafLifeNode.CreateNode(nwbit, nebit, swbit, sebit);
            }
            else
            {
                retval = LifeNode.CreateNode(w.NE.SE, e.NW.SW, w.SE.NE, e.SW.NW);
            }
            return retval;
        }

        // Given two nodes, n and s, pull out these bits
        // XX|XX|XX|XX
        // XX|XX|XX|XX
        // XX|XX|XX|XX
        // XX|NW|NE|XX
        //-------------
        // XX|SW|SE|XX
        // XX|XX|XX|XX
        // XX|XX|XX|XX
        // XX|XX|XX|XX  
        public static LifeNodeBase TemporaryCenterYNode(LifeNodeBase n, LifeNodeBase s)
        {
            Debug.Assert(n.Dimension == s.Dimension);
            LifeNodeBase retval = null;
            if (n.IsOneStepLargerThanLeaf)
            {
                bool nwbit = n.SW.IsAlive(1, 1);
                bool nebit = n.SE.IsAlive(0, 1);
                bool swbit = s.NW.IsAlive(1, 0);
                bool sebit = s.NE.IsAlive(0, 0);
                retval = LeafLifeNode.CreateNode(nwbit, nebit, swbit, sebit);
            }
            else
            {
                retval = LifeNode.CreateNode(n.SW.SE, n.SE.SW, s.NW.NE, s.NE.NW);
            }
            return retval;
        }

        private static long nextCalls = 0;
        private static long nextCacheMiss = 0;
        public override LifeNodeBase Next()
        {
            if (this.population == 0)
                return this.nw;
            nextCalls++;
            if (this.next == null)
            {
                nextCacheMiss++;
                // If the children are leaves, we can directly calculate 
                // the new state of the center of this node.
                if (this.nw.IsLeaf)
                {
                    this.next = this.calcNewLeaf();
                }
                else
                {
                    // We put together 4 new nodes each half the size of this one 
                    // and designed to calculate the center half of this node.  In other words,
                    // we want to calculate the new value of this.nw.se, this.ne.sw, etc., so we 
                    // put together artificial subnodes that "wrap" nw.se, ne.sw, etc., calc their
                    // next generation and then stick those four results together into the new child node 
                    // X  X  X  X  |  X  X  X  X
                    // X  nw nw nc | nc ne ne  X
                    // X  nw nw nc | nc ne ne  X
                    // X  cw cw cc | cc ce ce  X
                    // X  cw cw cc | cc ce ce  X
                    // X  sw sw sc | sc se se  X
                    // X  sw sw sc | sc se se  X
                    // X  X  X  X  |  X  X  X  X
                    LifeNodeBase nw = ((LifeNode)this.nw).TemporaryCenterNode();
                    LifeNodeBase nc = LifeNode.TemporaryCenterXNode(this.nw, this.ne);
                    LifeNodeBase ne = ((LifeNode)this.ne).TemporaryCenterNode();
                    LifeNodeBase cw = LifeNode.TemporaryCenterYNode(this.nw, this.sw);
                    LifeNodeBase cc = this.TemporaryCenterCenterNode();
                    LifeNodeBase ce = LifeNode.TemporaryCenterYNode(this.ne, this.se);
                    LifeNodeBase se = ((LifeNode)this.se).TemporaryCenterNode();
                    LifeNodeBase sc = LifeNode.TemporaryCenterXNode(this.sw, this.se);
                    LifeNodeBase sw = ((LifeNode)this.sw).TemporaryCenterNode();

                    // From those 9 temporary nodes, we construct four larger nodes and 
                    // then calculate their next generation

                    LifeNodeBase tnw = LifeNode.CreateNode(nw, nc, cw, cc);
                    LifeNodeBase tne = LifeNode.CreateNode(nc, ne, cc, ce);
                    LifeNodeBase tsw = LifeNode.CreateNode(cw, cc, sw, sc);
                    LifeNodeBase tse = LifeNode.CreateNode(cc, ce, sc, se);
                    tnw = tnw.Next();
                    tne = tne.Next();
                    tsw = tsw.Next();
                    tse = tse.Next();

                    // Put those next gens together and we get the next gen of this node
                    // cache it of course
                    this.next = LifeNode.CreateNode(tnw, tne, tsw, tse);               
                }
            }
            return this.next;
        }

        public override bool IsOneStepLargerThanLeaf
        {
            get 
            {
                if (this.nw.IsLeaf)
                {
                    Debug.Assert(this.Dimension == 4);
                    Debug.Assert(this.ne.IsLeaf);
                    Debug.Assert(this.sw.IsLeaf);
                    Debug.Assert(this.se.IsLeaf);
                    return true;
                }
                return false;
            }
        }

        public override bool IsAlive(int x, int y)
        {
            int halfdim = nw.Dimension;
            int cellx = x / halfdim;
            int dx = x % halfdim;
            int celly = y / halfdim;
            int dy = y % halfdim;
            if ((cellx == 0) && (celly == 0))
                return this.nw.IsAlive(dx, dy);
            if ((cellx == 1) && (celly == 0))
                return this.ne.IsAlive(dx, dy);
            if ((cellx == 0) && (celly == 1))
                return this.sw.IsAlive(dx, dy);
            if ((cellx == 1) && (celly == 1))
                return this.se.IsAlive(dx, dy);
            throw new IndexOutOfRangeException();
        }

        public override int Dimension
        {
            get { return this.dimension; }
        }

        public override int Population
        {
            get
            {
                if (this.population == -1)
                {
                    this.population = this.nw.Population +
                        this.ne.Population +
                        this.sw.Population +
                        this.se.Population;
                }
                return this.population;
            }
        }

        public override bool IsLeaf
        {
            get { return false; }
        }

        public override bool Equals(object obj)
        {
            LifeNode other = obj as LifeNode;
            if (other != null)
            {
                if (this.dimension != other.dimension)
                    return false;
                // Because the leaves are cached already we can
                // do pointer equality on them

                return ((this.nw == other.nw) &&
                        (this.ne == other.ne) &&
                        (this.se == other.se) &&
                        (this.sw == other.sw));
             
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (true)
            {
#if true
                return this.dimension +
                    7 * RuntimeHelpers.GetHashCode(this.nw) +
                    379 * RuntimeHelpers.GetHashCode(this.ne) +
                    1009 * RuntimeHelpers.GetHashCode(this.sw) +
                    3527 * RuntimeHelpers.GetHashCode(this.se);
#else
                return this.dimension +
                    this.nw.GetHashCode() +
                    379 * this.ne.GetHashCode() +
                    1009 * this.sw.GetHashCode() +
                    3527 * this.se.GetHashCode();
#endif
            }
           
        }
    }
}
