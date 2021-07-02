﻿using System;
using XamlTest.Host;
using XamlTest.Input;

namespace XamlTest
{

    public sealed class MouseInput : IInput
    {
        internal class MouseInputData : IInput
        {
            public MouseData.Types.MouseEvent Event {get; set;}
            public string? Value { get; set; }
        }


        internal IInput[] Inputs { get; }

        internal MouseInput(params IInput[] inputs)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
        }

        public static MouseInput Delay(TimeSpan timespan)
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.Delay,
                Value = timespan.TotalMilliseconds.ToString()
            });
        }

        public static MouseInput MoveToElement(Position position = Position.Center)
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.MoveToElement,
                Value = position.ToString()
            });
        }

        public static MouseInput LeftDown()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.LeftDown
            });
        }

        public static MouseInput LeftUp()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.LeftUp
            });
        }

        public static MouseInput RightDown()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.RightDown
            });
        }

        public static MouseInput RightUp()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.RightUp
            });
        }

        public static MouseInput MiddleDown()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.MiddleDown
            });
        }

        public static MouseInput MiddleUp()
        {
            return new MouseInput(new MouseInputData
            {
                Event = MouseData.Types.MouseEvent.MiddleUp
            });
        }
    }
}
