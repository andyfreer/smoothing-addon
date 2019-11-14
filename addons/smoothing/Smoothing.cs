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

public class Smoothing : Spatial {

    Spatial _m_Target;
    Transform _m_trCurr;
    Transform _m_trPrev;
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
    [Export] bool basis { get; set; }
    [Export] bool slerp { get; set; }

    // call this on e.g. starting a level, AFTER moving the target
    // so we can update both the previous and current values
    public void Teleport() {
        bool tmpTranslate = translate;
        bool tmpBasis = basis;

        translate = basis = true;

        _RefreshTransform();
        _m_trPrev = _m_trCurr;

        // do one frame update to make sure all components are updated
        _Process(0);

        // resume old settings
        translate = tmpTranslate;
        basis = tmpBasis;
    }

    public override void _Ready() {
        _m_trCurr = new Transform();
        _m_trPrev = new Transform();
        _RefreshTransform();
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
        _m_trPrev = _m_trCurr;
        _m_trCurr = _m_Target.Transform;
    }

    void _FindTarget() {
        _m_Target = null;

        if (target.IsEmpty()) {
            return;
        }
        _m_Target = (Spatial)GetNode(target);

        if (_m_Target is Spatial) {
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
        Transform tr = new Transform();

        // translate
        if (translate) {
            tr.origin = _m_trPrev.origin + ((_m_trCurr.origin - _m_trPrev.origin) * f);
        }

        // rotate
        if (basis) {
            if (slerp) {
                tr.basis = _SlerpBasis(_m_trPrev.basis, _m_trCurr.basis, f);
            } else {
                tr.basis = _LerpBasis(_m_trPrev.basis, _m_trCurr.basis, f);
            }
        }
        this.Transform = tr;
    }

    public override void _PhysicsProcess(float delta) {
        // take care of the special case where multiple physics ticks
        // occur before a frame .. the data must flow!
        if (_dirty) {
            _RefreshTransform();
        }
        _dirty = true;
    }

    Basis _LerpBasis(Basis from, Basis to, float f) {
        return new Basis(from.x.LinearInterpolate(to.x, f),
            from.y.LinearInterpolate(to.y, f),
            from.z.LinearInterpolate(to.z, f));
    }

    Basis _SlerpBasis(Basis from, Basis to, float f) {
        return new Basis(from.x.Slerp(to.x, f),
            from.y.Slerp(to.y, f),
            from.z.Slerp(to.z, f));
    }
}
