using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : NetworkBehaviour
{
    [SerializeField] private Renderer colorRender;

    private void Start()
    {
        Color color = isOwned ? Color.green : Color.red;

        colorRender.material.SetColor("_BaseColor", color);
    }
}
