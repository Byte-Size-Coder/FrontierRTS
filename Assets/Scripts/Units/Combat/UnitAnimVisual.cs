using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem effect;
    
    // Start is called before the first frame update
    public void AttackEffect()
    {
        effect.Play();
    }
}
