using Microsoft.VisualStudio.TestTools.UnitTesting;
using ComparerThing;
using System;

namespace ComparerTest
{
    [TestClass]
    public class ComparerTest
    {
        [TestMethod]
        public void SingleLayerTest()
        {
            Random gen = new Random();
            for(int i = 0;i<1000;i++)
            {
                BST<int> test = new BST<int>();
                for(int j = 0;j<100;j++)
                {
                    test.Add(gen.Next());
                }
                Assert.IsTrue(RecursiveVerification(test.Head));
            }
        }

        [TestMethod]
        public void DoubleLayerTest()
        {
            Random gen = new Random();
            for (int i = 0; i < 1000; i++)
            {
                BST<Person> test = new BST<Person>();
                for (int j = 0; j < 100; j++)
                {
                    test.Add(new Person { Age = gen.Next() });
                }
                Assert.IsTrue(RecursiveVerification(test.Head));
            }
        }

        [TestMethod]
        public void TripleLayerTest()
        {
            Random gen = new Random();
            for (int i = 0; i < 1000; i++)
            {
                BST<Employee> test = new BST<Employee>();
                for (int j = 0; j < 100; j++)
                {
                    test.Add(new Employee { person = new Person { Age = gen.Next() } });
                }
                Assert.IsTrue(RecursiveVerification(test.Head));
            }
        }

        public bool RecursiveVerification(Node<int> node)
        {
            if (node.LeftChild != null)
            {
                if (node.LeftChild.Value >= node.Value) return false;
                if (!RecursiveVerification(node.LeftChild)) return false;
            }
            if (node.RightChild != null)
            {
                if (node.RightChild.Value <= node.Value) return false;
                if (!RecursiveVerification(node.RightChild)) return false;
            }
            return true;
        }
        public bool RecursiveVerification(Node<Person> node)
        {
            if (node.LeftChild != null)
            {
                if (node.LeftChild.Value.Age >= node.Value.Age) return false;
                if (!RecursiveVerification(node.LeftChild)) return false;
            }
            if (node.RightChild != null)
            {
                if (node.RightChild.Value.Age <= node.Value.Age) return false;
                if (!RecursiveVerification(node.RightChild)) return false;
            }
            return true;
        }
        public bool RecursiveVerification(Node<Employee> node)
        {
            if (node.LeftChild != null)
            {
                if (node.LeftChild.Value.person.Age >= node.Value.person.Age) return false;
                if (!RecursiveVerification(node.LeftChild)) return false;
            }
            if (node.RightChild != null)
            {
                if (node.RightChild.Value.person.Age <= node.Value.person.Age) return false;
                if (!RecursiveVerification(node.RightChild)) return false;
            }
            return true;
        }
    }
}
