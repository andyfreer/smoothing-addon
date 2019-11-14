using Godot;
using System;

public class Root : Spatial {

    public override void _Process(float delta) {

        if (Input.IsActionJustPressed("ui_cancel")) {
            GetTree().Quit();
        }

        if (Input.IsActionJustPressed("ui_select")) {
            GetNode("Example3D").GetNode<Spatial>("Target").Translation = Vector3.Zero;
            GetNode("Example3D").GetNode<Smoothing>("Smoothing").Teleport();

            GetNode("Example2D").GetNode<Node2D>("Target2D").Position = new Vector2(300, 300);
            GetNode("Example2D").GetNode<Smoothing2D>("Smoothing2D").Teleport();
        }
    }
}
