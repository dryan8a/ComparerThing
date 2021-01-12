using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ComparerThing
{
    class ComparerGenerator
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
            var leftExpr = Expression.Parameter(typeof(T), "left");
            var rightExpr = Expression.Parameter(typeof(T), "right");
            MethodCallExpression leftExprCall = default;
            MethodCallExpression rightExprCall = default;
            MethodInfo compareMethod = default;
            if (typeof(T).GetInterface("IComparable") != null)
            {
                Expression TCompareBody = Expression.Call(leftExpr, typeof(T).GetMethod("CompareTo", new Type[] { typeof(T) }), rightExpr);
                var TCompareDelegate = Expression.Lambda<Func<T, T, int>>(TCompareBody, leftExpr, rightExpr);
                return TCompareDelegate.Compile();
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
                compareMethod = comparableMembers.First().PropertyType.GetMethod("CompareTo",new Type[] { comparableMembers.First().PropertyType }); //compareto might not be the play look into it
            }
            Expression body = Expression.Call(leftExprCall, compareMethod, rightExprCall);
            var compareDelegate = Expression.Lambda<Func<T, T, int>>(body, leftExpr,rightExpr);
            return compareDelegate.Compile();
        }

        private static (Expression left, Expression right, MethodInfo compareMethod) GetComparableProperty(Type type,Expression leftExpr, Expression rightExpr)
        {
            Expression leftCall;
            Expression rightCall;
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
        Func<T, T, int> Comparer;
        Node<T> Head;
        public BST()
        {
            Head = null;
            Comparer = ComparerGenerator.GetCompareFunc<T>();
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
                if (Comparer(value, currentNode.Value) > 0)
                {
                    if (currentNode.RightChild == null)
                    {
                        currentNode.RightChild = new Node<T>(value);
                        return;
                    }
                    else currentNode = currentNode.RightChild;
                }
                else if (Comparer(value, currentNode.Value) < 0)
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
    class Test
    {
        [Comparable]
        public int num { get; set; }
    }
    class Person
    {
        public int Age { get; set; }
    }
    class Employee
    {
        public Person person { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var bstEmployee = new BST<Employee>();
            bstEmployee.Add(new Employee { person = new Person { Age = 6 } });
            bstEmployee.Add(new Employee { person = new Person { Age = 4 } });
            bstEmployee.Add(new Employee { person = new Person { Age = 7 } });
            bstEmployee.Add(new Employee { person = new Person { Age = 8 } });

            var bstPerson = new BST<Person>();
            bstPerson.Add(new Person { Age = 6 });
            bstPerson.Add(new Person { Age = 4 });
            bstPerson.Add(new Person { Age = 7 });
            bstPerson.Add(new Person { Age = 8 });

            var bstInt = new BST<int>();
            bstInt.Add(6);
            bstInt.Add(4);
            bstInt.Add(7);
            bstInt.Add(8);
        }
    }
}