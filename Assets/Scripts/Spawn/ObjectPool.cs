
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public GameObject subject;
    public int numberOfInstances;

    Queue<GameObject> unusedObjectsQueue;
    MyList<GameObject> usedObjects;


    private void Awake()
    {
        usedObjects = new MyList<GameObject>();
        unusedObjectsQueue = new Queue<GameObject>(numberOfInstances);

    }


    private void Start()
    {
        for (int i = 0; i < numberOfInstances; i++)
        {
            GameObject item = CreateSubject();
            item.name = item.name + " " + i + 1;
            unusedObjectsQueue.Enqueue(item);
        }
    }

    public GameObject RequestSubject()
    {
        GameObject subject;
        if (unusedObjectsQueue.Count != 0 || RetrieveFromUsed())
        {
            subject = unusedObjectsQueue.Dequeue();
        }
        else
        {
            subject = CreateSubject();
        }
        usedObjects.Add(subject);
        return subject;
    }

    public enum ObjectState
    {
        Active,
        Disabled,
        Any
    }

    public int GetNumberOfObjectsInPool(ObjectState state)
    {
        if (state == ObjectState.Any) return numberOfInstances;
        int count = 0;
        bool isActive = (state == ObjectState.Active);

        foreach (Node<GameObject> node in usedObjects)
        {
            if (node.Data.activeInHierarchy == isActive) count++;
        }

        foreach (GameObject go in unusedObjectsQueue)
        {
            if (go.gameObject.activeInHierarchy == isActive) count++;
        }

        return count;
    }



    private GameObject CreateSubject()
    {
        GameObject instance = Instantiate(subject, transform.position, Quaternion.identity, this.transform);
        instance.SetActive(false);
        return instance;
    }

    public void DeactivateAll()
    {
        foreach (GameObject go in unusedObjectsQueue)
        {
            go.SetActive(false);
        }

        foreach (Node<GameObject> node in usedObjects)
        {
            node.Data.SetActive(false);
        }
    }

    private bool RetrieveFromUsed()
    {
        bool retrieved = false;

        foreach (Node<GameObject> node in usedObjects)
        {
            if (node.Data.activeSelf == false)
            {
                unusedObjectsQueue.Enqueue(node.Data);
                usedObjects.RemoveNode(node);
                retrieved = true;
            }
        }
        return retrieved;
    }

    private class Node<T>
    {
        private Node<T> previous;
        private Node<T> next;
        private T data;


        public Node(T data, Node<T> next, Node<T> previous)
        {
            this.previous = previous;
            this.data = data;
            this.next = next;
        }

        public Node<T> Next
        {
            get { return next; }
            set { next = value; }
        }

        public Node<T> Previous
        {
            get { return previous; }
            set { previous = value; }
        }

        public T Data
        {
            get { return data; }
            set { data = value; }
        }

        public bool HasNext()
        {
            return next != null;
        }

        public bool HasPrevious()
        {
            return previous != null;
        }

    }


    private class MyList<T> : IEnumerable
    {
        public Node<T> head;
        public Node<T> tail;
        public int Count;


        public MyList()
        {
            head = new Node<T>(default(T), null, null);
            tail = head;
        }

        public void Add(T obj)
        {
            tail.Next = new Node<T>(obj, null, tail);
            tail = tail.Next;
            this.Count++;
        }

        public void RemoveNode(Node<T> node)
        {
            if (node == tail)
            {
                tail = node.Previous;
                tail.Next = null;
            }
            else if (node.Previous == head)
            {
                head = node;
                head.Previous = null;
            }
            else
            {
                node.Previous.Next = node.Next;
                node.Next.Previous = node.Previous;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new MyListEnumerator<T>(head);
        }

        public new string ToString
        {
            get
            {

                if (head == null)
                {
                    return "Empty List";
                }

                Node<T> temp = head;
                string result = "";
                while (temp != null)
                {
                    if (temp.HasPrevious()) result += " - Previous:" + temp.Previous.Data.ToString();
                    result += " - Current:" + temp.Data.ToString();
                    if (temp.HasNext()) result += " - Next:" + temp.Next.Data.ToString() + "\n";
                    temp = temp.Next;
                }

                return result;
            }
        }
    }

    private class MyListEnumerator<T> : IEnumerator
    {
        private Node<T> iteratorNode;

        public MyListEnumerator(Node<T> head)
        {
            iteratorNode = head;
        }

        public object Current => iteratorNode;

        public bool MoveNext()
        {
            if (iteratorNode.HasNext())
            {
                iteratorNode = iteratorNode.Next;
                return true;
            }
            return false;

        }

        public void Reset()
        {
        }
    }
}



