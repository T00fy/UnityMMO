using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MenuTree<T> : IEnumerable<MenuTree<T>>
{

    public T Data { get; set; }
    public MenuTree<T> Parent { get; set; }
    public ICollection<MenuTree<T>> Children { get; set; }

    public bool IsRoot
    {
        get { return Parent == null; }
    }

    public bool IsLeaf
    {
        get { return Children.Count == 0; }
    }

    public int Level
    {
        get
        {
            if (this.IsRoot)
                return 0;
            return Parent.Level + 1;
        }
    }


    public MenuTree(T data)
    {
        this.Data = data;
        this.Children = new LinkedList<MenuTree<T>>();

        this.ElementsIndex = new LinkedList<MenuTree<T>>();
        this.ElementsIndex.Add(this);
    }

    public MenuTree<T> AddChild(T child)
    {
        MenuTree<T> childNode = new MenuTree<T>(child) { Parent = this };
        this.Children.Add(childNode);

        this.RegisterChildForSearch(childNode);

        return childNode;
    }

    public bool RemoveChild(MenuTree<T> node)
    {
        return Children.Remove(node);
    }

    public override string ToString()
    {
        return Data != null ? Data.ToString() : "[data null]";
    }


    #region searching

    private ICollection<MenuTree<T>> ElementsIndex { get; set; }

    private void RegisterChildForSearch(MenuTree<T> node)
    {
        ElementsIndex.Add(node);
        if (Parent != null)
            Parent.RegisterChildForSearch(node);
    }

    public MenuTree<T> FindMenuTree(Func<MenuTree<T>, bool> predicate)
    {
        return this.ElementsIndex.FirstOrDefault(predicate);
    }

    #endregion


    #region iterating

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<MenuTree<T>> GetEnumerator()
    {
        yield return this;
        foreach (var directChild in this.Children)
        {
            foreach (var anyChild in directChild)
                yield return anyChild;
        }
    }

    #endregion
}
