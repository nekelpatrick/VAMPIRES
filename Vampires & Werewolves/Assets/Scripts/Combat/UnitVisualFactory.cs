using UnityEngine;

public static class UnitVisualFactory
{
    public static void CreateWerewolfVisual(Transform parent)
    {
        ClearChildren(parent);

        Color furColor = new Color(0.25f, 0.22f, 0.3f);
        Color furHighlight = new Color(0.4f, 0.35f, 0.45f);
        Color eyeColor = new Color(1f, 0.3f, 0.1f);
        Color clawColor = new Color(0.9f, 0.85f, 0.75f);

        GameObject body = CreatePart(parent, "Body", PrimitiveType.Capsule,
            new Vector3(0, 1.2f, 0), new Vector3(0.7f, 0.9f, 0.5f), furColor);

        GameObject chest = CreatePart(parent, "Chest", PrimitiveType.Sphere,
            new Vector3(0, 1.4f, 0.1f), new Vector3(0.75f, 0.6f, 0.5f), furHighlight);

        GameObject head = CreatePart(parent, "Head", PrimitiveType.Sphere,
            new Vector3(0, 2.1f, 0.15f), new Vector3(0.45f, 0.4f, 0.4f), furColor);

        GameObject snout = CreatePart(parent, "Snout", PrimitiveType.Capsule,
            new Vector3(0, 2.0f, 0.35f), new Vector3(0.2f, 0.2f, 0.25f), furColor);
        snout.transform.localRotation = Quaternion.Euler(90, 0, 0);

        GameObject leftEye = CreatePart(parent, "LeftEye", PrimitiveType.Sphere,
            new Vector3(-0.12f, 2.15f, 0.28f), new Vector3(0.1f, 0.08f, 0.05f), eyeColor);
        AddGlow(leftEye, eyeColor, 0.5f);

        GameObject rightEye = CreatePart(parent, "RightEye", PrimitiveType.Sphere,
            new Vector3(0.12f, 2.15f, 0.28f), new Vector3(0.1f, 0.08f, 0.05f), eyeColor);
        AddGlow(rightEye, eyeColor, 0.5f);

        GameObject leftEar = CreatePart(parent, "LeftEar", PrimitiveType.Cube,
            new Vector3(-0.15f, 2.35f, 0.05f), new Vector3(0.08f, 0.2f, 0.05f), furColor);
        leftEar.transform.localRotation = Quaternion.Euler(0, 0, 15);

        GameObject rightEar = CreatePart(parent, "RightEar", PrimitiveType.Cube,
            new Vector3(0.15f, 2.35f, 0.05f), new Vector3(0.08f, 0.2f, 0.05f), furColor);
        rightEar.transform.localRotation = Quaternion.Euler(0, 0, -15);

        CreateArm(parent, "LeftArm", -0.4f, furColor, clawColor);
        CreateArm(parent, "RightArm", 0.4f, furColor, clawColor);

        CreateLeg(parent, "LeftLeg", -0.15f, furColor);
        CreateLeg(parent, "RightLeg", 0.15f, furColor);

        GameObject tail = CreatePart(parent, "Tail", PrimitiveType.Capsule,
            new Vector3(0, 0.9f, -0.35f), new Vector3(0.12f, 0.4f, 0.12f), furColor);
        tail.transform.localRotation = Quaternion.Euler(-45, 0, 0);
    }

    static void CreateArm(Transform parent, string name, float xOffset, Color armColor, Color clawColor)
    {
        GameObject shoulder = CreatePart(parent, name + "_Shoulder", PrimitiveType.Sphere,
            new Vector3(xOffset * 0.9f, 1.5f, 0), new Vector3(0.25f, 0.2f, 0.2f), armColor);

        GameObject upperArm = CreatePart(parent, name + "_Upper", PrimitiveType.Capsule,
            new Vector3(xOffset, 1.2f, 0.1f), new Vector3(0.15f, 0.35f, 0.15f), armColor);
        upperArm.transform.localRotation = Quaternion.Euler(20, 0, xOffset > 0 ? -20 : 20);

        GameObject forearm = CreatePart(parent, name + "_Forearm", PrimitiveType.Capsule,
            new Vector3(xOffset * 1.2f, 0.85f, 0.25f), new Vector3(0.12f, 0.3f, 0.12f), armColor);
        forearm.transform.localRotation = Quaternion.Euler(45, 0, xOffset > 0 ? -10 : 10);

        GameObject hand = CreatePart(parent, name + "_Hand", PrimitiveType.Sphere,
            new Vector3(xOffset * 1.3f, 0.55f, 0.35f), new Vector3(0.18f, 0.12f, 0.15f), armColor);

        for (int i = 0; i < 4; i++)
        {
            float angle = (i - 1.5f) * 15f;
            Vector3 clawPos = hand.transform.localPosition + new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.08f,
                -0.08f,
                0.1f + Mathf.Cos(angle * Mathf.Deg2Rad) * 0.02f
            );

            GameObject claw = CreatePart(parent, name + "_Claw" + i, PrimitiveType.Capsule,
                clawPos, new Vector3(0.025f, 0.12f, 0.025f), clawColor);
            claw.transform.localRotation = Quaternion.Euler(70, angle, 0);
        }
    }

    static void CreateLeg(Transform parent, string name, float xOffset, Color legColor)
    {
        GameObject thigh = CreatePart(parent, name + "_Thigh", PrimitiveType.Capsule,
            new Vector3(xOffset, 0.7f, -0.05f), new Vector3(0.18f, 0.35f, 0.18f), legColor);

        GameObject shin = CreatePart(parent, name + "_Shin", PrimitiveType.Capsule,
            new Vector3(xOffset, 0.25f, 0.05f), new Vector3(0.12f, 0.3f, 0.12f), legColor);

        GameObject foot = CreatePart(parent, name + "_Foot", PrimitiveType.Sphere,
            new Vector3(xOffset, 0.05f, 0.1f), new Vector3(0.15f, 0.08f, 0.2f), legColor);
    }

    public static void CreateEnemyVisual(Transform parent, EnemyType type)
    {
        ClearChildren(parent);

        switch (type)
        {
            case EnemyType.Ghoul:
                CreateGhoulVisual(parent);
                break;
            case EnemyType.Skeleton:
                CreateSkeletonVisual(parent);
                break;
            case EnemyType.Demon:
                CreateDemonVisual(parent);
                break;
            case EnemyType.Wraith:
                CreateWraithVisual(parent);
                break;
            default:
                CreateGhoulVisual(parent);
                break;
        }
    }

    static void CreateGhoulVisual(Transform parent)
    {
        Color skinColor = new Color(0.4f, 0.5f, 0.35f);
        Color eyeColor = new Color(0.8f, 1f, 0.3f);

        GameObject body = CreatePart(parent, "Body", PrimitiveType.Capsule,
            new Vector3(0, 0.8f, 0), new Vector3(0.4f, 0.5f, 0.3f), skinColor);

        GameObject head = CreatePart(parent, "Head", PrimitiveType.Sphere,
            new Vector3(0, 1.4f, 0.05f), new Vector3(0.35f, 0.3f, 0.3f), skinColor);

        GameObject leftEye = CreatePart(parent, "LeftEye", PrimitiveType.Sphere,
            new Vector3(-0.08f, 1.45f, 0.15f), new Vector3(0.08f, 0.06f, 0.04f), eyeColor);
        AddGlow(leftEye, eyeColor, 0.3f);

        GameObject rightEye = CreatePart(parent, "RightEye", PrimitiveType.Sphere,
            new Vector3(0.08f, 1.45f, 0.15f), new Vector3(0.08f, 0.06f, 0.04f), eyeColor);
        AddGlow(rightEye, eyeColor, 0.3f);

        CreateSimpleArm(parent, "LeftArm", -0.25f, skinColor, true);
        CreateSimpleArm(parent, "RightArm", 0.25f, skinColor, true);
    }

    static void CreateSkeletonVisual(Transform parent)
    {
        Color boneColor = new Color(0.85f, 0.8f, 0.7f);
        Color eyeColor = new Color(0.8f, 0.2f, 0.1f);

        GameObject ribcage = CreatePart(parent, "Ribcage", PrimitiveType.Capsule,
            new Vector3(0, 0.9f, 0), new Vector3(0.35f, 0.45f, 0.2f), boneColor);

        GameObject spine = CreatePart(parent, "Spine", PrimitiveType.Capsule,
            new Vector3(0, 0.5f, 0), new Vector3(0.08f, 0.5f, 0.08f), boneColor);

        GameObject skull = CreatePart(parent, "Skull", PrimitiveType.Sphere,
            new Vector3(0, 1.5f, 0), new Vector3(0.3f, 0.35f, 0.3f), boneColor);

        GameObject jaw = CreatePart(parent, "Jaw", PrimitiveType.Cube,
            new Vector3(0, 1.35f, 0.1f), new Vector3(0.2f, 0.08f, 0.15f), boneColor);

        GameObject leftEye = CreatePart(parent, "LeftEye", PrimitiveType.Sphere,
            new Vector3(-0.08f, 1.55f, 0.12f), new Vector3(0.06f, 0.06f, 0.03f), eyeColor);
        AddGlow(leftEye, eyeColor, 0.4f);

        GameObject rightEye = CreatePart(parent, "RightEye", PrimitiveType.Sphere,
            new Vector3(0.08f, 1.55f, 0.12f), new Vector3(0.06f, 0.06f, 0.03f), eyeColor);
        AddGlow(rightEye, eyeColor, 0.4f);

        CreateBoneArm(parent, "LeftArm", -0.22f, boneColor);
        CreateBoneArm(parent, "RightArm", 0.22f, boneColor);
    }

    static void CreateDemonVisual(Transform parent)
    {
        Color skinColor = new Color(0.5f, 0.15f, 0.15f);
        Color eyeColor = new Color(1f, 0.8f, 0.2f);
        Color hornColor = new Color(0.2f, 0.1f, 0.1f);

        GameObject body = CreatePart(parent, "Body", PrimitiveType.Capsule,
            new Vector3(0, 1f, 0), new Vector3(0.5f, 0.7f, 0.4f), skinColor);

        GameObject head = CreatePart(parent, "Head", PrimitiveType.Sphere,
            new Vector3(0, 1.8f, 0), new Vector3(0.4f, 0.35f, 0.35f), skinColor);

        GameObject leftHorn = CreatePart(parent, "LeftHorn", PrimitiveType.Capsule,
            new Vector3(-0.15f, 2.1f, -0.05f), new Vector3(0.06f, 0.25f, 0.06f), hornColor);
        leftHorn.transform.localRotation = Quaternion.Euler(0, 0, 25);

        GameObject rightHorn = CreatePart(parent, "RightHorn", PrimitiveType.Capsule,
            new Vector3(0.15f, 2.1f, -0.05f), new Vector3(0.06f, 0.25f, 0.06f), hornColor);
        rightHorn.transform.localRotation = Quaternion.Euler(0, 0, -25);

        GameObject leftEye = CreatePart(parent, "LeftEye", PrimitiveType.Sphere,
            new Vector3(-0.1f, 1.85f, 0.15f), new Vector3(0.1f, 0.08f, 0.05f), eyeColor);
        AddGlow(leftEye, eyeColor, 0.6f);

        GameObject rightEye = CreatePart(parent, "RightEye", PrimitiveType.Sphere,
            new Vector3(0.1f, 1.85f, 0.15f), new Vector3(0.1f, 0.08f, 0.05f), eyeColor);
        AddGlow(rightEye, eyeColor, 0.6f);

        CreateSimpleArm(parent, "LeftArm", -0.35f, skinColor, false);
        CreateSimpleArm(parent, "RightArm", 0.35f, skinColor, false);
    }

    static void CreateWraithVisual(Transform parent)
    {
        Color ghostColor = new Color(0.3f, 0.35f, 0.5f, 0.7f);
        Color eyeColor = new Color(0.5f, 0.8f, 1f);

        GameObject body = CreatePart(parent, "Body", PrimitiveType.Capsule,
            new Vector3(0, 0.9f, 0), new Vector3(0.5f, 0.8f, 0.4f), ghostColor);

        GameObject cloak = CreatePart(parent, "Cloak", PrimitiveType.Capsule,
            new Vector3(0, 0.4f, 0), new Vector3(0.6f, 0.5f, 0.5f), ghostColor);

        GameObject hood = CreatePart(parent, "Hood", PrimitiveType.Sphere,
            new Vector3(0, 1.6f, 0), new Vector3(0.4f, 0.45f, 0.4f), ghostColor);

        GameObject leftEye = CreatePart(parent, "LeftEye", PrimitiveType.Sphere,
            new Vector3(-0.1f, 1.6f, 0.18f), new Vector3(0.1f, 0.1f, 0.05f), eyeColor);
        AddGlow(leftEye, eyeColor, 0.8f);

        GameObject rightEye = CreatePart(parent, "RightEye", PrimitiveType.Sphere,
            new Vector3(0.1f, 1.6f, 0.18f), new Vector3(0.1f, 0.1f, 0.05f), eyeColor);
        AddGlow(rightEye, eyeColor, 0.8f);
    }

    static void CreateSimpleArm(Transform parent, string name, float xOffset, Color armColor, bool hasClaws)
    {
        GameObject arm = CreatePart(parent, name, PrimitiveType.Capsule,
            new Vector3(xOffset, 0.9f, 0.1f), new Vector3(0.1f, 0.4f, 0.1f), armColor);
        arm.transform.localRotation = Quaternion.Euler(30, 0, xOffset > 0 ? -15 : 15);

        if (hasClaws)
        {
            Color clawColor = new Color(0.3f, 0.25f, 0.2f);
            for (int i = 0; i < 3; i++)
            {
                Vector3 clawPos = new Vector3(xOffset * 1.1f, 0.5f, 0.25f + i * 0.05f);
                GameObject claw = CreatePart(parent, name + "_Claw" + i, PrimitiveType.Capsule,
                    clawPos, new Vector3(0.02f, 0.08f, 0.02f), clawColor);
                claw.transform.localRotation = Quaternion.Euler(60, 0, 0);
            }
        }
    }

    static void CreateBoneArm(Transform parent, string name, float xOffset, Color boneColor)
    {
        GameObject upper = CreatePart(parent, name + "_Upper", PrimitiveType.Capsule,
            new Vector3(xOffset, 1.1f, 0), new Vector3(0.05f, 0.25f, 0.05f), boneColor);
        upper.transform.localRotation = Quaternion.Euler(0, 0, xOffset > 0 ? -20 : 20);

        GameObject lower = CreatePart(parent, name + "_Lower", PrimitiveType.Capsule,
            new Vector3(xOffset * 1.3f, 0.75f, 0.1f), new Vector3(0.04f, 0.2f, 0.04f), boneColor);
        lower.transform.localRotation = Quaternion.Euler(30, 0, xOffset > 0 ? -10 : 10);
    }

    static GameObject CreatePart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPos;
        part.transform.localScale = scale;

        Renderer r = part.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        r.material = mat;

        Object.Destroy(part.GetComponent<Collider>());

        return part;
    }

    static void AddGlow(GameObject obj, Color glowColor, float intensity)
    {
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = obj.name + "_Glow";
        glow.transform.SetParent(obj.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.5f;

        Renderer r = glow.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        Color c = glowColor;
        c.a = intensity * 0.3f;
        mat.color = c;
        r.material = mat;

        Object.Destroy(glow.GetComponent<Collider>());
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}

public enum EnemyType
{
    Ghoul,
    Skeleton,
    Demon,
    Wraith
}

