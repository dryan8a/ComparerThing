using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ComparerThing
{


    class Comparer
    {
        public static int GenericCompare<T>(T object1, T object2) //Inefficient generic compare
        {
            return ((IComparable)findComparableObject<T>(object1)).CompareTo(findComparableObject(object2));
        }
        public static int GenericAttributeCompare<T>(T object1,T object2)
        {
            return ((IComparable)findComparableAttributedObject<T>(object1)).CompareTo(findComparableAttributedObject(object2));
        } //old code, do not use
        private static object findComparableAttributedObject<T>(T Object)
        {
            if (Object.GetType().GetProperties().Length == 0) return null;
            var comparableMembers = Object.GetType().GetProperties().Where(a => a.CustomAttributes.Any(x => x.AttributeType == typeof(ComparableAttribute))).ToArray();
            if (comparableMembers.Count() == 0)
            {
                foreach (var member in Object.GetType().GetProperties())
                {
                    var comparable = findComparableAttributedObject(member.GetValue(Object));
                    if (comparable == null) continue;
                    return comparable;
                }
                return null;
            }
            return comparableMembers.FirstOrDefault().GetValue(Object);
        } //old code, do not use
        private static object findComparableObject<T>(T Object)
        {
            if (Object is IComparable) return Object;
            if (Object.GetType().GetProperties().Length == 0) return null;
            var comparableMembers = typeof(T).GetProperties().Where(a => a.PropertyType.GetInterfaces().Any(x => x.Name == "IComparable")).ToArray();
            if (comparableMembers.Count() == 0)
            {
                foreach(var member in Object.GetType().GetProperties())
                {
                    var comparable = findComparableObject(member.GetValue(Object));
                    if (comparable == null) continue;
                    return comparable;
                }
                return null;
            }
            return comparableMembers.FirstOrDefault().GetValue(Object);
        }
        public static Func<T, T, int> GetCompareFunc<T>()
        {
            var paramArray = Expression.Parameter(typeof(T[]),"params");
            var leftExpr = Expression.ArrayIndex(paramArray, Expression.Constant(0));
            var rightExpr = Expression.ArrayIndex(paramArray, Expression.Constant(1));
            MethodCallExpression leftExprCall = default;
            MethodCallExpression rightExprCall = default;
            MethodInfo compareMethod = default;
            if (typeof(T).GetInterface("IComparable") != null)
            {
                return (Func<T, T, int>)Delegate.CreateDelegate(typeof(Func<T, T, int>), typeof(T).GetMethod("CompareTo"));
            }
            var comparableMembers = typeof(T).GetProperties().Where(a => a.PropertyType.GetInterfaces().Any(x => x.Name == "IComparable")).ToArray();
            if(comparableMembers.Count() == 0)
            {
                var output = GetComparableProperty(typeof(T),leftExpr,rightExpr);
                if (output == (null, null, null)) throw new Exception("No comparable member exists");
               // leftExprCall = output.left;
               // rightExprCall = output.right;
                compareMethod = output.compareMethod;
            }
            else
            {
                leftExprCall = Expression.Call(leftExpr, comparableMembers.First().GetGetMethod());
                rightExprCall = Expression.Call(rightExpr, comparableMembers.First().GetGetMethod());
                compareMethod = comparableMembers.First().PropertyType.GetMethod("CompareTo"); //compareto might not be the play look into it
            }
            //Expression.Call()
            //return (Func<T, T, bool>)Delegate.CreateDelegate(typeof(Func<T, T, bool>), compareMethod);
            Expression body = Expression.Call(leftExprCall, compareMethod, rightExprCall);
            var compareDelegate = Expression.Lambda<Func<T, T, int>>(body, paramArray);
            return compareDelegate.Compile();
        }

        private static (MethodInfo left, MethodInfo right, MethodInfo compareMethod) GetComparableProperty(Type type,Expression leftExpr, Expression rightExpr)
        {
            MethodInfo left;
            MethodInfo right;
            if(type.GetInterface("IComparable") != null)
            {
                
            }
            foreach(var prop in type.GetProperties())
            {
                var output = GetComparableProperty(type,leftExpr,rightExpr);
                
            }
            return (null,null,null);
        }

        
    }
    class BST<T>
    {
        Func<T, T, int> comparer;
        Node<T> Head;
        public BST()
        {
            Head = null;
            comparer = Comparer.GetCompareFunc<T>();
        }

        public void Add(T value)
        {
            if(Head == null)
            {
                Head = new Node<T>(value);
                return;
            }

            Node<T> currentNode = Head;
            while(true)
            {
                if (Comparer.GenericCompare<T>(value, currentNode.Value) > 0)
                {
                    if (currentNode.RightChild == null)
                    {
                        currentNode.RightChild = new Node<T>(value);
                        return;
                    }
                    else currentNode = currentNode.RightChild;
                }
                else if (Comparer.GenericCompare<T>(value, currentNode.Value) < 0)
                {
                    if (currentNode.LeftChild == null)
                    {
                        currentNode.LeftChild = new Node<T>(value);
                        return;
                    }
                    else currentNode = currentNode.LeftChild;
                }
                else throw new Exception("Don't add two of same value");
            }
        }
    }
    class Node<T>
    {
        public T Value;
        public Node<T> LeftChild;
        public Node<T> RightChild;

        public Node(T value)
        {
            Value = value;
            
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ComparableAttribute : Attribute
    {
    }
    class Program
    {
        class Test
        {
            [Comparable]
            public int num { get; set; }
        }
        class Person
        {
            public int Age { get; set; }
            //public Test t { get; set; }
            //public string Name { get; set; }
        }

        class Employee
        {
            public Person person { get; set; }
        }

        //class EmployeeOfTheMonth
        //{
        //    Employee emp;
        //}

        static void Main(string[] args)
        {
            //int x = 7;
            //int y = 6;
            //var res1 = Comparer.GenericCompare(x, y);

            //Person a = new Person() { Age = 6 };
            //Person b = new Person() { Age = 6 };
            //var res2 = Comparer.GenericCompare(a,b);

            //var emp1 = new Employee { person = new Person { Age = 9, Name = "Bob", t = new Test() { num = 11 } } };
            //var emp2 = new Employee { person = new Person { Age = 7, Name = "William", t = new Test() { num = 9 } } };
            //var res3 = Comparer.GenericAttributeCompare(emp1, emp2);

            var bst = new BST<Person>();
            bst.Add( new Person { Age = 6 } );
            bst.Add(new Person { Age = 4 } );
            bst.Add(new Person { Age = 7 } );
            bst.Add(new Person { Age = 8 } );

        }
    }
}