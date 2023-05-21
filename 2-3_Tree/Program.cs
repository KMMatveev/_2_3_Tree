using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _2_3_Tree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            Node Tree = new Node(14);
            for (int i = 0; i < 10; i++) 
            {
                Tree.Insert(Tree,random.Next(0, 100));
            }
            Console.WriteLine(Tree);
            Console.ReadKey();
        }
    }

    public class Node
    {
        public int size;      // количество занятых ключей
        public int[] key;
        Node first;   // *first <= key[0];
        Node second;  // key[0] <= *second < key[1];
        Node third;   // key[1] <= *third < key[2];
        Node fourth;  // key[2] <= *fourth.
        Node parent; //Указатель на родителя нужен для того, потому что адрес корня может меняться при удалении

        //public Node(int a)
        //{
        //    size = 1;
        //    key=new int[3];
        //    key[0]=a;
        //    parent=null;
        //}
        public Node(int a, Node parent)
        {
            size = 1;
            key = new int[3];
            key[0] = a;
            this.parent = parent;
        }

        public Node(int k) : this(k, null, null, null, null, null) { }

        public Node(int k, Node first_, Node second_, Node third_, Node fourth_, Node parent_)
        {
            size = 1;
            key = new int[] { k, 0, 0 };
            first = first_;
            second = second_;
            third = third_;
            fourth = fourth_;
            parent = parent_;
        }

        public Node Insert(Node p, int k)
        {
            // вставка ключа k в дерево с корнем p; всегда возвращаем корень дерева, т.к. он может меняться
            if (p == null)
                return new Node(k); // если дерево пусто, то создаем первую 2-3-вершину (корень)
            if (p.IsLeaf())
                p.InsertToNode(k);
            else if (k <= p.key[0])
                Insert(p.first, k);
            else if ((p.size == 1) || ((p.size == 2) && k <= p.key[1]))
                Insert(p.second, k);
            else
                Insert(p.third, k);
            return Split(p);
        }

        Node Split(Node item)
        {
            if (item.size < 3)
                return item;
            Node x = new Node(item.key[0], item.first, item.second, null, null, item.parent); // Создаем две новые вершины,
            Node y = new Node(item.key[2], item.third, item.fourth, null, null, item.parent); // которые имеют такого же родителя, как и разделяющийся элемент.
            if (x.first != null)
                x.first.parent = x; // Правильно устанавливаем "родителя" "сыновей".
            if (x.second != null)
                x.second.parent = x; // После разделения, "родителем" "сыновей" является "дедушка",
            if (y.first != null)
                y.first.parent = y; // Поэтому нужно правильно установить указатели.
            if (y.second != null)
                y.second.parent = y;
            if (item.parent != null)
            {
                item.parent.InsertToNode(item.key[1]);
                if (item.parent.first == item)
                    item.parent.first = null;
                else if (item.parent.second == item)
                    item.parent.second = null;
                else if (item.parent.third == item)
                    item.parent.third = null;
                // Дальше происходит своеобразная сортировка ключей при разделении.
                if (item.parent.first == null)
                {
                    item.parent.fourth = item.parent.third;
                    item.parent.third = item.parent.second;
                    item.parent.second = y;
                    item.parent.first = x;
                }
                else if (item.parent.second == null)
                {
                    item.parent.fourth = item.parent.third;
                    item.parent.third = y;
                    item.parent.second = x;
                }
                else
                {
                    item.parent.fourth = y;
                    item.parent.third = x;
                }
                Node tmp = item.parent;
                //delete item;
                return tmp;
            }
            else
            {
                x.parent = item; // Так как в эту ветку попадает только корень,
                y.parent = item; // то мы "родителем" новых вершин делаем разделяющийся элемент.
                item.BecomeNode2(item.key[1], x, y);
                return item;
            }
        }

        public Node Search(Node p, int k)
        { // Поиск ключа k в 2-3 дереве с корнем p.
            if (p == null) return null;
            if (p.Find(k)) return p;
            else if (k < p.key[0]) return Search(p.first, k);
            else if ((p.size == 2) && (k < p.key[1]) || (p.size == 1)) return Search(p.second, k);
            else if (p.size == 2) return Search(p.third, k);
            else return null;
        }

        public Node SearchMin(Node p)
        { // Поиск узла с минимальным элементов в 2-3-дереве с корнем p.
            if (p==null) return p;
            if ((p.first)==null) return p;
            else return SearchMin(p.first);
        }

        public Node Remove(Node p, int k)
        {
            // Удаление ключа k в 2-3-дереве с корнем p.
            Node item = Search(p, k); // Ищем узел, где находится ключ k
            if (item == null)
                return p;
            Node min = null;
            if (item.key[0] == k)
                min = SearchMin(item.second); // Ищем эквивалентный ключ
            else
                min = SearchMin(item.third);
            if (min != null)
            {
                // Меняем ключи местами
                int z = (k == item.key[0] ? item.key[0] : item.key[1]);
                item.Swap(ref z, ref min.key[0]);
                item = min; // Перемещаем указатель на лист, т.к. min - всегда лист
            }
            item.RemoveFromNode(k); // И удаляем требуемый ключ из листа
            return Fix(item); // Вызываем функцию для восстановления свойств дерева.
        }

        Node Fix(Node leaf)
        {
            if (leaf.size == 0 && leaf.parent == null)
            {
                // Случай 0, когда удаляем единственный ключ в дереве
                //leaf = null;
                return null;
            }
            if (leaf.size != 0)
            {
                // Случай 1, когда вершина, в которой удалили ключ, имела два ключа
                if (leaf.parent != null)
                    return Fix(leaf.parent);
                else
                    return leaf;
            }
            Node parent = leaf.parent;
            if (parent.first.size == 2 || parent.second.size == 2 || parent.size == 2)
                leaf = Redistribute(leaf); // Случай 2, когда достаточно перераспределить ключи в дереве
            else if (parent.size == 2 && parent.third.size == 2)
                leaf = Redistribute(leaf); // Аналогично
            else
                leaf = Merge(leaf); // Случай 3, когда нужно произвести склеивание и пройтись вверх по дереву как минимум на еще одну вершину
            return Fix(leaf);
        }

        Node Merge(Node leaf)
        {
            Node parent = leaf.parent;
            if (parent.first == leaf)
            {
                parent.second.InsertToNode(parent.key[0]);
                parent.second.third = parent.second.second;
                parent.second.second = parent.second.first;
                if (leaf.first != null)
                    parent.second.first = leaf.first;
                else if (leaf.second != null)
                    parent.second.first = leaf.second;
                if (parent.second.first != null)
                    parent.second.first.parent = parent.second;
                parent.RemoveFromNode(parent.key[0]);
                //delete parent.first;
                parent.first = null;
            }
            else if (parent.second == leaf)
            {
                parent.first.InsertToNode(parent.key[0]);
                if (leaf.first != null)
                    parent.first.third = leaf.first;
                else if (leaf.second != null)
                    parent.first.third = leaf.second;
                if (parent.first.third != null)
                    parent.first.third.parent = parent.first;
                parent.RemoveFromNode(parent.key[0]);
                //delete parent.second;
                parent.second = null;
            }
            if (parent.parent == null)
            {
                Node tmp = null;
                if (parent.first != null)
                    tmp = parent.first;
                else
                    tmp = parent.second;
                tmp.parent = null;
                //delete parent;
                return tmp;
            }
            return parent;
        }


        Node Redistribute(Node leaf)
        {
            Node parent = leaf.parent;
            Node first = parent.first;
            Node second = parent.second;
            Node third = parent.third;
            if ((parent.size == 2) && (first.size < 2) && (second.size < 2) && (third.size < 2))
            {
                if (first == leaf)
                {
                    parent.first = parent.second;
                    parent.second = parent.third;
                    parent.third = null;
                    parent.first.InsertToNode(parent.key[0]);
                    parent.first.third = parent.first.second;
                    parent.first.second = parent.first.first;
                    if (leaf.first != null)
                        parent.first.first = leaf.first;
                    else if (leaf.second != null)
                        parent.first.first = leaf.second;
                    if (parent.first.first != null)
                        parent.first.first.parent = parent.first;
                    parent.RemoveFromNode(parent.key[0]);
                    //first=null;
                }
                else if (second == leaf)
                {
                    first.InsertToNode(parent.key[0]);
                    parent.RemoveFromNode(parent.key[0]);
                    if (leaf.first != null)
                        first.third = leaf.first;
                    else if (leaf.second != null)
                        first.third = leaf.second;
                    if (first.third != null)
                        first.third.parent = first;
                    parent.second = parent.third;
                    parent.third = null;
                    //second = null;
                }
                else if (third == leaf)
                {
                    second.InsertToNode(parent.key[1]);
                    parent.third = null;
                    parent.RemoveFromNode(parent.key[1]);
                    if (leaf.first != null)
                        second.third = leaf.first;
                    else if (leaf.second != null)
                        second.third = leaf.second;
                    if (second.third != null)
                        second.third.parent = second;
                    //third = null;
                }
            }
            else if ((parent.size == 2) && ((first.size == 2) || (second.size == 2) || (third.size == 2)))
            {
                if (third == leaf)
                {
                    if (leaf.first != null)
                    {
                        leaf.second = leaf.first;
                        leaf.first = null;
                    }
                    leaf.InsertToNode(parent.key[1]);
                    if (second.size == 2)
                    {
                        parent.key[1] = second.key[1];
                        second.RemoveFromNode(second.key[1]);
                        leaf.first = second.third;
                        second.third = null;
                        if (leaf.first != null)
                            leaf.first.parent = leaf;
                    }
                    else if (first.size == 2)
                    {
                        parent.key[1] = second.key[0];
                        leaf.first = second.second;
                        second.second = second.first;
                        if (leaf.first != null)
                            leaf.first.parent = leaf;
                        second.key[0] = parent.key[0];
                        parent.key[0] = first.key[1];
                        first.RemoveFromNode(first.key[1]);
                        second.first = first.third;
                        if (second.first != null)
                            second.first.parent = second;
                        first.third = null;
                    }
                }
                else if (second == leaf)
                {
                    if (third.size == 2)
                    {
                        if (leaf.first == null)
                        {
                            leaf.first = leaf.second;
                            leaf.second = null;
                        }
                        second.InsertToNode(parent.key[1]);
                        parent.key[1] = third.key[0];
                        third.RemoveFromNode(third.key[0]);
                        second.second = third.first;
                        if (second.second != null)
                            second.second.parent = second;
                        third.first = third.second;
                        third.second = third.third;
                        third.third = null;
                    }
                    else if (first.size == 2)
                    {
                        if (leaf.second == null)
                        {
                            leaf.second = leaf.first;
                            leaf.first = null;
                        }
                        second.InsertToNode(parent.key[0]);
                        parent.key[0] = first.key[1];
                        first.RemoveFromNode(first.key[1]);
                        second.first = first.third;
                        if (second.first != null)
                            second.first.parent = second;
                        first.third = null;
                    }
                }
                else if (first == leaf)
                {
                    if (leaf.first == null)
                    {
                        leaf.first = leaf.second;
                        leaf.second = null;
                        first.InsertToNode(parent.key[0]);
                        if (second.size == 2)
                        {
                            parent.key[0] = second.key[0];
                            second.RemoveFromNode(second.key[0]);
                            first.second = second.first;
                            if (first.second != null)
                                first.second.parent = first;
                            second.first = second.second;
                            second.second = second.third;
                            second.third = null;
                        }
                        else if (third.size == 2)
                        {
                            parent.key[0] = second.key[0];
                            second.key[0] = parent.key[1];
                            parent.key[1] = third.key[0];
                            third.RemoveFromNode(third.key[0]);
                            first.second = second.first;
                            if (first.second != null)
                                first.second.parent = first;
                            second.first = second.second;
                            second.second = third.first;
                            if (second.second != null)
                                second.second.parent = second;
                            third.first = third.second;
                            third.second = third.third;
                            third.third = null;
                        }
                    }
                }
                else if (parent.size == 1)
                {
                    leaf.InsertToNode(parent.key[0]);
                    if (first == leaf && second.size == 2)
                    {
                        parent.key[0] = second.key[0];
                        second.RemoveFromNode(second.key[0]);
                        if (leaf.first == null)
                            leaf.first = leaf.second;
                        leaf.second = second.first;
                        second.first = second.second;
                        second.second = second.third;
                        second.third = null;
                        if (leaf.second != null)
                            leaf.second.parent = leaf;
                    }
                    else if (second == leaf && first.size == 2)
                    {
                        parent.key[0] = first.key[1];
                        first.RemoveFromNode(first.key[1]);
                        if (leaf.second == null)
                            leaf.second = leaf.first;
                        leaf.first = first.third;
                        first.third = null;
                        if (leaf.first != null)
                            leaf.first.parent = leaf;
                    }
                }
                return parent;
            }
            return null;
        }


        bool Find(int k)
        { // Этот метод возвращает true, если ключ k находится в вершине, иначе false.
            for (int i = 0; i < size; ++i)
                if (key[i] == k) return true;
            return false;
        }

        void Swap(ref int x, ref int y)
        {
            int r = x;
            x = y;
            y = r;
        }

        void Sort2(ref int x, ref int y)
        {
            if (x > y) Swap(ref x, ref y);
        }

        void Sort3(ref int x, ref int y, ref int z)
        {
            if (x > y) Swap(ref x, ref y);
            if (x > z) Swap(ref x, ref z);
            if (y > z) Swap(ref y, ref z);
        }

        void Sort()
        { // Ключи в вершинах должны быть отсортированы
            if (size == 1) return;
            if (size == 2) Sort2(ref key[0], ref key[1]);
            if (size == 3) Sort3(ref key[0], ref key[1], ref key[2]);
        }

        void InsertToNode(int k)
        {  // Вставляем ключ k в вершину (не в дерево)
            key[size] = k;
            size++;
            Sort();
        }

        void RemoveFromNode(int k)
        { // Удаляем ключ k из вершины (не из дерева)
            if (size >= 1 && key[0] == k)
            {
                key[0] = key[1];
                key[1] = key[2];
                size--;
            }
            else if (size == 2 && key[1] == k)
            {
                key[1] = key[2];
                size--;
            }
        }

        void BecomeNode2(int k, Node first_, Node second_)
        {  // Преобразовать в 2-вершину.
            key[0] = k;
            first = first_;
            second = second_;
            third = null;
            fourth = null;
            //parent = null;
            size = 1;
        }

        bool IsLeaf()
        { // Является ли узел листом; проверка используется при вставке и удалении.
            return (first == null) && (second == null) && (third == null);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Val: ");
            sb.Append(key[0]);
            sb.Append(" ");
            sb.Append(key[1]);
            if (first != null)
            {
                sb.Append('\n');
                sb.Append($"{key[0]} 1:");
                sb.Append(first.ToString());
            }
            if (second != null) 
            {
                sb.Append('\n');
                sb.Append($"{ key[0]} 2:");
                sb.Append(second.ToString()); 
            }
            if (third != null) 
            {
                sb.Append('\n');
                sb.Append($"{key[0]} 3:");
                sb.Append(third.ToString()); 
            }
            return sb.ToString();
        }
    }
}
