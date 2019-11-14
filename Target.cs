using Godot;
using System;

public class Target : Spatial {

    int m_Dir = 1;
    Vector3 m_Scale = new Vector3(1, 1, 1);
    float m_Angle = 0.0f;

    public override void _PhysicsProcess(float delta) {

        var tr = this.Transform;

        var x = tr.origin.x;
        if (x >= 5) {
            m_Dir = -1;
        }
        if (x <= -5) {
            m_Dir = +1;
        }

        x += m_Dir * 0.5f;
        tr.origin.x = x;

        m_Angle += 0.1f;
        if (m_Angle > (Mathf.Pi * 2)) {
            m_Angle -= Mathf.Pi * 2;
        }

        var rotvec = new Vector3(1, 0.5f, 0.2f);
        rotvec = rotvec.Normalized();

        tr.basis = new Basis(rotvec, m_Angle);

        m_Scale.x = RandScale(m_Scale.x);
        m_Scale.y = RandScale(m_Scale.y);
        m_Scale.z = RandScale(m_Scale.z);

        tr.basis = tr.basis.Scaled(m_Scale);

        this.Transform = tr;
    }

    float RandScale(float v) {
        v += ((float)new Random().NextDouble() - 0.5f) * 0.2f;
        return Mathf.Clamp(v, 0.3f, 3.0f);
    }
}
