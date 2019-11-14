using Godot;
using System;

public class Target2D : Node2D {
    const int MOVE_DIST = 100;
    int m_Dir = MOVE_DIST;

    public override void _PhysicsProcess(float delta) {
        var x = this.Position.x;

        if (x > 1000) {
            m_Dir = -MOVE_DIST;
        }
        if (x < 0) {
            m_Dir = MOVE_DIST;
        }
        x += m_Dir;

        this.Position = new Vector2(x, this.Position.y);

        this.Rotate(0.1f);

        float sc = x / 1000.0f;

        this.Scale = new Vector2(sc * 3, (1.0f - sc) * 3);
    }
}
