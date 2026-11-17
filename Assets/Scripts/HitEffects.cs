using UnityEngine;

public static class HitEffects
{
    static Material particleMaterial;

    public static void BallWall(Vector3 position)
    {
        Play(position, new Color(0.3f, 0.8f, 1f, 1f), 7, 2.5f);
    }

    public static void BallBlock(Vector3 position)
    {
        Play(position, new Color(1f, 0.85f, 0.25f, 1f), 11, 3f);
    }

    public static void ProjectileBlock(Vector3 position, Color color)
    {
        Play(position, color, 15, 4.5f);
    }

    public static void Bomb(Vector3 position)
    {
        Play(position, new Color(1f, 0.35f, 0.1f, 1f), 32, 6f);
    }

    public static void Play(Vector3 position, Color color, int count, float speed)
    {
        var effectObject = new GameObject("Hit Effect");
        effectObject.SetActive(false);
        effectObject.transform.position = position;

        var particleSystem = effectObject.AddComponent<ParticleSystem>();
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particleSystem.main;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.6f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.startColor = color;
        main.gravityModifier = 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var particleRenderer = effectObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = GetParticleMaterial();
        particleRenderer.sortingOrder = 5;

        effectObject.SetActive(true);
        particleSystem.Play();
    }

    static Material GetParticleMaterial()
    {
        if (particleMaterial != null)
            return particleMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        particleMaterial = new Material(shader);
        return particleMaterial;
    }
}
