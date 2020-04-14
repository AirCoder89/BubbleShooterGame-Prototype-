using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BubbleExploseFx : MonoBehaviour
{
    private ParticleSystem _particles;

    public void Play(Color color,Vector2 position)
    {
        if (_particles == null) _particles = GetComponent<ParticleSystem>();
        transform.position = position;
        _particles.startColor = color;
        _particles.collision.SetPlane(0,GameController.Instance.particleCollider);
        _particles.Play();
        var totalDuration = _particles.duration + _particles.startLifetime;
        Invoke("DestroyParticle",totalDuration);
    }

    private void DestroyParticle()
    {
        transform.SetParent(PoolManager.Pools["FxPool"].transform);
        PoolManager.Pools["FxPool"].Despawn(this.transform);
    }
}
