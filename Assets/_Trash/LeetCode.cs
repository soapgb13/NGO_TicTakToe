using System.Collections.Generic;
using UnityEngine;

#region Classes Needed

public class ListNode
{
    public int val;
    public ListNode next;

    public ListNode(int val = 0, ListNode next = null)
    {
        this.val = val;
        this.next = next;
    }
}

public class LinkedListHelper
{
    // Function to print all values in the linked list
    public static string PrintList(ListNode head)
    {
        int loopBreaker = 0;
        ListNode current = head;
        string log = "";
        while (current != null)
        {
            log += current.val + " -> ";
            current = current.next;

            loopBreaker++;
            if (loopBreaker > 100)
            {
                break;
            }
        }

        return log;
    }

    // Function to convert an array of numbers into a linked list
    public static ListNode ArrayToListNode(int[] nums)
    {
        if (nums == null || nums.Length == 0)
            return null;
        
        ListNode head = new ListNode(nums[0]);
        ListNode current = head;
        
        for (int i = 1; i < nums.Length; i++)
        {
            current.next = new ListNode(nums[i]);
            current = current.next;
        }
        
        return head;
    }
}

#endregion

public class LeetCode : MonoBehaviour
{

    [ContextMenu("Test")]
    public void CallMethod()
    {
        int[] nums = new int[] { 3,2,3 };
        int target = 6;

        int[] solution = TwoSum(nums, target);

        for (int i = 0; i < solution.Length; i++)
        {
            Debug.Log(solution[i] + " - ");
        }
    }


    public int[] TwoSum(int[] nums, int target)
    {

        int[] solution = new int[2];

        Dictionary<int, int> dict = new Dictionary<int, int>();

        for (int i = 0; i < nums.Length - 1; i++)
        {
            int neededNumber = target - nums[i];

            if (dict.ContainsKey(neededNumber))
            {
                Debug.Log($"");
                solution[0] = i;
                solution[1] = dict[neededNumber];
                return solution;
            }

            dict.Add(nums[i], i);

        }

        return solution;
    }
    
    [ContextMenu("Merge Test")]
    void LeetCode21MergeTwoSortedLists()
        {
            ListNode list1 , list2;
            
            int[] data1 = new[] { 1, 2, 4 };
            int[] data2 = new[] { 1, 3, 4 };
            
            list1 = LinkedListHelper.ArrayToListNode(data1);
            list2 = LinkedListHelper.ArrayToListNode(data2);
            
            ListNode resultListNode = MergeTwoLists(list1, list2);
            
            Debug.Log("Final List :"+ LinkedListHelper.PrintList(resultListNode));
        }

    public ListNode MergeTwoLists(ListNode list1, ListNode list2) {
        var list1Current = list1;
        var list2Current = list2;
        var head = new ListNode();
        var current = head;
        
        int breakLoop = 0;
        
        while(list1Current is not null || list2Current is not null) {
            var list1Val = list1Current?.val ?? int.MaxValue;
            var list2Val = list2Current?.val ?? int.MaxValue;
            if (list1Val < list2Val) {
                current.next = list1Current;
                list1Current = list1Current?.next;
            } else {
                current.next = list2Current;
                list2Current = list2Current?.next;
            }
            current = current?.next;

            breakLoop++;
            if (breakLoop> 200)
            {
                break;
            }
        }
        return head.next;
    }
}
