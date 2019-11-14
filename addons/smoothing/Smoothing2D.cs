//	Copyright (c) 2019 Lawnjelly
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all
//	copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//	SOFTWARE.

using Godot;
using System;

public class Smoothing2D : Node2D {

    Node2D _m_Target;
    Vector2 m_Pos_curr = new Vector2();
    Vector2 m_Pos_prev = new Vector2();

    float m_Angle_curr;
    float m_Angle_prev;

    Vector2 m_Scale_curr = new Vector2();
    Vector2 m_Scale_prev = new Vector2();

    bool _dirty = false;
    bool _invisible = false;
    bool _enabled = false;

    NodePath target;
    [Export]
    public NodePath Target {
        get {
            return target;
        }
        set {
            target = value;

            if (this.IsInsideTree()) {
                _FindTarget();
            }
        }
    }

    [Export]
    public bool enabled {
        get {
            return _enabled;
        }
        set {
            _enabled = value;
            _SetProcessing();
        }
    }

    [Export] bool translate { get; set; }
    [Export] bool rotate { get; set; }
    [Export] bool scale { get; set; }
    [Export] bool globalIn { get; set; }
    [Export] bool globalOut { get; set; }

    // call this on e.g. starting a level, AFTER moving the target
    // so we can update both the previous and current values
    public void Teleport() {

        bool tmpTranslate = translate;
        bool tmpRotate = rotate;
        bool tmpScale = scale;

        translate = rotate = scale = true;

        _RefreshTransform();
        m_Pos_prev = m_Pos_curr;
        m_Angle_prev = m_Angle_curr;
        m_Scale_prev = m_Scale_curr;

        // call frame upate to make sure all components of the node are set
        _Process(0);

        // get back the old settings
        translate = tmpTranslate;
        rotate = tmpRotate;
        scale = tmpScale;
    }

    public override void _Ready() {
        m_Angle_curr = 0;
        m_Angle_prev = 0;
    }

    void _SetProcessing() {
        if (_invisible) {
            enabled = false;
        }
        this.SetProcess(enabled);
        this.SetPhysicsProcess(enabled);
    }

    public override void _EnterTree() {
        // might have been moved
        _FindTarget();
    }

    public override void _Notification(int what) {
        base._Notification(what);
        switch (what) {
            case NotificationVisibilityChanged:
                _invisible = this.IsVisibleInTree() == false;
                _SetProcessing();
                break;
        }
    }

    void _RefreshTransform() {
        _dirty = false;

        if (_HasTarget() == false) {
            return;
        }

        if (globalIn) {
            if (translate) {
                m_Pos_prev = m_Pos_curr;
                m_Pos_curr = _m_Target.GlobalPosition;
            }
            if (rotate) {
                m_Angle_prev = m_Angle_curr;
                m_Angle_curr = _m_Target.GlobalRotation;
            }
            if (scale) {
                m_Scale_prev = m_Scale_curr;
                m_Scale_curr = _m_Target.GlobalScale;
            }
        } else {
            if (translate) {
                m_Pos_prev = m_Pos_curr;
                m_Pos_curr = _m_Target.Position;
            }
            if (rotate) {
                m_Angle_prev = m_Angle_curr;
                m_Angle_curr = _m_Target.Rotation;
            }
            if (scale) {
                m_Scale_prev = m_Scale_curr;
                m_Scale_curr = _m_Target.Scale;
            }
        }
    }

    void _FindTarget() {
        _m_Target = null;

        if (target.IsEmpty()) {
            return;
        }

        _m_Target = (Node2D)GetNode(target);

        if (_m_Target is Node2D) {

            return;
        }

        _m_Target = null;
    }

    bool _HasTarget() {
        if (_m_Target == null) {
            return false;
        }

        // has not been deleted?
        if (Godot.Object.IsInstanceValid(_m_Target)) {
            return true;
        }

        _m_Target = null;

        return false;
    }

    public override void _Process(float delta) {
        if (_dirty) {
            _RefreshTransform();
        }

        float f = Engine.GetPhysicsInterpolationFraction();

        if (globalOut) {
            // translate
            if (translate) {
                this.GlobalPosition = m_Pos_prev.LinearInterpolate(m_Pos_curr, f);
            }
            // rotate
            if (rotate) {
                this.GlobalRotation = _LerpAngle(m_Angle_prev, m_Angle_curr, f);
            }
            if (scale) {
                this.GlobalScale = m_Scale_prev.LinearInterpolate(m_Scale_curr, f);
            }
        } else {
            // translate
            if (translate) {
                this.Position = m_Pos_prev.LinearInterpolate(m_Pos_curr, f);
            }
            // rotate
            if (rotate) {
                this.Rotation = _LerpAngle(m_Angle_prev, m_Angle_curr, f);
            }
            if (scale) {
                this.Scale = m_Scale_prev.LinearInterpolate(m_Scale_curr, f);
            }
        }
    }

    public override void _PhysicsProcess(float delta) {
        // take care of the special case where multiple physics ticks
        // occur before a frame .. the data must flow!
        if (_dirty) {
            _RefreshTransform();
        }
        _dirty = true;
    }

    float _LerpAngle(float from, float to, float weight) {
        return from + _ShortAngleDist(from, to) * weight;
    }

    float _ShortAngleDist(float from, float to) {
        float max_angle = 2 * Mathf.Pi;
        float diff = (to - from) % max_angle;
        return ((2.0f * diff) % max_angle) - diff;
    }
}
