﻿using Meadow.Hardware;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    /// Provide an interface for ePaper monochrome displays
    /// </summary>
    public abstract class EPaperMonoBase : EPaperBase, IGraphicsDisplay
    {
        /// <summary>
        /// Display color mode 
        /// </summary>
        public ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// The buffer the holds the pixel data for the display
        /// </summary>
        public IPixelBuffer PixelBuffer => imageBuffer;

        /// <summary>
        /// Buffer to hold display data
        /// </summary>
        protected readonly Buffer1bpp imageBuffer;

        /// <summary>
        /// Width of display in pixels
        /// </summary>
        public int Width => imageBuffer.Width;

        /// <summary>
        /// Height of display in pixels
        /// </summary>
        public int Height => imageBuffer.Height;

        private EPaperMonoBase()
        { }

        /// <summary>
        /// Create a new ePaper display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public EPaperMonoBase(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width, int height):
            this(spiBus, device.CreateDigitalOutputPort(chipSelectPin), device.CreateDigitalOutputPort(dcPin, false),
                device.CreateDigitalOutputPort(resetPin, true), device.CreateDigitalInputPort(busyPin),
                width, height)
        {
        }

        /// <summary>
        /// Create a new ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        /// <param name="width">Width of display in pixels</param>
        /// <param name="height">Height of display in pixels</param>
        public EPaperMonoBase(ISpiBus spiBus, 
            IDigitalOutputPort chipSelectPort, 
            IDigitalOutputPort dataCommandPort, 
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort,
            int width, int height)
        {
            this.dataCommandPort = dataCommandPort;
            this.chipSelectPort = chipSelectPort;
            this.resetPort = resetPort;
            this.busyPort = busyPort;

            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);

            imageBuffer = new Buffer1bpp(width, height);

            imageBuffer.Clear(true);

            Initialize();
        }

        /// <summary>
        /// Initialize display
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Clear display buffer
        /// </summary>
        /// <param name="updateDisplay">force display update</param>
        public void Clear(bool updateDisplay = false)
        {
            Clear(false, updateDisplay);
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="color">Color to set the display (not used on ePaper displays)</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Fill(Color color, bool updateDisplay = false)
        {
            Clear(color.Color1bpp, updateDisplay);
        }

        /// <summary>
        /// Fill the display buffer with a color
        /// </summary>
        /// <param name="x">x location in pixels to start fill</param>
        /// <param name="y">y location in pixels to start fill</param>
        /// <param name="width">width in pixels to fill</param>
        /// <param name="height">height in pixels to fill</param>
        /// <param name="color">color to fill</param>
        public void Fill(int x, int y, int width, int height, Color color)
        {
            imageBuffer.Fill(x, y, width, height, color);
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        /// <param name="colored">Set the display dark when true</param>
        /// <param name="updateDisplay">Update the dipslay once the buffer has been cleared when true.</param>
        public void Clear(bool colored, bool updateDisplay = false)
        {
            for (int i = 0; i < imageBuffer.ByteCount; i++)
            {
                imageBuffer.Buffer[i] = colored ? (byte)0 : (byte)255;
            }

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
            {
                imageBuffer.Buffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
            else
            {
                imageBuffer.Buffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Invert color of pixel
        /// </summary>
        /// <param name="x">x coordinate of pixel</param>
        /// <param name="y">y coordinate of pixel</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.Buffer[(x + y * Width) / 8] ^= (byte)(0x80 >> (x % 8));
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="r">red component of color in bytes (0-255)</param>
        /// <param name="g">green component of color in bytes (0-255)</param>
        /// <param name="b">blue component of color in bytes (0-255)</param>
        public void DrawPixel(int x, int y, byte r, byte g, byte b)
        {
            DrawPixel(x, y, r > 0 || g > 0 || b > 0);
        }

        /// <summary>
        /// Draw the display buffer to screen
        /// </summary>
        public virtual void Show(int left, int top, int right, int bottom)
        {
            SetFrameMemory(imageBuffer.Buffer, left, top, right - left, top - bottom);
            DisplayFrame();
        }

        /// <summary>
        /// Draw the display buffer to screen
        /// </summary>
        public virtual void Show()
        {
            SetFrameMemory(imageBuffer.Buffer);
            DisplayFrame();
        }

        /// <summary>
        /// Draw a buffer at a specific location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="displayBuffer"></param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Set frame buffer memory of display
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="width">width in pixels</param>
        /// <param name="height">height in pixels</param>
        public virtual void SetFrameMemory(byte[] buffer,
                                int x,
                                int y,
                                int width,
                                int height)
        {
            int xEnd;
            int yEnd;

            if (buffer == null ||  x < 0 || width < 0 ||  y < 0 || height < 0)
            {
                return;
            }

            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            x &= 0xF8;
            width &= 0xF8;
            if (x + width >= Width)
            {
                xEnd = Width - 1;
            }
            else
            {
                xEnd = x + width - 1;
            }

            if (y + height >= Height)
            {
                yEnd = Height - 1;
            }
            else
            {
                yEnd = y + height - 1;
            }

            SetMemoryArea(x, y, xEnd, yEnd);
            SetMemoryPointer(x, y);
            SendCommand(Command.WRITE_RAM);
            /* send the image data */
            for (int j = 0; j < yEnd - y + 1; j++)
            {
                for (int i = 0; i < (xEnd - x + 1) / 8; i++)
                {
                    SendData(buffer[i + j * (width / 8)]);
                }
            }
        }

        /// <summary>
        /// Set frame buffer memory of display (full screen)
        /// </summary>
        /// <param name="buffer">buffer</param>

        public virtual void SetFrameMemory(byte[] buffer)
        {
            SetMemoryArea(0, 0, Width - 1, Height - 1);
            SetMemoryPointer(0, 0);
            SendCommand(Command.WRITE_RAM);
            /* send the image data */

            dataCommandPort.State = DataState;
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                spiPeripheral.Write(buffer[i]);
            }
        }

        /// <summary>
        /// Clear frame buffer memory
        /// </summary>
        /// <param name="color">color to clear to</param>
        public virtual void ClearFrameMemory(byte color)
        {
            SetMemoryArea(0, 0, Width - 1, Height - 1);
            SetMemoryPointer(0, 0);
            SendCommand(Command.WRITE_RAM);
            /* send the color data */
            for (int i = 0; i < Width / 8 * Height; i++)
            {
                SendData(color);
            }
        }

        /// <summary>
        /// Display frame buffer 
        /// </summary>
        public virtual void DisplayFrame()
        {
            SendCommand(Command.DISPLAY_UPDATE_CONTROL_2);
            SendData(0xC4);
            SendCommand(Command.MASTER_ACTIVATION);
            SendCommand(Command.TERMINATE_FRAME_READ_WRITE);
            WaitUntilIdle();
        }

        void SetMemoryArea(int x_start, int y_start, int x_end, int y_end)
        {
            SendCommand(Command.SET_RAM_X_ADDRESS_START_END_POSITION);
            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            SendData((x_start >> 3) & 0xFF);
            SendData((x_end >> 3) & 0xFF);
            SendCommand(Command.SET_RAM_Y_ADDRESS_START_END_POSITION);
            SendData(y_start & 0xFF);
            SendData((y_start >> 8) & 0xFF);
            SendData(y_end & 0xFF);
            SendData((y_end >> 8) & 0xFF);
        }

        void SetMemoryPointer(int x, int y)
        {
            SendCommand(Command.SET_RAM_X_ADDRESS_COUNTER);
            /* x point must be the multiple of 8 or the last 3 bits will be ignored */
            SendData((x >> 3) & 0xFF);
            SendCommand(Command.SET_RAM_Y_ADDRESS_COUNTER);
            SendData(y & 0xFF);
            SendData((y >> 8) & 0xFF);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set display to sleep mode
        /// </summary>
        protected void Sleep()
        {
            SendCommand(Command.DEEP_SLEEP_MODE);
            WaitUntilIdle();
        }

        /// <summary>
        /// Set command to display
        /// </summary>
        /// <param name="command">command</param>
        protected void SendCommand(Command command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Display commands
        /// </summary>
        protected enum Command : byte
        {
            DRIVER_OUTPUT_CONTROL = 0x01,
            BOOSTER_SOFT_START_CONTROL = 0x0C,
            GATE_SCAN_START_POSITION = 0x0F,
            DEEP_SLEEP_MODE = 0x10,
            DATA_ENTRY_MODE_SETTING = 0x11,
            SW_RESET = 0x12,
            TEMPERATURE_SENSOR_CONTROL = 0x1A,
            MASTER_ACTIVATION = 0x20,
            DISPLAY_UPDATE_CONTROL_1 = 0x21,
            DISPLAY_UPDATE_CONTROL_2 = 0x22,
            WRITE_RAM = 0x24,
            WRITE_VCOM_REGISTER = 0x2C,
            WRITE_LUT_REGISTER = 0x32,
            SET_DUMMY_LINE_PERIOD = 0x3A,
            SET_GATE_TIME = 0x3B,
            BORDER_WAVEFORM_CONTROL = 0x3C,
            SET_RAM_X_ADDRESS_START_END_POSITION = 0x44,
            SET_RAM_Y_ADDRESS_START_END_POSITION = 0x45,
            SET_RAM_X_ADDRESS_COUNTER = 0x4E,
            SET_RAM_Y_ADDRESS_COUNTER = 0x4F,
            TERMINATE_FRAME_READ_WRITE = 0xFF,
        }
    }
}