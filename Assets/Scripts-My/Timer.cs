using UnityEngine;
using System.Collections;
using System;

public class Timer
{
    private static MonoBehaviour behaviour;
    public delegate void Task();
    public delegate void Taski(float i);
    public delegate void TaskINT(int i);

    public static void Schedule(MonoBehaviour _behaviour, float delay, Task task)
    {
        behaviour = _behaviour;
        behaviour.StartCoroutine(DoTask(task, delay));
    }

    private static IEnumerator DoTask(Task task, float delay)
    {
        yield return new WaitForSeconds(delay);
        task();
    }

    public static void Schedulei(MonoBehaviour _behaviour, float delay, Taski task, float index)
    {
        behaviour = _behaviour;
        behaviour.StartCoroutine(DoTaski(task, delay, index));
    }

    private static IEnumerator DoTaski(Taski taski, float delay, float index)
    {
        yield return new WaitForSeconds(delay);
        taski(index);
    }

    //

    public static void ScheduleINT(MonoBehaviour _behaviour, float delay, TaskINT task, int index)
    {
        behaviour = _behaviour;
        behaviour.StartCoroutine(DoTaskINT(task, delay, index));
    }

    private static IEnumerator DoTaskINT(TaskINT taski, float delay, int index)
    {
        yield return new WaitForSeconds(delay);
        taski(index);
    }
}
