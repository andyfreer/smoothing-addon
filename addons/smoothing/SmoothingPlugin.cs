#if TOOLS
using Godot;
using System;

[Tool]
public class SmoothingPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here
        // Add the new type with a name, a parent type, a script and an icon
        add_custom_type("Smoothing", "Spatial", preload("Smoothing.cs"), preload("smoothing.png"));
        add_custom_type("Smoothing2D", "Node2D", preload("Smoothing2d.cs"), preload("smoothing_2d.png"));
        pass
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here
        // Always remember to remove it from the engine when deactivated
        remove_custom_type("Smoothing");
        remove_custom_type("Smoothing2D");
    }
}
#endif