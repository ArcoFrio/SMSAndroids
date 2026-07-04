using System.Collections;
using UnityEngine;

public class MouthTalkAnimation : MonoBehaviour
{
    public float minInterval = 0.05f;

    public float maxInterval = 0.2f;

    private Transform[] children;

    private Coroutine toggleCoroutine;

    private void Awake()
    {
        int childCount = base.transform.childCount;
        children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = base.transform.GetChild(i);
        }
    }

    private void OnEnable()
    {
        toggleCoroutine = StartCoroutine(ToggleChildren());
    }

    private void OnDisable()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }

        Transform[] array = children;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].gameObject.SetActive(value: false);
        }
    }

    private IEnumerator ToggleChildren()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            Transform[] array = children;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].gameObject.SetActive(value: false);
            }

            int num = Random.Range(0, children.Length);
            children[num].gameObject.SetActive(value: true);
        }
    }
}