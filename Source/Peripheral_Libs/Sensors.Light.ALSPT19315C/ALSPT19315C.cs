﻿using Meadow;
using Meadow.Hardware;

namespace Meadow.Fountation.Sensors.Light
{
    public class ALSPT19315C
    {
        #region Properties

        /// <summary>
        ///     Voltage being output by the sensor.
        /// </summary>
        public double Voltage
        {
            get
            {
                if (_referenceVoltagePort != null)
                {
                    _referenceVoltage = _referenceVoltagePort.RawValue * 3.3;
                }
                return _sensor.RawValue * _referenceVoltage;
            }
        }
        #endregion Properties

        #region Member variables / fields

        /// <summary>
        ///     Analog port connected to the sensor.
        /// </summary>
        private readonly AnalogInputPort _sensor;

        /// <summary>
        ///     Analog port connected to the reference voltage.
        /// </summary>
        private readonly AnalogInputPort _referenceVoltagePort;

        /// <summary>
        ///     Reference voltage.
        /// </summary>
        private double _referenceVoltage;

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it being used).
        /// </summary>
        private ALSPT19315C()
        {
        }

        /// <summary>
        ///     Create a new light sensor object using a static reference voltage.
        /// </summary>
        /// <param name="pin">AnalogChannel connected to the sensor.</param>
        /// <param name="referenceVoltage">Reference voltage.</param>
        public ALSPT19315C(IAnalogPin pin, double referenceVoltage)
        {
            _sensor = new AnalogInputPort(pin);
            _referenceVoltagePort = null;
            _referenceVoltage = referenceVoltage;
        }

        /// <summary>
        ///     Create a new light sensor object using a dynaic reference voltage.
        /// </summary>
        /// <param name="pin">Analog channel connected to the sensor.</param>
        /// <param name="referenceVoltagePin">Analog channel connected to the reference voltage souce.</param>
        public ALSPT19315C(IAnalogPin pin, IAnalogPin referenceVoltagePin)
        {
            _sensor = new AnalogInputPort(pin);
            _referenceVoltagePort = new AnalogInputPort(referenceVoltagePin);
        }

        #endregion Constructors
    }
}