/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Hardware {

  public enum ControlMode {
    Undefined,
    Software,  // explicitly set by the user
    Default,
    Controlled,  // set automatically by a fan controller
  }

  public interface IControl {

    Identifier Identifier { get; }

    ControlMode ControlMode { get; }

    float? DesiredValue { get; }
    
    float SoftwareValue { get; }
    float? ControlValue { get; }

    void SetDefault();

    float MinSoftwareValue { get; }
    float MaxSoftwareValue { get; }

    void SetSoftware(float value);

    void EnableAutomaticControl();
    void SetControlled(float value);
  }
}
