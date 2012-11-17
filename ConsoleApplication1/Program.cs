using System;
using System.Collections.Generic;
using System.Text;
using NodeLife;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestBits();
            TestLifeNode1();
            TestPowerOfTwo();
            TestEmptySpace();
            TestExpandSpace();
            TestRandomSpace();
            TestTemporaryNodes();
            TestRun();
        }

        static LifeNode createBlock()
        {
            LeafLifeNode nw = LeafLifeNode.CreateNode(8);
            LeafLifeNode ne = LeafLifeNode.CreateNode(4);
            LeafLifeNode sw = LeafLifeNode.CreateNode(2);
            LeafLifeNode se = LeafLifeNode.CreateNode(1);
            LifeNode block = LifeNode.CreateNode(nw, ne, sw, se);
            Debug.Assert(block.Population == 4);
            return block;
        }

        static void TestBits()
        {
            for (int i = 0; i < 16; i++)
            {
                LeafLifeNode temp = LeafLifeNode.CreateNode(i);
                int count = 0;
                for(int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        if (temp.IsAlive(j, k))
                            count++;
                    }
                Debug.Assert(count == temp.Population);
            }
        }

        static void TestLifeNode1()
        {
            LifeNode block = createBlock();
            Debug.Assert(block.Population == 4);
            LifeNodeBase next = block.Next();
            Debug.Assert(next.IsLeaf);
            Debug.Assert(next.Dimension == 2);
            Debug.Assert(next.Population == 4);

            LifeNode newBlock = block.ExpandUniverse();
            Debug.Assert(newBlock.Population == 4);
            Debug.Assert(newBlock.Dimension == 8);
            next = newBlock.Next();
            Debug.Assert(next.Dimension == 4);
            Debug.Assert(next.Population == 4);
            Debug.Assert(next.Equals(block));
            Debug.Assert(next == block);
        }

        static void TestEmptySpace()
        {
            LifeNodeBase t = LifeNode.EmptySpace(2);
            Debug.Assert(t.Dimension == 2);
            Debug.Assert(t.Population == 0);
            Debug.Assert(t is LeafLifeNode);

            t = LifeNode.EmptySpace(1024);
            Debug.Assert(t.Dimension == 1024);
            Debug.Assert(t.Population == 0);

            /* 
             * Turned off this error check for perf reasons
            bool pass = false;
            try
            {
                t = LifeNode.EmptySpace(7);
            }
            catch (InvalidOperationException)
            {
                pass = true;
            }
            Debug.Assert(pass);
             * */
            
        }

        static void TestRandomSpace()
        {
            LifeNodeBase node = LifeNode.RandomSpace(8);
            Debug.Assert(node.Dimension == 8);

            // Population is random, but chances are very, very high it falls within 
            // this range
            Debug.Assert(node.Population > 16);
            Debug.Assert(node.Population < 48);
        }

        static void TestExpandSpace()
        {
            LifeNodeBase empty = LifeNode.EmptySpace(8);
            Debug.Assert(empty.Population == 0);
            LifeNodeBase largerSpace = empty.ExpandUniverse();
            Debug.Assert(largerSpace.Dimension == 16);
            Debug.Assert(largerSpace.Population == 0);

            empty = LifeNode.EmptySpace(2);
            largerSpace = empty.ExpandUniverse();
            Debug.Assert(largerSpace.Population == 0);
            Debug.Assert(largerSpace.Dimension == 4);

            LifeNodeBase block = createBlock();
            largerSpace = block.ExpandUniverse();
            Debug.Assert(largerSpace.Population == 4);
            Debug.Assert(largerSpace.Dimension == 8);

            block = LeafLifeNode.CreateNode(0x0f);
            largerSpace = block.ExpandUniverse();
            Debug.Assert(largerSpace.Equals(createBlock()));
            Debug.Assert(largerSpace.Population == 4);
            Debug.Assert(largerSpace.Dimension == 4);
            
        }

        static void TestPowerOfTwo()
        {
            Debug.Assert(Utils.IsPowerOfTwo(2));
            Debug.Assert(Utils.IsPowerOfTwo(3) == false);
            Debug.Assert(Utils.IsPowerOfTwo(65536));
            Debug.Assert(Utils.IsPowerOfTwo(999) == false);
        }

        static void TestTemporaryNodes()
        {
            LifeNode block = createBlock();
            LifeNodeBase temp = block.TemporaryCenterNode();
            Debug.Assert(temp.IsLeaf);
            Debug.Assert(temp.Population == 4);
            LifeNode w = LifeNode.CreateNode(LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(8), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(2));
            LifeNode e = LifeNode.CreateNode(LeafLifeNode.CreateNode(4), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0));

            LifeNodeBase hx = LifeNode.TemporaryCenterXNode(w, e);
            Debug.Assert(hx.IsLeaf);
            Debug.Assert(hx.Population == 3);
            Debug.Assert(hx.IsAlive(0, 0));
            Debug.Assert(hx.IsAlive(1, 0));
            Debug.Assert(hx.IsAlive(0, 1));
            Debug.Assert(!hx.IsAlive(1, 1));

            LifeNode n = LifeNode.CreateNode(LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(8), LeafLifeNode.CreateNode(4));
            LifeNode s = LifeNode.CreateNode(LeafLifeNode.CreateNode(2), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0));

            LifeNodeBase vy = LifeNode.TemporaryCenterYNode(n, s);
            Debug.Assert(vy.IsLeaf);
            Debug.Assert(vy.Population == 3);
            Debug.Assert(vy.IsAlive(0, 0));
            Debug.Assert(vy.IsAlive(1, 0));
            Debug.Assert(vy.IsAlive(0, 1));
            Debug.Assert(!vy.IsAlive(1, 1));

            LifeNode random = (LifeNode)LifeNode.RandomSpace(16);
            LifeNodeBase center = random.TemporaryCenterNode();
            Debug.Assert(center.Dimension == 8);
            for (int i = 0; i < 8; i++)
                for(int j = 0; j < 8; j++)
                {
                    Debug.Assert(random.IsAlive(i + 4, j + 4) == center.IsAlive(i, j));
                }
            LifeNode random2 = (LifeNode)LifeNode.RandomSpace(16);
            LifeNodeBase centerx = LifeNode.TemporaryCenterXNode(random, random2);
            Debug.Assert(centerx.Dimension == 8);
            for(int i = 0; i < 4; i++)
                for (int j = 0; j < 8; j++)
                {
                    Debug.Assert(random.IsAlive(i+12, j+4) == centerx.IsAlive(i,j));
                    Debug.Assert(random2.IsAlive(i, j+4) == centerx.IsAlive(i+4, j));
                }

            LifeNodeBase centery = LifeNode.TemporaryCenterYNode(random, random2);
            Debug.Assert(centery.Dimension == 8);
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 4; j++)
                {
                    Debug.Assert(random.IsAlive(i + 4, j + 12) == centery.IsAlive(i, j));
                    Debug.Assert(random2.IsAlive(i+4, j) == centery.IsAlive(i, j+4));
                }
        }

        static LifeNode HorizontalLine()
        {
            LeafLifeNode leaf = LeafLifeNode.CreateNode(3);
            LifeNode level1 = LifeNode.CreateNode(leaf, leaf, LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0));
            int dim = 4;
            LifeNode retval = level1;
            for (int i = 0; i < 7; i++)
            {
                retval = LifeNode.CreateNode(retval, retval, LifeNode.EmptySpace(dim), LifeNode.EmptySpace(dim));
                dim = dim * 2;
            }
            return retval;
        }

        static LifeNode Blinker()
        {
            LifeNode level1 = LifeNode.CreateNode(LeafLifeNode.CreateNode(3), LeafLifeNode.CreateNode(1), LeafLifeNode.CreateNode(0), LeafLifeNode.CreateNode(0));
            return level1;
        }

        static LifeNode Glider()
        {
            LifeNode level1 = LifeNode.CreateNode(LeafLifeNode.CreateNode(13), LeafLifeNode.CreateNode(1), LeafLifeNode.CreateNode(2), LeafLifeNode.CreateNode(0));
            return level1;
        }

        static void TestRun()
        {
            System.Diagnostics.Stopwatch stopWatch = new Stopwatch();
            LifeNode space = null; // (LifeNode)LifeNode.RandomSpace(256);

            space = HorizontalLine();
            stopWatch.Start();
            for (int i = 0; i < 10; i++)
            {
                while (space.NeedsExpansion())
                    space = space.ExpandUniverse();
                space = (LifeNode)space.Next();
            }
            stopWatch.Stop();
            Console.WriteLine("10 Gens Done");
            Console.WriteLine(stopWatch.Elapsed.ToString());

            LifeNode.ClearStats();
            stopWatch.Reset();
            stopWatch.Start();
            for (int i = 0; i < 4000; i++)
            {
                //if ((i % 307) == 1) LifeNode.GarbageCollect(space);
                while (space.NeedsExpansion())
                    space = space.ExpandUniverse();
                space = (LifeNode)space.Next();
            }
            stopWatch.Stop();
            Console.WriteLine("Done");
            Console.WriteLine("Population = " + space.Population.ToString());
            Console.WriteLine("1000 gens: "+ stopWatch.Elapsed.ToString());
        }
    }
}
