/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware {

  internal delegate void ControlEventHandler(Control control);

  internal class Control : IControl {

    private readonly Identifier identifier;
    private readonly ISettings settings;
    private ControlMode mode;
    private float softwareValue;
    private float? controlValue;  // null when no control decision has been made
    private float minSoftwareValue;
    private float maxSoftwareValue;

    public Control(ISensor sensor, ISettings settings, float minSoftwareValue,
      float maxSoftwareValue) {
      this.identifier = new Identifier(sensor.Identifier, "control");
      this.settings = settings;
      this.minSoftwareValue = minSoftwareValue;
      this.maxSoftwareValue = maxSoftwareValue;

      if (!float.TryParse(settings.GetValue(
          new Identifier(identifier, "value").ToString(), "0"),
        NumberStyles.Float, CultureInfo.InvariantCulture,
        out this.softwareValue)) {
        this.softwareValue = 0;
      }
      int mode;
      if (!int.TryParse(settings.GetValue(
          new Identifier(identifier, "mode").ToString(),
          ((int)ControlMode.Undefined).ToString(CultureInfo.InvariantCulture)),
        NumberStyles.Integer, CultureInfo.InvariantCulture,
        out mode)) {
        this.mode = ControlMode.Undefined;
      } else {
        this.mode = (ControlMode)mode;
      }
    }

    public Identifier Identifier {
      get {
        return identifier;
      }
    }

    public ControlMode ControlMode {
      get {
        return mode;
      }
      private set {
        if (mode == value) return;
        mode = value;
        SendEvent();
        this.settings.SetValue(new Identifier(identifier, "mode").ToString(),
          ((int)mode).ToString(CultureInfo.InvariantCulture));
      }
    }

    public float? DesiredValue {
      get {
        switch (ControlMode) {
          case ControlMode.Software:
            return SoftwareValue;
          case ControlMode.Controlled:
            return ControlValue;
          default:
            return null;
        }
      }
    }

    public float SoftwareValue {
      get {
        return softwareValue;
      }
      private set {
        if (softwareValue == value) return;
        softwareValue = value;
        if (ControlMode == ControlMode.Software) SendEvent();
        this.settings.SetValue(new Identifier(identifier,
          "value").ToString(),
          value.ToString(CultureInfo.InvariantCulture));
      }
    }

    public void SetDefault() {
      ControlMode = ControlMode.Default;
    }

    public float MinSoftwareValue {
      get {
        return minSoftwareValue;
      }
    }

    public float MaxSoftwareValue {
      get {
        return maxSoftwareValue;
      }
    }

    public void SetSoftware(float value) {
      SoftwareValue = value;
      ControlMode = ControlMode.Software;
    }

    public float? ControlValue {
      get {
        return controlValue;
      }
      private set {
        if (controlValue == value) return;
        controlValue = value;
        if (ControlMode == ControlMode.Controlled) SendEvent();
      }
    }
  
    public void EnableAutomaticControl() {
      ControlMode = ControlMode.Controlled;
    }

    public void SetControlled(float value) {
      ControlValue = value;
    }

    private void SendEvent() {
      if (ControlChanged != null) ControlChanged(this);
    }

    // This event is triggered when the control is changed, due to either the
    // control mode changing (e.g. default -> manual control mode), or the
    // desired control value changing (e.g. due to an automatic fan controller).
    //
    // In most cases this event can be handled by simply inspecting DesiredValue.
    internal event ControlEventHandler ControlChanged;
  }
}
