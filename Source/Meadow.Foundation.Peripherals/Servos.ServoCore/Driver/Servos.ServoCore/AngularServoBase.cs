﻿using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos
{
    public abstract class AngularServoBase : ServoBase, IAngularServo
    {
        /// <summary>
        /// Returns the current angle. Returns -1 if the angle is unknown.
        /// </summary>
        public Angle? Angle { get; protected set; }

        /// <summary>
        /// Instantiates a new Servo on the specified PWM Pin with the specified config.
        /// </summary>
        /// <param name="pwm"></param>
        /// <param name="config"></param>
        public AngularServoBase(IPwmPort pwm, ServoConfig config)
            : base(pwm, config)
        {
        }

        /// <summary>
        /// Rotates the servo to a given angle.
        /// </summary>
        /// <param name="angle">The angle to rotate to.</param>
        /// <param name="stopAfterMotion">When <b>true</b> the PWM will stop after motion is complete.</param>
        public void RotateTo(Angle angle, bool stopAfterMotion = false)
        {
            if (!PwmPort.State)
            {
                PwmPort.Start();
            }

            // angle check
            if (angle < Config.MinimumAngle || angle > Config.MaximumAngle)
            {
                throw new ArgumentOutOfRangeException(nameof(angle), "Angle must be within servo configuration tolerance.");
            }

            // calculate the appropriate pulse duration for the angle
            float pulseDuration = CalculatePulseDuration(angle);

            // send our pulse to the servo to make it move
            SendCommandPulse(pulseDuration);

            // update the state
            Angle = angle;
        }

        /// <summary>
        /// Stops the signal that controls the servo angle.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            Angle = null;
        }

        protected float CalculatePulseDuration(Angle angle)
        {
            // offset + (angle percent * duration length)
            return Config.MinimumPulseDuration + (float)((angle.Degrees / Config.MaximumAngle.Degrees) * (Config.MaximumPulseDuration - Config.MinimumPulseDuration));
            // sample calcs:
            // 0 degrees time = 1000 + ( (0 / 180) * 1000 ) = 1,000 microseconds
            // 90 degrees time = 1000 + ( (90 / 180) * 1000 ) = 1,500 microseconds
            // 180 degrees time = 1000 + ( (180 / 180) * 1000 ) = 2,000 microseconds
        }
    }
}