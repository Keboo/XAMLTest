﻿namespace XamlTest.Host;

partial class TestService
{
    protected override async Task<KeyboardFocusResult> MoveKeyboardFocus(KeyboardFocusRequest request)
    {
        KeyboardFocusResult reply = new();
        bool success = await Dispatcher.TryInvokeAsync(() =>
        {
            //if (GetCachedElement<DependencyObject>(request.ElementId) is not IInputElement element)
            //{
            //    reply.ErrorMessages.Add("Could not find element");
            //    return;
            //}
            //if (element is DependencyObject @do &&
            //    Window.GetWindow(@do) is Window window)
            //{
            //    if (!ActivateWindow(window))
            //    {
            //        reply.ErrorMessages.Add($"Failed to activate window.");
            //        return;
            //    }
            //}
            //else
            //{
            //    reply.ErrorMessages.Add($"Failed to find parent window.");
            //}

            //if (Keyboard.Focus(element) != element)
            //{
            //    reply.ErrorMessages.Add($"Failed to move focus to element {element}");
            //}
            //if (element is UIElement uIElement)
            //{
            //    uIElement.Focus();
            //}
        });
        if (!success)
        {
            reply.ErrorMessages.Add($"Failed to process {nameof(MoveKeyboardFocus)}");
        }
        return reply;
    }

    protected override async Task<InputResult> SendInput(InputRequest request)
    {
        InputResult reply = new();
        bool success = await Dispatcher.TryInvokeAsync(() =>
        {
            /*
            try
            {
                IntPtr windowHandle = IntPtr.Zero;
                if (!(GetCachedElement<DependencyObject>(request.ElementId) is { } element))
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }

                Window window = Window.GetWindow(element);
                if (window is null)
                {
                    reply.ErrorMessages.Add("Failed to find parent window");
                    return;
                }
                windowHandle = new WindowInteropHelper(window).EnsureHandle();

                if (!ActivateWindow(window))
                {
                    reply.ErrorMessages.Add($"Failed to active window");
                    return;
                }

                if (windowHandle != IntPtr.Zero)
                {
                    foreach (MouseData mouseData in request.MouseData)
                    {
                        switch (mouseData.Event)
                        {
                            case MouseData.Types.MouseEvent.MoveToElement:
                                if (element is FrameworkElement frameworkElement)
                                {
                                    Rect coordinates = GetCoordinates(frameworkElement);
                                    Position position = Position.Center;
                                    if (!string.IsNullOrEmpty(mouseData.Value))
                                    {
                                        _ = Enum.TryParse(mouseData.Value, out position);
                                    }
                                    Point location = position switch
                                    {
                                        Position.TopLeft => coordinates.TopLeft,
                                        Position.TopCenter => new Point(coordinates.Center().X, coordinates.Top),
                                        Position.TopRight => coordinates.TopRight,
                                        Position.RightCenter => new Point(coordinates.Right, coordinates.Center().Y),
                                        Position.BottomRight => coordinates.BottomRight,
                                        Position.BottomCenter => new Point(coordinates.Center().X, coordinates.Bottom),
                                        Position.BottomLeft => coordinates.BottomLeft,
                                        Position.LeftCenter => new Point(coordinates.Left, coordinates.Center().Y),
                                        _ => coordinates.Center()
                                    };
                                    Input.MouseInput.MoveCursor(location);
                                }
                                break;
                            case MouseData.Types.MouseEvent.MoveRelative:
                                if (TryParsePoint(mouseData.Value, out int relX, out int relY))
                                {
                                    Point current = Input.MouseInput.GetCursorPosition();
                                    Input.MouseInput.MoveCursor(new Point(current.X + relX, current.Y + relY));
                                }
                                else
                                {
                                    reply.ErrorMessages.Add($"Failed to parse '{mouseData.Value}' as cursor offset");
                                }
                                break;
                            case MouseData.Types.MouseEvent.MoveAbsolute:
                                if (TryParsePoint(mouseData.Value, out int absX, out int absY))
                                {
                                    Input.MouseInput.MoveCursor(absX, absY);
                                }
                                else
                                {
                                    reply.ErrorMessages.Add($"Failed to parse '{mouseData.Value}' as cursor position");
                                }
                                break;
                            case MouseData.Types.MouseEvent.LeftDown:
                                Input.MouseInput.LeftDown();
                                break;
                            case MouseData.Types.MouseEvent.LeftUp:
                                Input.MouseInput.LeftUp();
                                break;
                            case MouseData.Types.MouseEvent.MiddleDown:
                                Input.MouseInput.MiddleDown();
                                break;
                            case MouseData.Types.MouseEvent.MiddleUp:
                                Input.MouseInput.MiddleUp();
                                break;
                            case MouseData.Types.MouseEvent.RightDown:
                                Input.MouseInput.RightDown();
                                break;
                            case MouseData.Types.MouseEvent.RightUp:
                                Input.MouseInput.RightUp();
                                break;
                            case MouseData.Types.MouseEvent.Delay:
                                if (!string.IsNullOrEmpty(mouseData.Value) &&
                                    int.TryParse(mouseData.Value, out int millisecondsDelay))
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(millisecondsDelay));
                                }
                                break;
                        }
                    }
                    if (request.KeyboardData.Any() && element is IInputElement inputElement)
                    {
                        if (Keyboard.Focus(inputElement) != element)
                        {
                            reply.ErrorMessages.Add($"Failed to move focus to element {element}");
                            return;
                        }
                    }
                    await Task.Run(async () =>
                    {
                        foreach (KeyboardData keyboardData in request.KeyboardData)
                        {
                            if (!string.IsNullOrEmpty(keyboardData.TextInput))
                            {
                                Input.KeyboardInput.SendKeysForText(windowHandle, keyboardData.TextInput);
                            }
                            if (keyboardData.Keys.Any())
                            {
                                Input.KeyboardInput.SendKeys(windowHandle, keyboardData.Keys.Cast<Key>().ToArray());
                            }
                            await Task.Delay(10);
                        }
                    });
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
            }*/
        });
        if (!success)
        {
            reply.ErrorMessages.Add($"Failed to process {nameof(SendInput)}");
        }

        Point point = Input.MouseInput.GetCursorPosition();
        reply.CursorX = (int)point.X;
        reply.CursorY = (int)point.Y;

        return reply;

        static bool TryParsePoint(string value, out int x, out int y)
        {
            x = 0; y = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;
            var parts = value.Split(';');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out x) &&
                int.TryParse(parts[1], out y))
            {
                return true;
            }
            return false;
        }
    }
}