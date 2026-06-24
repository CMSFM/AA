using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPTextRotator : MonoBehaviour
{
    [Header("회전시킬 TextMeshPro 리스트")]
    [SerializeField] private List<TMP_Text> targetTexts = new();

    [Header("회전 속도")]
    [SerializeField] private Vector3 rotateSpeed = new Vector3(0f, 0f, 90f);

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (TMP_Text text in targetTexts)
        {
            if (text == null) continue;

            text.transform.Rotate(rotateSpeed * deltaTime);
        }
    }
}